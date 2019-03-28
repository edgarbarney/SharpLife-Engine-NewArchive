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
using SharpLife.Utility.Mathematics;
using System;
using System.Numerics;

namespace SharpLife.Engine.Physics
{
    public sealed partial class GameMovement
    {
        private void Physics_None(Collider ent)
        {
            RunThink(ent);
        }

        /*
        private void Physics_Follow(Collider ent)
        {
            if (!RunThink(ent))
            {
                return;
            }

            var aimEntity = _entityList.GetEntity(ent.AimEntity);

            if (aimEntity != null)
            {
                ent.Transform.Origin = aimEntity.Origin + ent.ViewAngle;
                ent.Transform.Angles = aimEntity.Angles;

                _physics.LinkEdict(ent, true);
            }
            else
            {
                _logger.Debug("{0} movetype {1} with null aiment", ent.Entity.ClassName, nameof(MoveType.Follow));
                ent.MoveType = MoveType.None;
            }
        }
        */

        private void Physics_Noclip(Collider ent)
        {
            if (RunThink(ent))
            {
                ent.Transform.Angles += (float)_frameTime * ent.Transform.AngularVelocity;
                ent.Transform.Origin += (float)_frameTime * ent.Transform.Velocity;

                _physics.LinkEdict(ent, false);
            }
        }

        private void Physics_Pusher(Collider ent)
        {
            var oldTime = ent.LastThinkTime;
            var thinkTime = ent.NextThink;

            float frameTime;

            if (thinkTime < _frameTime + oldTime)
            {
                frameTime = thinkTime - oldTime;
            }
            else
            {
                frameTime = (float)_frameTime;
            }

            if (frameTime > 0)
            {
                if (ent.Transform.AngularVelocity != Vector3.Zero)
                {
                    if (ent.Transform.Velocity != Vector3.Zero)
                    {
                        if (PushRotate(ent, frameTime))
                        {
                            var newTime = ent.LastThinkTime;

                            ent.LastThinkTime = oldTime;
                            PushMove(ent, frameTime);

                            if (newTime > ent.LastThinkTime)
                            {
                                ent.LastThinkTime = newTime;
                            }
                        }
                    }
                    else
                    {
                        PushRotate(ent, frameTime);
                    }
                }
                else
                {
                    PushMove(ent, frameTime);
                }
            }

            var angles = ent.Transform.Angles;

            for (int i = 0; i < 3; ++i)
            {
                if (angles.Index(i) < -3600.0 || angles.Index(i) > 3600.0)
                {
                    angles.Index(i, angles.Index(i) % 3600);
                }
            }

            ent.Transform.Angles = angles;

            if (thinkTime > oldTime && ((ent.Entity.Flags & EntityFlags.AlwaysThink) != 0 || ent.LastThinkTime >= thinkTime))
            {
                ent.NextThink = 0;
                _gameTime.ElapsedTime = _engineTime.ElapsedTime;
                ent.Entity.SendMessage(BuiltInComponentMethods.PhysicsUpdate);
            }
        }

        private void WaterMove(Collider pSelf)
        {
            //TODO: needs to be moved to a component
#if false
            if (pSelf.MoveType == MoveType.Noclip)
            {
                pSelf.AirFinished = (float)_engineTime.ElapsedTime + 12.0f;
                return;
            }

            if (pSelf.Health < 0.0)
            {
                return;
            }

            var playerHeadHeight = pSelf.DeadFlag != DeadFlag.No ? WaterLevel.Feet : WaterLevel.Head;

            if ((pSelf.Entity.Flags & (EntityFlags.ImmuneWater | EntityFlags.GodMode)) == 0)
            {
                if ((pSelf.Entity.Flags & EntityFlags.Swim) != 0 || pSelf.WaterLevel >= playerHeadHeight)
                {
                    if (_engineTime.ElapsedTime > pSelf.AirFinished)
                    {
                        if (_engineTime.ElapsedTime > pSelf.PainFinished)
                        {
                            if (pSelf.Damage > 15.0)
                            {
                                pSelf.Damage = 10;
                            }

                            pSelf.PainFinished = (float)_engineTime.ElapsedTime + 1.0f;
                        }
                    }
                }
                else
                {
                    pSelf.AirFinished = (float)_engineTime.ElapsedTime + 12.0f;
                    pSelf.Damage = 2;
                }
            }

            if (pSelf.WaterLevel == WaterLevel.Dry)
            {
                if ((pSelf.Entity.Flags & EntityFlags.InWater) != 0)
                {
                    var v12 = _random.Next(0, 3);
                    if (v12 == 1)
                    {
                        SV_StartSound(0, pSelf, Channel.Body, "player/pl_wade2.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);
                    }
                    else if (v12 == 0)
                    {
                        SV_StartSound(0, pSelf, Channel.Body, "player/pl_wade1.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);
                    }
                    else if (v12 == 2)
                    {
                        SV_StartSound(0, pSelf, Channel.Body, "player/pl_wade3.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);
                    }
                    else if (v12 == 3)
                    {
                        SV_StartSound(0, pSelf, Channel.Body, "player/pl_wade4.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);
                    }
                    pSelf.Entity.Flags &= ~EntityFlags.InWater;
                }

                pSelf.AirFinished = (float)_engineTime.ElapsedTime + 12.0f;
                return;
            }

            if (pSelf.WaterType == Contents.Lava)
            {
                if ((pSelf.Entity.Flags & (EntityFlags.ImmuneLava | EntityFlags.GodMode)) == 0 && _engineTime.ElapsedTime > pSelf.DamageTime)
                {
                    if (_engineTime.ElapsedTime < pSelf.RadSuitFinished)
                    {
                        pSelf.DamageTime = (float)_engineTime.ElapsedTime + 0.2f;
                    }
                    else
                    {
                        pSelf.DamageTime = (float)_engineTime.ElapsedTime + 1.0f;
                    }
                }
            }
            else if (pSelf.WaterType == Contents.Slime)
            {
                if ((pSelf.Entity.Flags & (EntityFlags.ImmuneSlime | EntityFlags.GodMode)) == 0 && _engineTime.ElapsedTime > pSelf.DamageTime)
                {
                    if (_engineTime.ElapsedTime < pSelf.RadSuitFinished)
                    {
                        pSelf.DamageTime = (float)_engineTime.ElapsedTime + 1.0f;
                    }
                }
            }

            if ((pSelf.Entity.Flags & EntityFlags.InWater) == 0)
            {
                if (pSelf.WaterType == Contents.Water)
                {
                    var v18 = _random.Next(0, 3);
                    if (v18 == 1)
                    {
                        SV_StartSound(0, pSelf, Channel.Body, "player/pl_wade2.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);
                    }
                    else if (v18 == 0)
                    {
                        SV_StartSound(0, pSelf, Channel.Body, "player/pl_wade1.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);
                    }
                    else if (v18 == 2)
                    {
                        SV_StartSound(0, pSelf, Channel.Body, "player/pl_wade3.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);
                    }
                    else if (v18 == 3)
                    {
                        SV_StartSound(0, pSelf, Channel.Body, "player/pl_wade4.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);
                    }
                }
                pSelf.Entity.Flags |= EntityFlags.InWater;
                pSelf.DamageTime = 0;
            }

            if ((pSelf.Entity.Flags & EntityFlags.WaterJump) == 0)
            {
                pSelf.Transform.Velocity += (int)pSelf.WaterLevel * -0.8f * (float)_frameTime * pSelf.Transform.Velocity;
            }
#endif
        }

        private int FlyMove(Collider ent, float time)
        {
            Trace? nullTrace = null;

            return FlyMove(ent, time, ref nullTrace);
        }

        private int FlyMove(Collider ent, float time, ref Trace? steptrace)
        {
            var monsterClip = (ent.Entity.Flags & EntityFlags.MonsterClip) != 0;

            int blocked = 0;
            int planeCount = 0;
            var moveTime = time;

            var original_velocity = ent.Transform.Velocity;
            var new_velocity = Vector3.Zero;

            const int MaxPlanes = 5;

            var planes = new Vector3[MaxPlanes];

            for (int iteration = 0; iteration < 4; ++iteration)
            {
                if (ent.Transform.Velocity == Vector3.Zero)
                {
                    return blocked;
                }

                var end = ent.Transform.Origin + (ent.Transform.Velocity * moveTime);

                var trace = _physics.Move(ent.Transform.Origin, ent.Mins, ent.Maxs, end, TraceType.None, ent, false, monsterClip);

                if (trace.AllSolid)
                {
                    ent.Transform.Velocity = Vector3.Zero;
                    return 4;
                }

                if (trace.Fraction > 0)
                {
                    var test = _physics.Move(trace.EndPosition, ent.Mins, ent.Maxs, trace.EndPosition, TraceType.None, ent, false, monsterClip);

                    if (!test.AllSolid)
                    {
                        planeCount = 0;

                        ent.Transform.Origin = trace.EndPosition;
                        original_velocity = ent.Transform.Velocity;
                    }
                }

                if (trace.Fraction == 1.0)
                {
                    return blocked;
                }

                if (trace.Entity == null)
                {
                    throw new InvalidOperationException("FlyMove: trace.Entity == null");
                }

                if (trace.Plane.Normal.Z > 0.7)
                {
                    blocked |= 1;

                    if (trace.Entity.Solid == Solid.BSP || trace.Entity.MoveType == MoveType.PushStep
                        || trace.Entity.Solid == Solid.SlideBox || (ent.Entity.Flags & EntityFlags.Client) != 0)
                    {
                        ent.Entity.Flags |= EntityFlags.OnGround;
                        ent.GroundEntity = trace.Entity;
                    }
                }

                if (trace.Plane.Normal.Z == 0)
                {
                    blocked |= 2;
                    if (steptrace.HasValue)
                    {
                        steptrace = trace;
                    }
                }

                Impact(ent, trace.Entity, trace);

                if (ent.Entity.Destroyed)
                {
                    return blocked;
                }

                if (planeCount > 4)
                {
                    ent.Transform.Velocity = Vector3.Zero;
                    return blocked;
                }

                planes[planeCount++] = trace.Plane.Normal;

                if (planeCount == 1 && ent.MoveType == MoveType.Walk && ((ent.Entity.Flags & EntityFlags.OnGround) == 0 || ent.Friction != 1.0))
                {
                    if (planes[0].Z <= 0.7)
                    {
                        var maxs = ((1.0f - ent.Friction) * _sv_bounce.Value) + 1.0f;
                        ClipVelocity(original_velocity, ref planes[0], out new_velocity, maxs);
                    }
                    else
                    {
                        ClipVelocity(original_velocity, ref planes[0], out new_velocity, 1.0f);
                    }

                    ent.Transform.Velocity = new_velocity;
                    original_velocity = new_velocity;
                }
                else
                {
                    int index;

                    for (index = 0; index < planeCount; ++index)
                    {
                        ClipVelocity(original_velocity, ref planes[index], out new_velocity, 1.0f);

                        int index2;

                        for (index2 = 0; index2 < planeCount; ++index2)
                        {
                            if (index != index2
                                && Vector3.Dot(planes[index2], new_velocity) < 0.0)
                            {
                                break;
                            }
                        }

                        if (index2 == planeCount)
                        {
                            break;
                        }
                    }

                    if (index == planeCount)
                    {
                        if (planeCount != 2)
                        {
                            return blocked;
                        }

                        var dir = Vector3.Cross(planes[0], planes[1]);
                        var start = Vector3.Dot(dir, ent.Transform.Velocity);
                        ent.Transform.Velocity = dir * start;
                    }
                    else
                    {
                        ent.Transform.Velocity = new_velocity;
                    }

                    if (Vector3.Dot(original_velocity, ent.Transform.Velocity) <= 0.0)
                    {
                        ent.Transform.Velocity = Vector3.Zero;
                        return blocked;
                    }
                }

                moveTime -= trace.Fraction * moveTime;
            }

            return blocked;
        }

        private void Physics_Step(Collider ent)
        {
            WaterMove(ent);
            CheckVelocity(ent);

            var inWater = CheckWater(ent);

            if ((ent.Entity.Flags & EntityFlags.Float) != 0 && ent.WaterLevel > WaterLevel.Dry)
            {
                var waterLevel = Submerged(ent) * ent.Buoyancy * (float)_frameTime;

                var entGravity = ent.Gravity != 0.0 ? ent.Gravity : 1.0f;

                waterLevel = ent.Transform.Velocity.Z - (entGravity * _sv_gravity.Value * (float)_frameTime);
                ent.Transform.Velocity.Z = waterLevel + ((float)_frameTime * ent.Transform.BaseVelocity.Z);
                ent.Transform.BaseVelocity.Z = 0;

                CheckVelocity(ent);

                ent.Transform.Velocity.Z += waterLevel;
            }

            var wasOnGround = (ent.Entity.Flags & EntityFlags.OnGround) != 0;

            if (!wasOnGround
                && (ent.Entity.Flags & EntityFlags.Fly) == 0
                && ((ent.Entity.Flags & EntityFlags.Swim) == 0 || ent.WaterLevel <= WaterLevel.Dry)
                && !inWater)
            {
                var entGravity = ent.Gravity != 0.0 ? ent.Gravity : 1.0f;

                var waterLevel = ent.Transform.Velocity.Z - (entGravity * _sv_gravity.Value * (float)_frameTime);
                ent.Transform.Velocity.Z = waterLevel + ((float)_frameTime * ent.Transform.BaseVelocity.Z);
                ent.Transform.BaseVelocity.Z = 0;

                CheckVelocity(ent);
            }

            if (VectorUtils.VectorsEqual(ent.Transform.Velocity, Vector3.Zero)
                && VectorUtils.VectorsEqual(ent.Transform.BaseVelocity, Vector3.Zero))
            {
                if (ForceRetouch != 0)
                {
                    var trace = _physics.Move(
                        ent.Transform.Origin,
                        ent.Mins,
                        ent.Maxs,
                        ent.Transform.Origin,
                        TraceType.None,
                        ent,
                        false,
                        (ent.Entity.Flags & EntityFlags.MonsterClip) != 0);

                    if (trace.Fraction < 1.0 || trace.StartSolid)
                    {
                        if (trace.Entity != null)
                        {
                            Impact(ent, trace.Entity, trace);
                        }
                    }
                }
            }
            else
            {
                ent.Entity.Flags &= ~EntityFlags.OnGround;

                //TODO: this entire method needs to be moved to a component
                if (wasOnGround
                    && (/*ent.Health > 0.0 ||*/ CheckBottom(ent)))
                {
                    var length2D = new Vector2(ent.Transform.Velocity.X, ent.Transform.Velocity.Y).Length();

                    if (length2D != 0)
                    {
                        ent.Friction = 1;

                        var stopSpeed = Math.Max(_sv_stopspeed.Value, length2D);

                        var speed = length2D - (_sv_friction.Value * ent.Friction * (stopSpeed * (float)_frameTime));

                        if (speed < 0)
                            speed = 0;

                        speed /= length2D;

                        ent.Transform.Velocity.X *= speed;
                        ent.Transform.Velocity.Y *= speed;
                    }
                }

                ent.Transform.Velocity += ent.Transform.BaseVelocity;

                CheckVelocity(ent);
                FlyMove(ent, (float)_frameTime);
                CheckVelocity(ent);

                ent.Transform.Velocity -= ent.Transform.BaseVelocity;

                CheckVelocity(ent);

                if (IsOnGround(ent))
                {
                    ent.Entity.Flags |= EntityFlags.OnGround;
                }

                _physics.LinkEdict(ent, true);
            }

            RunThink(ent);
            CheckWaterTransition(ent);
        }

        private void Physics_Toss(Collider ent)
        {
            CheckWater(ent);

            if (!RunThink(ent))
            {
                return;
            }

            if (ent.Transform.Velocity.Z > 0.0 || ent.GroundEntity == null || (ent.GroundEntity.Entity.Flags & (EntityFlags.Monster | EntityFlags.Client)) != 0)
            {
                ent.Entity.Flags &= ~EntityFlags.OnGround;
            }

            if ((ent.Entity.Flags & EntityFlags.OnGround) != 0 && VectorUtils.VectorsEqual(ent.Transform.Velocity, Vector3.Zero))
            {
                ent.Transform.AngularVelocity = Vector3.Zero;

                if (VectorUtils.VectorsEqual(ent.Transform.BaseVelocity, Vector3.Zero))
                    return;
            }

            CheckVelocity(ent);

            if (ent.MoveType != MoveType.BounceMissile
                && ent.MoveType != MoveType.Fly
                && ent.MoveType != MoveType.FlyMissile)
            {
                var entGravity = ent.Gravity != 0 ? ent.Gravity : 1;

                var newVelocityZ = ent.Transform.Velocity.Z - (entGravity * _sv_gravity.Value * (float)_frameTime);
                ent.Transform.Velocity.Z = newVelocityZ + ((float)_frameTime * ent.Transform.BaseVelocity.Z);
                ent.Transform.BaseVelocity.Z = 0;
                CheckVelocity(ent);
            }

            ent.Transform.Angles += (float)_frameTime * ent.Transform.AngularVelocity;

            ent.Transform.Velocity += ent.Transform.BaseVelocity;

            CheckVelocity(ent);

            var move = ent.Transform.Velocity * (float)_frameTime;

            ent.Transform.Velocity -= ent.Transform.BaseVelocity;

            var trace = PushEntity(ent, move);

            CheckVelocity(ent);

            if (trace.AllSolid)
            {
                ent.Transform.Velocity = Vector3.Zero;
                ent.Transform.AngularVelocity = Vector3.Zero;
                return;
            }

            if (trace.Fraction != 1.0)
            {
                if (ent.Entity.Destroyed)
                {
                    return;
                }

                float vecc;

                if (ent.MoveType == MoveType.Bounce)
                {
                    vecc = 2.0f - ent.Friction;
                }
                else if (ent.MoveType == MoveType.BounceMissile)
                {
                    vecc = 2.0f;
                }
                else
                {
                    vecc = 1.0f;
                }

                ClipVelocity(ent.Transform.Velocity, ref trace.Plane.Normal, out ent.Transform.Velocity, vecc);

                if (trace.Plane.Normal.Z > 0.7)
                {
                    move = ent.Transform.Velocity + ent.Transform.BaseVelocity;

                    if ((float)_frameTime * _sv_gravity.Value > move.Z)
                    {
                        ent.Entity.Flags |= EntityFlags.OnGround;
                        ent.Transform.Velocity.Z = 0;
                        ent.GroundEntity = trace.Entity;
                    }

                    if (move.LengthSquared() >= 900.0
                        && (ent.MoveType == MoveType.Bounce || ent.MoveType == MoveType.BounceMissile))
                    {
                        move = ent.Transform.Velocity * ((float)_frameTime * (1.0f - trace.Fraction) * 0.9f);

                        move += (1.0f - trace.Fraction) * (float)_frameTime * 0.9f * ent.Transform.BaseVelocity;

                        trace = PushEntity(ent, move);
                    }
                    else
                    {
                        ent.Entity.Flags |= EntityFlags.OnGround;
                        ent.GroundEntity = trace.Entity;

                        ent.Transform.Velocity = Vector3.Zero;
                        ent.Transform.AngularVelocity = Vector3.Zero;
                    }
                }

                CheckWaterTransition(ent);
            }
        }

        public void RunPhysics(double frameTime)
        {
            _frameTime = frameTime;

            foreach (var pEntity in _scene.Entities.EnumerateAll())
            {
                var collider = pEntity.GetComponent<Collider>();

                if (collider == null)
                {
                    continue;
                }

                var transform = collider.Transform;

                if (ForceRetouch != 0)
                {
                    _physics.LinkEdict(collider, true);
                }

                //Don't run for players
                //TODO: update entity list to properly assign indices to match
                //TODO: reimplement
#if false
                if (pEntity.Id != 0 && pEntity.Id <= 32/*_serverClients.MaxClients*/)
                {
                    continue;
                }
#endif

                if ((pEntity.Flags & EntityFlags.OnGround) != 0)
                {
                    if (collider.GroundEntity != null
                        && (collider.GroundEntity.Entity.Flags & EntityFlags.Conveyor) != 0)
                    {
                        if ((pEntity.Flags & EntityFlags.BaseVelocity) != 0)
                        {
                            transform.BaseVelocity += collider.GroundEntity.Transform.Speed * collider.GroundEntity.Transform.MoveDirection;
                        }
                        else
                        {
                            transform.BaseVelocity = collider.GroundEntity.Transform.Speed * collider.GroundEntity.Transform.MoveDirection;
                        }

                        pEntity.Flags |= EntityFlags.BaseVelocity;
                    }
                }

                if ((pEntity.Flags & EntityFlags.BaseVelocity) == 0)
                {
                    var scale = (0.5f * (float)_frameTime) + 1.0f;

                    transform.Velocity += scale * transform.BaseVelocity;

                    transform.BaseVelocity = Vector3.Zero;
                }

                pEntity.Flags &= ~EntityFlags.BaseVelocity;

                switch (collider.MoveType)
                {
                    case MoveType.None:
                        Physics_None(collider);
                        break;

                    /*
                case MoveType.Follow:
                    Physics_Follow(collider);
                    break;
                    */

                    case MoveType.Noclip:
                        Physics_Noclip(collider);
                        break;

                    case MoveType.Push:
                        Physics_Pusher(collider);
                        break;

                    case MoveType.Step:
                    case MoveType.PushStep:
                        Physics_Step(collider);
                        break;

                    case MoveType.Bounce:
                    case MoveType.Toss:
                    case MoveType.BounceMissile:
                    case MoveType.Fly:
                    case MoveType.FlyMissile:
                        Physics_Toss(collider);
                        break;

                    default:
                        throw new InvalidOperationException($"SV_Physics: {pEntity.ClassName} bad movetype {collider.MoveType}");
                }
            }

            if (ForceRetouch != 0)
            {
                --ForceRetouch;
            }
        }
    }
}
