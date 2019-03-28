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

using SharpLife.Engine.Entities;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Entities.KeyValues;
using SharpLife.Engine.ObjectEditor;
using SharpLife.Utility.Mathematics;
using System;
using System.Numerics;

namespace SharpLife.Game.Entities.Factories.Lighting
{
    public sealed class LightEnvironment : Component
    {
        private Vector3 _skyColor;
        private Vector3 _skyNormal;

        [ObjectEditorVector3(DisplayFormat = Vector3DisplayFormat.Color24)]
        public Vector3 SkyColor
        {
            get => _skyColor;

            set
            {
                _skyColor = value;

                //Update sky values if they have changed
                if (_skyColor != EntitySystem.Scene.WorldState.Renderer.SkyColor)
                {
                    EntitySystem.Scene.WorldState.Renderer.SkyColor = SkyColor;
                }
            }
        }

        [ObjectEditorVector3(DisplayFormat = Vector3DisplayFormat.Normal)]
        public Vector3 SkyNormal
        {
            get => _skyNormal;

            set
            {
                _skyNormal = value;

                //Update sky values if they have changed
                if (_skyNormal != EntitySystem.Scene.WorldState.Renderer.SkyNormal)
                {
                    EntitySystem.Scene.WorldState.Renderer.SkyNormal = _skyNormal;
                }
            }
        }

        //Don't use this in the object editor, use SkyColor
        [KeyValue(Name = "_light")]
        public string SkyColorValue
        {
            get => VectorUtils.ToString(SkyColor);

            set
            {
                var values = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int r, g, b;

                if (values.Length == 1)
                {
                    int.TryParse(values[0], out r);
                    g = b = r;
                }
                else if (values.Length == 4)
                {
                    int.TryParse(values[0], out r);
                    int.TryParse(values[1], out g);
                    int.TryParse(values[2], out b);
                    int.TryParse(values[3], out var v);

                    r = (int)(r * (v / 255.0));
                    g = (int)(g * (v / 255.0));
                    b = (int)(b * (v / 255.0));
                }
                else
                {
                    //Fall back to fullbright
                    r = g = b = byte.MaxValue;
                    //TODO: log warning
                }

                // simulate qrad direct, ambient,and gamma adjustments, as well as engine scaling
                r = (int)(Math.Pow(r / 114.0, 0.6) * 264);
                g = (int)(Math.Pow(g / 114.0, 0.6) * 264);
                b = (int)(Math.Pow(b / 114.0, 0.6) * 264);

                SkyColor = new Vector3(r, g, b);
            }
        }

        public void Activate()
        {
            var transform = Entity.GetComponent<Transform>();

            var vectors = VectorUtils.AngleToAimVectors(transform.Angles);

            SkyNormal = vectors.Forward;
        }
    }
}
