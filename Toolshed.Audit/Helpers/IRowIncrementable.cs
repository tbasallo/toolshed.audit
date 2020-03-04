using System;
using System.Collections.Generic;
using System.Text;

namespace Toolshed.Audit
{
    public interface IRowIncrementable
    {
        string RowKey { get; set; }
    }
}
