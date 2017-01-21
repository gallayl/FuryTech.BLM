using BLM.Interfaces;

namespace BLM.EF6.Tests
{
    public class MockInterpretedEntityModifyInterpreter : InterpretBeforeModify<MockInterpretedEntity>
    {
        public override MockInterpretedEntity DoInterpret(MockInterpretedEntity originalEntity, MockInterpretedEntity modifiedEntity,
            IContextInfo context)
        {
            modifiedEntity.MockInterpretedValue = MockInterpretedValue.ModifyInterpreted;
            return modifiedEntity;
        }
    }
}