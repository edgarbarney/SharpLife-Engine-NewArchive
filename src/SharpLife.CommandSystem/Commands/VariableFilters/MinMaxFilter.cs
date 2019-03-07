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
    /// Clamps an input value to a range
    /// </summary>
    public class MinMaxFilter<T> : IVariableFilter<T>
        where T : struct, IComparable<T>, IEquatable<T>
    {
        public T? Min { get; }

        public T? Max { get; }

        public bool DenyOutOfRangeValues { get; }

        /// <summary>
        /// Creates a new min-max filter
        /// You must specify at least one value
        /// </summary>
        /// <param name="min">Optional. Minimum value to clamp to</param>
        /// <param name="max">Optional. Maximum value to clamp to</param>
        /// <param name="denyOutOfRangeValues">If true, values out of range are denied instead of clamping them</param>
        public MinMaxFilter(T? min, T? max, bool denyOutOfRangeValues = false)
        {
            if (min == null && max == null)
            {
                throw new ArgumentException($"{nameof(MinMaxFilter<T>)} has no purpose if both values are null", nameof(min));
            }

            if (min.HasValue && max.HasValue && max.Value.CompareTo(min.Value) <= 0)
            {
                throw new ArgumentOutOfRangeException("Minimum value must be less than maximum value");
            }

            Min = min;
            Max = max;
            DenyOutOfRangeValues = denyOutOfRangeValues;
        }

        private static T Clamp(in T value, in T min, in T max)
        {
            var result = value;

            if (result.CompareTo(min) < 0)
            {
                result = min;
            }

            if (result.CompareTo(max) > 0)
            {
                result = max;
            }

            return result;
        }

        public bool Filter(IVariable<T> variable, ref T value)
        {
            var min = Min ?? value;
            var max = Max ?? value;

            //For unbounded clamps make sure the range is correct
            if (max.CompareTo(min) < 0)
            {
                var temp = min;
                min = max;
                max = temp;
            }

            var clampedValue = Clamp(value, min, max);

            if (!clampedValue.Equals(value))
            {
                if (DenyOutOfRangeValues)
                {
                    return false;
                }

                value = clampedValue;
            }

            return true;
        }
    }
}
