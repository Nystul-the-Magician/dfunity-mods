//Enhanced Sky for Daggerfall Tools for Unity by Lypyl, contact at lypyl@dfworkshop.net
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: LypyL
///Contact: Lypyl@dfworkshop.net
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)

using UnityEngine;

namespace EnhancedSky
{
    public class PresetContainer : MonoBehaviour
    {
        public const int MOONSCALENORMAL = 4;
        public const int MOONSCALELARGE = 8;
        //public const float SUNSIZENORMAL = 0.11F;     //sizes for simple sun disk
        //public const float SUNSIZELARGE = 0.145F;
        public const float SUNSIZENORMAL = 0.05F;
        public const float SUNSIZELARGE = 0.06F;

        public const float SUNFLARESIZENORMAL = 0.57F;
        public const float SUNFLARESIZELARGE = 0.80F; 

        public const int MAXCLOUDDIMENSION = 1500;
        public const int MINCLOUDDIMENSION = 1;

        public static PresetContainer _instance;

        public Gradient colorBase;
        public Gradient colorOver;

        public Gradient fogBase;
        public Gradient fogOver;

        public Gradient starGradient;

        public Gradient cloudNoiseBase;
        public Gradient cloudNoiseOver;

        public AnimationCurve atmosphereBase;
        public AnimationCurve atmosphereOver;

        public AnimationCurve moonAlphaBase;
        public AnimationCurve moonAlphaOver;

        public Color skyTint;
        public Color MasserColor = new Color(.5216f, .5216f, .5216f);
        public Color SecundaColor = new Color(.7647f, .7647f, .7647f);

        public float atmsphrOffset = .5f;

        public static PresetContainer Instance { get { return (_instance != null) ? _instance : _instance = FindPreset(); } private set { _instance = value;} }

        void Awake()
        {
            if (_instance != null)
                this.enabled = false;
            _instance = this;
        }

        void Destroy()
        {
            Instance = null;
        }

        /*
        void Start()
        {
            Gradient gradient = new Gradient(); // this.colorBase;
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
            this.colorBase = gradient;
        }
        */

        private static PresetContainer FindPreset()
        {
            PresetContainer pc = GameObject.FindObjectOfType<PresetContainer>();
            if (pc == null)
            {
                DaggerfallWorkshop.DaggerfallUnity.LogMessage("Could not locate PresetContainer in scene");
                return null;

            }
            else
            {
                return pc;
            }
                

        }

    }
}