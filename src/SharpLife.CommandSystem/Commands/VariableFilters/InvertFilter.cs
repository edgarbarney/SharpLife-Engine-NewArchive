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
    /// Filter to invert the result of another filter
    /// </summary>
    public class InvertFilter<T> : IVariableFilter<T>
    {
        private readonly IVariableFilter<T> _filter;

        public InvertFilter(IVariableFilter<T> filter)
        {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public bool Filter(ref VariableChangeEvent<T> changeEvent)
        {
            return !_filter.Filter(ref changeEvent);
        }
    }
}
