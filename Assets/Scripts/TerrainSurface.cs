using UnityEngine;
using System.Collections;
public class TerrainSurface
{
    public static float[] GetTextureMix(Vector3 worldPos)
    {
        Terrain ter = Terrain.activeTerrain;
        TerrainData terData = ter.terrainData;
        Vector3 terPos = ter.transform.position;
// calculate which splat map cell the worldPos falls within (ignoring y)
        int mapX = (int)(((worldPos.x-terPos.x)/terData.size.x) * terData.alphamapWidth);
        int mapZ = (int)(((worldPos.z-terPos.z)/terData.size.z) * terData.alphamapHeight);
// get the splat data for this cell as 1x1xN 3d array (where N=number of textures)
        float[,,] splatMapData = terData.GetAlphamaps(mapX,mapZ,1,1);
// extract the 3D array data to a 1D array:
        float[] cellMix = new float[splatMapData.GetUpperBound(2)+1];
        for (int n=0; n < cellMix.Length; ++n)
                cellMix[n] = splatMapData[0,0,n];
        return cellMix;
    }
    public static int GetMainTexture(Vector3 worldPos)
    {
            float[] mix = GetTextureMix(worldPos);
            float maxMix = 0;
            int maxIndex = 0;
// loop through each mix value and find the maximum
            for (int n=0; n < mix.Length; ++n)
            { if (mix[n] > maxMix)
                    { maxIndex = n;
                            maxMix = mix[n];
                    }
            }
            return maxIndex;
    }
}