using BLM.Attributes;

namespace BLM.Tests
{
    public class LogicalDeleteEntity : MockEntity
    {
        [LogicalDelete]
        public bool IsDeleted { get; set; }
    }
}