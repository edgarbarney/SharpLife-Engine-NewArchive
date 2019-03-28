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
    /// <summary>
    /// The event system allows named events to be dispatched to listeners that want to know about them
    /// Events can contain data, represented as classes inheriting from a base event data class
    /// Listeners cannot be added or removed while an event dispatch is ongoing, they will be queued up and processed after the dispatch
    /// </summary>
    public class EventSystem : IEventSystem
    {
        private readonly QueuedEventSystem _queuedEventSystem = new QueuedEventSystem();
        private readonly DirectEventSystem _directEventSystem = new DirectEventSystem();

        private IEventSystem _internalEventSystem;

        /// <summary>
        /// Indicates whether the event system is currently dispatching events
        /// </summary>
        private bool IsDispatching => _internalEventSystem == _queuedEventSystem;

        public EventSystem()
        {
            _internalEventSystem = _directEventSystem;
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Event name must be valid", nameof(name));
            }
        }

        public void AddListener(string name, Listener listener)
        {
            ValidateName(name);

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            _internalEventSystem.AddListener(name, listener);
        }

        public void RemoveListeners(string name)
        {
            ValidateName(name);

            _internalEventSystem.RemoveListeners(name);
        }

        public void RemoveListener(Listener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            _internalEventSystem.RemoveListener(listener);
        }

        public void RemoveListener(string name, Listener listener)
        {
            ValidateName(name);

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            _internalEventSystem.RemoveListener(name, listener);
        }

        public void RemoveListener(object listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            _internalEventSystem.RemoveListener(listener);
        }

        public void RemoveAllListeners()
        {
            _internalEventSystem.RemoveAllListeners();
        }

        public void DispatchEvent(string name, object data = null)
        {
            ValidateName(name);

            //if we're already dispatching an event, queue this up for later
            if (IsDispatching)
            {
                _internalEventSystem.DispatchEvent(name, data);
            }
            else
            {
                //Swap to the queued system to avoid corrupting execution state
                _internalEventSystem = _queuedEventSystem;

                _directEventSystem.DispatchEvent(name, data);

                while (_queuedEventSystem.HasOperations)
                {
                    _queuedEventSystem.ApplyTo(_directEventSystem);
                }

                //Reset this after applying operations to avoid edge cases where a queued up dispatch allows operations to go to the direct system
                _internalEventSystem = _directEventSystem;
            }
        }
    }
}
