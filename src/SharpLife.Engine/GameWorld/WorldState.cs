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
using SharpLife.Engine.Models.BSP;
using SharpLife.Engine.Models.MDL;
using SharpLife.Engine.Models.SPR;
using SharpLife.Engine.Shared;
using SharpLife.Engine.Shared.Events;
using SharpLife.FileSystem;
using SharpLife.Models;
using SharpLife.Models.BSP;
using SharpLife.Models.BSP.FileFormat;
using SharpLife.Utility.Events;
using System;
using System.IO;

namespace SharpLife.Engine.GameWorld
{
    internal sealed class WorldState
    {
        private readonly ILogger _logger;

        private readonly IEventSystem _eventSystem;

        private readonly IFileSystem _fileSystem;

        public IModelManager Models { get; }

        public BSPModelUtils BSPUtils { get; }

        public MapInfo MapInfo { get; private set; }

        public WorldState(ILogger logger, IEventSystem eventSystem, IFileSystem fileSystem)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventSystem = eventSystem ?? throw new ArgumentNullException(nameof(eventSystem));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            Models = new ModelManager(fileSystem, new IModelLoader[]
            {
                new SpriteModelLoader(),
                new StudioModelLoader(),

                //BSP loader comes last due to not having a way to positively recognize the format
                new BSPModelLoader(Framework.BSPModelNamePrefix)
            });

            BSPUtils = new BSPModelUtils(Framework.BSPModelNamePrefix, Framework.Directory.Maps, Framework.Extension.BSP);
        }

        /// <summary>
        /// Returns whether the given map name is valid
        /// </summary>
        /// <param name="mapName">The map name without directory or extension</param>
        public bool IsMapValid(string mapName)
        {
            return _fileSystem.Exists(BSPUtils.FormatMapFileName(mapName));
        }

        public bool TryLoadMap(string mapName)
        {
            var mapFileName = BSPUtils.FormatMapFileName(mapName);

            IModel worldModel;

            try
            {
                worldModel = Models.Load(mapFileName);
            }
            catch (Exception e)
            {
                //TODO: needs a rework
                if (e is InvalidOperationException
                    || e is InvalidBSPVersionException
                    || e is IOException)
                {
                    worldModel = null;
                }
                else
                {
                    throw;
                }
            }

            if (worldModel == null)
            {
                _logger.Information($"Couldn't spawn server {mapFileName}");
                return false;
            }

            _eventSystem.DispatchEvent(EngineEvents.ServerMapDataFinishLoad);

            if (!(worldModel is BSPModel bspWorldModel))
            {
                _logger.Information($"Model {mapFileName} is not a map");
                return false;
            }

            _eventSystem.DispatchEvent(EngineEvents.ServerMapCRCComputed);

            MapInfo = new MapInfo(mapName, MapInfo?.Name, bspWorldModel);

            //Load the fallback model now to ensure that BSP indices are matched up
            Models.LoadFallbackModel(Framework.FallbackModelName);

            return true;
        }

        public void InitializeMap(bool loadGame)
        {
            //TODO
        }

        public void Clear()
        {
            Models.Clear();
        }
    }
}
