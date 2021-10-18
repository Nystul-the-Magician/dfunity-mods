//Enhanced Sky for Daggerfall Tools for Unity by Lypyl, contact at lypyl@dfworkshop.net
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: LypyL
///Contact: Lypyl@dfworkshop.net
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)
// v. 1.9.1

/*  moon textures by Doubloonz @ nexus mods, w/ permission to use for DFTFU.
 *http://www.nexusmods.com/skyrim/mods/40785/
 * 
 */

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Utility;

using DaggerfallWorkshop.Game.Utility.ModSupport;



/* Setup instructions:
 * 1. Add EnhancedSkyController prefab to scene (shouldn't be a child of something that gets disabled like Exterior object)
 * 
 * 2. Add new layer named SkyLayer.
 * 
 * 3. Change fog to expon. squared, and set density to something like .000012
 *
 * 4. Change Main Camera Clear flags to depth only.
 * 
 * 5. Uncheck SkyLayer in Main Camera's Culling Mask list (same for other cameras again - only skyCam should have it checked).
 *
 * 6. Make sure all the public refrences are set on the Controller in scene (DaggerfallSky Rig, 
 * Daggerfall WeatherManager, PlayerEnterExit & Exterior Parent).
 * 
 * Tested w/ Daggerfall tools for Unity v. 1.4.45 (WIP)
 */

using SkyObjectSize = System.Int32;

namespace EnhancedSky
{/*
    public struct SkyObjectSize
    {
        private int value;

        private SkyObjectSize(int value)
        {
            this.value = value;
        }

        public static implicit operator SkyObjectSize(int value)
        {
            return new SkyObjectSize(value);
        }

        public static implicit operator int(SkyObjectSize record)
        {
            return record.value;
        }
    }*/

    public struct SkyObjectSizes
    {
        public const int

            Normal = 0,
            Large = 1;
    }

    public class SkyManager : MonoBehaviour
    {


        #region Fields       
        public const int DAYINSECONDS = 86400;
        public const int OFFSET = 21600;
        public const int TIMEINSIDELIMIT = 1;

        private Mod modSelf = null;

        private Material _skyMat;
        private Material _cloudMat;
        private Material _masserMat;
        private Material _secundaMat;
        private Material _starsMat;
        private Material _skyObjMat;

        // used to set post processing fog settings (excludeSkybox setting)
        private PostProcessLayer postProcessingLayer;

        public int cloudQuality = 400;
        public int cloudSeed = -1;
        public bool EnhancedSkyCurrentToggle = false;

        //daggerfall tools references
        public GameObject dfallSky;
        public WeatherManager weatherMan;
        public PlayerEnterExit playerEE;
        public GameObject exteriorParent;

        System.Diagnostics.Stopwatch _stopWatch;
        CloudGenerator _cloudGen;
        GameObject _container;
        #endregion

        #region Properties
        public Mod ModSelf { get { return (modSelf); } set { modSelf = value; } }

        DaggerfallUnity DfUnity { get { return DaggerfallUnity.Instance; } }
        DaggerfallDateTime TimeScript { get { return DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime; } }

        public Material SkyObjMat { get { return _skyObjMat; } set { _skyObjMat = value; } }
        public Material SkyMat { get { return _skyMat; } set { _skyMat = value; } }
        public Material CloudMat { get { return (_cloudMat) ? _cloudMat : _cloudMat = GetInstanceMaterial(); } set { _cloudMat = value; } }
        public Material MasserMat { get { return (_masserMat) ? _masserMat : _masserMat = GetInstanceMaterial(); } set { _masserMat = value; } }
        public Material SecundaMat { get { return (_secundaMat) ? _secundaMat : _secundaMat = GetInstanceMaterial(); } set { _secundaMat = value; } }
        public Material StarsMat { get; set; }
        public Material StarMaskMat { get; set; }

        public GameObject Container { get { return (_container); } set { _container = value; } }

        public SkyObjectSize SkyObjectSizeSetting { get; set; }
        public CloudGenerator CloudGen { get { return (_cloudGen != null) ? _cloudGen : _cloudGen = this.GetComponent<CloudGenerator>(); } }
        public bool UseSunFlare { get; set; }
        public bool IsOvercast { get { return (weatherMan != null) ? weatherMan.IsOvercast : false; } }
        public bool IsNight { get { return (TimeScript != null) ? TimeScript.IsNight : false; } }
        public float CurrentSeconds { get { return UpdateTime(); } }
        public float TimeRatio { get { return (CurrentSeconds / DAYINSECONDS); } }
        public int DawnTime { get; private set; }
        public int DuskTime { get; private set; }
        public int TimeInside { get; set; }
        #endregion

        #region Singleton
        private static SkyManager _instance;


        public static SkyManager instance
        {
            get
            {
                if (_instance == null)
                    _instance = GameObject.FindObjectOfType<SkyManager>();
                return _instance;
            }
            set { _instance = value; }
        }
        #endregion

        #region Events & Handlers
        //Events & handlers
        public delegate void SkyEvent(bool isOverCast);
        public delegate void UpdateSkyObjectSettings();

        public static event SkyEvent updateSkyEvent;
        public static event SkyEvent toggleSkyObjectsEvent; //no longer needed
        public static event UpdateSkyObjectSettings updateSkySettingsEvent;

        /// <summary>
        /// Get InteriorTransition & InteriorDungeonTransition events from PlayerEnterExit
        /// </summary>
        /// <param name="args"></param>
        public void InteriorTransitionEvent(PlayerEnterExit.TransitionEventArgs args)      //player went indoors (or dungeon), disable sky objects
        {
            _stopWatch.Reset();
            _stopWatch.Start();
        }

        /// <summary>
        /// Get ExteriorTransition & DungeonExteriorTransition events from PlayerEnterExit
        /// </summary>
        /// <param name="args"></param>
        public void ExteriorTransitionEvent(PlayerEnterExit.TransitionEventArgs args)   //player transitioned to exterior from indoors or dungeon
        {
            _stopWatch.Stop();
            TimeInside = _stopWatch.Elapsed.Minutes;
            if (EnhancedSkyCurrentToggle)
                ToggleSkyObjects(true);                 //enable sky objects
        }

        public void WeatherManagerSkyEventsHandler(DaggerfallWorkshop.Game.Weather.WeatherType weather)
        {
            if (updateSkyEvent != null)
                updateSkyEvent(IsOvercast);

            if (postProcessingLayer != null)
            {
                if (weather == DaggerfallWorkshop.Game.Weather.WeatherType.Fog)
                {
                    if (postProcessingLayer != null)
                    {
                        postProcessingLayer.fog.excludeSkybox = false;
                    }
                }
                else
                {
                    if (postProcessingLayer != null)
                    {
                        postProcessingLayer.fog.excludeSkybox = true;
                    }
                }
            }
        }


        /// <summary>
        /// Disables / enables the Enhanced sky objects
        /// </summary>
        /// <param name="toggle"></param>
        private void ToggleSkyObjects(bool toggle)
        {
            try
            {
                if (!toggle)
                {
                    dfallSky.SetActive(true);
                    Destroy(_container);
                }
                else if (toggle)
                {
                    GetRefrences();

                    if (SkyMat)
                        RenderSettings.skybox = SkyMat;
                    else
                        throw new System.NullReferenceException();

                    dfallSky.SetActive(false);
                    SkyObjectSizeChange(SkyObjectSizeSetting);
                }

            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Error enabling or diabling Daggerfall Sky object. ");
                Debug.LogWarning(ex.Message + " | in ToggleSkyObjects toggle: " + toggle);
            }

            //trigger toggleSkyObject event - this event is not used by ESKY anymore
            if (toggleSkyObjectsEvent != null)
                toggleSkyObjectsEvent(IsOvercast);

        }


        /// <summary>
        /// Updates enhanced sky objects
        /// </summary>
        /// <param name="worldPos"></param>
        public void EnhancedSkyUpdate(DFPosition worldPos)                         //player teleporting
        {
            //Debug.Log("EnhancedSkyUpdate");
            if (updateSkyEvent != null && SkyManager.instance.EnhancedSkyCurrentToggle)   //only trigger if eSky on
            {
                //Debug.Log("triggering fastTravelEvent");
                updateSkyEvent(IsOvercast);
            }

        }
        #endregion

        #region Unity
        void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (this != instance)
            {
                Destroy(this.gameObject);
            }

            /*
            PresetContainer presetContainer = this.gameObject.GetComponent<PresetContainer>(); //this.gameObject.AddComponent<PresetContainer>();

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
            */
            _stopWatch = new System.Diagnostics.Stopwatch();

            PlayerEnterExit.OnTransitionInterior += InteriorTransitionEvent; //interior transition
            PlayerEnterExit.OnTransitionDungeonInterior += InteriorTransitionEvent; //dungeon interior transition
            PlayerEnterExit.OnTransitionExterior += ExteriorTransitionEvent; //exterior transition
            PlayerEnterExit.OnTransitionDungeonExterior += ExteriorTransitionEvent; //dungeon exterior transition
            StreamingWorld.OnTeleportToCoordinates += EnhancedSkyUpdate;
            WeatherManager.OnWeatherChange += WeatherManagerSkyEventsHandler;

            //ToggleEnhancedSky(true);
        }


        // Use this for initialization
        void Start()
        {
            GameObject container = GameManager.Instance.ExteriorParent.transform.Find("NewEnhancedSkyContainer(Clone)").gameObject;
            container.AddComponent<MoonController>();
            container.AddComponent<AmbientFogLightController>();

            container.transform.Find("SkyCam").gameObject.AddComponent<SkyCam>();
            container.transform.Find("Stars").Find("StarParticles").gameObject.AddComponent<StarController>();
            container.transform.Find("Rotator").gameObject.AddComponent<RotationScript>();
            container.transform.Find("cloudPrefab").gameObject.AddComponent<Cloud>();

            DuskTime = DaggerfallDateTime.DuskHour * 3600;
            DawnTime = DaggerfallDateTime.DawnHour * 3600;

            GetRefrences();
            //Register Console Commands
            //EnhancedSkyConsoleCommands.RegisterCommands();


            //if (DaggerfallUnity.Instance.IsReady)
            //    EnhancedSkyCurrentToggle = DaggerfallUnity.Settings.LypyL_EnhancedSky;


            // player starting outside & ESKY starting on
            if (playerEE != null && !playerEE.IsPlayerInside)
                ToggleEnhancedSky(EnhancedSkyCurrentToggle);


        }

        void OnDestroy()
        {
            ToggleSkyObjects(false);

            //Unsubscribe from events
            PlayerEnterExit.OnTransitionInterior -= InteriorTransitionEvent; //interior transition
            PlayerEnterExit.OnTransitionDungeonInterior -= InteriorTransitionEvent; //dungeon interior transition
            PlayerEnterExit.OnTransitionExterior -= ExteriorTransitionEvent; //exterior transition
            PlayerEnterExit.OnTransitionDungeonExterior -= ExteriorTransitionEvent; //dungeon exterior transition
            StreamingWorld.OnTeleportToCoordinates -= EnhancedSkyUpdate;
            WeatherManager.OnWeatherChange -= WeatherManagerSkyEventsHandler;

            StopAllCoroutines();
            if (_instance == this)
                _instance = null;
        }
        #endregion

        #region methods



        private bool GetRefrences()
        {
            try
            {
                if (!_cloudGen)
                    _cloudGen = this.GetComponent<CloudGenerator>();
                if (!_cloudGen)
                    _cloudGen = this.gameObject.AddComponent<CloudGenerator>();

                if (!dfallSky)
                    dfallSky = GameManager.Instance.SkyRig.gameObject;
                if (!playerEE)
                    playerEE = GameManager.Instance.PlayerEnterExit;
                if (!exteriorParent)
                    exteriorParent = GameManager.Instance.ExteriorParent;
                if (!weatherMan)
                    weatherMan = GameManager.Instance.WeatherManager;

                if (!postProcessingLayer)
                    postProcessingLayer = Camera.main.GetComponent<PostProcessLayer>();
            }
            catch
            {
                DaggerfallUnity.LogMessage("Error in SkyManager.GetRefrences()", true);
                return false;
            }
            if (dfallSky && playerEE && exteriorParent && weatherMan && _cloudGen && StarMaskMat && _skyObjMat && StarsMat && SkyMat)
                return true;
            else
                return false;

        }


        private Material GetInstanceMaterial()
        {
            return Instantiate(SkyObjMat);
        }

        private float UpdateTime()
        {
            try
            {
                return (TimeScript.MinuteOfDay * 60) + TimeScript.Second;
            }
            catch
            {
                GetRefrences();
                if (DfUnity != null && TimeScript != null)
                    return (TimeScript.MinuteOfDay * 60) + TimeScript.Second;
                else
                {
                    Debug.LogWarning("SkyManager couldn't UpdateTime");
                    return -1;
                }
            }
        }


        public void ToggleEnhancedSky(bool toggle)
        {
            if (!GetRefrences() && toggle)
            {
                DaggerfallUnity.LogMessage("Skymanager missing refrences, can't enable");
                return;

            }

            EnhancedSkyCurrentToggle = toggle;
            Debug.Log("before ToggleSkyObjects");
            ToggleSkyObjects(toggle);
        }


        public void SkyObjectSizeChange(SkyObjectSize size)
        {
            SkyObjectSizeSetting = size;
            if (!EnhancedSkyCurrentToggle || SkyMat == null)
            {
                //Debug.Log("Sky Material was null");
                return;
            }

            if (size == SkyObjectSizes.Normal)
                SkyMat.SetFloat("_SunSize", PresetContainer.SUNSIZENORMAL);
            else
                SkyMat.SetFloat("_SunSize", PresetContainer.SUNSIZELARGE);


            if (updateSkySettingsEvent != null)
                updateSkySettingsEvent();
        }


        public void SetCloudTextureResolution(int resolution)
        {
            if (resolution < PresetContainer.MINCLOUDDIMENSION)
                resolution = PresetContainer.MINCLOUDDIMENSION;
            else if (resolution > PresetContainer.MAXCLOUDDIMENSION)
                resolution = PresetContainer.MAXCLOUDDIMENSION;
            else
                cloudQuality = resolution;
            if (updateSkySettingsEvent != null)
                updateSkySettingsEvent();
        }

        #endregion

    }

}