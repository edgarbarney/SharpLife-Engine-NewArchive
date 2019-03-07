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
using System.Collections.Generic;

namespace SharpLife.CommandSystem.Commands
{
    internal abstract class Variable<T> : BaseCommand, IVariable<T>
    {
        public Type Type => typeof(T);

        public object InitialValueObject => InitialValue;

        public string InitialValueString => Proxy.ToString(InitialValue, _commandContext._commandSystem._provider);

        public T InitialValue { get; }

        public object ValueObject
        {
            get => Value;
            set => SetValue((T)value);
        }

        public string ValueString
        {
            get => Proxy.ToString(Value, _commandContext._commandSystem._provider);
            set => SetString(value);
        }

        public abstract T Value { get; set; }

        public ITypeProxy<T> Proxy { get; }

        public event VariableChangeHandler<T> OnChange;

        protected Variable(CommandContext commandContext, string name, in T initialValue, CommandFlags flags, string helpInfo,
            ITypeProxy<T> typeProxy,
            IReadOnlyList<VariableChangeHandler<T>> changeHandlers,
            object tag = null)
            : base(commandContext, name, flags, helpInfo, tag)
        {
            Proxy = typeProxy ?? throw new ArgumentNullException(nameof(typeProxy));

            InitialValue = initialValue;

            if (changeHandlers != null)
            {
                foreach (var handler in changeHandlers)
                {
                    OnChange += handler;
                }
            }
        }

        public void RevertToInitialValue()
        {
            Value = InitialValue;
        }

        private void SetString(string stringValue, bool suppressChangeMessage = false)
        {
            if (Proxy.TryParse(stringValue, _commandContext._commandSystem._provider, out var result))
            {
                SetValue(result, suppressChangeMessage);
            }
            else
            {
                _commandContext._logger.Information("Could not parse value \"{StringValue}\" to type {Type}", stringValue, typeof(T).FullName);
            }
        }

        internal void SetValue(T value, bool suppressChangeMessage = false, bool invokeChangeHandlers = true)
        {
            var changeEvent = new VariableChangeEvent<T>(this, Value);

            Value = value;

            if (invokeChangeHandlers)
            {
                var handlers = OnChange;

                if (handlers != null)
                {
                    foreach (VariableChangeHandler<T> handler in handlers.GetInvocationList())
                    {
                        handler.Invoke(ref changeEvent);

                        //Bow out as soon as a single handler has vetoed the change
                        if (changeEvent.Veto)
                        {
                            return;
                        }
                    }
                }
            }

            if (changeEvent.Different)
            {
                Value = changeEvent.Value;

                if (!suppressChangeMessage
                    && (Flags & CommandFlags.UnLogged) == 0)
                {
                    //If none of the change handlers reverted the change, print a change message
                    var newValue = (Flags & CommandFlags.Protected) != 0 ? _commandContext.ProtectedVariableChangeString : ValueString;

                    _commandContext._logger.Information($"\"{Name}\" changed to \"{newValue}\"");
                }
            }
        }

        internal sealed override void OnCommand(ICommandArgs command)
        {
            if (command.Count == 0)
            {
                _commandContext._logger.Information($"\"{Name}\" is \"{ValueString}\"");
                return;
            }

            if (command.Count == 1)
            {
                SetString(command[0], false);
                return;
            }

            _commandContext._logger.Information("Variables can only be set with syntax \"name value\"");
        }
    }
}
