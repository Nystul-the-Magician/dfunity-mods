using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features

using ReflectionsMod;

namespace ReflectionsMod
{
    public class _startupMod : MonoBehaviour
    {
        private static GameObject gameobjectReflectionsMod = null;
        private static UpdateReflectionTextures componentUpdateReflectionTextures = null;

        [Invoke(StateManager.StateTypes.Start)]
        public static void InitStart(InitParams initParams)
        {
            initMod();
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
            Debug.Log("init of ReflectionMod standalone");

            gameobjectReflectionsMod = new GameObject("ReflectionsMod");
            componentUpdateReflectionTextures = gameobjectReflectionsMod.AddComponent<UpdateReflectionTextures>();
        }
    }
}
