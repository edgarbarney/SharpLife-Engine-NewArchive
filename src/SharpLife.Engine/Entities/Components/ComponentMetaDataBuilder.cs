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

using SharpLife.Engine.Entities.KeyValues;
using SharpLife.Engine.Entities.KeyValues.Converters;
using SharpLife.Engine.ObjectEditor;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace SharpLife.Engine.Entities.Components
{
    public sealed class ComponentMetaDataBuilder
    {
        private static readonly ImmutableDictionary<string, KeyValueMetaData> EmptyDictionary = ImmutableDictionary.Create<string, KeyValueMetaData>();
        private static readonly ImmutableArray<SpawnFlagMetaData> EmptyArray = ImmutableArray.Create<SpawnFlagMetaData>();

        private readonly ImmutableDictionary<Type, IKeyValueConverter> _keyValueConverters;

        private readonly ImmutableDictionary<Type, ComponentMetaData>.Builder _metaDataBuilder = ImmutableDictionary.CreateBuilder<Type, ComponentMetaData>();

        private readonly HashSet<Assembly> _referencedAssemblies = new HashSet<Assembly>();

        private readonly Dictionary<Type, IKeyValueConverter> _convertersByConverterType;

        private readonly EnumConverter _enumConverter = new EnumConverter();

        public ComponentMetaDataBuilder(ImmutableDictionary<Type, IKeyValueConverter> keyValueConverters)
        {
            _keyValueConverters = keyValueConverters ?? throw new ArgumentNullException(nameof(keyValueConverters));

            //Initialize map with existing converters
            _convertersByConverterType = _keyValueConverters.Select(p => p.Value).ToDictionary(c => c.GetType(), c => c);
        }

        public void AddComponentTypesFromAssembly(Assembly assembly)
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

            //Build metadata for all instantiable types
            foreach (var type in assembly.DefinedTypes
                .Where(
                t => typeof(Component).IsAssignableFrom(t)
                && !typeof(Component).Equals(t)
                && t.IsPublic
                && !t.IsAbstract))
            {
                _metaDataBuilder.Add(type, BuildOne(type));
            }
        }

        private ComponentMetaData BuildOne(Type type)
        {
            //Build keyvalue map
            var keyValueBuilder = ImmutableDictionary.CreateBuilder<string, KeyValueMetaData>();
            var spawnFlagsBuilder = ImmutableArray.CreateBuilder<SpawnFlagMetaData>();

            foreach (var member in type
                .GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(t => t is FieldInfo || t is PropertyInfo))
            {
                var objectEditorVisible = member.GetCustomAttribute<ObjectEditorVisibleAttribute>();

                if (objectEditorVisible?.Visible != false)
                {
                    //Allow members to be used as both keyvalues and spawnflags
                    var keyValueAttr = member.GetCustomAttribute<KeyValueAttribute>();
                    var spawnFlagAttr = member.GetCustomAttribute<SpawnFlagAttribute>();

                    Type memberType;

                    switch (member)
                    {
                        case FieldInfo field:
                            memberType = field.FieldType;
                            break;

                        case PropertyInfo prop:
                            memberType = prop.PropertyType;
                            break;

                        //Shouldn't happen since we're filtering them
                        default: throw new NotSupportedException("Unsupported member type");
                    }

                    if (spawnFlagAttr != null)
                    {
                        if (memberType != typeof(bool))
                        {
                            throw new NotSupportedException($"{nameof(SpawnFlagAttribute)} can only be used on boolean members ({type.FullName}.{member.Name})");
                        }

                        if (spawnFlagAttr.Flag == 0 || (spawnFlagAttr.Flag & (spawnFlagAttr.Flag - 1)) != 0)
                        {
                            throw new InvalidOperationException($"{nameof(SpawnFlagAttribute)} should be given a flags value with exactly one bit set ({type.FullName}.{member.Name})");
                        }

                        //For efficient processing add the metadata at the index for the given flag
                        var spawnFlag = new SpawnFlagMetaData(member, spawnFlagAttr.Flag);

                        var index = (int)Math.Log(spawnFlagAttr.Flag, 2);

                        spawnFlagsBuilder.Add(new SpawnFlagMetaData(member, spawnFlagAttr.Flag));
                    }

                    var name = member.Name;

                    if (keyValueAttr != null)
                    {
                        name = keyValueAttr.Name;
                    }

                    if (keyValueBuilder.ContainsKey(name))
                    {
                        throw new NotSupportedException("Using the same name for multiple keyvalues is not allowed");
                    }

                    var converter = GetConverter(member, memberType, keyValueAttr);

                    keyValueBuilder.Add(name, new KeyValueMetaData(member, memberType, converter));
                }
            }

            var keyValues = keyValueBuilder.Count > 0 ? keyValueBuilder.ToImmutable() : EmptyDictionary;

            var spawnFlags = spawnFlagsBuilder.Count > 0 ? spawnFlagsBuilder.ToImmutable() : EmptyArray;

            return new ComponentMetaData(type, keyValues, spawnFlags);
        }

        private IKeyValueConverter GetConverter(MemberInfo member, Type memberType, KeyValueAttribute keyValueAttr)
        {
            IKeyValueConverter converter;

            if (keyValueAttr?.ConverterType == null)
            {
                if (memberType.IsEnum)
                {
                    return _enumConverter;
                }
                else if (!_keyValueConverters.TryGetValue(memberType, out converter))
                {
                    throw new ArgumentException($"KeyValue {member.DeclaringType.FullName}.{member.Name} uses type {memberType.FullName} with no converter");
                }
            }
            else
            {
                if (!typeof(IKeyValueConverter).IsAssignableFrom(keyValueAttr.ConverterType))
                {
                    throw new ArgumentException($"{keyValueAttr.ConverterType.FullName} must implement {nameof(IKeyValueConverter)}");
                }

                if (!keyValueAttr.ConverterType.IsPublic || keyValueAttr.ConverterType.IsAbstract)
                {
                    throw new ArgumentException($"{keyValueAttr.ConverterType.FullName} must be public and instantiable");
                }

                if (!_convertersByConverterType.TryGetValue(keyValueAttr.ConverterType, out converter))
                {
                    converter = (IKeyValueConverter)Activator.CreateInstance(keyValueAttr.ConverterType);

                    _convertersByConverterType.Add(keyValueAttr.ConverterType, converter);
                }
            }

            return converter;
        }

        public ImmutableDictionary<Type, ComponentMetaData> Build()
        {
            return _metaDataBuilder.ToImmutable();
        }
    }
}
