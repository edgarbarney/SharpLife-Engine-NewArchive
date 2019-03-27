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

using SharpLife.Engine.Client.UI.Rendering;
using SharpLife.Engine.Client.UI.Rendering.Models;
using SharpLife.Engine.Entities;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Entities.KeyValues;
using SharpLife.Engine.Models.MDL.FileFormat;
using SharpLife.Engine.Models.MDL.Rendering;
using SharpLife.Engine.ObjectEditor;
using System;
using System.Diagnostics;
using Transform = SharpLife.Engine.Entities.Components.Transform;

namespace SharpLife.Engine.Models.MDL
{
    public class StudioRenderableComponent : RenderableComponent
    {
        private StudioModel _studioModel;

        private uint _sequence;

        public override IModel Model
        {
            get => StudioModel;
            set => StudioModel = (StudioModel)value;
        }

        protected override Type ModelFormat => typeof(StudioModel);

        [ObjectEditorVisible(Visible = false)]
        public Transform Transform { get; private set; }

        [ObjectEditorVisible(Visible = false)]
        public StudioModel StudioModel
        {
            get => _studioModel;

            set
            {
                var hadModel = _studioModel != null;

                _studioModel = value;

                if (hadModel != (_studioModel != null))
                {
                    if (_studioModel != null)
                    {
                        InvokeRepeating("Animate", 0.01f);
                    }
                    else
                    {
                        CancelInvocations("Animate");
                    }
                }

                Sequence = 0;
            }
        }

        public uint Sequence
        {
            get => _sequence;
            set
            {
                if (StudioModel != null && value < StudioModel.StudioFile.Sequences.Count)
                {
                    _sequence = value;
                }
                else
                {
                    _sequence = 0;
                }

                ResetSequenceInfo();
            }
        }

        public float LastTime { get; set; }

        public float Frame { get; set; }

        [KeyValue(Name = "framerate")]
        public float FrameRate { get; set; }

        public uint Body { get; set; }

        public uint Skin { get; set; }

        [ObjectEditorVisible(Visible = false)]
        public byte[] Controllers { get; } = new byte[MDLConstants.MaxControllers];

        [ObjectEditorVisible(Visible = false)]
        public byte[] Blenders { get; } = new byte[MDLConstants.MaxBlenders];

        public int RenderFXLightMultiplier { get; set; }

        private float _lastEventCheck;

        public bool SequenceLoops { get; private set; }

        public bool SequenceFinished { get; private set; }

        public float SequenceFrameRate { get; private set; }

        public float SeqenceGroundSpeed { get; private set; }

        public void Initialize()
        {
            Transform = Entity.GetComponent<Transform>();

            if (Transform == null)
            {
                EntitySystem.Scene.Logger.Warning($"Missing {nameof(Transform)} component for {nameof(StudioRenderableComponent)}");
            }
        }

        protected override bool InternalTrySetModel(IModel model)
        {
            if (model is StudioModel studio)
            {
                StudioModel = studio;
                return true;
            }

            //TODO: set error model instead?
            return false;
        }

        internal override void Render(IRendererModels renderer, in RenderContext renderContext)
        {
            renderer.GetRenderer<StudioModelRenderer>().Render(renderContext, this);
        }

        public SequenceFlags GetSequenceFlags()
        {
            if (StudioModel != null)
            {
                return StudioModelUtils.GetSequenceFlags(StudioModel.StudioFile, Sequence);
            }

            return SequenceFlags.None;
        }

        public void ResetSequenceInfo()
        {
            if (StudioModel != null)
            {
                StudioModelUtils.GetSequenceInfo(StudioModel.StudioFile, Sequence, out var sequenceFrameRate, out var groundSpeed);
                SequenceFrameRate = sequenceFrameRate;
                SeqenceGroundSpeed = groundSpeed;

                SequenceLoops = (GetSequenceFlags() & SequenceFlags.Looping) != 0;

                LastTime = (float)EntitySystem.Time.ElapsedTime;
                _lastEventCheck = (float)EntitySystem.Time.ElapsedTime;

                FrameRate = 1.0f;
                SequenceFinished = false;
            }
        }

        public uint GetBodyGroup(uint group)
        {
            if (StudioModel != null)
            {
                return StudioModelUtils.GetBodyGroupValue(StudioModel.StudioFile, Body, group);
            }

            return 0;
        }

        public void SetBodyGroup(uint group, uint value)
        {
            if (StudioModel != null)
            {
                Body = StudioModelUtils.CalculateBodyGroupValue(StudioModel.StudioFile, Body, group, value);
            }
        }

        private float InternalSetBoneController(StudioFile studioFile, int controllerIndex, float value)
        {
            Debug.Assert(0 <= controllerIndex && controllerIndex < MDLConstants.MaxControllers);

            var result = StudioModelUtils.CalculateControllerValue(studioFile, controllerIndex, value, out value);

            if (result.HasValue)
            {
                Controllers[controllerIndex] = result.Value;
            }

            return value;
        }

        public float SetBoneController(int controllerIndex, float value)
        {
            if (StudioModel != null)
            {
                value = InternalSetBoneController(StudioModel.StudioFile, controllerIndex, value);
            }

            return value;
        }

        public void InitBoneControllers()
        {
            if (StudioModel != null)
            {
                for (var i = 0; i < MDLConstants.MaxControllers; ++i)
                {
                    InternalSetBoneController(StudioModel.StudioFile, i, 0.0f);
                }
            }
        }

        public float SetBlending(int blenderIndex, float value)
        {
            if (StudioModel != null)
            {
                var result = StudioModelUtils.CalculateBlendingValue(StudioModel.StudioFile, Sequence, blenderIndex, value, out value);

                if (result.HasValue)
                {
                    Blenders[blenderIndex] = result.Value;
                }
            }

            return value;
        }

        /// <summary>
        /// advance the animation frame up to the current time
        /// if an flInterval is passed in, only advance animation that number of seconds
        /// </summary>
        /// <param name="flInterval"></param>
        private float StudioFrameAdvance(float flInterval = 0.0f)
        {
            if (flInterval == 0.0)
            {
                flInterval = (float)(EntitySystem.Time.ElapsedTime - LastTime);

                if (flInterval <= 0.001)
                {
                    LastTime = (float)EntitySystem.Time.ElapsedTime;
                    return 0.0f;
                }
            }

            if (LastTime == 0)
            {
                flInterval = 0.0f;
            }

            Frame += flInterval * SequenceFrameRate * FrameRate;
            LastTime = (float)EntitySystem.Time.ElapsedTime;

            if (Frame < 0.0 || Frame >= 256.0)
            {
                if (SequenceLoops)
                {
                    Frame -= (int)(Frame / 256.0f) * 256.0f;
                }
                else
                {
                    Frame = (Frame < 0.0) ? 0 : 255;
                }

                SequenceFinished = true; // just in case it wasn't caught in GetEvents
            }

            return flInterval;
        }

        public void Animate()
        {
            StudioFrameAdvance();
        }
    }
}
