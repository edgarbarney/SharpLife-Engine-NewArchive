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

using Serilog;
using SharpLife.Engine.Shared.Entities.Components;
using SharpLife.Engine.Shared.Entities.Factories;
using SharpLife.Engine.Shared.Entities.KeyValues;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpLife.Engine.Shared.Entities
{
    public sealed class EntitySystemMetaDataBuilder
    {
        private readonly EntityDictionaryBuilder _entityDictionary;
        private readonly KeyValueConvertersBuilder _keyValueConverters;

        private readonly HashSet<Assembly> _referencedAssemblies = new HashSet<Assembly>();
        //Track the order of insertion to ensure errors make sense
        private readonly List<Assembly> _referencedAssembliesInInsertOrder = new List<Assembly>();

        public EntitySystemMetaDataBuilder(ILogger logger)
        {
            _entityDictionary = new EntityDictionaryBuilder(logger);
            _keyValueConverters = new KeyValueConvertersBuilder(logger);
        }

        public void AddKeyValueConverter(Type targetType, IKeyValueConverter converter) => _keyValueConverters.AddConverter(targetType, converter);

        public void AddAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            _entityDictionary.AddFactoriesFromAssembly(assembly);
            _keyValueConverters.AddConvertersFromAssembly(assembly);

            if (_referencedAssemblies.Add(assembly))
            {
                _referencedAssembliesInInsertOrder.Add(assembly);
            }
        }

        public EntitySystemMetaData Build()
        {
            var keyValueConverters = _keyValueConverters.Build();

            //Component metadata depends on knowing which converters there are, so it has to be done this way
            var componentMetaData = new ComponentMetaDataBuilder(keyValueConverters);

            foreach (var assembly in _referencedAssembliesInInsertOrder)
            {
                componentMetaData.AddComponentTypesFromAssembly(assembly);
            }

            return new EntitySystemMetaData(_entityDictionary.Build(), keyValueConverters, componentMetaData.Build());
        }
    }
}
