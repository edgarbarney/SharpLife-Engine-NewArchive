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

namespace SharpLife.Engine.Shared.Entities.Factories
{
    public sealed class EntityDictionaryBuilder
    {
        private readonly ILogger _logger;

        private readonly ImmutableDictionary<string, EntityFactory>.Builder _factoriesBuilder = ImmutableDictionary.CreateBuilder<string, EntityFactory>();

        private readonly HashSet<Assembly> _referencedAssemblies = new HashSet<Assembly>();

        public EntityDictionaryBuilder(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddFactoriesFromAssembly(Assembly assembly)
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

            foreach (var type in assembly.DefinedTypes.Where(t => typeof(EntityFactory).IsAssignableFrom(t) && t.IsPublic))
            {
                foreach (var link in type.GetCustomAttributes<LinkEntityToFactoryAttribute>())
                {
                    if (link.ClassName == null)
                    {
                        throw new ArgumentException(nameof(LinkEntityToFactoryAttribute) + " " + nameof(LinkEntityToFactoryAttribute.ClassName) + " must be non-null");
                    }

                    if (string.IsNullOrWhiteSpace(link.ClassName))
                    {
                        throw new ArgumentException(nameof(LinkEntityToFactoryAttribute) + " " + nameof(LinkEntityToFactoryAttribute.ClassName) + " must be valid");
                    }

                    if (_factoriesBuilder.ContainsKey(link.ClassName))
                    {
                        _logger.Warning("Entity classname {ClassName} already has a factory, ignoring", link.ClassName);
                    }
                    else
                    {
                        var factory = (EntityFactory)Activator.CreateInstance(type);

                        _factoriesBuilder.Add(link.ClassName, factory);
                    }
                }
            }
        }

        public ImmutableDictionary<string, EntityFactory> Build()
        {
            return _factoriesBuilder.ToImmutable();
        }
    }
}
