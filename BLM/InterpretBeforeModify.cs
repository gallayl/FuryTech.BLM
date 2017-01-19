namespace BLM.Interfaces.Interpret
{
    public abstract class InterpretBeforeModify<T> : IInterpretBeforeModify<T, T>
    {
        /// <summary>
        /// Possibility to interpret an entity on modification before saving into the DB
        /// </summary>
        /// <param name="originalEntity"></param>
        /// <param name="modifiedEntity"></param>
        /// <param name="context"></param>
        /// <returns>The entity to be modified</returns>
        public abstract T DoInterpret(T originalEntity, T modifiedEntity, IContextInfo context);

        /// <summary>
        /// Possibility to interpret an entity on modification before saving into the DB
        /// </summary>
        /// <param name="originalEntity"></param>
        /// <param name="modifiedEntity"></param>
        /// <param name="context"></param>
        /// <returns>The entity to be modified</returns>
        public object DoInterpret(object originalEntity, object modifiedEntity, IContextInfo context)
        {
            return DoInterpret((T)originalEntity, (T)modifiedEntity, context);

        }
    }
}