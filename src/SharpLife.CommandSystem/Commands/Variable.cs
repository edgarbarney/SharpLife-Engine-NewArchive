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

using SharpLife.CommandSystem.Commands.VariableFilters;
using SharpLife.CommandSystem.TypeProxies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpLife.CommandSystem.Commands
{
    internal class Variable<T> : BaseCommand, IVariable<T>
    {
        private readonly IVariableFilter<T>[] _filters;

        public object InitialValueObject { get; }

        public string InitialValueString { get; }

        public T InitialValue { get; }

        public object ValueObject
        {
            get => Value;
            set => Value = (T)value;
        }

        public string ValueString
        {
            get => Proxy.ToString(Value, _commandContext._commandSystem._provider);
            set => SetString(value);
        }

        public T Value { get; set; }

        public ITypeProxy<T> Proxy { get; }

        public event VariableChangeHandler<T> OnChange;

        public Variable(CommandContext commandContext, string name, in T value, CommandFlags flags, string helpInfo,
            ITypeProxy<T> typeProxy,
            IReadOnlyList<IVariableFilter<T>> filters,
            IReadOnlyList<VariableChangeHandler<T>> changeHandlers,
            object tag = null)
            : base(commandContext, name, flags, helpInfo, tag)
        {
            Proxy = typeProxy ?? throw new ArgumentNullException(nameof(typeProxy));

            SetValue(value, true);

            InitialValue = value;

            _filters = filters?.ToArray();

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

        internal void SetValue(T value, bool suppressChangeMessage = false)
        {
            if (_filters != null)
            {
                foreach (var filter in _filters)
                {
                    if (!filter.Filter(this, ref value))
                    {
                        return;
                    }
                }
            }

            var changeEvent = new VariableChangeEvent<T>(this, Value);

            Value = value;

            OnChange?.Invoke(ref changeEvent);

            if (!suppressChangeMessage
                && (Flags & CommandFlags.UnLogged) == 0
                && changeEvent.Different)
            {
                //If none of the change handlers reverted the change, print a change message
                var newValue = (Flags & CommandFlags.Protected) != 0 ? _commandContext.ProtectedVariableChangeString : ValueString;

                _commandContext._logger.Information($"\"{Name}\" changed to \"{newValue}\"");
            }
        }

        internal override void OnCommand(ICommandArgs command)
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

            throw new InvalidCommandSyntaxException("Variables can only be set with syntax \"name value\"");
        }
    }
}
