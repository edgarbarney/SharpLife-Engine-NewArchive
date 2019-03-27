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
using SharpLife.Engine.Models.SPR.Rendering;
using SharpLife.Engine.ObjectEditor;
using System;
using Transform = SharpLife.Engine.Entities.Components.Transform;

namespace SharpLife.Engine.Models.SPR
{
    public class SpriteRenderableComponent : RenderableComponent
    {
        private SpriteModel _spriteModel;

        private float _lastTime;

        public override IModel Model
        {
            get => SpriteModel;
            set => SpriteModel = (SpriteModel)value;
        }

        protected override Type ModelFormat => typeof(SpriteModel);

        [ObjectEditorVisible(Visible = false)]
        public Transform Transform { get; private set; }

        [ObjectEditorVisible(Visible = false)]
        public SpriteModel SpriteModel
        {
            get => _spriteModel;

            set
            {
                var hadModel = _spriteModel != null;

                _spriteModel = value;

                if (hadModel != (_spriteModel != null))
                {
                    if (_spriteModel != null)
                    {
                        InvokeRepeating("Animate", 0.01f);
                    }
                    else
                    {
                        CancelInvocations("Animate");
                    }
                }
            }
        }

        public float Frame { get; private set; }

        [KeyValue(Name = "framerate")]
        public float FrameRate { get; set; }

        public void Initialize()
        {
            Transform = Entity.GetComponent<Transform>();

            if (Transform == null)
            {
                EntitySystem.Scene.Logger.Warning($"Missing {nameof(Transform)} component for {nameof(SpriteRenderableComponent)}");
            }
        }

        protected override bool InternalTrySetModel(IModel model)
        {
            if (model is SpriteModel sprite)
            {
                SpriteModel = sprite;
                return true;
            }

            //TODO: set error model instead?
            return false;
        }

        internal override void Render(IRendererModels renderer, in RenderContext renderContext)
        {
            renderer.GetRenderer<SpriteModelRenderer>().Render(renderContext, this);
        }

        public void Animate()
        {
            if (Model is SpriteModel spriteModel)
            {
                Frame += (float)(FrameRate * (EntitySystem.Time.ElapsedTime - _lastTime));

                if (Frame >= spriteModel.SpriteFile.Frames.Count)
                {
                    Frame %= spriteModel.SpriteFile.Frames.Count;
                }

                _lastTime = (float)EntitySystem.Time.ElapsedTime;
            }
        }
    }
}
