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

using SharpLife.Engine.Client.UI.Rendering;
using SharpLife.Engine.Entities.KeyValues;
using SharpLife.Engine.Models;
using SharpLife.Engine.ObjectEditor;
using System;
using System.Numerics;

namespace SharpLife.Engine.Entities.Components
{
    /// <summary>
    /// Properties for rendering models
    /// </summary>
    public class RenderProperties : Component
    {
        [KeyValue(Name = "renderfx")]
        public RenderFX RenderFX { get; set; }

        [KeyValue(Name = "rendermode")]
        public RenderMode RenderMode { get; set; }

        [KeyValue(Name = "renderamt")]
        public int RenderAmount { get; set; }

        [KeyValue(Name = "rendercolor")]
        [ObjectEditorVector3(DisplayFormat = Vector3DisplayFormat.Color24)]
        public Vector3 RenderColor { get; set; }

        public static int CalculateFXBlend(IViewState viewState, Transform transform, RenderFX renderFX, int renderAmount)
        {
            //Offset is random based on entity index
            var offset = transform.Entity.Id * 363.0f;

            int result;

            //Not all render effects update the render amount
            switch (renderFX)
            {
                //All effects not handled use entity render amount (no special effect)
                default:
                    result = renderAmount;
                    break;

                //Pulsating transparency
                case RenderFX.PulseSlow:
                case RenderFX.PulseFast:
                case RenderFX.PulseSlowWide:
                case RenderFX.PulseFastWide:
                    {
                        var multiplier1 = (renderFX == RenderFX.PulseSlow || renderFX == RenderFX.PulseSlowWide) ? 2.0 : 8.0;
                        var multiplier2 = (renderFX == RenderFX.PulseSlow || renderFX == RenderFX.PulseFast) ? 16.0 : 64.0;
                        result = (int)Math.Floor(renderAmount + (Math.Sin(offset + (EntitySystem.Time.ElapsedTime * multiplier1)) * multiplier2));
                        break;
                    }

                //Fade out from solid to translucent
                case RenderFX.FadeSlow:
                case RenderFX.FadeFast:
                    result = renderAmount = Math.Max(0, renderAmount - (renderFX == RenderFX.FadeSlow ? 1 : 4));
                    break;

                //Fade in from translucent to solid
                case RenderFX.SolidSlow:
                case RenderFX.SolidFast:
                    result = renderAmount = Math.Min(255, renderAmount + (renderFX == RenderFX.SolidSlow ? 1 : 4));
                    break;

                //A strobing effect where the model becomes visible every so often
                case RenderFX.StrobeSlow:
                case RenderFX.StrobeFast:
                case RenderFX.StrobeFaster:
                    {
                        double multiplier;

                        switch (renderFX)
                        {
                            case RenderFX.StrobeSlow:
                                multiplier = 4.0;
                                break;
                            case RenderFX.StrobeFast:
                                multiplier = 16.0;
                                break;
                            case RenderFX.StrobeFaster:
                                multiplier = 36.0;
                                break;

                            //Will never happen, silences compiler error
                            default: throw new InvalidOperationException("Update switch statement to handle render fx strobe cases");
                        }

                        if ((int)Math.Floor(Math.Sin(offset + (EntitySystem.Time.ElapsedTime * multiplier)) * 20.0) < 0)
                        {
                            return 0;
                        }

                        result = renderAmount;
                        break;
                    }

                //Flicker in and out of existence
                case RenderFX.FlickerSlow:
                case RenderFX.FlickerFast:
                    {
                        double multiplier1;
                        double multiplier2;

                        if (renderFX == RenderFX.FlickerSlow)
                        {
                            multiplier1 = 2.0;
                            multiplier2 = 17.0;
                        }
                        else
                        {
                            multiplier1 = 16.0;
                            multiplier2 = 23.0;
                        }

                        if ((int)Math.Floor(Math.Sin(offset * EntitySystem.Time.ElapsedTime * multiplier2) + (Math.Sin(EntitySystem.Time.ElapsedTime * multiplier1) * 20.0)) < 0)
                        {
                            return 0;
                        }

                        result = renderAmount;
                        break;
                    }

                //Similar to pulse, but clamped to [148, 211], more chaotic
                case RenderFX.Distort:
                //Hologram effect based on player position and view direction relative to entity
                case RenderFX.Hologram:
                    {
                        int amount;
                        if (renderFX == RenderFX.Distort)
                        {
                            amount = renderAmount = 180;
                        }
                        else
                        {
                            var dot = Vector3.Dot(transform.AbsoluteOrigin - viewState.Origin, viewState.ViewVectors.Forward);

                            if (dot <= 0)
                            {
                                return 0;
                            }

                            renderAmount = 180;

                            if (dot <= 100)
                            {
                                amount = 180;
                            }
                            else
                            {
                                amount = (int)Math.Floor((1 - ((dot - 100) * 0.0025)) * 180);
                            }
                        }
                        result = EntitySystem.Scene.Random.Next(-32, 31) + amount;
                        break;
                    }
            }

            return Math.Clamp(result, 0, 255);
        }
    }
}
