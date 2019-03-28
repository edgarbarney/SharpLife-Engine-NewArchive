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

using SharpLife.Utility.Events.Queue;
using System;
using System.Collections.Generic;

namespace SharpLife.Utility.Events
{
    /// <summary>
    /// An event system that queues up each operation and can execute it on a given event system
    /// </summary>
    internal sealed class QueuedEventSystem : IEventSystem
    {
        private List<IOperation> _operations = new List<IOperation>();

        private List<IOperation> _swapList = new List<IOperation>();

        public bool HasOperations => _operations.Count > 0;

        public void AddListener(string name, Listener listener)
        {
            _operations.Add(new AddListenerOperation(name, listener));
        }

        public void RemoveListeners(string name)
        {
            _operations.Add(new RemoveListenersOperation(name));
        }

        public void RemoveListener(Listener listener)
        {
            _operations.Add(new RemoveListenerDelegateOperation(listener));
        }

        public void RemoveListener(string name, Listener listener)
        {
            _operations.Add(new RemoveListenerOperation(name, listener));
        }

        public void RemoveListener(object listener)
        {
            _operations.Add(new RemoveListenerObjectOperation(listener));
        }

        public void RemoveAllListeners()
        {
            _operations.Add(new RemoveAllListenersOperation());
        }

        public void DispatchEvent(string name, object data = null)
        {
            _operations.Add(new DispatchEventOperation(name, data));
        }

        public void ApplyTo(IEventSystem eventSystem)
        {
            if (eventSystem == null)
            {
                throw new ArgumentNullException(nameof(eventSystem));
            }

            //To avoid issues where queued up events can be added to the operations list while applying operations,
            //Use 2 lists to ensure no corruption occurs
            var operations = _operations;

            _operations = _swapList;

            _swapList = operations;

            foreach (var operation in _swapList)
            {
                operation.Execute(eventSystem);
            }

            _swapList.Clear();
        }
    }
}
