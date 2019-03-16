/***
*
*	Copyright (c) 1996-2001, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   This source code contains proprietary and confidential information of
*   Valve LLC and its suppliers.  Access to this code is restricted to
*   persons who have executed a written SDK license with Valve.  Any access,
*   use or distribution of this code by or to any unlicensed person is illegal.
*
****/

using SharpLife.CommandSystem.TypeProxies;
using System;

namespace SharpLife.CommandSystem
{
    public interface ICommandSystem : IDisposable
    {
        ICommandQueue Queue { get; }

        /// <summary>
        /// Adds a new type proxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeProxy"></param>
        /// <exception cref="System.ArgumentException">If there is already a type proxy for the given type</exception>
        void AddTypeProxy<T>(ITypeProxy<T> typeProxy);

        /// <summary>
        /// Creates a new context
        /// </summary>
        /// <param name="name"></param>
        /// <param name="protectedVariableChangeString">Optional string to display in logs when a protected variable is changed</param>
        /// <param name="sharedContexts">
        /// List of contexts whose commands should be shared with this context
        /// Shared commands will only be able to execute commands in the context that they were executed in
        /// </param>
        ICommandContext CreateContext(string name, string protectedVariableChangeString = null, params ICommandContext[] sharedContexts);

        void Execute();
    }
}
