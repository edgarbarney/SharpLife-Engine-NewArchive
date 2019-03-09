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
using System.Linq.Expressions;

namespace SharpLife.CommandSystem.Commands
{
    /// <summary>
    /// Contains information about a command variable
    /// A command variable has a value of a specific type, specified as type parameter <typeparamref name="T"/>
    /// </summary>
    public abstract class VariableInfo<T, TDerived> : BaseCommandInfo<TDerived>
        where TDerived : VariableInfo<T, TDerived>
    {
        internal readonly List<VariableChangeHandler<T>> _onChangeDelegates = new List<VariableChangeHandler<T>>();

        internal bool? _isReadOnly;

        internal ITypeProxy<T> _typeProxy;

        public IReadOnlyList<VariableChangeHandler<T>> ChangeHandlers => _onChangeDelegates;

        internal VariableFiltersBuilder<T> _filters;

        protected VariableInfo(string name)
            : base(name)
        {
        }

        public TDerived WithChangeHandler(VariableChangeHandler<T> changeHandler)
        {
            if (changeHandler == null)
            {
                throw new ArgumentNullException(nameof(changeHandler));
            }

            _onChangeDelegates.Add(changeHandler);

            return this as TDerived;
        }

        public TDerived MakeReadOnly()
        {
            _isReadOnly = true;

            return this as TDerived;
        }

        public TDerived WithTypeProxy(ITypeProxy<T> typeProxy)
        {
            _typeProxy = typeProxy ?? throw new ArgumentNullException(nameof(typeProxy));

            return this as TDerived;
        }

        public TDerived ConfigureFilters(Action<VariableFiltersBuilder<T>> configurer)
        {
            if (configurer == null)
            {
                throw new ArgumentNullException(nameof(configurer));
            }

            if (_filters == null)
            {
                _filters = new VariableFiltersBuilder<T>();
            }

            configurer(_filters);

            return this as TDerived;
        }

        internal List<VariableChangeHandler<T>> CreateChangeHandlerList()
        {
            //Add the filter aggregate to the front to allow vetoing ahead of time
            if (_filters?.HasFilters == true)
            {
                _onChangeDelegates.Insert(0, _filters.CreateAggregate().OnChange);
            }

            return _onChangeDelegates;
        }
    }

    public sealed class VirtualVariableInfo<T> : VariableInfo<T, VirtualVariableInfo<T>>
    {
        public T Value { get; }

        public VirtualVariableInfo(string name, in T defaultValue = default)
            : base(name)
        {
            Value = defaultValue;
        }
    }

    public sealed class ProxyVariableInfo<T> : VariableInfo<T, ProxyVariableInfo<T>>
    {
        public Expression<Func<T>> Expression { get; }

        public ProxyVariableInfo(string name, Expression<Func<T>> expression)
            : base(name)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }
    }
}
