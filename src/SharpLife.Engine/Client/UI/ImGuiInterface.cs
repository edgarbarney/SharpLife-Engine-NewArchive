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

using ImGuiNET;
using Serilog;
using SharpLife.Utility;
using System;

namespace SharpLife.Engine.Client.UI
{
    internal sealed class ImGuiInterface
    {
        private readonly FrameTimeAverager _fta = new FrameTimeAverager(0.666);

        private readonly ILogger _logger;

        private readonly EngineClient _client;

        private readonly Console _console;

        public ImGuiInterface(ILogger logger, EngineClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));

            _console = new Console(logger, client);
        }

        public void Update(float deltaSeconds)
        {
            _fta.AddTime(deltaSeconds);
        }

        public void Draw()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Tools"))
                {
                    _console.AddMenuItem();

                    ImGui.EndMenu();
                }

                ImGui.Text(_fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms"));

                //TODO: probably should get the scene from somewhere else
                var viewState = _client.UserInterface.Renderer.Scene;

                ImGui.TextUnformatted($"Camera Position: {viewState.Origin} Camera Angles: Pitch {viewState.Angles.X} Yaw {viewState.Angles.Y}");

                ImGui.EndMainMenuBar();
            }

            _console.Draw();
        }
    }
}
