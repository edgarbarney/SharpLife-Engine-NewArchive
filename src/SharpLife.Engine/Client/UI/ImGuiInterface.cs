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
using SharpLife.Engine.Logging;
using SharpLife.Utility;
using System;
using System.Numerics;
using System.Text;

namespace SharpLife.Engine.Client.UI
{
    internal sealed class ImGuiInterface : ILogListener
    {
        /// <summary>
        /// In order to handle text addition properly, we have to track the state across frames
        /// When added, the next frame won't scroll yet, since the content size is out of date
        /// </summary>
        private enum TextAdded
        {
            No = 0,
            Yes,
            ApplyScroll
        }

        private readonly FrameTimeAverager _fta = new FrameTimeAverager(0.666);

        private readonly ILogger _logger;

        private readonly EngineClient _client;

        private bool _consoleVisible;

        private string _consoleText = string.Empty;

        private const int _maxConsoleChars = ushort.MaxValue;

        private TextAdded _textAdded;

        private readonly byte[] _consoleInputBuffer = new byte[1024];

        public ImGuiInterface(ILogger logger, EngineClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));

            _client.LogListener = this;
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
                    ImGui.Checkbox("Toggle Console", ref _consoleVisible);

                    ImGui.EndMenu();
                }

                ImGui.Text(_fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms"));

                //TODO: probably should get the scene from somewhere else
                var viewState = _client.UserInterface.Renderer.Scene;

                ImGui.TextUnformatted($"Camera Position: {viewState.Origin} Camera Angles: Pitch {viewState.Angles.X} Yaw {viewState.Angles.Y}");

                ImGui.EndMainMenuBar();
            }

            DrawConsole();
        }

        public void Write(char value)
        {
            _consoleText += value;

            _textAdded = TextAdded.Yes;

            TruncateConsoleText();
        }

        public void Write(char[] buffer, int index, int count)
        {
            var text = new string(buffer, index, count);
            _consoleText += text;

            _textAdded = TextAdded.Yes;

            TruncateConsoleText();
        }

        private void TruncateConsoleText()
        {
            if (_consoleText.Length > _maxConsoleChars)
            {
                _consoleText = _consoleText.Substring(_consoleText.Length - _maxConsoleChars);
            }
        }

        private void DrawConsole()
        {
            if (_consoleVisible && ImGui.Begin("Console", ref _consoleVisible, ImGuiWindowFlags.NoCollapse))
            {
                var textHeight = ImGuiNative.igGetTextLineHeight();

                var contentMin = ImGui.GetWindowContentRegionMin();
                var contentMax = ImGui.GetWindowContentRegionMax();

                var wrapWidth = contentMax.X - contentMin.X;
                //Leave some space at the bottom
                var maxHeight = contentMax.Y - contentMin.Y - (textHeight * 3);

                //Display as input text area to allow text selection
                ImGui.InputTextMultiline("##consoleText", ref _consoleText, _maxConsoleChars, new Vector2(wrapWidth, maxHeight), ImGuiInputTextFlags.ReadOnly, null);

                //Scroll to bottom when new text is added
                ImGuiNative.igBeginGroup();
                var id = ImGui.GetID("##consoleText");
                ImGui.BeginChildFrame(id, new Vector2(wrapWidth, maxHeight), ImGuiWindowFlags.None);

                switch (_textAdded)
                {
                    case TextAdded.Yes:
                        {
                            _textAdded = TextAdded.ApplyScroll;
                            break;
                        }

                    case TextAdded.ApplyScroll:
                        {
                            _textAdded = TextAdded.No;

                            var yPos = ImGuiNative.igGetScrollMaxY();

                            ImGuiNative.igSetScrollY(yPos);
                            break;
                        }
                }

                ImGui.EndChildFrame();
                ImGuiNative.igEndGroup();

                ImGui.Text("Command:");
                ImGui.SameLine();

                //Max width for the input
                ImGui.PushItemWidth(-1);

                if (ImGui.InputText("##consoleInput", _consoleInputBuffer, (uint)_consoleInputBuffer.Length, ImGuiInputTextFlags.EnterReturnsTrue, null))
                {
                    //Needed since GetString doesn't understand null terminated strings
                    var stringLength = StringUtils.NullTerminatedByteLength(_consoleInputBuffer);

                    if (stringLength > 0)
                    {
                        var commandText = Encoding.UTF8.GetString(_consoleInputBuffer, 0, stringLength);
                        _logger.Information($"] {commandText}");
                        _client.CommandContext.QueueCommands(commandText);
                    }

                    Array.Fill<byte>(_consoleInputBuffer, 0, 0, stringLength);
                }

                ImGui.PopItemWidth();

                ImGui.End();
            }
        }
    }
}
