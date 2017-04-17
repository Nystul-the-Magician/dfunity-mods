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

        [Invoke(StateManager.StateTypes.Start)]
        public static void InitStart(InitParams initParams)
        {
            // Get this mod
            mod = initParams.Mod;

            // Load settings. Pass this mod as paramater
            ModSettings settings = new ModSettings(mod);

            // settings
            floorReflectionTextureWidth = settings.GetInt("FloorReflectionTexture", "width");
            floorReflectionTextureHeight = settings.GetInt("FloorReflectionTexture", "height");
            lowerLevelReflectionTextureWidth = settings.GetInt("LowerLevelReflectionTexture", "width");
            lowerLevelReflectionTextureHeight = settings.GetInt("LowerLevelReflectionTexture", "height");
            roughnessMultiplier = settings.GetFloat("ReflectionParameters", "roughnessMultiplier");

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
            componentUpdateReflectionTextures.ShaderTilemapWithReflections = mod.GetAsset<Shader>("Shaders/DaggerfallTilemapWithReflections.shader");
            componentUpdateReflectionTextures.ShaderCreateLookupReflectionTextureCoordinates = mod.GetAsset<Shader>("Shaders/CreateLookupReflectionTextureCoordinates.shader");
            componentUpdateReflectionTextures.ShaderCreateLookupReflectionTextureIndex = mod.GetAsset<Shader>("Shaders/CreateLookupReflectionTextureIndex.shader");
            componentUpdateReflectionTextures.ShaderDeferredPlanarReflections = mod.GetAsset<Shader>("Shaders/DeferredPlanarReflections.shader");
            componentUpdateReflectionTextures.TextureTileatlasReflective = mod.GetAsset<Texture2D>("Resources/tileatlas_reflective");
            componentUpdateReflectionTextures.TextureTileatlasReflectiveRaining = mod.GetAsset<Texture2D>("Resources/tileatlas_reflective_raining");
        }
    }
}
