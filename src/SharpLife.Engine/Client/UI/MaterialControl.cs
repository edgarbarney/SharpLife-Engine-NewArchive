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
using SharpLife.CommandSystem;
using SharpLife.CommandSystem.Commands;
using SharpLife.Engine.Client.UI.Rendering.Utility;
using System;

namespace SharpLife.Engine.Client.UI
{
    internal sealed class MaterialControl
    {
        private readonly ICommandContext _commandContext;

        private bool _materialControlVisible;

        private IVariable<uint> _fpsMax;
        private IVariable<float> _mainGamma;
        private IVariable<float> _textureGamma;
        private IVariable<float> _lightingGamma;
        private IVariable<float> _brightness;
        private IVariable<bool> _overbright;
        private IVariable<bool> _fullbright;
        private IVariable<uint> _maxSize;
        private IVariable<uint> _roundDown;
        private IVariable<uint> _picMip;
        private IVariable<bool> _powerOf2Textures;

        public MaterialControl(ICommandContext commandContext)
        {
            _commandContext = commandContext ?? throw new ArgumentNullException(nameof(commandContext));
        }

        public void AddMenuItem()
        {
            ImGui.Checkbox("Toggle Material Control", ref _materialControlVisible);
        }

        private void CacheVariable<T>(ref IVariable<T> variable, string name)
        {
            if (variable == null)
            {
                variable = _commandContext.FindCommand<IVariable<T>>(name) ?? throw new NotSupportedException($"Couldn't get console variable {name}");
            }
        }

        private void CacheConsoleVariables()
        {
            CacheVariable(ref _fpsMax, "fps_max");
            CacheVariable(ref _mainGamma, "mat_gamma");
            CacheVariable(ref _textureGamma, "mat_texgamma");
            CacheVariable(ref _lightingGamma, "mat_lightgamma");
            CacheVariable(ref _brightness, "mat_brightness");
            CacheVariable(ref _overbright, "mat_overbright");
            CacheVariable(ref _fullbright, "mat_fullbright");
            CacheVariable(ref _maxSize, "mat_max_size");
            CacheVariable(ref _roundDown, "mat_round_down");
            CacheVariable(ref _picMip, "mat_picmip");
            CacheVariable(ref _powerOf2Textures, "mat_powerof2textures");
        }

        private void DrawIntSlider(IVariable<int> variable, string sliderLabel, int min, int max, string displayText)
        {
            var value = variable.Value;

            if (ImGui.SliderInt(sliderLabel, ref value, min, max, displayText))
            {
                variable.Value = value;
            }
        }

        private void DrawUIntSlider(IVariable<uint> variable, string sliderLabel, uint min, uint max, string displayText)
        {
            var value = (int)variable.Value;

            if (ImGui.SliderInt(sliderLabel, ref value, (int)min, (int)max, displayText))
            {
                variable.Value = (uint)value;
            }
        }

        private void DrawFloatSlider(IVariable<float> variable, string sliderLabel, float min, float max, string displayText)
        {
            var value = variable.Value;

            if (ImGui.SliderFloat(sliderLabel, ref value, min, max, displayText, 1))
            {
                variable.Value = value;
            }
        }

        private void DrawCheckbox(IVariable<bool> variable, string label)
        {
            var value = variable.Value;

            if (ImGui.Checkbox(label, ref value))
            {
                variable.Value = value;
            }
        }

        public void Draw()
        {
            if (_materialControlVisible && ImGui.Begin("Material Control", ref _materialControlVisible, ImGuiWindowFlags.NoCollapse))
            {
                CacheConsoleVariables();

                DrawUIntSlider(_fpsMax, "Maximum Frames Per Second", 0, 1000, "%d FPS");

                DrawFloatSlider(_mainGamma, "Main Gamma", 0, 10, "%0.1f");
                DrawFloatSlider(_textureGamma, "Texture Gamma", 0, 10, "%0.1f");
                DrawFloatSlider(_lightingGamma, "Lighting Gamma", 0, 10, "%0.1f");
                DrawFloatSlider(_brightness, "Brightness Override", 0, 10, "%0.1f");

                DrawUIntSlider(_maxSize, "Constrain texture scales to this maximum size", ImageConversionUtils.MinimumMaxImageSize, 1 << 14, "%d");
                DrawUIntSlider(_roundDown, "Round Down texture scales using this exponent", ImageConversionUtils.MinSizeExponent, ImageConversionUtils.MaxSizeExponent, "%d");
                DrawUIntSlider(_picMip, "Scale down texture scales this many times", ImageConversionUtils.MinSizeExponent, ImageConversionUtils.MaxSizeExponent, "%d");

                DrawCheckbox(_overbright, "Enable overbright");
                DrawCheckbox(_fullbright, "Enable fullbright");
                DrawCheckbox(_powerOf2Textures, "Enable power of 2 texture rescaling");

                ImGui.End();
            }
        }
    }
}
