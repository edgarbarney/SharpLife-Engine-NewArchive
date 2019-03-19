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
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace SharpLife.Engine.Entities.KeyValues
{
    public sealed class KeyValueConvertersBuilder
    {
        private readonly ILogger _logger;

        private readonly ImmutableDictionary<Type, IKeyValueConverter>.Builder _convertersBuilder = ImmutableDictionary.CreateBuilder<Type, IKeyValueConverter>();

        private readonly HashSet<Assembly> _referencedAssemblies = new HashSet<Assembly>();

        public KeyValueConvertersBuilder(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddConverter(Type targetType, IKeyValueConverter converter)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            if (_convertersBuilder.ContainsKey(targetType))
            {
                throw new ArgumentException($"Type {targetType.FullName} already has a keyvalue converter", nameof(targetType));
            }

            _convertersBuilder.Add(targetType, converter);
        }

        public void AddConvertersFromAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            //Allow redundant additions (if client and server add the same assemblies)
            if (_referencedAssemblies.Contains(assembly))
            {
                return;
            }

            _referencedAssemblies.Add(assembly);

            foreach (var type in assembly.DefinedTypes.Where(t => typeof(IKeyValueConverter).IsAssignableFrom(t) && t.IsPublic))
            {
                IKeyValueConverter converter = null;

                foreach (var attr in type.GetCustomAttributes<KeyValueConverterAttribute>())
                {
                    if (attr.Type == null)
                    {
                        throw new ArgumentException(nameof(KeyValueConverterAttribute) + " " + nameof(KeyValueConverterAttribute.Type) + " must be non-null");
                    }

                    if (_convertersBuilder.ContainsKey(attr.Type))
                    {
                        throw new ArgumentException($"Type {attr.Type.FullName} already has a keyvalue converter");
                    }

                    //Create one instance for N type conversions
                    if (converter == null)
                    {
                        converter = (IKeyValueConverter)Activator.CreateInstance(type);
                    }

                    _convertersBuilder.Add(attr.Type, converter);
                }
            }
        }

        public ImmutableDictionary<Type, IKeyValueConverter> Build()
        {
            return _convertersBuilder.ToImmutable();
        }
    }
}
