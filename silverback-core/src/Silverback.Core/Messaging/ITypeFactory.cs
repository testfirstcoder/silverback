﻿using System;

namespace Silverback.Messaging
{
    /// <summary>
    /// Provides an instance of the specified type and is used to resolve message handlers and other types that
    /// need to be dinamically instantiated.
    /// </summary>
    public interface ITypeFactory
    {
        /// <summary>
        /// Returns an instance of the specified type.
        /// </summary>
        /// <param name="type">The type to be instantiated.</param>
        /// <returns></returns>
        object GetInstance(Type type);

        /// <summary>
        /// Returns an instance of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to be instantiated.</typeparam>
        /// <returns></returns>
        T GetInstance<T>();
    }
}
