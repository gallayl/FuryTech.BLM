using BLM.NetStandard.Attributes;

namespace BLM.NetStandard.Tests
{
    public class LogicalDeleteEntity : MockEntity
    {
        [LogicalDelete]
        public bool IsDeleted { get; set; }
    }
}