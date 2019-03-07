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

namespace SharpLife.CommandSystem.Commands
{
    public struct VariableChangeEvent<T>
    {
        private readonly Variable<T> _variable;

        public IVariable<T> Variable => _variable;

        public T Value
        {
            get => _variable.Value;
            set => _variable.SetValue(value, true);
        }

        public T OldValue { get; }

        /// <summary>
        /// Indicates whether the variable is different from its old value
        /// </summary>
        public bool Different => !_variable.Proxy.Comparer.Equals(Value, OldValue);

        /// <summary>
        /// Gets or sets whether this change has been vetoed
        /// A vetoed change will be ignored
        /// </summary>
        public bool Veto { get; set; }

        internal VariableChangeEvent(Variable<T> variable, T oldValue)
        {
            _variable = variable ?? throw new ArgumentNullException(nameof(variable));
            OldValue = oldValue;

            Veto = false;
        }
    }
}
