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
        private static bool enableSeaReflections = true;
        private static bool enableImprovedTerrain = true;
        private static bool indicateLocations = true;

        [Invoke(StateManager.StateTypes.Start)]
        public static void InitStart(InitParams initParams)
        {
            // Get this mod
            mod = initParams.Mod;

            // Load settings. Pass this mod as paramater
            ModSettings settings = new ModSettings(mod);

            // settings
            enableTerrainTransition = settings.GetBool("TerrainTransition", "enable");
            enableSeaReflections = settings.GetBool("SeaReflections", "enable");
            enableImprovedTerrain = settings.GetBool("ImprovedTerrain", "enable");
            indicateLocations = settings.GetBool("ImprovedTerrain", "indicateLocations");

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
            Debug.Log("init of DistantTerrain standalone");
            gameobjectDistantTerrain = new GameObject("DistantTerrain");
            componentDistantTerrain = gameobjectDistantTerrain.AddComponent<DistantTerrain>();
            componentDistantTerrain.EnableTerrainTransition = enableTerrainTransition;
            componentDistantTerrain.EnableSeaReflections = enableSeaReflections;
            componentDistantTerrain.EnableImprovedTerrain = enableImprovedTerrain;
            componentDistantTerrain.IndicateLocations = indicateLocations;
            componentDistantTerrain.ShaderDistantTerrainTilemap = mod.GetAsset<Shader>("Shaders/DaggerfallDistantTerrainTilemap.shader");
            componentDistantTerrain.ShaderBillboardBatchFaded = mod.GetAsset<Shader>("Shaders/DaggerfallBillboardBatchFaded.shader");
            componentDistantTerrain.ShaderTransitionRingTilemap = mod.GetAsset<Shader>("Shaders/TransitionRingTilemap.shader");
        }
    }
}
