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

namespace SharpLife.CommandSystem.Commands.VariableFilters
{
    /// <summary>
    /// Contains an aggregate of filters that are treated as a single change handler
    /// </summary>
    internal sealed class FilterAggregate<T>
    {
        private readonly IVariableFilter<T>[] _filters;

        public FilterAggregate(IVariableFilter<T>[] filters)
        {
            _filters = filters ?? throw new ArgumentNullException(nameof(filters));
        }

        public void OnChange(ref VariableChangeEvent<T> changeEvent)
        {
            foreach (var filter in _filters)
            {
                if (!filter.Filter(ref changeEvent))
                {
                    changeEvent.Veto = true;
                    return;
                }
            }
        }
    }
}
