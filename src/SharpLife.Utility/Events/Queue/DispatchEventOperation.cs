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

namespace SharpLife.Utility.Events.Queue
{
    internal sealed class DispatchEventOperation : IOperation
    {
        private readonly string _name;
        private readonly object _data;

        public DispatchEventOperation(string name, object data)
        {
            _name = name;
            _data = data;
        }

        public void Execute(IEventSystem eventSystem)
        {
            eventSystem.DispatchEvent(_name, _data);
        }
    }
}
