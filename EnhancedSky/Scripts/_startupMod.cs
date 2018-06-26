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

// only use this define when not building the mod but using it as sub-project to debug (will trigger different prefab injection for debugging)
// IMPORTANT: if this line is used mod build will fail - so uncomment before building mod
//#define DEBUG

using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings; //required for mod settings

#if DEBUG
using UnityEditor; // only used for AssetDatabase when debugging from editor - no use in normal mod's execution
#endif


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

        private static GameObject containerPrefab = null;

        private static Material skyObjMat = null;
        private static Material skyMat = null;
        private static Material cloudMat = null;
        private static Material masserMat = null;
        private static Material secundaMat = null;
        private static Material starsMat = null;
        private static Material starMaskMat = null;

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
            enableSunFlare = settings.GetBool("GeneralSettings", "UseSunFlare");

            shaderDepthMask = mod.GetAsset<Shader>("Materials/Resources/DepthMask.shader");
            shaderUnlitAlphaWithFade = mod.GetAsset<Shader>("Materials/Resources/UnlitAlphaWithFade.shader");

            prefabEnhancedSkyController = mod.GetAsset<GameObject>("Prefabs/Resources/EnhancedSkyController.prefab");

            starsMat = mod.GetAsset<Material>("Materials/Resources/Stars") as Material;
            skyMat = mod.GetAsset<Material>("Materials/Resources/Sky") as Material;
            containerPrefab = mod.GetAsset<GameObject>("Prefabs/Resources/EnhancedSkyContainer.prefab");

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
            shaderDepthMask = Shader.Find("Transparent/DepthMask");
            shaderUnlitAlphaWithFade = Shader.Find("Unlit/UnlitAlphaWithFade");

#if DEBUG
            if (GameObject.Find("debug_EnhancedSky"))
            {
                prefabEnhancedSkyController = AssetDatabase.LoadAssetAtPath("Assets/Game/Addons/EnhancedSky_standalone/Prefabs/Resources/EnhancedSkyController.prefab", typeof(GameObject));

                starsMat = Instantiate(Resources.Load("Stars")) as Material;
                skyMat = Instantiate(Resources.Load("Sky")) as Material;
                containerPrefab = Resources.Load("EnhancedSkyContainer", typeof(GameObject)) as GameObject;
            }
#endif

            initMod();
        }

        public static void initMod()
        {
            Debug.Log("init of EnhancedSky standalone");
            Debug.Log("init 0");
            gameobjectEnhancedSky = Instantiate(prefabEnhancedSkyController) as GameObject;
            Debug.Log("init 1");
            componentSkyManager = gameobjectEnhancedSky.GetComponent<SkyManager>();

            componentSkyManager.DepthMaskShader = shaderDepthMask;
            componentSkyManager.UnlitAlphaFadeShader = shaderUnlitAlphaWithFade;

            starMaskMat = new Material(shaderDepthMask);
            skyObjMat = new Material(shaderUnlitAlphaWithFade);

            componentSkyManager.ContainerPrefab = containerPrefab;
            componentSkyManager.StarMaskMat = starMaskMat;
            componentSkyManager.SkyObjMat = skyObjMat;
            componentSkyManager.StarsMat = starsMat;
            componentSkyManager.SkyMat = skyMat;

            Debug.Log("init 2");
            componentSkyManager.ToggleEnhancedSky(true);
            Debug.Log("init 3");
            componentSkyManager.UseSunFlare = enableSunFlare;
            Debug.Log("init 4");
        }
    }
}
