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
        public delegate void InvokableMethod(Component instance, object parameter);

        private readonly struct MethodKey : IEquatable<MethodKey>
        {
            public readonly string Name;
            public readonly Type ParameterType;

            public MethodKey(string name, Type parameterType)
            {
                Name = name;
                ParameterType = parameterType;
            }

            public override bool Equals(object obj)
            {
                return obj is MethodKey methodKey && Equals(methodKey);
            }

            public bool Equals(MethodKey other)
            {
                return Name == other.Name && ParameterType == other.ParameterType;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, ParameterType);
            }

            public static bool operator ==(in MethodKey key1, in MethodKey key2)
            {
                return key1.Equals(key2);
            }

            public static bool operator !=(in MethodKey key1, in MethodKey key2)
            {
                return !(key1 == key2);
            }
        }

        public readonly Type ComponentType;
        public readonly Type InvokerType;

        public readonly TypeAccessor Accessor;

        //Allocated on demand only to reduce memory usage for simple types
        private Dictionary<MethodKey, InvokableMethod> _invokableMethods;

        public ImmutableDictionary<string, KeyValueMetaData> KeyValues { get; }

        public ImmutableArray<SpawnFlagMetaData> SpawnFlags { get; }

        internal ComponentMetaData(Type componentType, ImmutableDictionary<string, KeyValueMetaData> keyValues, ImmutableArray<SpawnFlagMetaData> spawnFlags)
        {
            ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
            InvokerType = typeof(DelegateInvoker<>).MakeGenericType(componentType);

            Accessor = TypeAccessor.Create(componentType);

            KeyValues = keyValues ?? throw new ArgumentNullException(nameof(keyValues));
            SpawnFlags = spawnFlags;
        }

        private MethodInfo FindMethod(string methodName, Type parameterType)
        {
            var parameters = parameterType != typeof(void) ? new[] { parameterType } : Array.Empty<Type>();

            return ComponentType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, parameters, null);
        }

        private InvokableMethod CreateInvoker(MethodInfo methodInfo, Type parameterType)
        {
            var invokerType = parameterType == typeof(void) ? InvokerType : typeof(DelegateInvoker<,>).MakeGenericType(new[] { ComponentType, parameterType });

            var invoker = Activator.CreateInstance(invokerType, methodInfo);

            return (InvokableMethod)Delegate.CreateDelegate(typeof(InvokableMethod), invoker, invokerType.GetMethod(nameof(DelegateInvoker<Component>.Invoke)));
        }

        internal bool TryGetMethod(string methodName, object parameter, out InvokableMethod invokable)
        {
            var parameterType = parameter != null ? parameter.GetType() : typeof(void);

            var key = new MethodKey(methodName, parameterType);

            invokable = null;

            if (_invokableMethods?.TryGetValue(key, out invokable) != true)
            {
                var method = FindMethod(methodName, parameterType);

                if (method == null && parameterType != typeof(void))
                {
                    //Find an overload with no parameters
                    parameterType = typeof(void);

                    //Don't change the key, otherwise repeated calls will keep recreating everything

                    method = FindMethod(methodName, parameterType);
                }

                //TODO: could add this to the dictionary as well to speed up lookup, but since this is a rare case it probably won't be needed
                if (method == null)
                {
                    return false;
                }

                if (_invokableMethods == null)
                {
                    _invokableMethods = new Dictionary<MethodKey, InvokableMethod>();
                }

                invokable = CreateInvoker(method, parameterType);

                _invokableMethods.Add(key, invokable);
            }

            return true;
        }

        internal bool TryGetMethodAndInvoke(string methodName, Component instance, object parameter = null)
        {
            if (TryGetMethod(methodName, parameter, out var invokable))
            {
                invokable.Invoke(instance, parameter);

                return true;
            }

            return false;
        }
    }
}
