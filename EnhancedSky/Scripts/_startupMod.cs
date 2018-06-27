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
        private static CloudGenerator componentCloudGenerator = null;

        // Settings
        private static bool enableSunFlare = true;

        private static Shader shaderDepthMask = null;
        private static Shader shaderUnlitAlphaWithFade = null;

        private static Object prefabEnhancedSkyController = null;

        private static Object containerPrefab = null;

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
            Debug.Log("InitStart");
            // Get this mod
            mod = initParams.Mod;

            // Load settings. Pass this mod as paramater
            ModSettings settings = new ModSettings(mod);

            // settings
            enableSunFlare = settings.GetBool("GeneralSettings", "UseSunFlare");

            shaderDepthMask = mod.GetAsset<Shader>("Materials/Resources/DepthMask.shader") as Shader;
            if (shaderDepthMask == null)
                Debug.Log("failed to load shaderDepthMask");
            else
                Debug.Log("loaded shaderDepthMask");
            shaderUnlitAlphaWithFade = mod.GetAsset<Shader>("Materials/Resources/UnlitAlphaWithFade.shader") as Shader;
            if (shaderUnlitAlphaWithFade == null)
                Debug.Log("failed to load shaderUnlitAlphaWithFade");
            else
                Debug.Log("loaded shaderUnlitAlphaWithFade");

            prefabEnhancedSkyController = mod.GetAsset<Object>("Prefabs/Resources/NewEnhancedSkyController.prefab") as Object;
            if (prefabEnhancedSkyController == null)
                Debug.Log("failed to load prefabEnhancedSkyController");
            else
                Debug.Log("loaded prefabEnhancedSkyController");

            starsMat = mod.GetAsset<Material>("Materials/Resources/Stars") as Material;
            skyMat = mod.GetAsset<Material>("Materials/Resources/Sky") as Material;
            containerPrefab = mod.GetAsset<Object>("Prefabs/Resources/NewPrefab.prefab") as Object;
            if (containerPrefab == null)
                Debug.Log("failed to load containerPrefab");
            else
                Debug.Log("loaded containerPrefab");

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
                prefabEnhancedSkyController = AssetDatabase.LoadAssetAtPath("Assets/Game/Addons/EnhancedSky_standalone/Prefabs/Resources/EnhancedSkyController.prefab", typeof(Object));

                starsMat = Instantiate(Resources.Load("Stars")) as Material;
                skyMat = Instantiate(Resources.Load("Sky")) as Material;
                containerPrefab = Resources.Load("EnhancedSkyContainer", typeof(Object)) as Object;
            }
#endif

            initMod();
        }

        public static void initMod()
        {
            Debug.Log("init of EnhancedSky standalone");
            Debug.Log("init 0");

            gameobjectEnhancedSky = Instantiate(prefabEnhancedSkyController) as GameObject;
            /*MonoBehaviour[] scripts = gameobjectEnhancedSky.GetComponentsInChildren<SkyManager>();
            foreach (MonoBehaviour script in scripts)
            {
                Debug.Log(string.Format("script name: {0}", script.name));
            }*/
            //gameobjectEnhancedSky.AddComponent<SkyManager>();
            Debug.Log("init 1");
            //componentSkyManager = gameobjectEnhancedSky.GetComponent<SkyManager>() as SkyManager;
            //componentSkyManager = GameObject.Find("EnhancedSkyController(Clone)").GetComponent<SkyManager>();
            
            componentSkyManager = gameobjectEnhancedSky.AddComponent<SkyManager>() as SkyManager;
            SkyManager.instance = componentSkyManager;            

            if (componentSkyManager == null)
                Debug.Log("componentSkyManager is null");
            else
                Debug.Log("componentSkyManager != null");

            componentSkyManager.ModSelf = mod;

            Debug.Log("init 2");
            starMaskMat = new Material(shaderDepthMask);
            Debug.Log("init 3");
            skyObjMat = new Material(shaderUnlitAlphaWithFade);
            Debug.Log("init 4");
            componentSkyManager.ContainerPrefab = containerPrefab;

            //GameObject _container = Instantiate(containerPrefab) as GameObject;
            //_container.transform.SetParent(GameManager.Instance.ExteriorParent.transform, true);

            Debug.Log("init 5");
            componentSkyManager.StarMaskMat = starMaskMat;
            Debug.Log("init 6");
            componentSkyManager.SkyObjMat = skyObjMat;
            Debug.Log("init 7");
            componentSkyManager.StarsMat = starsMat;
            Debug.Log("init 8");
            componentSkyManager.SkyMat = skyMat;

            Debug.Log("init 9");

            if (containerPrefab)
            {
                GameObject container = Instantiate(containerPrefab) as GameObject;
                container.transform.SetParent(GameManager.Instance.ExteriorParent.transform, true);
            }
            else
                throw new System.NullReferenceException();

            //componentSkyManager.ToggleEnhancedSky(true);
            Debug.Log("init 10");
            componentSkyManager.UseSunFlare = enableSunFlare;
            Debug.Log("init 11");
        }
    }
}
