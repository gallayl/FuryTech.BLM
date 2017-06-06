using BLM.NetStandard;
using BLM.NetStandard.Interfaces;

namespace BLM.EF7.Tests
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