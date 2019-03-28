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
    /// <summary>
    /// Allows to specify which spawn flag maps to a particular member
    /// The member must be a <see cref="bool"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class SpawnFlagAttribute : Attribute
    {
        public uint Flag { get; }

        public SpawnFlagAttribute(uint flag)
        {
            Flag = flag;
        }
    }
}
