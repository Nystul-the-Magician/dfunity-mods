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

        private static Shader shaderTilemapWithReflections = null;
        private static Shader shaderTilemapTextureArrayWithReflections = null;
        private static Shader shaderCreateLookupReflectionTextureCoordinates = null;
        private static Shader shaderCreateLookupReflectionTextureIndex = null;
        private static Shader shaderDeferredPlanarReflections = null;
        private static Shader shaderDungeonWaterWithReflections = null;
        private static Shader shaderInvisible = null; // used for reflection planes (which must be rendered for reflections to be generated, but should not render anything visible)
        private static Texture2D textureTileatlasReflective = null;
        private static Texture2D textureTileatlasReflectiveRaining = null;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            gameobjectRealtimeReflections = new GameObject("RealtimeReflections");
            componentUpdateReflectionTextures = gameobjectRealtimeReflections.AddComponent<UpdateReflectionTextures>();

            ModSettings settings = mod.GetSettings();

            shaderTilemapWithReflections = mod.GetAsset<Shader>("Shaders/DaggerfallTilemapWithReflections.shader");
            shaderTilemapTextureArrayWithReflections = mod.GetAsset<Shader>("Shaders/DaggerfallTilemapTextureArrayWithReflections.shader");
            shaderCreateLookupReflectionTextureCoordinates = mod.GetAsset<Shader>("Shaders/CreateLookupReflectionTextureCoordinates.shader");
            shaderCreateLookupReflectionTextureIndex = mod.GetAsset<Shader>("Shaders/CreateLookupReflectionTextureIndex.shader");
            shaderDeferredPlanarReflections = mod.GetAsset<Shader>("Shaders/DeferredPlanarReflections.shader");
            shaderDungeonWaterWithReflections = mod.GetAsset<Shader>("Shaders/DungeonWaterWithReflections.shader");
            shaderInvisible = mod.GetAsset<Shader>("Shaders/Invisible.shader");
            textureTileatlasReflective = mod.GetAsset<Texture2D>("Resources/tileatlas_reflective");
            textureTileatlasReflectiveRaining = mod.GetAsset<Texture2D>("Resources/tileatlas_reflective_raining");

            componentUpdateReflectionTextures.IsEnabledOutdoorGroundReflections = settings.GetValue<bool>("GeneralSettings", "OutdoorGroundReflections");
            componentUpdateReflectionTextures.IsEnabledOutdoorSeaReflections = settings.GetValue<bool>("GeneralSettings", "OutdoorSeaReflections");
            componentUpdateReflectionTextures.IsEnabledIndoorBuildingFloorReflections = settings.GetValue<bool>("GeneralSettings", "IndoorBuildingFloorReflections");
            componentUpdateReflectionTextures.IsEnabledIndoorBuildingLowerLevelReflection = settings.GetValue<bool>("GeneralSettings", "IndoorBuildingLowerLevelReflection");
            componentUpdateReflectionTextures.IsEnabledDungeonGroundReflections = settings.GetValue<bool>("GeneralSettings", "DungeonGroundReflections");
            componentUpdateReflectionTextures.IsEnabledDungeonWaterReflections = settings.GetValue<bool>("GeneralSettings", "DungeonWaterReflections");
            componentUpdateReflectionTextures.IsFeatureEnabledFakeParallaxReflections = settings.GetValue<bool>("Features", "FakeParallaxReflections");
            componentUpdateReflectionTextures.FloorReflectionTextureWidth = settings.GetValue<int>("FloorReflectionTexture", "width");
            componentUpdateReflectionTextures.FloorReflectionTextureHeight = settings.GetValue<int>("FloorReflectionTexture", "height");
            componentUpdateReflectionTextures.LowerLevelReflectionTextureWidth = settings.GetValue<int>("LowerLevelReflectionTexture", "width");
            componentUpdateReflectionTextures.LowerLevelReflectionTextureHeight = settings.GetValue<int>("LowerLevelReflectionTexture", "height");
            componentUpdateReflectionTextures.RoughnessMultiplier = settings.GetValue<float>("ReflectionParameters", "roughnessMultiplier");
            componentUpdateReflectionTextures.ShaderTilemapWithReflections = shaderTilemapWithReflections;
            componentUpdateReflectionTextures.ShaderTilemapTextureArrayWithReflections = shaderTilemapTextureArrayWithReflections;
            componentUpdateReflectionTextures.ShaderCreateLookupReflectionTextureCoordinates = shaderCreateLookupReflectionTextureCoordinates;
            componentUpdateReflectionTextures.ShaderCreateLookupReflectionTextureIndex = shaderCreateLookupReflectionTextureIndex;
            componentUpdateReflectionTextures.ShaderDeferredPlanarReflections = shaderDeferredPlanarReflections;
            componentUpdateReflectionTextures.ShaderDungeonWaterWithReflections = shaderDungeonWaterWithReflections;
            componentUpdateReflectionTextures.ShaderInvisible = shaderInvisible;
            componentUpdateReflectionTextures.TextureTileatlasReflective = textureTileatlasReflective;
            componentUpdateReflectionTextures.TextureTileatlasReflectiveRaining = textureTileatlasReflectiveRaining;
        }

        void Awake()
        {
            mod.IsReady = true;
        }
    }
}
