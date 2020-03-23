using System;
using System.Collections.Generic;
using System.Text;

namespace Toolshed.Audit
{
    public class PropertyComparison
    {
        public PropertyComparison() { }
        public PropertyComparison(string name, object oldValue, object newValue)
        {
            Name = name;
            Type = oldValue.GetType().Name;
            if (newValue is Enum)
            {
                OldValue = oldValue.ToString();
                Newvalue = newValue.ToString();
            }
            else
            {
                OldValue = oldValue.ToString();
                Newvalue = newValue.ToString();
            }
        }


        public string Name { get; set; }
        public string OldValue { get; set; }
        public string Newvalue { get; set; }
        public string Type { get; set; }
    }
}
