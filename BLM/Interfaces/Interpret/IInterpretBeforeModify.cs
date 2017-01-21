namespace BLM.Interfaces.Interpret
{
    internal interface IInterpretBeforeModify : IBlmEntry
    {
        /// <summary>
        /// Possibility to interpret an entity on modification before saving into the DB
        /// </summary>
        /// <param name="originalEntity"></param>
        /// <param name="modifiedEntity"></param>
        /// <param name="context"></param>
        /// <returns>The entity to be modified</returns>
        object DoInterpret(object originalEntity, object modifiedEntity, IContextInfo context);
    }

    internal interface IInterpretBeforeModify<in TInput, out TOutput> : IInterpretBeforeModify
    {
        /// <summary>
        /// Possibility to interpret an entity on modification before saving into the DB
        /// </summary>
        /// <param name="originalEntity"></param>
        /// <param name="modifiedEntity"></param>
        /// <param name="context"></param>
        /// <returns>The entity to be modified</returns>
        TOutput DoInterpret(TInput originalEntity, TInput modifiedEntity, IContextInfo context);
    }
}
