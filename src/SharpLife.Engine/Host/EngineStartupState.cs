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
using SharpLife.Engine.Entities;
using SharpLife.Engine.Models;
using SharpLife.Engine.Plugins;
using System;
using System.Collections.Generic;

namespace SharpLife.Engine.Host
{
    /// <summary>
    /// Contains state about engine startup that is shared with the client and server systems during startup
    /// Used to allow client and server to configure engine settings that are used to finalize startup
    /// </summary>
    public sealed class EngineStartupState
    {
        public PluginManagerBuilder PluginManager { get; }

        public EntitySystemMetaDataBuilder EntitySystemMetaData { get; }

        public IReadOnlyList<IModelFormatProvider> ModelFormats { get; }

        public EngineStartupState(ILogger logger, string gameDirectory, IReadOnlyList<IModelFormatProvider> modelFormats)
        {
            PluginManager = new PluginManagerBuilder(logger, gameDirectory);
            EntitySystemMetaData = new EntitySystemMetaDataBuilder(logger);
            ModelFormats = modelFormats ?? throw new ArgumentNullException(nameof(modelFormats));
        }
    }
}
