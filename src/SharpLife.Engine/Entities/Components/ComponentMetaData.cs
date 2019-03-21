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

using FastMember;
using SharpLife.Engine.Entities.KeyValues;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace SharpLife.Engine.Entities.Components
{
    public sealed class ComponentMetaData
    {
        public delegate void InvokableMethod(Component instance);

        public readonly Type ComponentType;
        public readonly Type InvokerType;

        public readonly TypeAccessor Accessor;

        //Allocated on demand only to reduce memory usage for simple types
        private Dictionary<string, InvokableMethod> _invokableMethods;

        public ImmutableDictionary<string, KeyValueMetaData> KeyValues { get; }

        internal ComponentMetaData(Type componentType, ImmutableDictionary<string, KeyValueMetaData> keyValues)
        {
            ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
            InvokerType = typeof(DelegateInvoker<>).MakeGenericType(componentType);

            Accessor = TypeAccessor.Create(componentType);

            KeyValues = keyValues ?? throw new ArgumentNullException(nameof(keyValues));
        }

        private MethodInfo FindMethod(string methodName)
        {
            return ComponentType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, Array.Empty<Type>(), null);
        }

        private InvokableMethod CreateInvoker(MethodInfo methodInfo)
        {
            var invoker = Activator.CreateInstance(InvokerType, methodInfo);

            return (InvokableMethod)Delegate.CreateDelegate(typeof(InvokableMethod), invoker, InvokerType.GetMethod(nameof(DelegateInvoker<Component>.Invoke)));
        }

        internal bool TryGetMethod(string methodName, out InvokableMethod invokable)
        {
            invokable = null;

            if (_invokableMethods?.TryGetValue(methodName, out invokable) != true)
            {
                var method = FindMethod(methodName);

                //TODO: could add this to the dictionary as well to speed up lookup, but since this is a rare case it probably won't be needed
                if (method == null)
                {
                    return false;
                }

                if (_invokableMethods == null)
                {
                    _invokableMethods = new Dictionary<string, InvokableMethod>();
                }

                invokable = CreateInvoker(method);

                _invokableMethods.Add(methodName, invokable);
            }

            return true;
        }

        internal bool TryGetMethodAndInvoke(string methodName, Component instance)
        {
            if (TryGetMethod(methodName, out var invokable))
            {
                invokable.Invoke(instance);

                return true;
            }

            return false;
        }
    }
}
