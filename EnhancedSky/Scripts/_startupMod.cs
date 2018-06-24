//Enhanced Sky for Daggerfall Tools for Unity by Lypyl, contact at lypyl@dfworkshop.net
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: LypyL
//Contact: Lypyl@dfworkshop.net
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)
//this file was added by Michael Rauter (Nystul)

/*  moon textures by Doubloonz @ nexus mods, w/ permission to use for DFTFU.
 *http://www.nexusmods.com/skyrim/mods/40785/
 * 
 */

using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings; //required for mod settings
using UnityEditor; // only used for AssetDatabase when debugging from editor - no use in normal mod's execution

using EnhancedSky;

namespace EnhancedSky
{
    public class _startupMod : MonoBehaviour
    {
        public static Mod mod;
        private static GameObject gameobjectEnhancedSky = null;
        private static SkyManager componentSkyManager = null;

        // Settings
        private static bool enableSunFlare = true;

        private static Shader shaderDepthMask = null;
        private static Shader shaderUnlitAlphaWithFade = null;

        private static Object prefabEnhancedSkyController = null;

        [Invoke(StateManager.StateTypes.Start)]
        public static void InitStart(InitParams initParams)
        {
            // check if debug gameobject is present, if so do not initalize mod
            if (GameObject.Find("debug_EnhancedSky"))
            {
                return;
            }

            // Get this mod
            mod = initParams.Mod;

            // Load settings. Pass this mod as paramater
            ModSettings settings = new ModSettings(mod);

            // settings
            enableSunFlare = settings.GetBool("EnableSunFlare", "enable");

            shaderDepthMask = mod.GetAsset<Shader>("Materials/Resources/DepthMask.shader");
            shaderUnlitAlphaWithFade = mod.GetAsset<Shader>("Materials/Resources/UnlitAlphaWithFade.shader");

            prefabEnhancedSkyController = mod.GetAsset<GameObject>("Prefabs/EnhancedSkyController.prefab");

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
            if (GameObject.Find("debug_EnhancedSky"))
            {
                // if so get prefab from project
                prefabEnhancedSkyController = AssetDatabase.LoadAssetAtPath("Assets/Game/Addons/EnhancedSky_standalone/Prefabs/EnhancedSkyController.prefab", typeof(GameObject));
            }

            shaderDepthMask = Shader.Find("Transparent/DepthMask");
            shaderUnlitAlphaWithFade = Shader.Find("Unlit/UnlitAlphaWithFade");

            initMod();
        }

        public static void initMod()
        {
            Debug.Log("init of EnhancedSky standalone");
            gameobjectEnhancedSky = Instantiate(prefabEnhancedSkyController) as GameObject;
            componentSkyManager = gameobjectEnhancedSky.GetComponent<SkyManager>();

            componentSkyManager.ToggleEnhancedSky(true);
            //componentSkyManager.EnableSunFlare = enableSunFlare;            
        }
    }
}
