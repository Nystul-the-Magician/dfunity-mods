//Distant Terrain Mod for Daggerfall-Unity
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: Michael Rauter (a.k.a. Nystul)
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)

using UnityEngine;
using System;
using System.Collections;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DistantTerrain;

//namespace DaggerfallWorkshop
namespace DistantTerrain
{
    /// <summary>
    /// Default TerrainSampler for StreamingWorld.
    /// </summary>
    public class ImprovedTerrainSampler : TerrainSampler
    {
        // Scale factors for this sampler implementation
        public const float baseHeightScale = 8f; //12f; //16f; // 8f;
        public const float maxNoiseMapScale = 15f; //4f; //15f; //4f;
        public const float defaultNoiseMapScale = 15f;
        public const float defaultExtraNoiseScale = 3f;
        // additional height noise based on climate      
        public const float noiseMapScaleClimateOcean = 0.0f;
        public const float noiseMapScaleClimateDesert = 1.25f;
        public const float noiseMapScaleClimateDesert2 = 10.0f;
        public const float noiseMapScaleClimateMountain = 15.0f;
        public const float noiseMapScaleClimateRainforest = 7.5f;
        public const float noiseMapScaleClimateSwamp = 2.5f;
        public const float noiseMapScaleClimateSubtropical = 3.25f;
        public const float noiseMapScaleClimateMountainWoods = 12.5f;
        public const float noiseMapScaleClimateWoodlands = 10.0f;
        public const float noiseMapScaleClimateHauntedWoodlands = 8.0f;
        // extra noise scale based on climate
        public const float extraNoiseScaleClimateOcean = 0.0f;
        public const float extraNoiseScaleClimateDesert = 7f;
        public const float extraNoiseScaleClimateDesert2 = 19f;
        public const float extraNoiseScaleClimateMountain = 62f;
        public const float extraNoiseScaleClimateRainforest = 16f;
        public const float extraNoiseScaleClimateSwamp = 9f;
        public const float extraNoiseScaleClimateSubtropical = 17f;
        public const float extraNoiseScaleClimateMountainWoods = 32f;
        public const float extraNoiseScaleClimateWoodlands = 24f;
        public const float extraNoiseScaleClimateHauntedWoodlands = 22f;
        //public const float extraNoiseScale = 3f; //10f; //3f;
        public const float scaledOceanElevation = 3.4f * baseHeightScale;
        public const float scaledBeachElevation = 5.0f * baseHeightScale;

        // Max terrain height of this sampler implementation
        public const float maxTerrainHeight = ImprovedWorldTerrain.maxHeightsExaggerationMultiplier * baseHeightScale * 128 + maxNoiseMapScale * 128 + 128;//1380f; //26115f;

        public override int Version
        {
            get { return 2; }
        }

        public ImprovedTerrainSampler()
        {
            HeightmapDimension = defaultHeightmapDimension;
            MaxTerrainHeight = maxTerrainHeight;
            OceanElevation = scaledOceanElevation;
            BeachElevation = scaledBeachElevation;
        }

        public override float TerrainHeightScale(int x, int y)
        {
            return ImprovedWorldTerrain.computeHeightMultiplier(x, y) * ImprovedTerrainSampler.baseHeightScale + this.GetNoiseMapScaleBasedOnClimate(x, y);
        }



        public override void GenerateSamples(ref MapPixelData mapPixel)
        {
            DaggerfallUnity dfUnity = DaggerfallUnity.Instance;

            // Create samples arrays
            mapPixel.tilemapSamples = new TilemapSample[MapsFile.WorldMapTileDim, MapsFile.WorldMapTileDim];
            mapPixel.heightmapSamples = new float[HeightmapDimension, HeightmapDimension];
            
            // Divisor ensures continuous 0-1 range of tile samples
            float div = (float)(HeightmapDimension - 1) / 3f;

            // Read neighbouring height samples for this map pixel
            int mx = mapPixel.mapPixelX;
            int my = mapPixel.mapPixelY;

            // Seed random with terrain key
            UnityEngine.Random.InitState(TerrainHelper.MakeTerrainKey(mx, my));

            byte[,] shm = dfUnity.ContentReader.WoodsFileReader.GetHeightMapValuesRange(mx - 2, my - 2, 4);
            byte[,] lhm = dfUnity.ContentReader.WoodsFileReader.GetLargeHeightMapValuesRange(mx - 1, my, 3);

            float[,] multiplierValue = new float[4, 4];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    int mapPixelX = Math.Max(0, Math.Min(mx + x - 2, WoodsFile.mapWidthValue));
                    int mapPixelY = Math.Max(0, Math.Min(my + y - 2, WoodsFile.mapHeightValue));

                    multiplierValue[x, y] = ImprovedWorldTerrain.computeHeightMultiplier(mapPixelX, mapPixelY);
                }
            }

            float[,] waterMap = new float[4, 4];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (shm[x, y] <= 2)
                        waterMap[x, y] = 0.0f;
                    else
                        waterMap[x, y] = 1.0f;
                }
            }

            float[,] climateMap = new float[4, 4];
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    int mapPixelX = Math.Max(0, Math.Min(mx + x - 1, WoodsFile.mapWidthValue));
                    int mapPixelY = Math.Max(0, Math.Min(my + y - 1, WoodsFile.mapHeightValue));
                    climateMap[x, y] = GetNoiseMapScaleBasedOnClimate(mapPixelX, mapPixelY);
                }
            }         

            // Extract height samples for all chunks
            float averageHeight = 0;
            float maxHeight = float.MinValue;
            float baseHeight, noiseHeight;
            float x1, x2, x3, x4;
            int dim = HeightmapDimension;
            //mapPixel.heightmapSamples = new float[dim, dim];
            for (int y = 0; y < dim; y++)
            {
                for (int x = 0; x < dim; x++)
                {
                    float rx = (float)x / div;
                    float ry = (float)y / div;
                    int ix = Mathf.FloorToInt(rx);
                    int iy = Mathf.FloorToInt(ry);
                    float sfracx = (float)x / (float)(dim - 1);
                    float sfracy = (float)y / (float)(dim - 1);
                    float fracx = (float)(x - ix * div) / div;
                    float fracy = (float)(y - iy * div) / div;
                    float scaledHeight = 0;

                    // Bicubic sample small height map for base terrain elevation
                    x1 = TerrainHelper.CubicInterpolator(shm[0, 3] * multiplierValue[0, 3], shm[1, 3] * multiplierValue[1, 3], shm[2, 3] * multiplierValue[2, 3], shm[3, 3] * multiplierValue[3, 3], sfracx);
                    x2 = TerrainHelper.CubicInterpolator(shm[0, 2] * multiplierValue[0, 2], shm[1, 2] * multiplierValue[1, 2], shm[2, 2] * multiplierValue[2, 2], shm[3, 2] * multiplierValue[3, 2], sfracx);
                    x3 = TerrainHelper.CubicInterpolator(shm[0, 1] * multiplierValue[0, 1], shm[1, 1] * multiplierValue[1, 1], shm[2, 1] * multiplierValue[2, 1], shm[3, 1] * multiplierValue[3, 1], sfracx);
                    x4 = TerrainHelper.CubicInterpolator(shm[0, 0] * multiplierValue[0, 0], shm[1, 0] * multiplierValue[1, 0], shm[2, 0] * multiplierValue[2, 0], shm[3, 0] * multiplierValue[3, 0], sfracx);
                    baseHeight = TerrainHelper.CubicInterpolator(x1, x2, x3, x4, sfracy);
                    scaledHeight += baseHeight * baseHeightScale;

                    // Bicubic sample large height map for noise mask over terrain features
                    x1 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 0], lhm[ix + 1, iy + 0], lhm[ix + 2, iy + 0], lhm[ix + 3, iy + 0], fracx);
                    x2 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 1], lhm[ix + 1, iy + 1], lhm[ix + 2, iy + 1], lhm[ix + 3, iy + 1], fracx);
                    x3 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 2], lhm[ix + 1, iy + 2], lhm[ix + 2, iy + 2], lhm[ix + 3, iy + 2], fracx);
                    x4 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 3], lhm[ix + 1, iy + 3], lhm[ix + 2, iy + 3], lhm[ix + 3, iy + 3], fracx);
                    noiseHeight = TerrainHelper.CubicInterpolator(x1, x2, x3, x4, fracy);

                    x1 = TerrainHelper.CubicInterpolator(waterMap[0, 3], waterMap[1, 3], waterMap[2, 3], waterMap[3, 3], sfracx);
                    x2 = TerrainHelper.CubicInterpolator(waterMap[0, 2], waterMap[1, 2], waterMap[2, 2], waterMap[3, 2], sfracx);
                    x3 = TerrainHelper.CubicInterpolator(waterMap[0, 1], waterMap[1, 1], waterMap[2, 1], waterMap[3, 1], sfracx);
                    x4 = TerrainHelper.CubicInterpolator(waterMap[0, 0], waterMap[1, 0], waterMap[2, 0], waterMap[3, 0], sfracx);
                    float waterMultiplier = TerrainHelper.CubicInterpolator(x1, x2, x3, x4, sfracy);

                    //float climateMultiplier = TerrainHelper.BilinearInterpolator(climateMultiplierValue[0, 3], climateMultiplierValue[0, 0], climateMultiplierValue[3, 3], climateMultiplierValue[3, 0], sfracx, sfracy);
                    x1 = TerrainHelper.CubicInterpolator(climateMap[0, 3], climateMap[1, 3], climateMap[2, 3], climateMap[3, 3], sfracx);
                    x2 = TerrainHelper.CubicInterpolator(climateMap[0, 2], climateMap[1, 2], climateMap[2, 2], climateMap[3, 2], sfracx);
                    x3 = TerrainHelper.CubicInterpolator(climateMap[0, 1], climateMap[1, 1], climateMap[2, 1], climateMap[3, 1], sfracx);
                    x4 = TerrainHelper.CubicInterpolator(climateMap[0, 0], climateMap[1, 0], climateMap[2, 0], climateMap[3, 0], sfracx);
                    float climateMultiplier = TerrainHelper.CubicInterpolator(x1, x2, x3, x4, sfracy);

                    scaledHeight += noiseHeight * waterMultiplier * climateMultiplier;

                    // Additional noise mask for small terrain features at ground level
                    // small terrain features' height scale should depend on climate of map pixel
                    float extraNoiseScale = GetExtraNoiseScaleBasedOnClimate(mx, my);
                    // prevent seams between different climate map pixels
                    if (x <= 0 || y <= 0 || x >= dim - 1 || y >= dim - 1)
                    {                        
                        extraNoiseScale = defaultExtraNoiseScale;
                    }
                    int noisex = mapPixel.mapPixelX * (HeightmapDimension - 1) + x;
                    int noisey = (MapsFile.MaxMapPixelY - mapPixel.mapPixelY) * (HeightmapDimension - 1) + y;
                    float lowFreq = TerrainHelper.GetNoise(noisex, noisey, 0.3f, 0.5f, 0.5f, 1);
                    float highFreq = TerrainHelper.GetNoise(noisex, noisey, 0.9f, 0.5f, 0.5f, 1);                    
                    scaledHeight += (lowFreq * highFreq) * extraNoiseScale;

                    // Clamp lower values to ocean elevation
                    if (scaledHeight < scaledOceanElevation)
                        scaledHeight = scaledOceanElevation;

                    // Accumulate average height
                    averageHeight += scaledHeight;

                    // Get max height
                    if (scaledHeight > maxHeight)
                        maxHeight = scaledHeight;

                    // Set sample
                    float height = Mathf.Clamp01(scaledHeight / MaxTerrainHeight);
                    mapPixel.heightmapSamples[y, x] = height;
                }
            }

            // Average and max heights are passed back for locations
            mapPixel.averageHeight = (averageHeight /= (float)(dim * dim)) / MaxTerrainHeight;
            mapPixel.maxHeight = maxHeight / MaxTerrainHeight;
        }

        private float GetNoiseMapScaleBasedOnClimate(int mapPixelX, int mapPixelY)
        {
            int worldClimate = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetClimateIndex(mapPixelX, mapPixelY);
            float noiseMapScaleClimate = defaultNoiseMapScale;
            switch (worldClimate)
            {
                case (int)MapsFile.Climates.Ocean:
                    noiseMapScaleClimate = noiseMapScaleClimateOcean;
                    break;
                case (int)MapsFile.Climates.Desert:
                    noiseMapScaleClimate = noiseMapScaleClimateDesert;
                    break;
                case (int)MapsFile.Climates.Desert2:
                    noiseMapScaleClimate = noiseMapScaleClimateDesert2;
                    break;
                case (int)MapsFile.Climates.Mountain:
                    noiseMapScaleClimate = noiseMapScaleClimateMountain;
                    break;
                case (int)MapsFile.Climates.Rainforest:
                    noiseMapScaleClimate = noiseMapScaleClimateRainforest;
                    break;
                case (int)MapsFile.Climates.Swamp:
                    noiseMapScaleClimate = noiseMapScaleClimateSwamp;
                    break;
                case (int)MapsFile.Climates.Subtropical:
                    noiseMapScaleClimate = noiseMapScaleClimateSubtropical;
                    break;
                case (int)MapsFile.Climates.MountainWoods:
                    noiseMapScaleClimate = noiseMapScaleClimateMountainWoods;
                    break;
                case (int)MapsFile.Climates.Woodlands:
                    noiseMapScaleClimate = noiseMapScaleClimateWoodlands;
                    break;
                case (int)MapsFile.Climates.HauntedWoodlands:
                    noiseMapScaleClimate = noiseMapScaleClimateHauntedWoodlands;
                    break;
            }
            return noiseMapScaleClimate;
        }

        private float GetExtraNoiseScaleBasedOnClimate(int mapPixelX, int mapPixelY)
        {
            // small terrain features' height should depend on climate of map pixel
            float extraNoiseScale = defaultExtraNoiseScale;
            int worldClimate = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetClimateIndex(mapPixelX, mapPixelY);

            switch (worldClimate)
            {
                case (int)MapsFile.Climates.Ocean:
                    extraNoiseScale = extraNoiseScaleClimateOcean;
                    break;
                case (int)MapsFile.Climates.Desert:
                    extraNoiseScale = extraNoiseScaleClimateDesert;
                    break;
                case (int)MapsFile.Climates.Desert2:
                    extraNoiseScale = extraNoiseScaleClimateDesert2;
                    break;
                case (int)MapsFile.Climates.Mountain:
                    extraNoiseScale = extraNoiseScaleClimateMountain;
                    break;
                case (int)MapsFile.Climates.Rainforest:
                    extraNoiseScale = extraNoiseScaleClimateRainforest;
                    break;
                case (int)MapsFile.Climates.Swamp:
                    extraNoiseScale = extraNoiseScaleClimateSwamp;
                    break;
                case (int)MapsFile.Climates.Subtropical:
                    extraNoiseScale = extraNoiseScaleClimateSubtropical;
                    break;
                case (int)MapsFile.Climates.MountainWoods:                    
                    extraNoiseScale = extraNoiseScaleClimateMountainWoods;
                    break;
                case (int)MapsFile.Climates.Woodlands:                    
                    extraNoiseScale = extraNoiseScaleClimateWoodlands;
                    break;
                case (int)MapsFile.Climates.HauntedWoodlands:                    
                    extraNoiseScale = extraNoiseScaleClimateHauntedWoodlands;
                    break;
            }
            return extraNoiseScale;
        }
    }
}