using System;

namespace BLM.NetStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LogicalDeleteAttribute : Attribute
    {
        public LogicalDeleteAttribute()
        {
            LogicalDelete = true;
        }

        public bool LogicalDelete { get; set; }
    }
}
