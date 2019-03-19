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
using SharpLife.CommandSystem;
using SharpLife.Engine.Configuration;
using SharpLife.Engine.Events;
using SharpLife.Engine.Host;
using SharpLife.Utility;
using SharpLife.Utility.Events;
using System;
using System.Diagnostics;

namespace SharpLife.Engine.Server
{
    /// <summary>
    /// Handles all server specific engine operations, manages server state
    /// </summary>
    internal sealed class EngineServer
    {
        private readonly Host.Engine _engine;

        private readonly ILogger _logger;

        private int _spawnCount;

        private readonly SnapshotTime _gameTime = new SnapshotTime();

        public ICommandContext CommandContext { get; }

        public IEventSystem EventSystem => _engine.EventSystem;

        public bool Active { get; private set; }

        public EngineServer(Host.Engine engine, ILogger logger, EngineStartupState startupState)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            CommandContext = _engine.CommandSystem.CreateContext("ServerContext", _engine.EngineContext);

            startupState.PluginManager.AddAllAssembliesFromConfiguration(engine.EngineConfiguration.GameAssemblies, GameAssemblyTarget.Server);
        }

        public void Shutdown()
        {
            CommandContext.Dispose();
        }

        public bool Start(string mapName, string startSpot = null, ServerStartFlags flags = ServerStartFlags.None)
        {
            //TODO: start transitioning clients

            _logger.Information($"Loading map \"{mapName}\"");

            //TODO: print server vars

            //TODO: set hostname

            if (startSpot != null)
            {
                _logger.Debug($"Spawn Server {mapName}: [{startSpot}]\n");
            }
            else
            {
                _logger.Debug($"Spawn Server {mapName}\n");
            }

            ++_spawnCount;

            //TODO: clear custom data if size exceeds maximum

            //TODO: allocate client memory

            EventSystem.DispatchEvent(EngineEvents.ServerMapDataStartLoad);

            if (!TryMapLoadBegin(mapName, flags))
            {
                Stop();
                return false;
            }

            return true;
        }

        private bool TryMapLoadBegin(string mapName, ServerStartFlags flags)
        {
            //Reset timers
            _gameTime.ElapsedTime = _engine.EngineTime.ElapsedTime;
            _gameTime.FrameTime = 0;

            _engine.EventSystem.DispatchEvent(new MapStartedLoading(
                mapName,
                _engine.World.MapInfo?.Name,
                (flags & ServerStartFlags.ChangeLevel) != 0,
                (flags & ServerStartFlags.LoadGame) != 0));

            if (!_engine.World.TryLoadMap(mapName))
            {
                return false;
            }

            _engine.World.InitializeMap((flags & ServerStartFlags.LoadGame) != 0);

            //TODO: initialize sky

            return true;
        }

        public void Activate()
        {
            Debug.Assert(!Active);

            //_game.Activate();

            //TODO: implement
            Active = true;

            //_game.PostActivate();
        }

        public void Deactivate()
        {
            //TODO: implement
            //TODO: notify Steam

            /*
            if (_game != null)
            {
                if (Active)
                {
                    _game.Deactivate();
                }
            }
            */
        }

        public void Stop()
        {
            //TODO: implement
            if (Active)
            {
                Deactivate();

                Active = false;

                /*
                foreach (var client in _netServer.ClientList)
                {
                    _netServer.DropClient(client, NetMessages.ServerShutdownMessage);
                }
                */
            }
        }
    }
}
