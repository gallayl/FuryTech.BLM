using System.Collections.Generic;

namespace BLM.NetStandard.Tests
{
    public class MockNestedEntity
    {
        public int Id { get; set; }
        public virtual List<MockEntity> MockEntities { get; set; }
    }
}
