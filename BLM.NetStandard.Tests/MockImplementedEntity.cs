namespace BLM.NetStandard.Tests
{
    public class MockImplementedEntity : IMockEntity
    {
        public string Guid { get; set; }
        public bool IsVisible { get; set; }
        public bool IsValid { get; set; }
    }
}
