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
        public const float noiseMapScaleClimateWoodlands = 8.0f;
        public const float noiseMapScaleClimateHauntedWoodlands = 5.0f;
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
            get { return 1; }
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

            float[,] noiseMapScaleClimate = new float[3, 3];
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    int mapPixelX = Math.Max(0, Math.Min(mx + x - 1, WoodsFile.mapWidthValue));
                    int mapPixelY = Math.Max(0, Math.Min(my + y - 1, WoodsFile.mapHeightValue));

                    noiseMapScaleClimate[x, y] = GetNoiseMapScaleBasedOnClimate(mapPixelX, mapPixelY);

                    //// test neighborhood for water noise map scales and dilate if so (prevent cliffs)
                    //bool waterInNeighborhood = false;
                    //for (int iy = Mathf.Max(0, my - 1); iy <= Mathf.Min(WoodsFile.mapHeightValue - 1, my + 1); iy++)
                    //{
                    //    for (int ix = Mathf.Max(0, mx - 1); ix <= Mathf.Min(WoodsFile.mapWidthValue - 1, mx + 1); ix++)
                    //    {
                    //        if (mx != ix || my != iy)
                    //        {
                    //            int worldClimate = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetClimateIndex(ix, iy);
                    //            if (worldClimate == (int)MapsFile.Climates.Ocean)
                    //            {
                    //                waterInNeighborhood = true;
                    //            }
                    //        }
                    //    }
                    //}
                    //if (waterInNeighborhood == true)
                    //{
                    //    noiseMapScaleClimate[x, y] = noiseMapScaleClimateOcean;
                    //}
                }
            }            

            int dimClimateMultiplier = 4;
            float[,] climateMultiplierValue = new float[dimClimateMultiplier, dimClimateMultiplier];
            for (int y = 0; y < dimClimateMultiplier; y++)
            {
                for (int x = 0; x < dimClimateMultiplier; x++)
                {
                    int sample1X = x / (dimClimateMultiplier / 2);
                    int sample1Y = y / (dimClimateMultiplier / 2);
                    int sample2X = sample1X + 1;
                    int sample2Y = sample1Y + 1;

                    float amountX = Math.Abs((float)(dimClimateMultiplier / 2) - x);
                    if (dimClimateMultiplier % 2 == 0)
                        amountX = Math.Max(amountX, Math.Abs((float)(dimClimateMultiplier / 2) - (x + 1f)));
                    float w1x = amountX / (float)(dimClimateMultiplier);
                    if (x >= dimClimateMultiplier / 2)
                        w1x = 1f - w1x;
                    float w2x = 1.0f - w1x;

                    float amountY = Math.Abs((float)(dimClimateMultiplier / 2) - y);
                    if (dimClimateMultiplier % 2 == 0)
                        amountY = Math.Max(amountY, Math.Abs((float)(dimClimateMultiplier / 2) - (y + 1f)));
                    float w1y = amountY / (float)(dimClimateMultiplier);
                    if (y >= dimClimateMultiplier / 2)
                        w1y = 1f - w1y;
                    float w2y = 1.0f - w1y;

                    climateMultiplierValue[x, y] = noiseMapScaleClimate[sample1X, sample1Y] * w1x * w1y +
                                                   noiseMapScaleClimate[sample2X, sample1Y] * w2x * w1y +
                                                   noiseMapScaleClimate[sample1X, sample2Y] * w1x * w2y +
                                                   noiseMapScaleClimate[sample2X, sample2Y] * w2x * w2y;
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
                    //x1 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 0] * climateMultiplierValue[0, 0], lhm[ix + 1, iy + 0] * climateMultiplierValue[1, 0], lhm[ix + 2, iy + 0] * climateMultiplierValue[2, 0], lhm[ix + 3, iy + 0] * climateMultiplierValue[3, 0], fracx);
                    //x2 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 1] * climateMultiplierValue[0, 1], lhm[ix + 1, iy + 1] * climateMultiplierValue[1, 1], lhm[ix + 2, iy + 1] * climateMultiplierValue[2, 1], lhm[ix + 3, iy + 1] * climateMultiplierValue[3, 1], fracx);
                    //x3 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 2] * climateMultiplierValue[0, 2], lhm[ix + 1, iy + 2] * climateMultiplierValue[1, 2], lhm[ix + 2, iy + 2] * climateMultiplierValue[2, 2], lhm[ix + 3, iy + 2] * climateMultiplierValue[3, 2], fracx);
                    //x4 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 3] * climateMultiplierValue[0, 3], lhm[ix + 1, iy + 3] * climateMultiplierValue[1, 3], lhm[ix + 2, iy + 3] * climateMultiplierValue[2, 3], lhm[ix + 3, iy + 3] * climateMultiplierValue[3, 3], fracx);
                    x1 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 0], lhm[ix + 1, iy + 0], lhm[ix + 2, iy + 0], lhm[ix + 3, iy + 0], fracx);
                    x2 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 1], lhm[ix + 1, iy + 1], lhm[ix + 2, iy + 1], lhm[ix + 3, iy + 1], fracx);
                    x3 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 2], lhm[ix + 1, iy + 2], lhm[ix + 2, iy + 2], lhm[ix + 3, iy + 2], fracx);
                    x4 = TerrainHelper.CubicInterpolator(lhm[ix, iy + 3], lhm[ix + 1, iy + 3], lhm[ix + 2, iy + 3], lhm[ix + 3, iy + 3], fracx);
                    noiseHeight = TerrainHelper.CubicInterpolator(x1, x2, x3, x4, fracy);

                    //x1 = TerrainHelper.CubicInterpolator(climateMultiplierValue[0, 3], climateMultiplierValue[1, 3], climateMultiplierValue[2, 3], climateMultiplierValue[3, 3], sfracx);
                    //x2 = TerrainHelper.CubicInterpolator(climateMultiplierValue[0, 2], climateMultiplierValue[1, 2], climateMultiplierValue[2, 2], climateMultiplierValue[3, 2], sfracx);
                    //x3 = TerrainHelper.CubicInterpolator(climateMultiplierValue[0, 1], climateMultiplierValue[1, 1], climateMultiplierValue[2, 1], climateMultiplierValue[3, 1], sfracx);
                    //x4 = TerrainHelper.CubicInterpolator(climateMultiplierValue[0, 0], climateMultiplierValue[1, 0], climateMultiplierValue[2, 0], climateMultiplierValue[3, 0], sfracx);
                    //float climateMultiplier = TerrainHelper.CubicInterpolator(x1, x2, x3, x4, sfracy);
                    float climateMultiplier = TerrainHelper.BilinearInterpolator(climateMultiplierValue[0, 3], climateMultiplierValue[0, 0], climateMultiplierValue[3, 3], climateMultiplierValue[3, 0], sfracx, sfracy);
                    if (scaledHeight <= scaledOceanElevation)
                        climateMultiplier = 1.0f;

                    scaledHeight += noiseHeight * climateMultiplier; // * noiseMapScale;

                    //float climateMultiplierX1 = TerrainHelper.CubicInterpolator(climateMultiplierValue[ix, iy + 0], climateMultiplierValue[ix + 1, iy + 0], climateMultiplierValue[ix + 2, iy + 0], climateMultiplierValue[ix + 3, iy + 0], sfracx);
                    //float climateMultiplierX2 = TerrainHelper.CubicInterpolator(climateMultiplierValue[ix, iy + 1], climateMultiplierValue[ix + 1, iy + 1], climateMultiplierValue[ix + 2, iy + 1], climateMultiplierValue[ix + 3, iy + 1], sfracx);
                    //float climateMultiplierX3 = TerrainHelper.CubicInterpolator(climateMultiplierValue[ix, iy + 2], climateMultiplierValue[ix + 1, iy + 2], climateMultiplierValue[ix + 2, iy + 2], climateMultiplierValue[ix + 3, iy + 2], sfracx);
                    //float climateMultiplierX4 = TerrainHelper.CubicInterpolator(climateMultiplierValue[ix, iy + 3], climateMultiplierValue[ix + 1, iy + 3], climateMultiplierValue[ix + 2, iy + 3], climateMultiplierValue[ix + 3, iy + 3], sfracx);
                    //float climateMultiplier = TerrainHelper.CubicInterpolator(climateMultiplierX1, climateMultiplierX2, climateMultiplierX3, climateMultiplierX4, sfracy);
                    

                    // Additional noise mask for small terrain features at ground level
                    // small terrain features' height scale should depend on climate of map pixel
                    float extraNoiseScale = GetExtraNoiseScaleBasedOnClimate(mx, my);
                    // prevent seams between different climate map pixels
                    if (x <= 0 || y <= 0 || x >= dim - 1 || y >= dim - 1)
                    {
                        //noiseMapScale = defaultNoiseMapScale;
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