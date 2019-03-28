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
using SharpLife.CommandSystem;
using SharpLife.CommandSystem.Commands;
using SharpLife.CommandSystem.Commands.VariableFilters;
using SharpLife.Engine.Audio;
using SharpLife.Engine.Entities;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Models.BSP.FileFormat;
using SharpLife.Utility;
using SharpLife.Utility.Mathematics;
using System;
using System.Numerics;

namespace SharpLife.Engine.Physics
{
    public sealed partial class GameMovement
    {
        private struct MoveCache
        {
            public Collider Entity;
            public Vector3 Origin;
        }

        private readonly ILogger _logger;

        private readonly Scene _scene;

        private readonly ITime _engineTime;

        private readonly SnapshotTime _gameTime;

        private readonly Random _random;

        private readonly GamePhysics _physics;

        //TODO: create
        private readonly IVariable<float> _sv_maxvelocity;

        private readonly IVariable<float> _sv_stepsize;

        private readonly IVariable<float> _sv_bounce;

        private readonly IVariable<float> _sv_gravity;

        private readonly IVariable<float> _sv_stopspeed;

        private readonly IVariable<float> _sv_friction;

        //Tracked separately from engine frametime to allow independent updating of physics
        private double _frameTime;

        private Trace _currentTrace;

        public ref Trace CurrentTrace => ref _currentTrace;

        public int ForceRetouch { get; set; }

        private MoveCache[] _moveCache = new MoveCache[0];

        public GameMovement(ILogger logger,
            Scene scene,
            ITime engineTime, SnapshotTime gameTime,
            Random random,
            GamePhysics physics,
            ICommandContext commandContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
            _engineTime = engineTime ?? throw new ArgumentNullException(nameof(engineTime));
            _gameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _physics = physics ?? throw new ArgumentNullException(nameof(physics));

            //TODO: add a filter to enforce positive values only
            _sv_maxvelocity = commandContext.RegisterVariable(
                new VirtualVariableInfo<float>("sv_maxvelocity", 2000)
                .WithHelpInfo("The maximum velocity in any axis that an entity can have. Any velocity greater than this is clamped to this value")
                .ConfigureFilters(filters => filters.WithNumberSignFilter(true)));

            //TODO: mark as server cvar
            _sv_stepsize = commandContext.RegisterVariable(
                new VirtualVariableInfo<float>("sv_stepsize", 18)
                .WithHelpInfo("Defines the maximum height that characters can still step over (e.g. stairs)")
                .ConfigureFilters(filters => filters.WithNumberSignFilter(true)));

            //TODO: mark as server cvar
            _sv_bounce = commandContext.RegisterVariable(
                new VirtualVariableInfo<float>("sv_bounce")
                .WithHelpInfo("Multiplier for physics bounce effect when objects collide with other objects")
                .ConfigureFilters(filters => filters.WithNumberSignFilter(true)));

            //TODO: mark as server cvar
            _sv_gravity = commandContext.RegisterVariable(
                new VirtualVariableInfo<float>("sv_gravity", 800)
                .WithHelpInfo("The world's gravity amount, in units per second"));

            //TODO: mark as server cvar
            _sv_stopspeed = commandContext.RegisterVariable(
                new VirtualVariableInfo<float>("sv_stopspeed", 100)
                .WithHelpInfo("Minimum stopping speed when on the ground")
                .ConfigureFilters(filters => filters.WithNumberSignFilter(true)));

            //TODO: mark as server cvar
            _sv_friction = commandContext.RegisterVariable(
                new VirtualVariableInfo<float>("sv_friction", 4)
                .WithHelpInfo("World friction")
                .ConfigureFilters(filters => filters.WithNumberSignFilter(true)));
        }

        private void SetGlobalTrace(in Trace trace)
        {
            _currentTrace = trace;

            //Convert null entities to the world
            _currentTrace.Entity = _currentTrace.Entity ?? _scene.World;
        }

        private void EnsureMoveCacheCapacity()
        {
            if (_moveCache.Length < _scene.Entities.Count)
            {
                _moveCache = new MoveCache[_scene.Entities.Count];
            }
        }

        /// <summary>
        /// Clears all references to entities in the cache
        /// </summary>
        private void ClearMoveCache()
        {
            Array.Clear(_moveCache, 0, _moveCache.Length);
        }

        private void CheckVelocity(Collider ent)
        {
            var velocity = ent.Transform.Velocity;
            var origin = ent.Transform.Origin;

            for (int i = 0; i < 3; ++i)
            {
                if (float.IsNaN(velocity.Index(i)))
                {
                    _logger.Information("Got a NaN velocity on {0}", ent.Entity.ClassName);
                    velocity.Index(i, 0);
                }

                if (float.IsNaN(origin.Index(i)))
                {
                    _logger.Information("Got a NaN origin on {0}", ent.Entity.ClassName);
                    origin.Index(i, 0);
                }

                if (velocity.Index(i) > _sv_maxvelocity.Value)
                {
                    _logger.Debug("Got a velocity too high on {0}", ent.Entity.ClassName);
                    velocity.Index(i, _sv_maxvelocity.Value);
                }
                else if (-_sv_maxvelocity.Value > velocity.Index(i))
                {
                    _logger.Debug("Got a velocity too low on {0}", ent.Entity.ClassName);
                    velocity.Index(i, -_sv_maxvelocity.Value);
                }
            }

            ent.Transform.Velocity = velocity;
            ent.Transform.Origin = origin;
        }

        private byte ClipVelocity(Vector3 input, ref Vector3 normal, out Vector3 output, float overbounce)
        {
            output = new Vector3();

            byte result = 0;

            if (normal.Z > 0.0)
            {
                result |= 1;
            }

            if (normal.Z == 0.0)
            {
                result |= 2;
            }

            var dot = Vector3.Dot(input, normal) * overbounce;

            for (int i = 0; i < 3; ++i)
            {
                var value = input.Index(i) - (normal.Index(i) * dot);

                output.Index(i, value);

                if (value > -0.1 && value < 0.1)
                {
                    output.Index(i, 0);
                }
            }

            return result;
        }

        private Collider TestEntityPosition(Collider ent)
        {
            var trace = _physics.Move(ent.Transform.Origin, ent.Mins, ent.Maxs, ent.Transform.Origin, TraceType.None, ent, false, false);

            if (trace.StartSolid)
            {
                SetGlobalTrace(trace);
                return trace.Entity;
            }

            return null;
        }

        private Trace PushEntity(Collider ent, in Vector3 push)
        {
            var end = ent.Transform.Origin + push;

            var type = TraceType.Missile;

            if (ent.MoveType != MoveType.FlyMissile)
            {
                type = ent.Solid <= Solid.Trigger ? TraceType.IgnoreMonsters : TraceType.None;
            }

            var trace = _physics.Move(ent.Transform.Origin, ent.Mins, ent.Maxs, end, type, ent, false, (ent.Entity.Flags & EntityFlags.MonsterClip) != 0);

            if (trace.Fraction != 0.0)
            {
                ent.Transform.Origin = trace.EndPosition;
            }

            _physics.LinkEdict(ent, true);

            if (trace.Entity != null)
            {
                Impact(ent, trace.Entity, trace);
            }

            return trace;
        }

        private bool RunThink(Collider ent)
        {
            if (!ent.Entity.Destroyed)
            {
                var thinkTime = ent.NextThink;

                if (0.0 < thinkTime && thinkTime < _frameTime + _engineTime.ElapsedTime)
                {
                    if (thinkTime < _engineTime.ElapsedTime)
                    {
                        thinkTime = (float)_engineTime.ElapsedTime;
                    }

                    ent.NextThink = 0;
                    _gameTime.ElapsedTime = thinkTime;
                    ent.Entity.SendMessage(BuiltInComponentMethods.PhysicsUpdate);
                }
            }

            return !ent.Entity.Destroyed;
        }

        private void Impact(Collider e1, Collider e2, in Trace ptrace)
        {
            _gameTime.ElapsedTime = _engineTime.ElapsedTime;

            if (!e1.Entity.Destroyed && !e2.Entity.Destroyed)
            {
                if (e1.GroupInfo != 0
                    && e2.GroupInfo != 0
                    && !_physics.TestGroupOperation(e1.GroupInfo, e2.GroupInfo))
                {
                    return;
                }

                if (e1.Solid != Solid.Not)
                {
                    SetGlobalTrace(ptrace);
                    e1.Entity.SendMessage(BuiltInComponentMethods.Touch, e2);
                }

                if (e2.Solid != Solid.Not)
                {
                    SetGlobalTrace(ptrace);
                    e2.Entity.SendMessage(BuiltInComponentMethods.Touch, e1);
                }
            }
        }

        //TODO: implement (not here)
        private void SV_StartSound(int recipients, Collider entity, Channel channel, string sample, float volume, float attenuation, int fFlags, int pitch)
        {
        }

        private bool CheckWater(Collider ent)
        {
            ent.WaterLevel = WaterLevel.Dry;
            ent.WaterType = Contents.Empty;

            _physics.GroupMask = ent.GroupInfo;

            var point = new Vector3(
                0.5f * (ent.AbsMin.X + ent.AbsMax.X),
                0.5f * (ent.AbsMin.Y + ent.AbsMax.Y),
                ent.AbsMin.Z + 1.0f
            );

            var contents = _physics.PointContents(ref point);

            if (contents <= Contents.Water)
            {
                ent.WaterType = contents;
                ent.WaterLevel = WaterLevel.Feet;

                if (ent.AbsMin.Z == ent.AbsMax.Z)
                {
                    ent.WaterLevel = WaterLevel.Head;
                }
                else
                {
                    point.Z = (ent.AbsMax.Z + ent.AbsMin.Z) * 0.5f;

                    if (_physics.PointContents(ref point) <= Contents.Water)
                    {
                        ent.WaterLevel = WaterLevel.Waist;

                        point += ent.Transform.ViewOffset;

                        if (_physics.PointContents(ref point) <= Contents.Water)
                        {
                            ent.WaterLevel = WaterLevel.Head;
                        }
                    }
                }

                if (contents <= Contents.Current0)
                {
                    ent.Transform.BaseVelocity += (int)ent.WaterLevel * 50.0f * PhysicsConstants.CurrentTable[Contents.Current0 - contents];
                }
            }

            return ent.WaterLevel > WaterLevel.Feet;
        }

        private void CheckWaterTransition(Collider ent)
        {
            _physics.GroupMask = ent.GroupInfo;

            var point = new Vector3(
                (ent.AbsMin.X + ent.AbsMax.X) * 0.5f,
                (ent.AbsMin.Y + ent.AbsMax.Y) * 0.5f,
                ent.AbsMin.Z + 1.0f
            );

            //TODO: this code and the SV_CheckWater function are very similar, and probably in the player physics code
            var contents = _physics.PointContents(ref point);

            if (ent.WaterType != Contents.Node)
            {
                if (contents <= Contents.Water)
                {
                    if (ent.WaterType != Contents.Empty)
                    {
                        SV_StartSound(0, ent, Channel.Auto, "player/pl_wade2.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);
                    }

                    ent.WaterType = Contents.Empty;
                    ent.WaterLevel = WaterLevel.Dry;
                }
                else
                {
                    if (ent.WaterType == Contents.Empty)
                    {
                        SV_StartSound(0, ent, Channel.Auto, "player/pl_wade1.wav", Volume.Normal, Attenuation.Normal, 0, Pitch.Normal);

                        var velocity = ent.Transform.Velocity;
                        velocity.Z *= 0.5f;
                        ent.Transform.Velocity = velocity;
                    }

                    ent.WaterType = contents;
                    ent.WaterLevel = WaterLevel.Feet;

                    if (ent.AbsMin.Z == ent.AbsMax.Z)
                    {
                        ent.WaterLevel = WaterLevel.Head;
                    }
                    else
                    {
                        point.Z = (ent.AbsMax.Z + ent.AbsMin.Z) * 0.5f;

                        if (_physics.PointContents(ref point) <= Contents.Water)
                        {
                            ent.WaterLevel = WaterLevel.Waist;

                            point += ent.Transform.ViewOffset;

                            if (_physics.PointContents(ref point) <= Contents.Water)
                            {
                                ent.WaterLevel = WaterLevel.Head;
                            }
                        }
                    }
                }
            }
            else
            {
                ent.WaterType = contents;
                ent.WaterLevel = WaterLevel.Feet;
            }
        }

        private float RecursiveWaterLevel(in Vector3 center, float output, float input, int count)
        {
            var offset = ((output - input) * 0.5f) + input;

            if (count > 4)
            {
                return offset;
            }

            var test = center;
            test.Z += offset;

            if (_physics.PointContents(ref test) == Contents.Water)
            {
                return RecursiveWaterLevel(center, output, offset, count + 1);
            }
            else
            {
                return RecursiveWaterLevel(center, offset, input, count + 1);
            }
        }

        private float Submerged(Collider ent)
        {
            var center = (ent.AbsMin + ent.AbsMax) * 0.5f;

            var bottom = ent.AbsMin.Z - center.Z;

            if (ent.WaterLevel != WaterLevel.Waist)
            {
                if (ent.WaterLevel != WaterLevel.Head)
                {
                    if (ent.WaterLevel != WaterLevel.Feet)
                    {
                        return 0;
                    }

                    return RecursiveWaterLevel(center, 0.0f, bottom, 0) - bottom;
                }

                _physics.GroupMask = ent.GroupInfo;

                var test = new Vector3(center.X, center.Y, ent.AbsMax.Z);

                if (_physics.PointContents(ref test) == Contents.Water)
                {
                    return ent.Maxs.Z - ent.Mins.Z;
                }
            }

            var top = ent.AbsMax.Z - center.Z;
            var halfTop = top * 0.5f;

            var point = new Vector3(
                center.X,
                center.Y,
                center.Z + halfTop
            );

            float waterLevel;

            if (_physics.PointContents(ref point) == Contents.Water)
            {
                waterLevel = RecursiveWaterLevel(center, top, halfTop, 1);
            }
            else
            {
                waterLevel = RecursiveWaterLevel(center, halfTop, 0.0f, 1);
            }

            return waterLevel - bottom;
        }

        private bool PushRotate(Collider pusher, float movetime)
        {
            if (pusher.Transform.AngularVelocity == Vector3.Zero)
            {
                pusher.LastThinkTime += movetime;
                return true;
            }

            var aVelocity = pusher.Transform.AngularVelocity * movetime;

            VectorUtils.AngleToVectors(pusher.Transform.Angles, out var forwardNow, out var rightNow, out var upNow);

            var savedAngles = pusher.Transform.Angles;

            pusher.Transform.Angles += aVelocity;

            VectorUtils.AngleToVectorsTranspose(pusher.Transform.Angles, out var forward, out var right, out var up);

            pusher.LastThinkTime += movetime;

            _physics.LinkEdict(pusher, false);

            if (pusher.Solid == Solid.Not)
            {
                return true;
            }

            EnsureMoveCacheCapacity();

            int num_moved = 0;

            foreach (var entity in _scene.Entities.EnumerateAll())
            {
                //Don't check against the world
                if (entity.IsWorld())
                {
                    continue;
                }

                var check = entity.GetComponent<Collider>();

                if (check == null)
                {
                    continue;
                }

                if (check.MoveType == MoveType.None
                    || check.MoveType == MoveType.Push
                    /*|| check.MoveType == MoveType.Follow*/
                    || check.MoveType == MoveType.Noclip)
                {
                    continue;
                }

                if ((entity.Flags & EntityFlags.OnGround) == 0 || check.GroundEntity != pusher)
                {
                    if (check.AbsMin.X >= pusher.AbsMax.X
                        || check.AbsMin.Y >= pusher.AbsMax.Y
                        || check.AbsMin.Z >= pusher.AbsMax.Z
                        || pusher.AbsMin.X >= check.AbsMax.X
                        || pusher.AbsMin.Y >= check.AbsMax.Y
                        || pusher.AbsMin.Z >= check.AbsMax.Z)
                    {
                        continue;
                    }

                    if (TestEntityPosition(check) == null)
                    {
                        continue;
                    }
                }

                if (check.MoveType != MoveType.Walk)
                {
                    entity.Flags &= ~EntityFlags.OnGround;
                }

                var savedOrigin = check.Transform.Origin;

                _moveCache[num_moved].Origin = check.Transform.Origin;
                _moveCache[num_moved].Entity = check;

                ++num_moved;

                if (num_moved >= _moveCache.Length)
                {
                    throw new InvalidOperationException("Out of edicts in simulator!");
                }

                Vector3 distance;

                if (check.MoveType == MoveType.PushStep)
                {
                    distance = ((check.AbsMin + check.AbsMax) * 0.5f) - pusher.Transform.Origin;
                }
                else
                {
                    distance = check.Transform.Origin - pusher.Transform.Origin;
                }

                pusher.Solid = Solid.Not;

                var mod = new Vector3(
                    Vector3.Dot(forwardNow, distance),
                    -Vector3.Dot(rightNow, distance),
                    Vector3.Dot(upNow, distance)
                );

                var move = new Vector3(
                    Vector3.Dot(mod, forward) - distance.X,
                    Vector3.Dot(mod, right) - distance.Y,
                    Vector3.Dot(mod, up) - distance.Z
                );

                var trace = PushEntity(check, move);

                pusher.Solid = Solid.BSP;

                if (check.MoveType != MoveType.PushStep)
                {
                    if ((entity.Flags & EntityFlags.Client) != 0)
                    {
                        check.Transform.FixAngle = FixAngleMode.AddAVelocity;

                        var avel = check.Transform.AngularVelocity;
                        avel.Y += aVelocity.Y;
                        check.Transform.AngularVelocity = avel;
                    }
                    else
                    {
                        check.Transform.Angles += new Vector3(0, aVelocity.Y, 0);
                    }
                }

                if (TestEntityPosition(check) != null
                    && check.Mins.X != check.Maxs.X)
                {
                    if (check.Solid != Solid.Not
                        && check.Solid != Solid.Trigger)
                    {
                        check.Transform.Origin = savedOrigin;

                        _physics.LinkEdict(check, true);

                        pusher.Transform.Angles = savedAngles;

                        _physics.LinkEdict(pusher, false);

                        pusher.LastThinkTime -= movetime;

                        pusher.Entity.SendMessage(BuiltInComponentMethods.Blocked, check);

                        for (int i = 0; i < num_moved; ++i)
                        {
                            var pMoved = _moveCache[i].Entity;

                            pMoved.Transform.Origin = _moveCache[i].Origin;

                            if ((pMoved.Entity.Flags & EntityFlags.Client) != 0)
                            {
                                var avel = pMoved.Transform.AngularVelocity;
                                avel.Y = 0;
                                pMoved.Transform.AngularVelocity = avel;
                            }
                            else if (pMoved.MoveType != MoveType.PushStep)
                            {
                                var angles = pMoved.Transform.Angles;
                                angles.Y -= aVelocity.Y;
                                pMoved.Transform.Angles = angles;
                            }

                            _physics.LinkEdict(pMoved, false);
                        }

                        ClearMoveCache();

                        return false;
                    }
                    else
                    {
                        var mins = check.Mins;
                        var maxs = check.Maxs;

                        mins.X = 0;
                        maxs.X = 0;
                        mins.Y = 0;
                        maxs.Y = 0;

                        check.Mins = mins;
                        check.Maxs = maxs;
                    }
                }
            }

            ClearMoveCache();

            return true;
        }

        private void PushMove(Collider pusher, float movetime)
        {
            if (pusher.Transform.Velocity == Vector3.Zero)
            {
                pusher.LastThinkTime += movetime;
                return;
            }

            var savedOrigin = pusher.Transform.Origin;

            var move = pusher.Transform.Velocity * movetime;

            var mins = pusher.AbsMin + move;
            var maxs = pusher.AbsMax + move;

            pusher.Transform.Origin = savedOrigin + move;

            pusher.LastThinkTime += movetime;

            _physics.LinkEdict(pusher, false);

            if (pusher.Solid != Solid.Not)
            {
                int num_moved = 0;

                foreach (var entity in _scene.Entities.EnumerateAll())
                {
                    //Don't check against the world
                    if (entity.IsWorld())
                    {
                        continue;
                    }

                    var check = entity.GetComponent<Collider>();

                    if (check == null)
                    {
                        continue;
                    }

                    if (check.MoveType == MoveType.None
                        || check.MoveType == MoveType.Push
                        /*|| check.MoveType == MoveType.Follow*/
                        || check.MoveType == MoveType.Noclip)
                    {
                        continue;
                    }

                    if ((entity.Flags & EntityFlags.OnGround) == 0 || check.GroundEntity != pusher)
                    {
                        if (check.AbsMin.X >= maxs.X
                            || check.AbsMin.Y >= maxs.Y
                            || check.AbsMin.Z >= maxs.Z
                            || mins.X >= check.AbsMax.X
                            || mins.Y >= check.AbsMax.Y
                            || mins.Z >= check.AbsMax.Z)
                        {
                            continue;
                        }

                        if (TestEntityPosition(check) == null)
                        {
                            continue;
                        }
                    }

                    if (check.MoveType != MoveType.Walk)
                        entity.Flags &= ~EntityFlags.OnGround;

                    var savedBlockOrigin = check.Transform.Origin;

                    _moveCache[num_moved].Origin = check.Transform.Origin;
                    _moveCache[num_moved].Entity = check;

                    ++num_moved;

                    pusher.Solid = Solid.Not;

                    PushEntity(check, move);

                    pusher.Solid = Solid.BSP;

                    if (TestEntityPosition(check) != null
                        && check.Mins.X != check.Maxs.X)
                    {
                        if (check.Solid != Solid.Not
                            && check.Solid != Solid.Trigger)
                        {
                            check.Transform.Origin = savedBlockOrigin;

                            _physics.LinkEdict(check, true);

                            pusher.Transform.Origin = savedOrigin;

                            _physics.LinkEdict(pusher, false);

                            pusher.LastThinkTime -= movetime;

                            pusher.Entity.SendMessage(BuiltInComponentMethods.Blocked, check);

                            for (int i = 0; i < num_moved; ++i)
                            {
                                var moved = _moveCache[i].Entity;
                                moved.Transform.Origin = _moveCache[i].Origin;

                                _physics.LinkEdict(moved, false);
                            }

                            break;
                        }

                        var checkMins = check.Mins;
                        var checkMaxs = check.Maxs;

                        checkMins.X = 0;
                        checkMaxs.X = 0;
                        checkMins.Y = 0;
                        checkMaxs.Y = 0;
                        checkMaxs.Z = checkMins.Z;

                        check.Mins = checkMins;
                        check.Maxs = checkMaxs;
                    }
                }

                ClearMoveCache();
            }
        }

        private bool InternalCheckBottom(Collider ent, ref Vector3 start, in Vector3 mins, in Vector3 maxs)
        {
            start.Z = mins.Z + _sv_stepsize.Value;

            start.X = (mins.X + maxs.X) * 0.5f;
            start.Y = (mins.Y + maxs.Y) * 0.5f;

            var stop = new Vector3(
                start.X,
                start.Y,
                start.Z - (2 * _sv_stepsize.Value)

            );

            var trace = _physics.Move(start, Vector3.Zero, Vector3.Zero, stop, TraceType.IgnoreMonsters, ent, false, (ent.Entity.Flags & EntityFlags.MonsterClip) != 0);

            if (trace.Fraction == 1.0)
            {
                return false;
            }

            var middle = trace.EndPosition.Z;

            for (int x = 0; x <= 1; ++x)
            {
                for (int y = 0; y <= 1; ++y)
                {
                    start.X = x != 0 ? maxs.X : mins.X;
                    start.Y = y != 0 ? maxs.Y : mins.Y;

                    trace = _physics.Move(start, Vector3.Zero, Vector3.Zero, stop, TraceType.IgnoreMonsters, ent, false, (ent.Entity.Flags & EntityFlags.MonsterClip) != 0);

                    if (trace.Fraction == 1.0 || middle - trace.EndPosition.Z > _sv_stepsize.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CheckBottom(Collider ent)
        {
            var mins = ent.Transform.Origin + ent.Mins;
            var maxs = ent.Transform.Origin + ent.Maxs;

            _physics.GroupMask = ent.GroupInfo;

            var start = new Vector3(
                0,
                0,
                mins.Z - 1.0f);

            for (int x = 0; x <= 1; ++x)
            {
                for (int y = 0; y <= 1; ++y)
                {
                    start.X = x != 0 ? maxs.X : mins.X;
                    start.Y = y != 0 ? maxs.Y : mins.Y;

                    if (_physics.PointContents(ref start) != Contents.Solid)
                    {
                        return InternalCheckBottom(ent, ref start, mins, maxs);
                    }
                }
            }

            return true;
        }

        private bool IsOnGround(Collider ent)
        {
            var mins = ent.Transform.Origin + ent.Mins;
            var maxs = ent.Transform.Origin + ent.Maxs;

            _physics.GroupMask = ent.GroupInfo;

            var point = new Vector3(
                0,
                0,
                mins.Z - 1.0f);

            for (int x = 0; x <= 1; ++x)
            {
                for (int y = 0; y <= 1; ++y)
                {
                    point.X = x != 0 ? maxs.X : mins.X;
                    point.Y = y != 0 ? maxs.Y : mins.Y;

                    if (_physics.PointContents(ref point) == Contents.Solid)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
