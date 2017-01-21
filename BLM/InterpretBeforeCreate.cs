using BLM.Interfaces;
using BLM.Interfaces.Interpret;

namespace BLM
{
    public abstract class InterpretBeforeCreate<T> : IInterpretBeforeCreate<T, T>
    {
        /// <summary>
        /// Possibility to interpret an entity on creation before saving into the DB
        /// </summary>
        /// <param name="entity">The entity to be created</param>
        /// <param name="context">The creation context</param>
        /// <returns>The interpreted entity to be created</returns>
        public abstract T DoInterpret(T entity, IContextInfo context);

        /// <summary>
        /// Possibility to interpret an entity on creation before saving into the DB
        /// </summary>
        /// <param name="entity">The entity to be created</param>
        /// <param name="context">The creation context</param>
        /// <returns>The interpreted entity to be created</returns>
        public object DoInterpret(object entity, IContextInfo context)
        {
            return DoInterpret((T)entity, context);

        }
    }
}