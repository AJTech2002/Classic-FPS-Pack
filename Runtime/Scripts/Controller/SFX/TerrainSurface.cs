using UnityEngine;
using System.Collections;


namespace ClassicFPS.Controller.SFX
{
    //Helper function to get the texture of the Player to change the footstep sounds
    
    public class TerrainSurface
    {

        public static float[] GetTextureMix(Vector3 worldPos)
        {
            Terrain terrain = Terrain.activeTerrain;

            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                Vector3 terrainPos = terrain.transform.position;

                int mapX = (int)(((worldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
                int mapZ = (int)(((worldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

                float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

                float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
                for (int n = 0; n < cellMix.Length; ++n)
                {
                    cellMix[n] = splatmapData[0, 0, n];
                }

                return cellMix;
            }

            return new float[0];
        }

        public static string GetMainTexture(Vector3 worldPos)
        {

            Terrain terrain = Terrain.activeTerrain;

            if (terrain != null)
            {
                float[] mix = GetTextureMix(worldPos);
                float maxMix = 0;
                int maxIndex = 0;

                for (int n = 0; n < mix.Length; ++n)
                {
                    if (mix[n] > maxMix)
                    {
                        maxIndex = n;
                        maxMix = mix[n];
                    }
                }

                if (maxIndex <= terrain.terrainData.terrainLayers.Length - 1)
                    return terrain.terrainData.terrainLayers[maxIndex].name;
                else
                    return "";
            }
            else
            {
                return "";
            }

        }

    }
}