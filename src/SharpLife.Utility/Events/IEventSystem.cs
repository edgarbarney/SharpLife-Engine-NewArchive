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

namespace SharpLife.Utility.Events
{
    /// <summary>
    /// The event system allows named events to be dispatched to listeners that want to know about them
    /// Events can contain data, represented as classes inheriting from a base event data class
    /// Listeners cannot be added or removed while an event dispatch is ongoing, they will be queued up and processed after the dispatch
    /// </summary>
    public interface IEventSystem
    {
        /// <summary>
        /// Adds a listener for a specific event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="listener"></param>
        void AddListener(string name, Listener listener);

        /// <summary>
        /// Removes all listeners of a specific event
        /// </summary>
        /// <param name="name"></param>
        void RemoveListeners(string name);

        /// <summary>
        /// Removes a listener by delegate
        /// </summary>
        /// <param name="listener"></param>
        void RemoveListener(Listener listener);

        /// <summary>
        /// Removes a listener from a specific event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="listener"></param>
        void RemoveListener(string name, Listener listener);

        /// <summary>
        /// Removes the given listener from all events that is it listening to
        /// </summary>
        /// <param name="listener"></param>
        void RemoveListener(object listener);

        /// <summary>
        /// Removes all listeners
        /// </summary>
        void RemoveAllListeners();

        /// <summary>
        /// Dispatches an event to all listeners of that event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data">Data to provide to listeners</param>
        /// <exception cref="ArgumentException">If name is invalid</exception>
        void DispatchEvent(string name, object data = null);
    }
}
