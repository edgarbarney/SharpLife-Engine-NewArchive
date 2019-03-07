﻿/***
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
    /// <summary>
    /// A command that acts as a proxy for a delegate
    /// Arguments are automatically converted to the target type
    /// </summary>
    /// <typeparam name="TDelegate"></typeparam>
    internal sealed class ProxyCommand<TDelegate> : Command
        where TDelegate : Delegate
    {
        private readonly TDelegate _delegate;

        public ProxyCommand(CommandContext commandContext, string name,
            IReadOnlyList<CommandExecutor> executors,
            TDelegate @delegate,
            CommandFlags flags = CommandFlags.None, string helpInfo = "",
            object tag = null)
            : base(commandContext, name, executors, flags, helpInfo, tag)
        {
            _delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
        }

        internal override void OnCommand(ICommandArgs command)
        {
            base.OnCommand(command);

            //Resolve each argument and attempt to convert it
            var parameters = _delegate.Method.GetParameters();
            var argumentCount = parameters.Length;

            if (command.Count < argumentCount)
            {
                _commandContext._logger.Information("Not enough arguments for proxy command {Name}: {ExpectedCount} expected, got {ReceivedCount}", Name, argumentCount, command.Count);
                return;
            }
            else if (command.Count > argumentCount)
            {
                _commandContext._logger.Information("Too many arguments for proxy command {Name}: {ExpectedCount} expected, got {ReceivedCount}", Name, argumentCount, command.Count);
                return;
            }

            var arguments = argumentCount > 0 ? new object[argumentCount] : null;

            for (var i = 0; i < argumentCount; ++i)
            {
                var proxy = _commandContext._commandSystem.GetTypeProxy(parameters[i].ParameterType);

                if (!proxy.TryParse(command[i], _commandContext._commandSystem._provider, out var result))
                {
                    _commandContext._logger.Information("Proxy command {Name}: could not convert argument {Index} to type {Type}", Name, i, parameters[i].ParameterType.Name);
                    return;
                }

                arguments[i] = result;
            }

            _delegate.DynamicInvoke(arguments);
        }
    }
}