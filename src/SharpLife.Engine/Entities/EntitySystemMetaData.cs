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

using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Entities.Factories;
using SharpLife.Engine.Entities.KeyValues;
using System;
using System.Collections.Immutable;

namespace SharpLife.Engine.Entities
{
    /// <summary>
    /// Contains immutable metadata about the entity system, including entity factories, keyvalue converters and component metadata
    /// </summary>
    public sealed class EntitySystemMetaData
    {
        public ImmutableDictionary<string, EntityFactory> EntityFactories { get; }

        public ImmutableDictionary<Type, IKeyValueConverter> KeyValueConverters { get; }

        public ImmutableDictionary<Type, ComponentMetaData> ComponentMetaData { get; }

        public EntitySystemMetaData(
            ImmutableDictionary<string, EntityFactory> entityFactories,
            ImmutableDictionary<Type, IKeyValueConverter> keyValueConverters,
            ImmutableDictionary<Type, ComponentMetaData> componentMetaData)
        {
            EntityFactories = entityFactories ?? throw new ArgumentNullException(nameof(entityFactories));
            KeyValueConverters = keyValueConverters ?? throw new ArgumentNullException(nameof(keyValueConverters));
            ComponentMetaData = componentMetaData ?? throw new ArgumentNullException(nameof(componentMetaData));
        }
    }
}
