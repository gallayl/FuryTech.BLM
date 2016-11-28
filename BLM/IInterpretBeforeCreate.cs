﻿namespace BLM
{
    public interface IInterpretBeforeCreate<T> : IBlmEntry
    {
        /// <summary>
        /// Possibility to interpret an entity on creation before saving into the DB
        /// </summary>
        /// <param name="entity">The entity to be created</param>
        /// <param name="context">The creation context</param>
        /// <returns>The interpreted entity to be created</returns>
        T InterpretBeforeCreate(T entity, IContextInfo context);
    }
}