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

        private readonly Dictionary<string, EventMetaData> _events = new Dictionary<string, EventMetaData>();

        /// <summary>
        /// Keeps track of our nested dispatch count
        /// </summary>
        private int _inDispatchCount;

        public void AddListener(string name, Listener listener)
        {
            if (!_events.TryGetValue(name, out var metaData))
            {
                metaData = new EventMetaData(name);

                _events.Add(name, metaData);
            }

            metaData.Listeners.Add(listener);
        }

        public void RemoveListeners(string name)
        {
            _events.Remove(name);
        }

        public void RemoveListener(Listener listener)
        {
            foreach (var metaData in _events)
            {
                metaData.Value.Listeners.RemoveAll(invoker => ReferenceEquals(invoker, listener));
            }
        }

        public void RemoveListener(string name, Listener listener)
        {
            if (_events.TryGetValue(name, out var metaData))
            {
                var index = metaData.Listeners.FindIndex(invoker => invoker.Equals(listener));

                if (index != -1)
                {
                    metaData.Listeners.RemoveAt(index);
                }

                if (metaData.Listeners.Count == 0)
                {
                    _events.Remove(name);
                }
            }
        }

        public void RemoveListener(object listener)
        {
            foreach (var metaData in _events)
            {
                metaData.Value.Listeners.RemoveAll(delegateListener => delegateListener.Target == listener);
            }
        }

        public void RemoveAllListeners()
        {
            _events.Clear();
        }

        public void DispatchEvent(string name, object data = null)
        {
            if (_events.TryGetValue(name, out var metaData))
            {
                ++_inDispatchCount;

                for (var i = 0; i < metaData.Listeners.Count; ++i)
                {
                    metaData.Listeners[i].Invoke(name, data);
                }

                --_inDispatchCount;
            }
        }
    }
}
