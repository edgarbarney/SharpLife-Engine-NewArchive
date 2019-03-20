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

using SharpLife.Engine.Client.UI.Renderer.Models;
using SharpLife.Engine.Models;
using SharpLife.Engine.ObjectEditor;

namespace SharpLife.Engine.Entities.Components
{
    public abstract class RenderableComponent : Component
    {
        [ObjectEditorVisible(Visible = false)]
        public abstract IModel Model { get; set; }

        public abstract void Render(RendererModels renderer);

        public void OnEnable()
        {
            EntitySystem.WorldState.RendererModels.AddRenderable(this);
        }

        public void OnDisable()
        {
            EntitySystem.WorldState.RendererModels.RemoveRenderable(this);
        }
    }
}
