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

using System;
using System.Collections.Generic;

namespace SharpLife.CommandSystem.Commands
{
    public abstract class AbstractCommandInfo<TDerived> : BaseCommandInfo<TDerived>
        where TDerived : AbstractCommandInfo<TDerived>
    {
        private readonly List<CommandExecutor> _onExecuteDelegates = new List<CommandExecutor>();

        public IReadOnlyList<CommandExecutor> Executors => _onExecuteDelegates;

        protected AbstractCommandInfo(string name, CommandExecutor executor)
            : base(name)
        {
            _onExecuteDelegates.Add(executor ?? throw new ArgumentNullException(nameof(executor)));
        }

        protected AbstractCommandInfo(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Adds another executor
        /// </summary>
        /// <param name="executor">Must be valid</param>
        public TDerived WithCallback(CommandExecutor executor)
        {
            _onExecuteDelegates.Add(executor ?? throw new ArgumentNullException(nameof(executor)));

            return this as TDerived;
        }
    }

    /// <summary>
    /// Contains information about a command
    /// </summary>
    public sealed class CommandInfo : AbstractCommandInfo<CommandInfo>
    {
        /// <summary>
        /// Creates a new info instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="executor">Must be valid</param>
        public CommandInfo(string name, CommandExecutor executor)
            : base(name, executor)
        {
        }
    }

    public sealed class ProxyCommandInfo<TDelegate> : AbstractCommandInfo<ProxyCommandInfo<TDelegate>>
        where TDelegate : Delegate
    {
        public TDelegate Delegate { get; }

        public ProxyCommandInfo(string name, TDelegate @delegate)
            : base(name)
        {
            Delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
        }
    }
}
