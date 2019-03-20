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
using SharpLife.Engine.Client.UI.Rendering;
using SharpLife.Engine.Client.UI.Rendering.Models;
using SharpLife.Engine.Entities;
using SharpLife.Engine.Events;
using SharpLife.Engine.Models;
using SharpLife.Engine.Models.BSP;
using SharpLife.Engine.Models.BSP.FileFormat;
using SharpLife.FileSystem;
using SharpLife.Utility.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpLife.Engine.GameWorld
{
    public sealed class WorldState
    {
        private readonly IEventSystem _eventSystem;

        private readonly IFileSystem _fileSystem;

        private readonly IRenderer _renderer;

        private readonly ModelCreator _modelCreator;

        public ILogger Logger { get; }

        public BSPModelUtils BSPUtils { get; }

        public EntitySystemMetaData EntitySystemMetaData { get; }

        public IRendererModels RendererModels { get; }

        public MapInfo MapInfo { get; private set; }

        public Entities.Scene Scene { get; private set; }

        public WorldState(
            ILogger logger,
            IEventSystem eventSystem,
            IFileSystem fileSystem,
            EntitySystemMetaData entitySystemMetaData,
            IRenderer renderer,
            IReadOnlyList<IModelFormatProvider> modelFormats)
        {
            if (modelFormats == null)
            {
                throw new ArgumentNullException(nameof(modelFormats));
            }

            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventSystem = eventSystem ?? throw new ArgumentNullException(nameof(eventSystem));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

            BSPUtils = new BSPModelUtils(Framework.BSPModelNamePrefix, Framework.Directory.Maps, Framework.Extension.BSP);

            EntitySystemMetaData = entitySystemMetaData ?? throw new ArgumentNullException(nameof(entitySystemMetaData));
            _modelCreator = new ModelCreator(fileSystem, modelFormats.Select(p => p.CreateLoader()));
            RendererModels = _renderer.Models ?? throw new ArgumentNullException(nameof(_renderer.Models));
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
            //TODO: need to make sure the scene can be queried properly
            var models = new ModelManager(_modelCreator, _renderer?.Scene);

            try
            {
                var mapFileName = BSPUtils.FormatMapFileName(mapName);

                IModel worldModel;

                try
                {
                    worldModel = models.Load(mapFileName);
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
                    Logger.Information($"Couldn't spawn server {mapFileName}");
                    return false;
                }

                _eventSystem.DispatchEvent(EngineEvents.ServerMapDataFinishLoad);

                if (!(worldModel is BSPModel bspWorldModel))
                {
                    Logger.Information($"Model {mapFileName} is not a map");
                    return false;
                }

                _eventSystem.DispatchEvent(EngineEvents.ServerMapCRCComputed);

                MapInfo = new MapInfo(mapName, MapInfo?.Name, bspWorldModel);

                //Load the fallback model now to ensure that BSP indices are matched up
                models.LoadFallbackModel(Framework.FallbackModelName);

                Scene = new Entities.Scene(this, EntitySystemMetaData, models);

                models = null;

                EntitySystem.SetScene(Scene);

                return true;
            }
            finally
            {
                models?.Dispose();
            }
        }

        public void InitializeMap(bool loadGame)
        {
            if (loadGame)
            {
                //TODO: load game
            }
            else
            {
                Scene.LoadEntities(MapInfo.Model.BSPFile.Entities);
            }

            Scene.Start();
        }

        public void Update(double currentTime)
        {
            Scene?.Update(currentTime);
        }

        public void Clear()
        {
            if (Scene != null)
            {
                Scene.Dispose();
                Scene = null;
            }
        }
    }
}
