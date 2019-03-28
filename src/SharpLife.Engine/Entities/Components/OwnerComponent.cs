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

using SharpLife.Engine.ObjectEditor;

namespace SharpLife.Engine.Entities.Components
{
    /// <summary>
    /// Stores the owner of an entity
    /// </summary>
    public sealed class OwnerComponent : Component
    {
        private Entity _owner;

        [ObjectEditorVisible(Visible = false)]
        public Entity Owner
        {
            get
            {
                //Free references to destroyed objects
                if (_owner?.Destroyed == true)
                {
                    _owner = null;
                }

                return _owner;
            }

            set => _owner = value;
        }
    }
}
