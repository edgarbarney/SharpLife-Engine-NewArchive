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

namespace SharpLife.Engine.Entities.KeyValues
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class KeyValueAttribute : Attribute
    {
        /// <summary>
        /// The name used to initialize this keyvalue with
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// If not null, this is the type of a <see cref="IKeyValueConverter"/> to use for conversion
        /// </summary>
        public Type ConverterType { get; set; }
    }
}
