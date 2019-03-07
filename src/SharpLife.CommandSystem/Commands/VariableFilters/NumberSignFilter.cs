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
    /// Denies any numeric inputs that are either positive or negative
    /// </summary>
    public class NumberSignFilter<T> : IVariableFilter<T>
        where T : IComparable<T>
    {
        private readonly bool _positive;

        /// <summary>
        /// Creates a new number sign filter
        /// </summary>
        /// <param name="positive">If true, positive numbers are allowed and negative numbers are denied. If false, the opposite is true</param>
        public NumberSignFilter(bool positive)
        {
            _positive = positive;
        }

        public bool Filter(IVariable<T> variable, ref T value)
        {
            //This takes advantage of the fact that default values are 0 for numeric types
            //Will not work quite as well for other types
            return (value.CompareTo(default) < 0) ^ _positive;
        }
    }
}
