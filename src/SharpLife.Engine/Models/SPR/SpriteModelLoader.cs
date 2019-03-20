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
using SharpLife.Engine.Models.SPR.FileFormat;
using SharpLife.Engine.Models.SPR.Rendering;
using SharpLife.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace SharpLife.Engine.Models.SPR
{
    public sealed class SpriteModelLoader : IModelLoader
    {
        public IReadOnlyList<IModel> Load(string name, IFileSystem fileSystem, Scene scene, BinaryReader reader, bool computeCRC)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            //Check if we can actually load this
            if (!SpriteLoader.IsSpriteFile(reader))
            {
                return null;
            }

            var loader = new SpriteLoader(reader);

            var spriteFile = loader.ReadSpriteFile();

            uint crc = 0;

            if (computeCRC)
            {
                crc = loader.ComputeCRC();
            }

            var model = new SpriteModel(name, crc, spriteFile);

            if (scene != null)
            {
                model.ResourceContainer = new SpriteModelResourceContainer(scene, model);

                scene.AddContainer(model.ResourceContainer);
            }

            return new[] { model };
        }
    }
}
