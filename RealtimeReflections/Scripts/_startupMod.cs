//RealtimeReflections for Daggerfall-Unity
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: Michael Rauter (a.k.a. Nystul)
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)

using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings; //required for mod settings

using RealtimeReflections;

namespace RealtimeReflections
{
    public class _startupMod : MonoBehaviour
    {
        public static Mod mod;
        private static GameObject gameobjectRealtimeReflections = null;
        private static UpdateReflectionTextures componentUpdateReflectionTextures = null;

        // Settings
        private static int floorReflectionTextureWidth = 512;
        private static int floorReflectionTextureHeight = 512;
        private static int lowerLevelReflectionTextureWidth = 512;
        private static int lowerLevelReflectionTextureHeight = 512;
        private static float roughnessMultiplier = 0.4f;

        private static Shader shaderTilemapWithReflections = null;
        private static Shader shaderTilemapTextureArrayWithReflections = null;
        private static Shader shaderCreateLookupReflectionTextureCoordinates = null;
        private static Shader shaderCreateLookupReflectionTextureIndex = null;
        private static Shader shaderDeferredPlanarReflections = null;
        private static Shader shaderDungeonWaterWithReflections = null;
        private static Texture2D textureTileatlasReflective = null;
        private static Texture2D textureTileatlasReflectiveRaining = null;

        [Invoke(StateManager.StateTypes.Start)]
        public static void InitStart(InitParams initParams)
        {
            // check if debug gameobject is present, if so do not initalize mod
            if (GameObject.Find("debug_RealtimeReflections"))
                return;

            // Get this mod
            mod = initParams.Mod;

            // Load settings. Pass this mod as paramater
            ModSettings settings = mod.GetSettings();

            // settings
            floorReflectionTextureWidth = settings.GetValue<int>("FloorReflectionTexture", "width");
            floorReflectionTextureHeight = settings.GetValue<int>("FloorReflectionTexture", "height");
            lowerLevelReflectionTextureWidth = settings.GetValue<int>("LowerLevelReflectionTexture", "width");
            lowerLevelReflectionTextureHeight = settings.GetValue<int>("LowerLevelReflectionTexture", "height");
            roughnessMultiplier = settings.GetValue<float>("ReflectionParameters", "roughnessMultiplier");

            shaderTilemapWithReflections = mod.GetAsset<Shader>("Shaders/DaggerfallTilemapWithReflections.shader");
            shaderTilemapTextureArrayWithReflections = mod.GetAsset<Shader>("Shaders/DaggerfallTilemapTextureArrayWithReflections.shader");
            shaderCreateLookupReflectionTextureCoordinates = mod.GetAsset<Shader>("Shaders/CreateLookupReflectionTextureCoordinates.shader");
            shaderCreateLookupReflectionTextureIndex = mod.GetAsset<Shader>("Shaders/CreateLookupReflectionTextureIndex.shader");
            shaderDeferredPlanarReflections = mod.GetAsset<Shader>("Shaders/DeferredPlanarReflections.shader");
            shaderDungeonWaterWithReflections = mod.GetAsset<Shader>("Shaders/DungeonWaterWithReflections.shader");
            textureTileatlasReflective = mod.GetAsset<Texture2D>("Resources/tileatlas_reflective");
            textureTileatlasReflectiveRaining = mod.GetAsset<Texture2D>("Resources/tileatlas_reflective_raining");

            initMod();

            //after finishing, set the mod's IsReady flag to true.
            ModManager.Instance.GetMod(initParams.ModTitle).IsReady = true;
        }

        /*  
        *   used for debugging
        *   howto debug:
        *       -) add a dummy GameObject to DaggerfallUnityGame scene
        *       -) attach this script (_startupMod) as component
        *       -) deactivate mod in mod list (since dummy gameobject will start up mod)
        *       -) attach debugger and set breakpoint to one of the mod's cs files and debug
        */
        void Awake()
        {
            shaderTilemapWithReflections = Shader.Find("Daggerfall/RealtimeReflections/TilemapWithReflections");
            shaderTilemapTextureArrayWithReflections = Shader.Find("Daggerfall/RealtimeReflections/TilemapTextureArrayWithReflections");
            shaderCreateLookupReflectionTextureCoordinates = Shader.Find("Daggerfall/RealtimeReflections/CreateLookupReflectionTextureCoordinates");
            shaderCreateLookupReflectionTextureIndex = Shader.Find("Daggerfall/RealtimeReflections/CreateLookupReflectionTextureIndex");
            shaderDeferredPlanarReflections = Shader.Find("Daggerfall/RealtimeReflections/DeferredPlanarReflections");
            shaderDungeonWaterWithReflections = Shader.Find("Daggerfall/RealtimeReflections/DungeonWaterWithReflections");
            textureTileatlasReflective = Resources.Load("tileatlas_reflective") as Texture2D;
            textureTileatlasReflectiveRaining = Resources.Load("tileatlas_reflective_raining") as Texture2D;

            initMod();
        }

        public static void initMod()
        {
            //Debug.Log("init of ReflectionMod standalone");
            gameobjectRealtimeReflections = new GameObject("RealtimeReflections");
            componentUpdateReflectionTextures = gameobjectRealtimeReflections.AddComponent<UpdateReflectionTextures>();
            componentUpdateReflectionTextures.FloorReflectionTextureWidth = floorReflectionTextureWidth;
            componentUpdateReflectionTextures.FloorReflectionTextureHeight = floorReflectionTextureHeight;
            componentUpdateReflectionTextures.LowerLevelReflectionTextureWidth = lowerLevelReflectionTextureWidth;
            componentUpdateReflectionTextures.LowerLevelReflectionTextureHeight = lowerLevelReflectionTextureHeight;
            componentUpdateReflectionTextures.RoughnessMultiplier = roughnessMultiplier;
            componentUpdateReflectionTextures.ShaderTilemapWithReflections = shaderTilemapWithReflections;
            componentUpdateReflectionTextures.ShaderTilemapTextureArrayWithReflections = shaderTilemapTextureArrayWithReflections;
            componentUpdateReflectionTextures.ShaderCreateLookupReflectionTextureCoordinates = shaderCreateLookupReflectionTextureCoordinates;
            componentUpdateReflectionTextures.ShaderCreateLookupReflectionTextureIndex = shaderCreateLookupReflectionTextureIndex;
            componentUpdateReflectionTextures.ShaderDeferredPlanarReflections = shaderDeferredPlanarReflections;
            componentUpdateReflectionTextures.ShaderDungeonWaterWithReflections = shaderDungeonWaterWithReflections;
            componentUpdateReflectionTextures.TextureTileatlasReflective = textureTileatlasReflective;
            componentUpdateReflectionTextures.TextureTileatlasReflectiveRaining = textureTileatlasReflectiveRaining;
        }
    }
}
