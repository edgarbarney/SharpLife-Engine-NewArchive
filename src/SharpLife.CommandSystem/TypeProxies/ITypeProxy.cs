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

namespace SharpLife.CommandSystem.TypeProxies
{
    /// <summary>
    /// Acts as a proxy for a type, providing a means to convert it to and from string, and provide an equality comparer
    /// </summary>
    public interface ITypeProxy
    {
        bool TryParse(string value, IFormatProvider provider, out object result);
    }

    public interface ITypeProxy<T> : ITypeProxy
    {
        IEqualityComparer<T> Comparer { get; }

        string ToString(T value, IFormatProvider provider);

        bool TryParse(string value, IFormatProvider provider, out T result);
    }
}
