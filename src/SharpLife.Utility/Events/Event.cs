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

using System;

namespace SharpLife.Utility.Events
{
    public struct Event
    {
        /// <summary>
        /// The event system that dispatched this event
        /// </summary>
        public EventSystem EventSystem { get; }

        /// <summary>
        /// Event name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Event data
        /// </summary>
        public object Data { get; }

        public Event(EventSystem eventSystem, string name, object data)
        {
            EventSystem = eventSystem ?? throw new ArgumentNullException(nameof(eventSystem));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Data = data;
        }
    }
}
