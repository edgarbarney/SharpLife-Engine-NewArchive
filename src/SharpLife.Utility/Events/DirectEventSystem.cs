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

namespace SharpLife.Utility.Events
{
    /// <summary>
    /// An event system that immediately executes given operations
    /// Does not perform error checking (handled in <see cref="EventSystem"/>
    /// </summary>
    internal sealed class DirectEventSystem : IEventSystem
    {
        /// <summary>
        /// Indicates whether the event system is currently dispatching events
        /// </summary>
        public bool IsDispatching => _inDispatchCount > 0;

        private readonly Dictionary<string, List<Listener>> _eventListeners = new Dictionary<string, List<Listener>>();

        /// <summary>
        /// Keeps track of our nested dispatch count
        /// </summary>
        private int _inDispatchCount;

        public void AddListener(string name, Listener listener)
        {
            if (!_eventListeners.TryGetValue(name, out var listeners))
            {
                listeners = new List<Listener>();

                _eventListeners.Add(name, listeners);
            }

            listeners.Add(listener);
        }

        public void RemoveListeners(string name)
        {
            _eventListeners.Remove(name);
        }

        public void RemoveListener(Listener listener)
        {
            foreach (var listeners in _eventListeners)
            {
                listeners.Value.RemoveAll(invoker => ReferenceEquals(invoker, listener));
            }
        }

        public void RemoveListener(string name, Listener listener)
        {
            if (_eventListeners.TryGetValue(name, out var listeners))
            {
                var index = listeners.FindIndex(invoker => invoker.Equals(listener));

                if (index != -1)
                {
                    listeners.RemoveAt(index);
                }

                if (listeners.Count == 0)
                {
                    _eventListeners.Remove(name);
                }
            }
        }

        public void RemoveListener(object listener)
        {
            foreach (var listeners in _eventListeners)
            {
                listeners.Value.RemoveAll(delegateListener => delegateListener.Target == listener);
            }
        }

        public void RemoveAllListeners()
        {
            _eventListeners.Clear();
        }

        public void DispatchEvent(string name, object data = null)
        {
            if (_eventListeners.TryGetValue(name, out var listeners))
            {
                ++_inDispatchCount;

                for (var i = 0; i < listeners.Count; ++i)
                {
                    listeners[i].Invoke(name, data);
                }

                --_inDispatchCount;
            }
        }
    }
}
