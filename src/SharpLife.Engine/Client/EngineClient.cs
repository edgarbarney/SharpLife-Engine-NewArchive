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
using SharpLife.Engine.Configuration;
using SharpLife.Engine.Host;
using SharpLife.Engine.Logging;
using System;

namespace SharpLife.Engine.Client
{
    /// <summary>
    /// Handles all client specific engine operations, manages client state
    /// </summary>
    internal sealed class EngineClient
    {
        public Host.Engine Engine { get; }

        public ICommandContext CommandContext { get; }

        public UserInterface UserInterface { get; }

        public ILogListener LogListener
        {
            get => Engine.LogTextWriter.Listener;
            set => Engine.LogTextWriter.Listener = value;
        }

        public EngineClient(Host.Engine engine, EngineStartupState startupState)
        {
            Engine = engine ?? throw new ArgumentNullException(nameof(engine));

            CommandContext = Engine.CommandSystem.CreateContext("ClientContext", Engine.EngineContext);

            var gameWindowName = Engine.EngineConfiguration.DefaultGameName;

            if (!string.IsNullOrWhiteSpace(Engine.EngineConfiguration.GameName))
            {
                gameWindowName = Engine.EngineConfiguration.GameName;
            }

            UserInterface = new UserInterface(Engine.Logger, Engine.EngineTime, Engine.FileSystem, CommandContext, this,
                startupState,
                Engine.CommandLine.Contains("-noontop"), gameWindowName, Engine.CommandLine.Contains("-noborder") ? SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS : 0);

            UserInterface.Quit += Engine.Exit;

            startupState.PluginManager.AddAllAssembliesFromConfiguration(engine.EngineConfiguration.GameAssemblies, GameAssemblyTarget.Client);
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

        public void ClearMemory()
        {
            UserInterface.Renderer.ClearBSP();
        }

        public void Shutdown()
        {
            UserInterface.Shutdown();

            CommandContext.Dispose();
        }

        public void LocalConnect()
        {
            UserInterface.Renderer.LoadModels(Engine.World.MapInfo.Model);

            //TODO: need a way to hook into this event before world is created
            UserInterface.Renderer.ImGui.ImGuiInterface.OnMapStart(Engine.World.Scene);
        }

        public void Disconnect(bool shutdownServer)
        {
            UserInterface.Renderer.ImGui.ImGuiInterface.OnMapEnd();
            //TODO
        }
    }
}
