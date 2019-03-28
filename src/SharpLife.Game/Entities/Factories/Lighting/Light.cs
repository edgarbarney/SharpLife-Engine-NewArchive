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

using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Entities.KeyValues;

namespace SharpLife.Game.Entities.Factories.Lighting
{
    public class Light : Component
    {
        [KeyValue(Name = "style")]
        public int Style;

        [KeyValue(Name = "pattern")]
        public int Pattern;

        [KeyValue(Name = "pitch")]
        public float Pitch
        {
            get => Entity.GetComponent<Transform>().Angles.X;

            set
            {
                var transform = Entity.GetComponent<Transform>();

                var angles = transform.Angles;

                angles.X = value;

                transform.Angles = angles;
            }
        }

        //TODO: implement
    }
}
