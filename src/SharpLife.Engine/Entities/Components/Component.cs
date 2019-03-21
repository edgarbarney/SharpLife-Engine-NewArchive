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

using SharpLife.Engine.ObjectEditor;
using System;

namespace SharpLife.Engine.Entities.Components
{
    /// <summary>
    /// Base class for components
    /// </summary>
    public abstract class Component : IEquatable<Component>
    {
        internal bool _startCalled;

        [ObjectEditorVisible(Visible = false)]
        public ComponentMetaData MetaData { get; }

        /// <summary>
        /// The entity that owns this component
        /// </summary>
        [ObjectEditorVisible(Visible = false)]
        public Entity Entity { get; private set; }

        public bool Enabled { get; private set; }

        protected Component()
        {
            MetaData = EntitySystem.Scene.Components.GetMetaData(GetType());
        }

        private void SetEnabled(bool state, bool forceInvokeHandlers)
        {
            var oldState = Enabled;

            if (oldState != state)
            {
                Enabled = state;

                if (oldState)
                {
                    EntitySystem.Scene.Components.RemoveComponent(this);
                }
                else
                {
                    EntitySystem.Scene.Components.AddComponent(this);
                }
            }

            if (oldState != state || forceInvokeHandlers)
            {
                if (oldState)
                {
                    MetaData.TryGetMethodAndInvoke(BuiltInComponentMethods.OnDisable, this);
                }
                else
                {
                    MetaData.TryGetMethodAndInvoke(BuiltInComponentMethods.OnEnable, this);
                }
            }
        }

        public void SetEnabled(bool state) => SetEnabled(state, false);

        public void Enable() => SetEnabled(true);

        public void Disable() => SetEnabled(false);

        internal void InternalAdded(Entity entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));

            SetEnabled(true, true);
        }

        internal void InternalRemoved()
        {
            SetEnabled(false, true);

            Entity = null;
        }

        public bool Invoke(string methodName) => EntitySystem.Scene.Components.InvokeImmediate(this, methodName);

        public bool Invoke(string methodName, float delay) => EntitySystem.Scene.Components.ScheduleInvocation(this, methodName, delay);

        public bool InvokeRepeating(string methodName, float delay, float interval) => EntitySystem.Scene.Components.ScheduleInvocation(this, methodName, delay, interval);

        public bool InvokeRepeating(string methodName, float delay) => EntitySystem.Scene.Components.ScheduleInvocation(this, methodName, delay, delay);

        public void CancelInvocations() => EntitySystem.Scene.Components.CancelInvocations(this);

        public void CancelInvocations(string methodName) => EntitySystem.Scene.Components.CancelInvocations(this, methodName);

        //Enforce reference equality for components
        public bool Equals(Component other) => ReferenceEquals(this, other);

        public sealed override bool Equals(object obj) => ReferenceEquals(this, obj);

        public sealed override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return $"{GetType().Name}";
        }
    }
}
