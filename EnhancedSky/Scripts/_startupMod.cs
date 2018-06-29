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
        private static PresetContainer componentPresetContainer = null;
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

        private static Preset presetPresetContainer = null;

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

            //prefabEnhancedSkyController = mod.GetAsset<Object>("Prefabs/Resources/NewEnhancedSkyController.prefab") as Object;
            //if (prefabEnhancedSkyController == null)
            //    Debug.Log("failed to load prefabEnhancedSkyController");
            //else
            //    Debug.Log("loaded prefabEnhancedSkyController");

            starsMat = mod.GetAsset<Material>("Materials/Resources/Stars") as Material;
            skyMat = mod.GetAsset<Material>("Materials/Resources/Sky") as Material;
            containerPrefab = mod.GetAsset<Object>("Prefabs/Resources/NewEnhancedSkyContainer.prefab") as Object;
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
                //containerPrefab = Resources.Load("EnhancedSkyContainer", typeof(Object)) as Object;
                //presetPresetContainer = Resources.Load("PresetContainer_preset1.preset", typeof(Preset)) as Preset;
            }
#endif

            initMod();
        }

        public static void initMod()
        {
            Debug.Log("init of EnhancedSky standalone");

            //gameobjectEnhancedSky = Instantiate(prefabEnhancedSkyController) as GameObject;
            gameobjectEnhancedSky = new GameObject("EnhancedSkyController");

            //componentSkyManager = gameobjectEnhancedSky.GetComponent<SkyManager>() as SkyManager;
            //componentSkyManager = GameObject.Find("EnhancedSkyController(Clone)").GetComponent<SkyManager>();

            componentPresetContainer = gameobjectEnhancedSky.AddComponent<PresetContainer>() as PresetContainer;
            SetPresetContainerValues(componentPresetContainer);

            componentSkyManager = gameobjectEnhancedSky.AddComponent<SkyManager>() as SkyManager;
            SkyManager.instance = componentSkyManager;                      

            componentCloudGenerator = gameobjectEnhancedSky.AddComponent<CloudGenerator>() as CloudGenerator;

            componentSkyManager.ModSelf = mod;

            starMaskMat = new Material(shaderDepthMask);
            skyObjMat = new Material(shaderUnlitAlphaWithFade);
            
            componentSkyManager.StarMaskMat = starMaskMat;
            componentSkyManager.SkyObjMat = skyObjMat;
            componentSkyManager.StarsMat = starsMat;
            componentSkyManager.SkyMat = skyMat;
            
            if (containerPrefab)
            {
                //GameObject container = Instantiate(containerPrefab) as GameObject;
                //container.transform.SetParent(GameManager.Instance.ExteriorParent.transform, true);

                //container.AddComponent<MoonController>();
                //container.AddComponent<AmbientFogLightController>();

                //container.transform.Find("SkyCam").gameObject.AddComponent<SkyCam>();
                //container.transform.Find("Stars").Find("StarParticles").gameObject.AddComponent<StarController>();
                //container.transform.Find("Rotator").gameObject.AddComponent<RotationScript>();
                //container.transform.Find("cloudPrefab").gameObject.AddComponent<Cloud>();
            }
            else
                throw new System.NullReferenceException();

            //componentSkyManager.ToggleEnhancedSky(true);
            componentSkyManager.UseSunFlare = enableSunFlare;
        }

        private static void SetPresetContainerValues(PresetContainer presetContainer)
        {            
            Gradient gradient = new Gradient();
            GradientAlphaKey[] gak = {
                new GradientAlphaKey(55.0f/255.0f, 0.0f),
                new GradientAlphaKey(75.0f/255.0f, 0.21f),
                new GradientAlphaKey(255.0f/255.0f, 0.31f),
                new GradientAlphaKey(255.0f/255.0f, 0.69f),
                new GradientAlphaKey(75.0f/255.0f, 0.79f),
                new GradientAlphaKey(75.0f/255.0f, 1.0f)
            };
            string[] colorsAsHex = { "#3C3C3C", "#727272", "#A8553E", "#DAD6D6", "#D6D6D6", "#C5BFBF", "#A8553E", "#3C3C3C" };
            Color[] colors = new Color[colorsAsHex.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                UnityEngine.ColorUtility.TryParseHtmlString(colorsAsHex[i], out colors[i]);
            }
            GradientColorKey[] gck = {
                new GradientColorKey(colors[0], 0.0f),
                new GradientColorKey(colors[1], 0.159f),
                new GradientColorKey(colors[2], 0.244f),
                new GradientColorKey(colors[3], 0.318f),
                new GradientColorKey(colors[4], 0.5f),
                new GradientColorKey(colors[5], 0.694f),
                new GradientColorKey(colors[6], 0.762f),
                new GradientColorKey(colors[7], 0.835f)
            };
            gradient.alphaKeys = gak;
            gradient.colorKeys = gck;
            presetContainer.colorBase = gradient;
        }
    }
}
