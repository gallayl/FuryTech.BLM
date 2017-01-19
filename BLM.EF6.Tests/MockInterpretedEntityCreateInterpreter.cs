using BLM.Interfaces.Interpret;

namespace BLM.EF6.Tests
{
    public class MockInterpretedEntityCreateInterpreter : InterpretBeforeCreate<MockInterpretedEntity>
    {
        public override MockInterpretedEntity DoInterpret(MockInterpretedEntity entity, IContextInfo context)
        {
            entity.MockInterpretedValue = MockInterpretedValue.CreateInterpreted;
            return entity;
        }
    }
}