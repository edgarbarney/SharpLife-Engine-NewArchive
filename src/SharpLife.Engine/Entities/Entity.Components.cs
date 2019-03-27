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

using SharpLife.Engine.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpLife.Engine.Entities
{
    public sealed partial class Entity
    {
        private readonly List<Component> _components = new List<Component>();

        public IEnumerable<Component> Components => _components;

        private bool InternalHasComponent(Type componentType) => GetComponent(componentType) != null;

        public bool HasComponent(Type componentType) => InternalHasComponent(componentType ?? throw new ArgumentNullException(nameof(componentType)));

        public bool HasComponent<TComponent>() where TComponent : Component => InternalHasComponent(typeof(TComponent));

        private Component InternalGetComponent(Type componentType, bool checkDerivedTypes = false)
        {
            foreach (var component in _components)
            {
                if (checkDerivedTypes ? componentType.IsInstanceOfType(component) : componentType.Equals(component.GetType()))
                {
                    return component;
                }
            }

            return null;
        }

        public Component GetComponent(Type componentType) => InternalGetComponent(componentType ?? throw new ArgumentNullException(nameof(componentType)));

        public TComponent GetComponent<TComponent>(bool checkDerivedTypes = false) where TComponent : Component => (TComponent)InternalGetComponent(typeof(TComponent), checkDerivedTypes);

        private bool InternalTryGetComponent(Type componentType, out Component component)
        {
            foreach (var candidate in _components)
            {
                if (componentType.Equals(candidate.GetType()))
                {
                    component = candidate;
                    return true;
                }
            }

            component = null;
            return false;
        }

        public bool TryGetComponent(Type componentType, out Component component)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }

            return InternalTryGetComponent(componentType, out component);
        }

        public bool TryGetComponent<TComponent>(out TComponent component)
            where TComponent : Component
        {
            if (InternalTryGetComponent(typeof(TComponent), out var result))
            {
                component = (TComponent)result;
                return true;
            }

            component = null;
            return false;
        }

        public Component[] GetComponents(Type componentType)
        {
            var list = new List<Component>();

            foreach (var component in _components)
            {
                if (componentType.Equals(component.GetType()))
                {
                    list.Add(component);
                }
            }

            return list.ToArray();
        }

        public TComponent[] GetComponents<TComponent>()
            where TComponent : Component
        {
            var list = new List<TComponent>();

            foreach (var component in _components)
            {
                if (typeof(TComponent).Equals(component.GetType()))
                {
                    list.Add((TComponent)component);
                }
            }

            return list.ToArray();
        }

        public List<Component> GetComponents(Type componentType, List<Component> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (var component in _components)
            {
                if (componentType.Equals(component.GetType()))
                {
                    results.Add(component);
                }
            }

            return results;
        }

        public List<TComponent> GetComponents<TComponent>(List<TComponent> results)
            where TComponent : Component
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (var component in _components)
            {
                if (typeof(TComponent).Equals(component.GetType()))
                {
                    results.Add((TComponent)component);
                }
            }

            return results;
        }

        private void InternalAddComponent(Component component)
        {
            _components.Add(component);

            EntitySystem.Scene.Components.OnComponentCreated(component);

            component.InternalAdded(this);

            if (EntitySystem.Scene.Running)
            {
                component.MetaData.TryGetMethodAndInvoke(BuiltInComponentMethods.Activate, component);
            }
        }

        private TComponent InternalAddComponentGeneric<TComponent>()
            where TComponent : Component
        {
            var component = Activator.CreateInstance<TComponent>();

            InternalAddComponent(component);

            return component;
        }

        public Component AddComponent(Type componentType)
        {
            if (Destroyed)
            {
                return null;
            }

            var component = (Component)Activator.CreateInstance(componentType);

            InternalAddComponent(component);

            return component;
        }

        public TComponent AddComponent<TComponent>()
            where TComponent : Component
        {
            if (Destroyed)
            {
                return null;
            }

            return InternalAddComponentGeneric<TComponent>();
        }

        public void AddComponents(IEnumerable<Type> componentTypes)
        {
            if (componentTypes == null)
            {
                throw new ArgumentNullException(nameof(componentTypes));
            }

            foreach (var componentType in componentTypes)
            {
                if (componentType == null)
                {
                    throw new ArgumentNullException(nameof(componentTypes));
                }

                if (!typeof(Component).IsAssignableFrom(componentType))
                {
                    throw new ArgumentException($"The type {componentType.FullName} does not inherit from {typeof(Component).FullName}", nameof(componentTypes));
                }
            }

            if (Destroyed)
            {
                return;
            }

            foreach (var componentType in componentTypes)
            {
                InternalAddComponent((Component)Activator.CreateInstance(componentType));
            }
        }

        public void AddComponents(params Type[] componentTypes)
        {
            if (componentTypes == null)
            {
                throw new ArgumentNullException(nameof(componentTypes));
            }

            AddComponents(componentTypes.AsEnumerable());
        }

        private void InternalCleanupRemovedComponent(Component component)
        {
            EntitySystem.Scene.Components.OnComponentDestroyed(component);

            component.InternalRemoved();
        }

        public void RemoveComponent(Component component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (Destroyed)
            {
                return;
            }

            if (!ReferenceEquals(component.Entity, this))
            {
                throw new InvalidOperationException("Component must be attached to the entity it is being removed from");
            }

            InternalCleanupRemovedComponent(component);

            _components.Remove(component);
        }

        private void InternalRemoveComponents(Type componentType, bool removeDerivedTypes)
        {
            for (var i = 0; i < _components.Count;)
            {
                if (removeDerivedTypes ? componentType.IsAssignableFrom(_components[i].GetType()) : componentType.Equals(_components[i].GetType()))
                {
                    InternalCleanupRemovedComponent(_components[i]);
                    _components.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        public void RemoveComponents(Type componentType, bool removeDerivedTypes = false)
        {
            if (componentType == null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }

            if (Destroyed)
            {
                return;
            }

            InternalRemoveComponents(componentType, removeDerivedTypes);
        }

        public void RemoveComponents<TComponent>(bool removeDerivedTypes = false)
        {
            if (Destroyed)
            {
                return;
            }

            InternalRemoveComponents(typeof(TComponent), removeDerivedTypes);
        }

        public void RemoveAllComponents()
        {
            foreach (var component in _components)
            {
                InternalCleanupRemovedComponent(component);
            }

            _components.Clear();
        }

        public TComponent GetRequiredComponent<TComponent>(bool checkDerivedTypes = false)
            where TComponent : Component
        {
            var component = GetComponent<TComponent>(checkDerivedTypes);

            if (component == null)
            {
                EntitySystem.Scene.Logger.Warning("Missing {ComponentTypeName} component for {ClassName}", typeof(TComponent).FullName, ClassName);
            }

            return component;
        }
    }
}
