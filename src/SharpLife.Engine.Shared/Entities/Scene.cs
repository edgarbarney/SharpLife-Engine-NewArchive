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

using SharpLife.Engine.Shared.Entities.Components;
using SharpLife.Utility;
using System;
using System.Diagnostics;

namespace SharpLife.Engine.Shared.Entities
{
    /// <summary>
    /// Represents a single scene containing a world
    /// </summary>
    public sealed class Scene : IDisposable
    {
        public EntitySystemMetaData EntitySystemMetaData { get; }

        public ComponentSystem Components { get; }

        public EntityList Entities { get; } = new EntityList();

        public bool Running { get; private set; }

        public bool Active { get; private set; }

        /// <summary>
        /// The game time instance associated with this scene
        /// </summary>
        public SnapshotTime Time { get; } = new SnapshotTime();

        /// <summary>
        /// Invoked when a scene is activated
        /// </summary>
        public event Action<Scene> SceneActivated;

        /// <summary>
        /// Invoked when a scene is deactivated
        /// </summary>
        public event Action<Scene> SceneDeactivated;

        public Scene(EntitySystemMetaData entitySystemMetaData)
        {
            EntitySystemMetaData = entitySystemMetaData ?? throw new ArgumentNullException(nameof(entitySystemMetaData));
            Components = new ComponentSystem(this);
        }

        internal void Activate()
        {
            Active = true;
            SceneActivated?.Invoke(this);
        }

        internal void Deactivate()
        {
            Active = false;
            SceneDeactivated?.Invoke(this);
        }

        private void AssertActive()
        {
            Debug.Assert(Active, "The current scene is not me");
        }

        public void Dispose()
        {
            if (Running)
            {
                Activate();

                Stop();

                Deactivate();
            }
            else if (Active)
            {
                Deactivate();
            }
        }

        public void Start()
        {
            AssertActive();

            Running = true;

            Components.Start();
        }

        public void Stop()
        {
            AssertActive();

            foreach (var entity in Entities.EnumerateAll())
            {
                entity.Destroy();
            }

            Running = false;
        }

        public void Update(double currentTime)
        {
            AssertActive();

            //TODO: set frametime
            Time.ElapsedTime = (float)currentTime;

            Components.Update();
        }

        public Entity CreateEntity() => Entities.CreateEntity();

        internal void DestroyEntity(Entity entity) => Entities.DestroyEntity(entity);
    }
}
