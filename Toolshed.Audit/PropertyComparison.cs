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
            if (oldValue != null)
            {
                Type = oldValue.GetType().Name;
                OldValue = oldValue.ToString();
            }
            if(newValue != null)
            {
                if (Type == null)
                {
                    Type = newValue.GetType().Name;
                }
                NewValue = newValue.ToString();
            }
        }


        public string Name { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Type { get; set; }
    }
}
