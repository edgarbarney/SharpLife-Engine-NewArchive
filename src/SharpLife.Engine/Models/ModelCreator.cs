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
using SharpLife.Engine.Client.UI.Rendering;
using SharpLife.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpLife.Engine.Models
{
    public sealed class ModelCreator
    {
        private readonly ILogger _logger;

        private readonly IFileSystem _fileSystem;

        //TODO: replace with List to allow use of stack allocated enumerator
        private readonly IReadOnlyList<IModelLoader> _modelLoaders;

        public ModelCreator(ILogger logger, IFileSystem fileSystem, IEnumerable<IModelLoader> loaders)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            if (loaders == null)
            {
                throw new ArgumentNullException(nameof(loaders));
            }

            //Avoid overhead involved with repeated enumeration of IEnumerable
            _modelLoaders = loaders.ToList();
        }

        public IReadOnlyList<IModel> TryLoadModel(string modelName, Scene scene)
        {
            try
            {
                var reader = new BinaryReader(_fileSystem.OpenRead(modelName));

                foreach (var loader in _modelLoaders)
                {
                    //TODO: this needs to be profiled to see if the allocations cause issues
                    //If so we'll need to optimize this
                    var models = loader.Load(modelName, _fileSystem, scene, reader, true);

                    if (models != null)
                    {
                        //This should never happen since a loader needs to return null to signal failure
                        if (models.Count == 0)
                        {
                            throw new InvalidOperationException($"Model loader {loader.GetType().Name} returned empty array");
                        }

                        return models;
                    }
                }

                return null;
            }
            catch (FileNotFoundException e)
            {
                _logger.Error(e, "Failed to load model {FileName}", modelName);
                return null;
            }
        }
    }
}
