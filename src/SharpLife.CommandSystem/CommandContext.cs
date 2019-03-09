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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SharpLife.CommandSystem
{
    internal sealed class CommandContext : ICommandContext
    {
        private const string DefaultProtectedVariableChangeString = "***PROTECTED***";

        internal readonly ILogger _logger;
        internal readonly CommandSystem _commandSystem;

        internal readonly CommandContext[] _sharedContexts;

        /// <summary>
        /// How many other contexts we're shared with
        /// </summary>
        internal int _sharedCount;

        private readonly Dictionary<string, IBaseCommand> _commands = new Dictionary<string, IBaseCommand>();

        private readonly Dictionary<string, string> _aliases = new Dictionary<string, string>();

        internal bool _disposed;

        public string Name { get; }

        public object Tag { get; }

        public string ProtectedVariableChangeString { get; }

        public IReadOnlyDictionary<string, IBaseCommand> Commands => _commands;

        public IReadOnlyDictionary<string, string> Aliases => _aliases;

        public event Action<IBaseCommand> CommandAdded;

        public CommandContext(ILogger logger, CommandSystem commandSystem, string name, object tag, string protectedVariableChangeString, params CommandContext[] sharedContexts)
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

            _sharedContexts = sharedContexts;

            if (_sharedContexts.Length != _sharedContexts.Distinct().Count())
            {
                throw new ArgumentException("Cannot specify the same context for sharing multiple times when creating contexts", nameof(sharedContexts));
            }

            foreach (var sharedContext in _sharedContexts)
            {
                if (sharedContext == null)
                {
                    throw new ArgumentException("Shared contexts may not be null", nameof(sharedContexts));
                }

                //Add all existing shared commands
                foreach (var command in sharedContext.Commands.Values)
                {
                    //Need to check for duplicates since shared contexts may have overlapping command names
                    AddSharedCommand(command);
                }

                sharedContext.CommandAdded += AddSharedCommand;

                ++sharedContext._sharedCount;
            }
        }

        ~CommandContext()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_sharedCount > 0)
                {
                    throw new InvalidOperationException("Cannot destroy context that is shared with another context");
                }

                if (!_commandSystem.RemoveContext(this))
                {
                    //This should never happen
                    throw new InvalidOperationException("Context was not known to the command system");
                }

                foreach (var sharedContext in _sharedContexts)
                {
                    --sharedContext._sharedCount;
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        public ICommand RegisterCommand(CommandInfo info)
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

            CommandAdded?.Invoke(command);

            return command;
        }

        public ICommand RegisterCommand<TDelegate>(ProxyCommandInfo<TDelegate> info)
            where TDelegate : Delegate
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (CheckForCommandExistence<ICommand>(info.Name, out var existingCommand))
            {
                return existingCommand;
            }

            var command = new ProxyCommand<TDelegate>(this, info.Name, info.Executors, info.Delegate, info.Flags, info.HelpInfo, info.Tag);

            _commands.Add(command.Name, command);

            CommandAdded?.Invoke(command);

            return command;
        }

        public IVariable<T> RegisterVariable<T>(VirtualVariableInfo<T> info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (CheckForCommandExistence<IVariable<T>>(info.Name, out var existingCommand))
            {
                return existingCommand;
            }

            var typeProxy = info._typeProxy ?? _commandSystem.GetTypeProxy<T>();

            var variable = new VirtualVariable<T>(this, info.Name, info.Value, info._isReadOnly ?? false, info.Flags, info.HelpInfo, typeProxy, info.CreateChangeHandlerList(), info.Tag);

            _commands.Add(variable.Name, variable);

            CommandAdded?.Invoke(variable);

            return variable;
        }

        public IVariable<T> RegisterVariable<T>(ProxyVariableInfo<T> info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (CheckForCommandExistence<IVariable<T>>(info.Name, out var existingCommand))
            {
                return existingCommand;
            }

            var expression = info.Expression;

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

            //Reject write-only properties
            if (memberInfo is PropertyInfo prop)
            {
                var getter = prop.GetGetMethod(true);

                if (getter == null)
                {
                    throw new ArgumentException($"The property {prop.Name} of type {prop.DeclaringType.FullName} has no get accessor", nameof(expression));
                }

                if (!getter.IsPublic)
                {
                    throw new ArgumentException($"The property {prop.Name} of type {prop.DeclaringType.FullName} has a non-public get accessor", nameof(expression));
                }
            }

            var typeProxy = info._typeProxy ?? _commandSystem.GetTypeProxy<T>();

            var variable = new ProxyVariable<T>(this, info.Name, info.Flags, info.HelpInfo, instance, memberInfo, info._isReadOnly, typeProxy, info.CreateChangeHandlerList(), info.Tag);

            _commands.Add(variable.Name, variable);

            CommandAdded?.Invoke(variable);

            return variable;
        }

        public void SetAlias(string aliasName, string commandText)
        {
            if (aliasName == null)
            {
                throw new ArgumentNullException(nameof(aliasName));
            }

            if (string.IsNullOrWhiteSpace(aliasName))
            {
                throw new ArgumentException("Alias name must be valid", nameof(aliasName));
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
            _commandSystem._queue.InternalQueueCommands(this, commandText);
        }

        public void InsertCommands(string commandText, int index = 0)
        {
            _commandSystem._queue.InternalInsertCommands(this, commandText, index);
        }

        private void AddSharedCommand(IBaseCommand command)
        {
            if (_commands.TryGetValue(command.Name, out var existing))
            {
                //It's possible for multiple shared contexts to contain the same command, so just ignore these cases
                if (ReferenceEquals(command, existing))
                {
                    return;
                }

                throw new ArgumentException($"A different command with the name {command.Name} already exists in context {Name}", nameof(command));
            }

            _commands.Add(command.Name, command);
        }
    }
}
