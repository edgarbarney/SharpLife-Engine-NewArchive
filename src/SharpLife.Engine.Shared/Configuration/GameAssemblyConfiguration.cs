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

using System.Collections.Generic;
using System.Xml.Serialization;

namespace SharpLife.Engine.Shared.Configuration
{
    /// <summary>
    /// Contains information about a single game assembly to be loaded
    /// </summary>
    public sealed class GameAssemblyConfiguration
    {
        public string AssemblyName { get; set; }

        [XmlArrayItem(ElementName = "Target")]
        public List<GameAssemblyTarget> Targets { get; set; }
    }
}
