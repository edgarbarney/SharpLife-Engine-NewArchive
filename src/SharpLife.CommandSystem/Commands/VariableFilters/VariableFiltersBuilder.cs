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

namespace SharpLife.CommandSystem.Commands.VariableFilters
{
    public sealed class VariableFiltersBuilder<T>
    {
        private readonly VariableInfo<T> _info;

        private readonly List<IVariableFilter<T>> _filters = new List<IVariableFilter<T>>();

        internal bool HasFilters => _filters.Count > 0;

        internal VariableFiltersBuilder(VariableInfo<T> info)
        {
            _info = info ?? throw new ArgumentNullException(nameof(info));
        }

        public VariableInfo<T> WithFilter(IVariableFilter<T> filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _filters.Add(filter);

            return _info;
        }

        internal FilterAggregate<T> CreateAggregate()
        {
            if (!HasFilters)
            {
                throw new InvalidOperationException("Cannot create empty filter aggregate");
            }

            return new FilterAggregate<T>(_filters.ToArray());
        }
    }
}
