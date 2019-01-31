using System;

namespace BLM.NetStandard.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class LogicalDeleteAttribute : Attribute
    {
        public LogicalDeleteAttribute()
        {
            LogicalDelete = true;
        }

        public bool LogicalDelete { get; set; }
    }
}
