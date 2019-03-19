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
using System.Reflection;

namespace SharpLife.Engine.Shared.Entities.Components
{
    internal sealed class DelegateInvoker<T>
    {
        private readonly Action<T> _delegate;

        public DelegateInvoker(MethodInfo methodInfo)
        {
            _delegate = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), methodInfo);
        }

        public void Invoke(object instance)
        {
            _delegate.Invoke((T)instance);
        }
    }
}
