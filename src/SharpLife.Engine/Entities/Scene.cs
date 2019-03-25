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

using Serilog;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Entities.Factories;
using SharpLife.Engine.GameWorld;
using SharpLife.Engine.Models;
using SharpLife.Utility;
using SharpLife.Utility.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SharpLife.Engine.Entities
{
    /// <summary>
    /// Represents a single scene containing a world
    /// </summary>
    public sealed class Scene : IDisposable
    {
        public WorldState WorldState { get; }

        public EntitySystemMetaData EntitySystemMetaData { get; }

        public ILogger Logger => WorldState.Logger;

        public EntityCreator EntityCreator { get; }

        public ComponentSystem Components { get; }

        public IModelManager Models { get; }

        public EntityList Entities { get; } = new EntityList();

        public bool Running { get; private set; }

        public bool Active { get; private set; }

        //TODO: should use the same generator used by the original engine
        public Random Random { get; } = new Random();

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

        public Scene(WorldState worldState, EntitySystemMetaData entitySystemMetaData, IModelManager modelManager)
        {
            WorldState = worldState ?? throw new ArgumentNullException(nameof(worldState));
            EntitySystemMetaData = entitySystemMetaData ?? throw new ArgumentNullException(nameof(entitySystemMetaData));
            EntityCreator = new EntityCreator(this);
            Components = new ComponentSystem(this);
            Models = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
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
                EntitySystem.SetScene(this);

                Stop();

                EntitySystem.SetScene(null);
            }
            else if (Active)
            {
                EntitySystem.SetScene(null);
            }

            Models.Dispose();
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

        public void LoadEntities(string entityData)
        {
            var keyvalues = KeyValuesParser.ParseAll(entityData);

            for (var index = 0; index < keyvalues.Count; ++index)
            {
                //Better error handling than the engine: if an entity couldn't be created, log it and keep going
                try
                {
                    LoadEntity(keyvalues[index], index);
                }
                catch (EntityInstantiationException e)
                {
                    Logger.Error(e, $"A problem occurred while creating entity {index}");
                }
            }
        }

        private string GetClassName(List<KeyValuePair<string, string>> block, int index)
        {
            var name = block.Find(kv => kv.Key == "classname");

            if (name.Key == null)
            {
                //The engine only handles this error if there is a classname key that the game doesn't handle
                throw new EntityInstantiationException($"No classname for entity {index}");
            }

            if (string.IsNullOrWhiteSpace(name.Value))
            {
                throw new EntityInstantiationException($"Classname for entity {index} is invalid");
            }

            return name.Value;
        }

        private void LoadEntity(List<KeyValuePair<string, string>> block, int index)
        {
            var className = GetClassName(block, index);

            if (EntityCreator.TryCreateEntity(className, block, out var entity))
            {
                Logger.Information("Spawning entity {ClassName} ({Index})", entity.ClassName, index);
            }
        }
    }
}
