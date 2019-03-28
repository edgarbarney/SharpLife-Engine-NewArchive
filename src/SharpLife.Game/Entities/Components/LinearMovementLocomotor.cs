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

using SharpLife.Engine.Entities;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.ObjectEditor;
using SharpLife.Engine.Physics;
using System.Diagnostics;
using System.Numerics;

namespace SharpLife.Game.Entities.Components
{
    /// <summary>
    /// Moves its owning entity from its current location to a given location
    /// </summary>
    public sealed class LinearMovementLocomotor : Component
    {
        public delegate void MoveDoneCallback();

        private Vector3 _finalDestination;

        private MoveDoneCallback _callWhenMoveDone;

        [ObjectEditorVisible(Visible = false)]
        public Transform Transform { get; private set; }

        [ObjectEditorVisible(Visible = false)]
        public Collider Collider { get; private set; }

        public void Initialize()
        {
            Transform = Entity.GetRequiredComponent<Transform>();
            Collider = Entity.GetRequiredComponent<Collider>();
        }

        public void PhysicsUpdate()
        {
            //Will only be called if the think time has been set
            MoveDone();
        }

        /// <summary>
        /// Calculate <see cref="Transform.Velocity"/> and <see cref="Collider.NextThink"/> to reach
        /// <paramref name="destination"/> from <see cref="Transform.Origin"/> traveling at <paramref name="speed"/>
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="speed"></param>
        /// <param name="callback"></param>
        public void MoveTo(in Vector3 destination, float speed, MoveDoneCallback callback = null)
        {
            Debug.Assert(speed != 0, "LinearMove:  no speed is defined!");
            //	Debug.Assert(_callWhenMoveDone != null, "LinearMove: no post-move function defined");

            _callWhenMoveDone = callback;

            _finalDestination = destination;

            // Already there?
            if (destination == Transform.Origin)
            {
                MoveDone();
                return;
            }

            // set destdelta to the vector needed to move
            var vecDestDelta = destination - Transform.Origin;

            // divide vector length by speed to get time to reach dest
            var flTravelTime = vecDestDelta.Length() / speed;

            // set nextthink to trigger a call to LinearMoveDone when dest is reached
            Collider.NextThink = Collider.LastThinkTime + flTravelTime;

            // scale the destdelta vector by the time spent traveling to get velocity
            Transform.Velocity = vecDestDelta / flTravelTime;
        }

        /// <summary>
        /// After moving, set origin to exact final destination, call "move done" function
        /// </summary>
        private void MoveDone()
        {
            var delta = _finalDestination - Transform.Origin;
            var error = delta.Length();
            if (error > 0.03125)
            {
                MoveTo(_finalDestination, 100);
                return;
            }

            Transform.Origin = _finalDestination;
            Transform.Velocity = Vector3.Zero;
            Collider.NextThink = -1;

            _callWhenMoveDone?.Invoke();
            _callWhenMoveDone = null;
        }
    }
}
