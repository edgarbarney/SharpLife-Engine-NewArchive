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

using SharpLife.Engine.Client.UI.Renderer;
using SharpLife.Engine.Client.UI.Renderer.Models;
using SharpLife.Engine.Entities;
using SharpLife.Engine.Entities.Components;
using SharpLife.Engine.Models.BSP.Rendering;
using SharpLife.Engine.Models.Rendering;
using SharpLife.Engine.ObjectEditor;
using System.Numerics;

namespace SharpLife.Engine.Models.BSP
{
    public class BSPRenderableComponent : RenderableComponent
    {
        private Transform _transform;

        public override IModel Model
        {
            get => BSPModel;
            set => BSPModel = (BSPModel)value;
        }

        [ObjectEditorVisible(Visible = false)]
        public BSPModel BSPModel { get; set; }

        public void Activate()
        {
            _transform = Entity.GetComponent<Transform>();

            if (_transform == null)
            {
                EntitySystem.Scene.Logger.Warning($"Missing {nameof(Transform)} component for {nameof(BSPRenderableComponent)}");
            }
        }

        public override void Render(ModelRenderer renderer)
        {
            BrushModelRenderData data = new BrushModelRenderData
            {
                Shared = new SharedModelRenderData
                {
                    Index = 0,

                    //TODO: fill in other values
                    Origin = _transform.AbsolutePosition,
                    Angles = Vector3.Zero,
                    Scale = Vector3.One,

                    Effects = EffectsFlags.None,

                    RenderMode = RenderMode.Normal,
                    RenderAmount = 0,
                    RenderColor = Vector3.Zero,
                    RenderFX = RenderFX.None,
                },
                Model = BSPModel
            };

            renderer.RenderBrushModel(ref data);
        }
    }
}
