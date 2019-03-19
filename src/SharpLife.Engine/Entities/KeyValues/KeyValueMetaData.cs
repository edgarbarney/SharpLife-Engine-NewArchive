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
using System.Reflection;

namespace SharpLife.Engine.Entities.KeyValues
{
    public sealed class KeyValueMetaData
    {
        public readonly MemberInfo Member;

        public readonly Type MemberType;

        public readonly IKeyValueConverter Converter;

        public KeyValueMetaData(MemberInfo member, Type memberType, IKeyValueConverter converter)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
            MemberType = memberType ?? throw new ArgumentNullException(nameof(memberType));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }
    }
}
