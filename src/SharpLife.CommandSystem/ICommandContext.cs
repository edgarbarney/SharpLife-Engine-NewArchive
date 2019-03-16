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

using SharpLife.CommandSystem.Commands;
using System;
using System.Collections.Generic;

namespace SharpLife.CommandSystem
{
    public interface ICommandContext : IDisposable
    {
        /// <summary>
        /// Name of this context
        /// Used for diagnostics
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The string to display when changes to protected variables are logged
        /// </summary>
        string ProtectedVariableChangeString { get; }

        IReadOnlyDictionary<string, IBaseCommand> Commands { get; }

        IReadOnlyDictionary<string, string> Aliases { get; }

        /// <summary>
        /// Invoked when a command has been added
        /// </summary>
        event Action<IBaseCommand> CommandAdded;

        TCommand FindCommand<TCommand>(string name) where TCommand : class, IBaseCommand;

        ICommand RegisterCommand(CommandInfo info);

        ICommand RegisterCommand<TDelegate>(ProxyCommandInfo<TDelegate> info)
            where TDelegate : Delegate;

        IVariable<T> RegisterVariable<T>(VirtualVariableInfo<T> info);

        IVariable<T> RegisterVariable<T>(ProxyVariableInfo<T> info);

        /// <summary>
        /// Sets an alias to the given command text
        /// </summary>
        /// <param name="aliasName"></param>
        /// <param name="commandText"></param>
        void SetAlias(string aliasName, string commandText);

        /// <summary>
        /// <see cref="ICommandQueue.QueueCommands(ICommandContext, string)"/>
        /// </summary>
        /// <param name="commandText"></param>
        void QueueCommands(string commandText);

        /// <summary>
        /// <see cref="ICommandQueue.InsertCommands(ICommandContext, string, int)"/>
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="index"></param>
        void InsertCommands(string commandText, int index = 0);
    }
}
