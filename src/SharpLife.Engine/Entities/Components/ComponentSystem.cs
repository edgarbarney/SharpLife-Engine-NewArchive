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
using System.Collections.Immutable;

namespace SharpLife.Engine.Entities.Components
{
    public sealed class ComponentSystem
    {
        private const float InvokeOnceInterval = -1;

        private struct InvokeData
        {
            public Component Component;

            public ComponentMetaData.InvokableMethod Method;

            public float InvocationTime;

            public float Interval;
        }

        private readonly ImmutableDictionary<Type, ComponentMetaData> _componentMetaData;

        private readonly List<Component> _allComponents = new List<Component>();

        private readonly List<Component> _activeComponents = new List<Component>();

        private readonly LinkedList<InvokeData> _invokeTargets = new LinkedList<InvokeData>();

        private readonly Scene _scene;

        public ComponentSystem(Scene scene)
        {
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));

            //Cache this off for faster lookup
            _componentMetaData = _scene.EntitySystemMetaData.ComponentMetaData;
        }

        internal void Start()
        {
            foreach (var component in _allComponents)
            {
                component.MetaData.TryGetMethodAndInvoke(BuiltInComponentMethods.Activate, component);
            }
        }

        internal void Update()
        {
            foreach (var component in _activeComponents)
            {
                if (!component._startCalled)
                {
                    component._startCalled = true;
                    component.MetaData.TryGetMethodAndInvoke(BuiltInComponentMethods.Start, component);
                }

                component.MetaData.TryGetMethodAndInvoke(BuiltInComponentMethods.Update, component);
            }

            for (var node = _invokeTargets.First; node != null;)
            {
                var next = node.Next;

                var data = node.Value;

                if (data.InvocationTime <= _scene.Time.ElapsedTime)
                {
                    data.Method(data.Component, null);

                    //The node could've been removed in the invocation
                    if (node.List != null)
                    {
                        if (data.Interval != InvokeOnceInterval)
                        {
                            data.InvocationTime = (float)(_scene.Time.ElapsedTime + data.Interval);
                            node.Value = data;
                        }
                        else
                        {
                            _invokeTargets.Remove(node);
                        }
                    }
                }

                node = next;
            }
        }

        internal ComponentMetaData GetMetaData(Type componentType)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }

            if (_componentMetaData.TryGetValue(componentType, out var metaData))
            {
                return metaData;
            }

            //Should never happen
            throw new InvalidOperationException($"Attempted to get metadata for unknown type {componentType.FullName}");
        }

        internal void OnComponentCreated(Component component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            _allComponents.Add(component);
        }

        internal void OnComponentDestroyed(Component component)
        {
            _allComponents.Remove(component);
        }

        internal void AddComponent(Component component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            _activeComponents.Add(component);
        }

        internal void RemoveComponent(Component component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            _activeComponents.Remove(component);

            CancelInvocations(component);
        }

        private bool TryGetMethod(Component component, string methodName, object parameter, out ComponentMetaData.InvokableMethod invokable)
        {
            if (!component.MetaData.TryGetMethod(methodName, parameter, out invokable))
            {
                //Since this method is a try, don't log failures
                //_scene.Logger.Warning("Method void {ComponentType}.{MethodName}() does not exist", component.GetType().FullName, methodName);
                return false;
            }

            return true;
        }

        internal bool InvokeImmediate(Component component, string methodName, object parameter = null)
        {
            if (!TryGetMethod(component, methodName, parameter, out var invokable))
            {
                return false;
            }

            invokable(component, parameter);

            return true;
        }

        internal bool ScheduleInvocation(Component component, string methodName, float delay, float interval = InvokeOnceInterval)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            //Needed because otherwise we could end up in an infinite loop invoking the same method over and over
            if (delay <= 0.0001f)
            {
                throw new ArgumentOutOfRangeException("Invocation delay must be greater than 0.0001");
            }

            if (interval != InvokeOnceInterval && interval <= 0.0001f)
            {
                throw new ArgumentOutOfRangeException("Invocation interval must be greater than 0.0001");
            }

            if (!TryGetMethod(component, methodName, null, out var method))
            {
                return false;
            }

            var data = new InvokeData
            {
                Component = component,
                Method = method,
                InvocationTime = (float)(_scene.Time.ElapsedTime + delay),
                Interval = interval,
            };

            _invokeTargets.AddLast(data);

            return true;
        }

        internal void CancelInvocations(Component component)
        {
            for (var node = _invokeTargets.First; node != null;)
            {
                var next = node.Next;

                if (ReferenceEquals(node.Value.Component, component))
                {
                    _invokeTargets.Remove(node);
                }

                node = next;
            }
        }

        internal void CancelInvocations(Component component, string methodName)
        {
            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            for (var node = _invokeTargets.First; node != null;)
            {
                var next = node.Next;

                var data = node.Value;

                if (ReferenceEquals(node.Value.Component, component)
                    && data.Method.Method.Name == methodName)
                {
                    _invokeTargets.Remove(node);
                }

                node = next;
            }
        }
    }
}
