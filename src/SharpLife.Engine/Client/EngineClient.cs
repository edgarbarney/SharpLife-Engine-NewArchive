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

using SDL2;
using SharpLife.CommandSystem;
using SharpLife.Engine.Client.UI;
using SharpLife.Engine.Shared.Logging;
using System;

namespace SharpLife.Engine.Client
{
    /// <summary>
    /// Handles all client specific engine operations, manages client state
    /// </summary>
    internal sealed class EngineClient
    {
        private readonly Host.Engine _engine;

        public ICommandContext CommandContext { get; }

        public UserInterface UserInterface { get; }

        public ILogListener LogListener
        {
            get => _engine.LogTextWriter.Listener;
            set => _engine.LogTextWriter.Listener = value;
        }

        public EngineClient(Host.Engine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));

            CommandContext = _engine.CommandSystem.CreateContext("ClientContext", _engine.EngineContext);

            var gameWindowName = _engine.EngineConfiguration.DefaultGameName;

            if (!string.IsNullOrWhiteSpace(_engine.EngineConfiguration.GameName))
            {
                gameWindowName = _engine.EngineConfiguration.GameName;
            }

            UserInterface = new UserInterface(_engine.Logger, _engine.EngineTime, _engine.FileSystem, CommandContext, this,
                _engine.CommandLine.Contains("-noontop"), gameWindowName, _engine.CommandLine.Contains("-noborder") ? SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS : 0);

            UserInterface.Quit += _engine.Exit;
        }

        public void SleepUntilInput(int milliSeconds)
        {
            UserInterface.SleepUntilInput(milliSeconds);
        }

        public void Update(float deltaSeconds)
        {
            UserInterface.Update(deltaSeconds);
        }

        public void Draw()
        {
            UserInterface.Draw();
        }

        public void Shutdown()
        {
            UserInterface.Shutdown();

            CommandContext.Dispose();
        }

        public void LocalConnect()
        {
            UserInterface.Renderer.LoadModels(_engine.World.MapInfo.Model, _engine.World.Models);
        }

        public void Disconnect(bool shutdownServer)
        {
            //TODO
        }
    }
}
