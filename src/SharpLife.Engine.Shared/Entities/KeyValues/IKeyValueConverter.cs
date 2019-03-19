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

namespace SharpLife.Engine.Shared.Entities.KeyValues
{
    /// <summary>
    /// Converts strings to a specific type
    /// Concrete instances must be marked with one or more <see cref="KeyValueConverterAttribute"/> binding them to a specific type
    /// Converters will be used for all instances of a type
    /// </summary>
    public interface IKeyValueConverter
    {
        object FromString(Type destinationType, string value);
    }
}
