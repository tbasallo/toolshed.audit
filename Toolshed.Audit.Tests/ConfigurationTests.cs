using Microsoft.Extensions.DependencyInjection;

namespace Toolshed.Audit.Tests;

//ServiceManager is static, so these tests cannot run in parallel
[Collection("ServiceManager")]
public class ConfigurationTests
{
    const string ConnectionString = "UseDevelopmentStorage=true";

    [Fact]
    public void NoConfiguration_UsesDefaultQueueWithoutPrefix()
    {
        ServiceManager.Init(ConnectionString);

        var queue = Assert.Single(ServiceManager.QueueTablePrefixes);
        Assert.Equal("auditor-pending-items", queue.Key);
        Assert.Null(queue.Value);
        Assert.Null(ServiceManager.GetTablePrefix(ServiceManager.DefaultQueueName));
    }

    [Fact]
    public void DefaultAndCustomQueues_AreBothRegistered()
    {
        ServiceManager.Init(ConnectionString, o => o.UseDefaultQueue().AddQueue("app2", "B"));

        Assert.Equal(2, ServiceManager.QueueTablePrefixes.Count);
        Assert.Null(ServiceManager.GetTablePrefix(ServiceManager.DefaultQueueName));
        Assert.Equal("B", ServiceManager.GetTablePrefix("APP2"));
    }

    [Fact]
    public void OnlyCustomQueues_DoesNotRegisterDefault()
    {
        ServiceManager.Init(ConnectionString, o => o.AddQueue("app1", "A").AddQueue("app2", "B"));

        Assert.False(ServiceManager.IsQueueRegistered(ServiceManager.DefaultQueueName));
        Assert.Throws<InvalidOperationException>(() => ServiceManager.GetTablePrefix(ServiceManager.DefaultQueueName));
        Assert.Throws<InvalidOperationException>(() => new AuditEnqueuer(ServiceManager.DefaultQueueName));
        Assert.Throws<InvalidOperationException>(() => new AuditRepository(ServiceManager.DefaultQueueName));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("bad_name")]
    [InlineData("-bad")]
    [InlineData("bad--name")]
    public void InvalidQueueName_Throws(string queueName)
    {
        Assert.Throws<ArgumentException>(() => new AuditOptions().AddQueue(queueName, "A"));
    }

    [Theory]
    [InlineData("1A")]
    [InlineData("A-B")]
    [InlineData(" ")]
    [InlineData("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")]
    public void InvalidTablePrefix_Throws(string tablePrefix)
    {
        Assert.Throws<ArgumentException>(() => new AuditOptions().AddQueue("app1", tablePrefix));
    }

    [Fact]
    public void DuplicateQueue_Throws()
    {
        Assert.Throws<ArgumentException>(() => new AuditOptions().AddQueue("app1", "A").AddQueue("APP1", "B"));
        Assert.Throws<ArgumentException>(() => new AuditOptions().UseDefaultQueue().UseDefaultQueue());
        Assert.Throws<ArgumentException>(() => new AuditOptions().AddQueue(ServiceManager.DefaultQueueName, "A"));
    }

    [Fact]
    public void AddToolshedAuditing_RegistersKeyedServicesOnly()
    {
        var provider = new ServiceCollection()
            .AddToolshedAuditing(ConnectionString, o => o.UseDefaultQueue().AddQueue("app2", "B"))
            .BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.Null(scope.ServiceProvider.GetService<AuditEnqueuer>());
        Assert.Null(scope.ServiceProvider.GetService<AuditRepository>());

        Assert.Equal("app2", scope.ServiceProvider.GetRequiredKeyedService<AuditEnqueuer>("app2").QueueName);
        Assert.Equal("B", scope.ServiceProvider.GetRequiredKeyedService<AuditRepository>("app2").TablePrefix);
        Assert.Null(scope.ServiceProvider.GetRequiredKeyedService<AuditRepository>(ServiceManager.DefaultQueueName).TablePrefix);
        Assert.NotNull(scope.ServiceProvider.GetService<AuditJanitor>());
    }

    [Fact]
    public void TablePrefixes_IncludeEveryRegisteredPrefix()
    {
        ServiceManager.Init(ConnectionString, o => o.AddQueue("app1", "A").AddQueue("app2", "B"));

        Assert.Equal(["A", "B"], ServiceManager.TablePrefixes.Order());
    }

    [Fact]
    public async Task QueueProcessor_RejectsActivityFromAnotherQueue()
    {
        ServiceManager.Init(ConnectionString, o => o.AddQueue("app1", "A").AddQueue("app2", "B"));
        var activity = new AuditActivity("Thing", 1, AuditActivityType.Update) { QueueName = "app1" };

        await Assert.ThrowsAsync<ArgumentException>(() => AuditQueueProcessor.AddActivity(activity, "app2"));
    }
}
