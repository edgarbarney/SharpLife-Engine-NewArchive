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
using System.Text.RegularExpressions;

namespace SharpLife.CommandSystem.Commands.VariableFilters
{
    /// <summary>
    /// Extensions to make adding filters to variables easier
    /// </summary>
    public static class VariableFiltersBuilderExtensions
    {
        /// <summary>
        /// <see cref="NumberSignFilter(bool)"/>
        /// </summary>
        /// <param name="this"></param>
        /// <param name="positive"></param>
        /// <returns></returns>
        public static VariableFiltersBuilder<T> WithNumberSignFilter<T>(this VariableFiltersBuilder<T> @this, bool positive)
            where T : IComparable<T>
        {
            return @this.WithFilter(new NumberSignFilter<T>(positive));
        }

        public static VariableFiltersBuilder<T> WithMinMaxFilter<T>(this VariableFiltersBuilder<T> @this, T? min, T? max, bool denyOutOfRangeValues = false)
            where T : struct, IComparable<T>, IEquatable<T>
        {
            return @this.WithFilter(new MinMaxFilter<T>(min, max, denyOutOfRangeValues));
        }

        public static VariableFiltersBuilder<string> WithRegexFilter(this VariableFiltersBuilder<string> @this, Regex regex)
        {
            return @this.WithFilter(new RegexFilter(regex));
        }

        public static VariableFiltersBuilder<string> WithRegexFilter(this VariableFiltersBuilder<string> @this, string pattern)
        {
            return @this.WithFilter(new RegexFilter(new Regex(pattern)));
        }

        public static VariableFiltersBuilder<string> WithStringListFilter(this VariableFiltersBuilder<string> @this, IReadOnlyList<string> strings)
        {
            return @this.WithFilter(new StringListFilter(strings));
        }

        public static VariableFiltersBuilder<string> WithStringListFilter(this VariableFiltersBuilder<string> @this, params string[] strings)
        {
            return @this.WithFilter(new StringListFilter(strings));
        }

        public static VariableFiltersBuilder<T> WithInvertedFilter<T>(this VariableFiltersBuilder<T> @this, IVariableFilter<T> filter)
        {
            return @this.WithFilter(new InvertFilter<T>(filter));
        }

        public static VariableFiltersBuilder<T> WithDelegateFilter<T>(this VariableFiltersBuilder<T> @this, DelegateFilter<T>.FilterDelegate @delegate)
        {
            return @this.WithFilter(new DelegateFilter<T>(@delegate));
        }

        public static VariableFiltersBuilder<string> WithPrintableCharactersFilter(this VariableFiltersBuilder<string> @this, string emptyValue = "")
        {
            return @this.WithFilter(new UnprintableCharactersFilter(emptyValue));
        }

        public static VariableFiltersBuilder<string> WithWhitespaceFilter(this VariableFiltersBuilder<string> @this)
        {
            return @this.WithFilter(new StripWhitespaceFilter());
        }
    }
}
