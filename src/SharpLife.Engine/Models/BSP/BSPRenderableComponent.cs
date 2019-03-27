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
using SharpLife.Engine.Models.BSP.Rendering;
using SharpLife.Engine.ObjectEditor;
using System;
using Transform = SharpLife.Engine.Entities.Components.Transform;

namespace SharpLife.Engine.Models.BSP
{
    public class BSPRenderableComponent : RenderableComponent
    {
        public override IModel Model
        {
            get => BSPModel;
            set => BSPModel = (BSPModel)value;
        }

        protected override Type ModelFormat => typeof(BSPModel);

        [ObjectEditorVisible(Visible = false)]
        public Transform Transform { get; private set; }

        [ObjectEditorVisible(Visible = false)]
        public BSPModel BSPModel { get; set; }

        public void Initialize()
        {
            Transform = Entity.GetComponent<Transform>();

            if (Transform == null)
            {
                EntitySystem.Scene.Logger.Warning($"Missing {nameof(Transform)} component for {nameof(BSPRenderableComponent)}");
            }
        }

        protected override bool InternalTrySetModel(IModel model)
        {
            if (model is BSPModel brush)
            {
                BSPModel = brush;
                return true;
            }

            //TODO: set error model instead?
            return false;
        }

        internal override void Render(IRendererModels renderer, in RenderContext renderContext)
        {
            renderer.GetRenderer<BrushModelRenderer>().Render(renderContext, this);
        }
    }
}
