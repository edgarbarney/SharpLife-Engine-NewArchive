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
using System.Collections.Generic;

namespace SharpLife.Utility.Events
{
    /// <summary>
    /// The event system allows named events to be dispatched to listeners that want to know about them
    /// Events can contain data, represented as classes inheriting from a base event data class
    /// Listeners cannot be added or removed while an event dispatch is ongoing, they will be queued up and processed after the dispatch
    /// </summary>
    public class EventSystem
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

        private readonly List<Delegates.PostDispatchCallback> _postDispatchCallbacks = new List<Delegates.PostDispatchCallback>();

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Event name must be valid", nameof(name));
            }
        }

        /// <summary>
        /// Adds a listener for a specific event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="listener"></param>
        public void AddListener(string name, Delegates.Listener listener)
        {
            ValidateName(name);

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot add listeners while dispatching");
            }

            if (!_events.TryGetValue(name, out var metaData))
            {
                metaData = new EventMetaData(name);

                _events.Add(name, metaData);
            }

            metaData.Listeners.Add(listener);
        }

        /// <summary>
        /// Adds a listener to multiple events
        /// <seealso cref="AddListener(string, Delegates.Listener)"/>
        /// </summary>
        /// <param name="names">List of names</param>
        /// <param name="listener"></param>
        public void AddListeners(string[] names, Delegates.Listener listener)
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot add listeners while dispatching");
            }

            foreach (var name in names)
            {
                AddListener(name, listener);
            }
        }

        /// <summary>
        /// Removes all listeners of a specific event
        /// </summary>
        /// <param name="name"></param>
        public void RemoveListeners(string name)
        {
            ValidateName(name);

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            if (_events.TryGetValue(name, out var metaData))
            {
                metaData.Listeners.Clear();
            }
        }

        /// <summary>
        /// Removes a listener by delegate
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(Delegates.Listener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            foreach (var metaData in _events)
            {
                metaData.Value.Listeners.RemoveAll(invoker => ReferenceEquals(invoker.Target, listener));
            }
        }

        /// <summary>
        /// Removes a listener from a specific event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="listener"></param>
        public void RemoveListener(string name, Delegates.Listener listener)
        {
            ValidateName(name);

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            if (_events.TryGetValue(name, out var metaData))
            {
                var index = metaData.Listeners.FindIndex(invoker => invoker.Equals(listener));

                if (index != -1)
                {
                    metaData.Listeners.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Removes the given listener from all events that is it listening to
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(object listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            foreach (var metaData in _events)
            {
                metaData.Value.Listeners.RemoveAll(delegateListener => delegateListener.Target == listener);
            }
        }

        /// <summary>
        /// Removes all listeners
        /// </summary>
        public void RemoveAllListeners()
        {
            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            foreach (var metaData in _events)
            {
                metaData.Value.Listeners.Clear();
            }
        }

        /// <summary>
        /// Dispatches an event to all listeners of that event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data">Data to provide to listeners</param>
        /// <exception cref="ArgumentNullException">If name is null</exception>
        public void DispatchEvent(string name, object data = null)
        {
            ValidateName(name);

            if (_events.TryGetValue(name, out var metaData))
            {
                var @event = new Event(this, name, data);

                ++_inDispatchCount;

                for (var i = 0; i < metaData.Listeners.Count; ++i)
                {
                    metaData.Listeners[i].Invoke(@event);
                }

                --_inDispatchCount;

                if (_inDispatchCount == 0 && _postDispatchCallbacks.Count > 0)
                {
                    _postDispatchCallbacks.ForEach(callback => callback(this));
                    _postDispatchCallbacks.Clear();
                    //Avoid wasting memory, since this is a rarely used operation
                    _postDispatchCallbacks.Capacity = 0;
                }
            }
        }

        /// <summary>
        /// Adds a post dispatch callback
        /// Use this when adding or removing listeners or events while in an event dispatch
        /// </summary>
        /// <param name="callback"></param>
        /// <exception cref="InvalidOperationException">If a callback is added while not in an event dispatch</exception>
        public void AddPostDispatchCallback(Delegates.PostDispatchCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (!IsDispatching)
            {
                throw new InvalidOperationException("Can only add post dispatch callbacks while dispatching events");
            }

            _postDispatchCallbacks.Add(callback);
        }
    }
}
