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
using SharpLife.CommandSystem;
using SharpLife.Engine.Client.UI.Rendering.Models;
using SharpLife.Engine.Client.UI.Rendering.Objects;
using SharpLife.Engine.FileFormats.WAD;
using SharpLife.Engine.Host;
using SharpLife.Engine.Models.BSP;
using SharpLife.Engine.Models.BSP.FileFormat;
using SharpLife.Engine.Models.BSP.Rendering;
using SharpLife.FileSystem;
using SharpLife.Utility;
using SharpLife.Utility.FileSystem;
using System;
using System.IO;
using System.Linq;
using Veldrid;

namespace SharpLife.Engine.Client.UI.Rendering
{
    /// <summary>
    /// Handles the rendering of the UI
    /// </summary>
    internal sealed class Renderer : IRenderer
    {
        private readonly UserInterface _userInterface;

        private readonly ITime _engineTime;

        private readonly IFileSystem _fileSystem;

        private readonly GraphicsDevice _gd;

        private readonly SceneContext _sc;

        private readonly FinalPass _finalPass;

        private readonly CommandList _frameCommands;

        private bool _windowResized = false;

        private event Action<int, int> _resizeHandled;

        private readonly RendererModels _models;

        private CoordinateAxes _coordinateAxes;

        private Skybox2D _skyboxRenderable;

        public IRendererModels Models => _models;

        public Scene Scene { get; }

        public ImGuiRenderable ImGui { get; }

        public Renderer(
            ILogger logger,
            ITime engineTime,
            ICommandContext commandContext,
            IFileSystem fileSystem,
            UserInterface userInterface,
            EngineClient client,
            EngineStartupState startupState,
            string shadersDirectory)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            _userInterface = userInterface ?? throw new ArgumentNullException(nameof(userInterface));
            _engineTime = engineTime ?? throw new ArgumentNullException(nameof(engineTime));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            //Configure Veldrid graphics device
            //Don't use a swap chain depth format, it won't render anything on Vulkan
            //It isn't needed right now so it should be disabled for the time being
            var options = new GraphicsDeviceOptions(false, null/*PixelFormat.R8_G8_B8_A8_UNorm*/, false, ResourceBindingModel.Improved, true, true);

            _gd = GraphicsDeviceUtils.CreateGraphicsDevice(logger, _userInterface.Window, options, GraphicsBackend.OpenGL);

            _gd.SyncToVerticalBlank = false;

            _userInterface.Window.GetSize(out var width, out var height);

            Scene = new Scene(_userInterface.InputSystem, commandContext, _gd, width, height);

            ImGui = new ImGuiRenderable(_userInterface.InputSystem, width, height, logger, client);

            _resizeHandled += ImGui.WindowResized;

            Scene.AddContainer(ImGui);
            Scene.AddRenderable(ImGui);
            Scene.AddUpdateable(ImGui);

            //Needed so the result can be properly adjusted to the target coordinate system
            //Without this, nothing will be visible on-screen
            _finalPass = new FinalPass();
            Scene.AddContainer(_finalPass);
            Scene.AddRenderable(_finalPass);

            _models = new RendererModels(startupState.ModelFormats.Select(p => p.CreateRenderer(Scene, logger, commandContext)));

            Scene.AddRenderable(_models);

            _sc = new SceneContext(fileSystem, commandContext, _models, shadersDirectory);

            _sc.SetCurrentScene(Scene);

            _frameCommands = _gd.ResourceFactory.CreateCommandList();
            _frameCommands.Name = "Frame Commands List";
            var initCL = _gd.ResourceFactory.CreateCommandList();
            initCL.Name = "Recreation Initialization Command List";
            initCL.Begin();
            _sc.CreateDeviceObjects(_gd, initCL, _sc);
            Scene.CreateAllDeviceObjects(_gd, initCL, _sc, ResourceScope.Global);
            initCL.End();
            _gd.SubmitCommands(initCL);
            initCL.Dispose();

            _userInterface.Window.Resized += WindowResized;
        }

        private void WindowResized()
        {
            _windowResized = true;
        }

        public void Update(float deltaSeconds)
        {
            Scene.Update(_engineTime, deltaSeconds);
        }

        public void Draw()
        {
            if (_windowResized)
            {
                _windowResized = false;

                _userInterface.Window.GetSize(out var width, out var height);

                _gd.ResizeMainWindow((uint)width, (uint)height);
                Scene.Camera.WindowResized(width, height);
                _resizeHandled?.Invoke(width, height);
                var cl = _gd.ResourceFactory.CreateCommandList();
                cl.Begin();
                _sc.RecreateWindowSizedResources(_gd, cl);
                cl.End();
                _gd.SubmitCommands(cl);
                cl.Dispose();
            }

            _frameCommands.Begin();

            Scene.RenderAllStages(_gd, _frameCommands, _sc);

            _gd.SwapBuffers();
        }

        private void UploadWADTextures(BSPModel worldModel)
        {
            //Load all WADs
            var wadList = new WADList(_fileSystem, Framework.Extension.WAD);

            var embeddedTexturesWAD = BSPUtilities.CreateWADFromBSP(worldModel.BSPFile);

            wadList.Add(worldModel.Name + FileExtensionUtils.AsExtension(Framework.Extension.WAD), embeddedTexturesWAD);

            var wadPath = BSPUtilities.ExtractWADPathKeyValue(worldModel.BSPFile.Entities);

            foreach (var wadName in wadPath.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var baseName = Path.GetFileNameWithoutExtension(wadName);

                //Never allow these to be loaded, they contain spray decals
                //TODO: refactor into blacklist
                if (baseName != "pldecal" && baseName != "tempdecal")
                {
                    //WAD loading only needs to consider the filename; the directory part is mapper specific
                    var fileName = Path.GetFileName(wadName);
                    wadList.Load(fileName);
                }
            }

            //Upload all used textures
            var usedTextures = BSPUtilities.GetUsedTextures(worldModel.BSPFile, wadList);

            WADUtilities.UploadTextures(usedTextures, _sc.TextureLoader, _gd, _sc.MapResourceCache);
        }

        /// <summary>
        /// Loads all models in the model manager
        /// This is when models are loaded
        /// </summary>
        /// <param name="worldModel"></param>
        public void LoadModels(BSPModel worldModel)
        {
            Scene.WorldModel = worldModel ?? throw new ArgumentNullException(nameof(worldModel));

            //Reset light styles
            Scene.InitializeLightStyles();

            UploadWADTextures(worldModel);

            _coordinateAxes = new CoordinateAxes();
            Scene.AddContainer(_coordinateAxes);
            Scene.AddRenderable(_coordinateAxes);

            //TODO: define default in config
            _skyboxRenderable = Skybox2D.LoadDefaultSkybox(_fileSystem, Framework.Path.EnvironmentMaps, "2desert");
            Scene.AddContainer(_skyboxRenderable);
            Scene.AddRenderable(_skyboxRenderable);

            //Set up graphics data
            CommandList initCL = _gd.ResourceFactory.CreateCommandList();
            initCL.Name = "Model Initialization Command List";
            initCL.Begin();

            Scene.CreateAllDeviceObjects(_gd, initCL, _sc, ResourceScope.Map);

            initCL.End();
            _gd.SubmitCommands(initCL);
            initCL.Dispose();
        }

        public void ClearBSP()
        {
            Scene.DestroyAllDeviceObjects(ResourceScope.Map);

            if (_skyboxRenderable != null)
            {
                Scene.RemoveContainer(_skyboxRenderable);
                Scene.RemoveRenderable(_skyboxRenderable);
                _skyboxRenderable.Dispose();
                _skyboxRenderable = null;
            }

            if (_coordinateAxes != null)
            {
                Scene.RemoveContainer(_coordinateAxes);
                Scene.RemoveRenderable(_coordinateAxes);
                _coordinateAxes.Dispose();
                _coordinateAxes = null;
            }

            //Clear all graphics data
            _sc.MapResourceCache.DestroyAllDeviceObjects();

            Scene.WorldModel = null;
        }
    }
}
