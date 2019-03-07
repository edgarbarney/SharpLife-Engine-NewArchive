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
    /// Base class for type proxies that use the default equality comparer and ToString method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseTypeProxy<T> : ITypeProxy<T>
    {
        public virtual IEqualityComparer<T> Comparer => EqualityComparer<T>.Default;

        public virtual string ToString(T value, IFormatProvider provider) => value.ToString();

        public abstract bool TryParse(string value, IFormatProvider provider, out T result);
    }
}
