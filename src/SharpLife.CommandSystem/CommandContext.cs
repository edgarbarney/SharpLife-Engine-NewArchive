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

using Serilog;
using SharpLife.CommandSystem.Commands;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SharpLife.CommandSystem
{
    internal class CommandContext : ICommandContext
    {
        private const string DefaultProtectedVariableChangeString = "***PROTECTED***";

        public string Name { get; }

        public object Tag { get; }

        public string ProtectedVariableChangeString { get; }

        public IReadOnlyDictionary<string, IBaseCommand> Commands => _commands;

        public IReadOnlyDictionary<string, string> Aliases => _aliases;

        internal readonly ILogger _logger;
        internal readonly CommandSystem _commandSystem;

        private readonly Dictionary<string, IBaseCommand> _commands = new Dictionary<string, IBaseCommand>();

        private readonly Dictionary<string, string> _aliases = new Dictionary<string, string>();

        public CommandContext(ILogger logger, CommandSystem commandSystem, string name, object tag = null, string protectedVariableChangeString = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandSystem = commandSystem ?? throw new ArgumentNullException(nameof(commandSystem));

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Context name must not be empty or contain whitespace", nameof(name));
            }

            Name = name;
            Tag = tag;

            //Allow empty strings
            if (protectedVariableChangeString == null)
            {
                protectedVariableChangeString = DefaultProtectedVariableChangeString;
            }

            ProtectedVariableChangeString = protectedVariableChangeString;
        }

        public TCommand FindCommand<TCommand>(string name)
            where TCommand : class, IBaseCommand
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_commands.TryGetValue(name, out var command))
            {
                return command as TCommand;
            }

            return null;
        }

        /// <summary>
        /// Checks if a command with the given name exists
        /// If so, and the type matches, the command is returned in <paramref name="existingCommand"/>
        /// If the type does not match, an exception is thrown
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="name"></param>
        /// <param name="existingCommand"></param>
        /// <returns></returns>
        private bool CheckForCommandExistence<TCommand>(string name, out TCommand existingCommand)
            where TCommand : class
        {
            if (_commands.TryGetValue(name, out var existing))
            {
                if (existing is TCommand existingVar)
                {
                    existingCommand = existingVar;
                    return true;
                }

                throw new ArgumentException($"A command \"{name}\" with a type \"{existing.GetType().Name}\" different from \"{typeof(TCommand).Name}\" has already been registered");
            }

            existingCommand = null;

            return false;
        }

        public virtual ICommand RegisterCommand(CommandInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (CheckForCommandExistence<ICommand>(info.Name, out var existingCommand))
            {
                return existingCommand;
            }

            var command = new Command(this, info.Name, info.Executors, info.Flags, info.HelpInfo, info.Tag);

            _commands.Add(command.Name, command);

            return command;
        }

        public virtual IVariable<T> RegisterVariable<T>(VariableInfo<T> info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (CheckForCommandExistence<IVariable<T>>(info.Name, out var existingCommand))
            {
                return existingCommand;
            }

            var typeProxy = _commandSystem.GetTypeProxy<T>();

            var variable = new VirtualVariable<T>(this, info.Name, info.Value, info.Flags, info.HelpInfo, typeProxy, info.Filters, info.ChangeHandlers, info.Tag);

            _commands.Add(variable.Name, variable);

            return variable;
        }

        public void RegisterVariable<T>(string name, Expression<Func<T>> expression)
        {
            if (!(expression.Body is MemberExpression memberAccess))
            {
                throw new ArgumentException("Invalid Expression. Expression should consist of a property or field access only", nameof(expression));
            }

            var instance = Expression.Lambda<Func<object>>(memberAccess.Expression).Compile()();

            if (instance == null)
            {
                throw new ArgumentException("Cannot register a variable on a null object", nameof(expression));
            }

            var memberInfo = memberAccess.Member;

            var typeProxy = _commandSystem.GetTypeProxy<T>();

            var variable = new ProxyVariable<T>(this, name, CommandFlags.None, "", instance, memberInfo, typeProxy);
        }

        public void SetAlias(string aliasName, string commandText)
        {
            if (aliasName == null)
            {
                throw new ArgumentNullException(nameof(aliasName));
            }

            if (string.IsNullOrWhiteSpace(aliasName))
            {
                throw new ArgumentException(nameof(aliasName));
            }

            if (commandText == null)
            {
                throw new ArgumentNullException(nameof(commandText));
            }

            if (!string.IsNullOrEmpty(commandText))
            {
                _aliases[aliasName] = commandText;
            }
            else
            {
                //Remove empty aliases to save memory
                _aliases.Remove(aliasName);
            }
        }

        public void QueueCommands(string commandText)
        {
            _commandSystem.Queue.QueueCommands(this, commandText);
        }

        public void InsertCommands(string commandText, int index = 0)
        {
            _commandSystem.Queue.InsertCommands(this, commandText, index);
        }

        public void AddSharedCommand(BaseCommand command)
        {
            if (_commands.ContainsKey(command.Name))
            {
                throw new ArgumentException($"A command with the name {command.Name} already exists in context {Name}", nameof(command));
            }

            _commands.Add(command.Name, command);
        }
    }
}
