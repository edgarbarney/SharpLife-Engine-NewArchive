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
using System;
using System.Collections;
using System.Collections.Generic;

namespace SharpLife.Engine.Models
{
    public sealed class ModelManager : IModelManager
    {
        //TODO: replace with List to allow use of stack allocated enumerator
        private readonly ModelCreator _creator;

        private readonly Scene _scene;

        private readonly Dictionary<string, IModel> _models;

        public IModel this[string modelName] => _models[modelName];

        public int Count => _models.Count;

        public IModel FallbackModel { get; private set; }

        public event Action<IModel> OnModelLoaded;

        public ModelManager(ModelCreator modelCreator, Scene scene)
        {
            _creator = modelCreator ?? throw new ArgumentNullException(nameof(modelCreator));

            //Names are case insensitive to account for differences in the filesystem
            _models = new Dictionary<string, IModel>(StringComparer.OrdinalIgnoreCase);

            _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        ~ModelManager()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            foreach (var model in _models.Values)
            {
                model.Dispose();
            }

            if (disposing)
            {
                _models.Clear();

                FallbackModel = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool Contains(string modelName)
        {
            return _models.ContainsKey(modelName);
        }

        private void AddModel(string modelName, IModel model)
        {
            _models.Add(modelName, model);

            OnModelLoaded?.Invoke(model);
        }

        private IModel LoadModel(string modelName)
        {
            var models = _creator.TryLoadModel(modelName, _scene);

            if (models != null)
            {
                //Add all models
                foreach (var model in models)
                {
                    AddModel(model.Name, model);
                }

                //Return first model as the model associated with this model name
                if (!_models.ContainsKey(modelName))
                {
                    AddModel(modelName, models[0]);
                }

                return models[0];
            }

            return null;
        }

        private IModel InternalLoad(string modelName, bool throwOnFailure)
        {
            if (_models.TryGetValue(modelName, out var model))
            {
                return model;
            }

            model = LoadModel(modelName);

            if (model == null)
            {
                if (FallbackModel == null)
                {
                    if (throwOnFailure)
                    {
                        throw new InvalidOperationException($"Couldn't load model {modelName}; no fallback model loaded");
                    }

                    //Used by fallback model loading
                    return null;
                }

                model = FallbackModel;

                //Insert it anyway to avoid constant load attempts
                AddModel(modelName, model);
            }

            return model;
        }

        public IModel Load(string modelName)
        {
            return InternalLoad(modelName, true);
        }

        public IModel LoadFallbackModel(string fallbackModelName)
        {
            FallbackModel = InternalLoad(fallbackModelName, false);

            //TODO: could construct a dummy model to use here
            if (FallbackModel == null)
            {
                throw new InvalidOperationException($"Couldn't load fallback model {fallbackModelName}");
            }

            return FallbackModel;
        }

        public IEnumerator<IModel> GetEnumerator()
        {
            return _models.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
