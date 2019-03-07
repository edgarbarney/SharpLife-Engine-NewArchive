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

namespace SharpLife.CommandSystem.TypeProxies
{
    public abstract class BaseFormattableTypeProxy<T> : BaseTypeProxy<T>
    {
        public delegate string ToStringDelegate(T value, IFormatProvider provider);

        private readonly ToStringDelegate _toStringMethod;

        protected BaseFormattableTypeProxy(ToStringDelegate toStringMethod)
        {
            _toStringMethod = toStringMethod ?? throw new ArgumentNullException(nameof(toStringMethod));
        }

        protected BaseFormattableTypeProxy(string toStringMethodName = "ToString")
            : this(CreateDelegate(toStringMethodName))
        {
        }

        public override string ToString(T value, IFormatProvider provider) => _toStringMethod(value, provider);

        private static ToStringDelegate CreateDelegate(string toStringMethodName)
        {
            if (toStringMethodName == null)
            {
                throw new ArgumentNullException(nameof(toStringMethodName));
            }

            var method = typeof(T).GetMethod(toStringMethodName, new[] { typeof(IFormatProvider) });

            if (method == null)
            {
                throw new ArgumentException($"No such ToString method \"{toStringMethodName}\" takes takes an {nameof(IFormatProvider)} argument", nameof(toStringMethodName));
            }

            return (ToStringDelegate)Delegate.CreateDelegate(typeof(ToStringDelegate), typeof(T), method);
        }
    }
}
