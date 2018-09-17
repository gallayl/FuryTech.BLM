namespace BLM.NetStandard.Interfaces.Interpret
{
    internal interface IInterpretBeforeCreate : IBlmEntry
    {
        /// <summary>
        /// Possibility to interpret an entity on creation before saving into the DB
        /// </summary>
        /// <param name="entity">The entity to be created</param>
        /// <param name="context">The creation context</param>
        /// <returns>The interpreted entity to be created</returns>
        object DoInterpret(object entity, IContextInfo context);
    }

    internal interface IInterpretBeforeCreate<in TInput, out TOutput> : IInterpretBeforeCreate
    {
        /// <summary>
        /// Possibility to interpret an entity on creation before saving into the DB
        /// </summary>
        /// <param name="entity">The entity to be created</param>
        /// <param name="context">The creation context</param>
        /// <returns>The interpreted entity to be created</returns>
        TOutput DoInterpret(TInput entity, IContextInfo context);
    }
}
