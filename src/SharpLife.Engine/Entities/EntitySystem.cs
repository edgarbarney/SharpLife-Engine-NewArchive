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

using SharpLife.Utility;
using System.Diagnostics;

namespace SharpLife.Engine.Entities
{
    /// <summary>
    /// Globals available to the entity system
    /// </summary>
    public static class EntitySystem
    {
        private static Scene _scene;

        public static Scene Scene
        {
            get
            {
                Debug.Assert(_scene != null, "A scene must be activated before accessing " + nameof(EntitySystem));
                return _scene;
            }
        }

        public static EntityList Entities => Scene.Entities;

        public static SnapshotTime Time => Scene.Time;

        public static void SetScene(Scene scene)
        {
            //Deactivate any old scene if active
            if (_scene != null)
            {
                _scene.Deactivate();
                _scene = null;
            }

            _scene = scene;

            _scene?.Activate();
        }
    }
}
