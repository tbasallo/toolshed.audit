using System;
using System.Collections.Generic;
using System.Text;
using Toolshed;

namespace Toolshed.Audit
{
    public static class TimeZoneHelper
    {
        public static DateTime GetDate(DateTime date)
        {
            if (string.IsNullOrEmpty(AuditSettings.PartitionTimeZone))
            {
                return date;
            }
            else
            {
                return date.ToTimeZone(AuditSettings.PartitionTimeZone);
            }
        }
        public static DateTimeOffset GetDate(DateTimeOffset date)
        {
            if (string.IsNullOrEmpty(AuditSettings.PartitionTimeZone))
            {
                return date;
            }
            else
            {
                return date.ToTimeZone(AuditSettings.PartitionTimeZone);
            }
        }
    }
}
