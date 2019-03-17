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

using SharpLife.Engine.Models.BSP;
using System;

namespace SharpLife.Engine.GameWorld
{
    /// <summary>
    /// Contains the state of the world for a particular map
    /// </summary>
    public sealed class MapInfo
    {
        /// <summary>
        /// The name of the map, without directory or extension
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The name of the map loaded prior to this one, if any
        /// </summary>
        public string PreviousMapName { get; }

        /// <summary>
        /// The map model
        /// </summary>
        public BSPModel Model { get; }

        public MapInfo(string name, string previousMapName, BSPModel model)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PreviousMapName = previousMapName;
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}
