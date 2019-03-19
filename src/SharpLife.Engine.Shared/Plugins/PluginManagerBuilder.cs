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
using SharpLife.Engine.Shared.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SharpLife.Engine.Shared.Plugins
{
    public sealed class PluginManagerBuilder
    {
        private readonly ILogger _logger;
        private readonly string _gameDirectory;

        private readonly HashSet<Assembly> _loadedAssemblies = new HashSet<Assembly>();

        private readonly List<Assembly> _assemblies = new List<Assembly>();

        public PluginManagerBuilder(ILogger logger, string gameDirectory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameDirectory = gameDirectory ?? throw new ArgumentNullException(nameof(gameDirectory));
        }

        /// <summary>
        /// Adds a previously loaded assembly as a plugin
        /// </summary>
        /// <param name="assembly"></param>
        public void AddAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (_loadedAssemblies.Add(assembly))
            {
                _assemblies.Add(assembly);

                _logger.Debug("Adding plugin assembly {AssemblyName}", assembly.FullName);
            }
            else
            {
                _logger.Debug("Ignoring redundant plugin assembly addition for {AssemblyName}", assembly.FullName);
            }
        }

        /// <summary>
        /// Loads and adds an assembly by filename
        /// </summary>
        /// <param name="fileName"></param>
        public void AddAssemblyByFileName(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var completeFileName = Path.Combine(_gameDirectory, fileName);

            _logger.Debug("Loading plugin assembly from {Path}", completeFileName);

            var assembly = Assembly.LoadFrom(completeFileName);

            AddAssembly(assembly);
        }

        /// <summary>
        /// Adds an assembly from a configuration entry
        /// </summary>
        /// <param name="configuration"></param>
        public void AddAssemblyFromConfiguration(GameAssemblyConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            AddAssemblyByFileName(configuration.AssemblyName);
        }

        /// <summary>
        /// Adds all assemblies from configuration entries that have a specific target
        /// </summary>
        /// <param name="configurations"></param>
        /// <param name="target"></param>
        public void AddAllAssembliesFromConfiguration(IEnumerable<GameAssemblyConfiguration> configurations, GameAssemblyTarget target)
        {
            if (configurations == null)
            {
                throw new ArgumentNullException(nameof(configurations));
            }

            foreach (var configuration in configurations)
            {
                if (configuration == null)
                {
                    throw new ArgumentNullException(nameof(configurations));
                }

                if (configuration.Targets.Contains(target))
                {
                    AddAssemblyFromConfiguration(configuration);
                }
            }
        }

        public PluginManager Build()
        {
            return new PluginManager(_assemblies.ToArray());
        }
    }
}
