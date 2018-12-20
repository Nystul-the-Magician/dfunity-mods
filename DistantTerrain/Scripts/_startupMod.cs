//Distant Terrain Mod for Daggerfall-Unity
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

using DistantTerrain;

namespace DistantTerrain
{
    public class _startupMod : MonoBehaviour
    {
        public static Mod mod;
        private static GameObject gameobjectDistantTerrain = null;
        private static DistantTerrain componentDistantTerrain = null;

        // Settings
        private static bool enableTerrainTransition = true;
        private static bool enableFadeIntoSkybox = true;
        private static bool enableSeaReflections = true;
        private static bool enableImprovedTerrain = true;
        private static bool indicateLocations = true;

        private static Shader shaderDistantTerrainTilemap = null;
        private static Shader shaderBillboardBatchFaded = null;
        private static Shader shaderTransitionRingTilemap = null;
        private static Shader shaderTransitionRingTilemapTextureArray = null;

        [Invoke(StateManager.StateTypes.Start)]
        public static void InitStart(InitParams initParams)
        {
            // check if debug gameobject is present, if so do not initalize mod
            if (GameObject.Find("debug_DistantTerrain"))
                return;

            // Get this mod
            mod = initParams.Mod;

            // Load settings.
            ModSettings settings = mod.GetSettings();

            // settings
            enableTerrainTransition = settings.GetBool("GeneralSettings", "TerrainTransition");
            enableFadeIntoSkybox = settings.GetBool("GeneralSettings", "FadeIntoSkybox");
            enableSeaReflections = settings.GetBool("GeneralSettings", "SeaReflections");
            enableImprovedTerrain = settings.GetBool("ImprovedTerrainSettings", "EnableImprovedTerrain");
            indicateLocations = settings.GetBool("ImprovedTerrainSettings", "IndicateLocations");

            shaderDistantTerrainTilemap = mod.GetAsset<Shader>("Shaders/DistantTerrainTilemap.shader");
            shaderBillboardBatchFaded = mod.GetAsset<Shader>("Shaders/DaggerfallBillboardBatchFaded.shader");
            shaderTransitionRingTilemap = mod.GetAsset<Shader>("Shaders/TransitionRingTilemap.shader");
            shaderTransitionRingTilemapTextureArray = mod.GetAsset<Shader>("Shaders/TransitionRingTilemapTextureArray.shader");

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
            shaderDistantTerrainTilemap = Shader.Find("Daggerfall/DistantTerrain/DistantTerrainTilemap");
            shaderBillboardBatchFaded = Shader.Find("Daggerfall/DistantTerrain/BillboardBatchFaded");
            shaderTransitionRingTilemap = Shader.Find("Daggerfall/DistantTerrain/TransitionRingTilemap");
            shaderTransitionRingTilemapTextureArray = Shader.Find("Daggerfall/DistantTerrain/TransitionRingTilemapTextureArray");

            initMod();
        }

        public static void initMod()
        {
            gameobjectDistantTerrain = new GameObject("DistantTerrain");
            componentDistantTerrain = gameobjectDistantTerrain.AddComponent<DistantTerrain>();
            componentDistantTerrain.EnableTerrainTransition = enableTerrainTransition;
            componentDistantTerrain.EnableFadeIntoSkybox = enableFadeIntoSkybox;
            componentDistantTerrain.EnableSeaReflections = enableSeaReflections;
            componentDistantTerrain.EnableImprovedTerrain = enableImprovedTerrain;
            componentDistantTerrain.IndicateLocations = indicateLocations;
            componentDistantTerrain.ShaderDistantTerrainTilemap = shaderDistantTerrainTilemap;
            componentDistantTerrain.ShaderBillboardBatchFaded = shaderBillboardBatchFaded;
            componentDistantTerrain.ShaderTransitionRingTilemap = shaderTransitionRingTilemap;
            componentDistantTerrain.ShaderTransitionRingTilemapTextureArray = shaderTransitionRingTilemapTextureArray;
        }
    }
}
