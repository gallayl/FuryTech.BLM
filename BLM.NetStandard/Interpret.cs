using System.Linq;
using BLM.NetStandard.Interfaces;
using BLM.NetStandard.Interfaces.Interpret;

namespace BLM.NetStandard
{
    public static class Interpret
    {
        public static T BeforeCreate<T>(T entity, IContextInfo context)
        {
            var createInterpreters = Loader.GetEntriesFor<IInterpretBeforeCreate<T, T>>();
            return createInterpreters.Cast<IInterpretBeforeCreate>().Aggregate(entity, (current, intr) => (T)intr.DoInterpret(current, context));
        }

        public static T BeforeModify<T>(T originalEntity, T modifiedEntity, IContextInfo context)
        {
            var modifyInterpreters = Loader.GetEntriesFor<IInterpretBeforeModify<T, T>>();
            return modifyInterpreters.Cast<IInterpretBeforeModify>().Aggregate(modifiedEntity, (current, intr) => (T)intr.DoInterpret(originalEntity, current, context));
        }
    }
}
