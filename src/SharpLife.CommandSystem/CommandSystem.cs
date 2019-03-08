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
using SharpLife.CommandSystem.TypeProxies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpLife.CommandSystem
{
    public sealed class CommandSystem : ICommandSystem
    {
        internal readonly ILogger _logger;
        internal readonly IFormatProvider _provider;

        private readonly Dictionary<Type, ITypeProxy> _typeProxies = new Dictionary<Type, ITypeProxy>();

        internal readonly CommandQueue _queue;

        private readonly List<CommandContext> _commandContexts = new List<CommandContext>();

        private readonly CommandContext _sharedContext;

        public ICommandQueue Queue => _queue;

        /// <summary>
        /// Creates a new command system
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="provider">The format provider used for type proxy conversions</param>
        public CommandSystem(ILogger logger, IFormatProvider provider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

            //Add proxies for primitive types
            AddTypeProxy(new BoolTypeProxy());
            AddTypeProxy(new CharTypeProxy());
            AddTypeProxy(new Int8TypeProxy());
            AddTypeProxy(new UInt8TypeProxy());
            AddTypeProxy(new Int16TypeProxy());
            AddTypeProxy(new UInt16TypeProxy());
            AddTypeProxy(new Int32TypeProxy());
            AddTypeProxy(new UInt32TypeProxy());
            AddTypeProxy(new Int64TypeProxy());
            AddTypeProxy(new UInt64TypeProxy());
            AddTypeProxy(new SingleTypeProxy());
            AddTypeProxy(new DoubleTypeProxy());
            AddTypeProxy(new DecimalTypeProxy());
            AddTypeProxy(new StringTypeProxy());
            AddTypeProxy(new DateTimeTypeProxy());
            AddTypeProxy(new DateTimeOffsetTypeProxy());

            _queue = new CommandQueue(_logger);

            //Shared context that informs the command system of all command additions to add them to other contexts
            _sharedContext = new CommandContext(_logger, this, "SharedCommandContext", null, null);

            _commandContexts.Add(_sharedContext);

            //Add as a shared command
            _sharedContext.RegisterCommand(new CommandInfo("wait", _ => _queue.Wait = true)
                .WithHelpInfo("Delay execution of remaining commands until the next execution"));
        }

        public ITypeProxy<T> GetTypeProxy<T>()
        {
            if (_typeProxies.TryGetValue(typeof(T), out var proxy))
            {
                return (ITypeProxy<T>)proxy;
            }

            throw new ArgumentException($"No type proxy for type {typeof(T).FullName}", nameof(T));
        }

        internal ITypeProxy GetTypeProxy(Type type)
        {
            if (_typeProxies.TryGetValue(type, out var proxy))
            {
                return proxy;
            }

            throw new ArgumentException($"No type proxy for type {type.FullName}", nameof(type));
        }

        public void AddTypeProxy<T>(ITypeProxy<T> typeProxy)
        {
            if (typeProxy == null)
            {
                throw new ArgumentNullException(nameof(typeProxy));
            }

            if (_typeProxies.ContainsKey(typeof(T)))
            {
                throw new ArgumentException($"A proxy for type {typeof(T).FullName} already exists", nameof(typeProxy));
            }

            _typeProxies.Add(typeof(T), typeProxy);
        }

        public ICommandContext CreateContext(string name, object tag = null, string protectedVariableChangeString = null, params ICommandContext[] sharedContexts)
        {
            //The command system context should always be shared
            var completeSharedContexts = sharedContexts.Cast<CommandContext>().Prepend(_sharedContext).ToArray();

            var context = new CommandContext(_logger, this, name, tag, protectedVariableChangeString, completeSharedContexts);

            _commandContexts.Add(context);

            return context;
        }

        public void DestroyContext(ICommandContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var internalContext = (CommandContext)context;

            if (internalContext._destroyed)
            {
                throw new ArgumentException("Tried to destroy an already destroyed context", nameof(context));
            }

            if (internalContext._sharedCount > 0)
            {
                throw new ArgumentException("Cannot destroy context that is shared with another context", nameof(context));
            }

            if (!_commandContexts.Contains(internalContext))
            {
                throw new ArgumentException(nameof(context));
            }

            _commandContexts.Remove(internalContext);

            internalContext._destroyed = true;

            foreach (var sharedContext in internalContext._sharedContexts)
            {
                --sharedContext._sharedCount;
            }
        }

        public void Execute()
        {
            _queue.Execute();
        }
    }
}
