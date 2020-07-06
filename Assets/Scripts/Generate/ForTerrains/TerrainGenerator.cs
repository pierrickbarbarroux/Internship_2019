using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[ExecuteInEditMode]
/// <summary>
/// Ce script doit être placé sur le terrain référent.
/// Il sera par la suite ajouter à chaque terrain.
/// Cette classe paramètre les objets terrains (futures tuiles).
/// </summary>
public class TerrainGenerator : MonoBehaviour
{
    [Tooltip("Largeur (en x) du terain. On prendra 256.")]
    public int width;

    [Tooltip("Profondeur (en z) du terain. On pendra 256.")]
    public int depth; 

    [Tooltip("Distane entre le point le plus bas du terrain et le point le plus haut du terrain.")]
    public float height;

    //Ne sert à rien. 
    [HideInInspector]
    public float scale;

    //altitudes du terrain.
    float[] altitudes;

    //Altitudes maximale et minimale du terrain.
    float maxalt;
    [HideInInspector]
    public float minalt;

    /// <summary>
    /// Initialisation des paramètres du terrain : altitudes, largeur, longueur, collider, transform,...
    /// </summary>
    public void GenerateTerrainPublic()
    {
        altitudes = GetComponent<Tile>().altitudes;
        maxalt = Mathf.Max(altitudes);
        minalt = Mathf.Min(altitudes);
        height = maxalt - minalt;
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
        GetComponent<TerrainCollider>().terrainData = terrain.terrainData;
        transform.Translate(new Vector3(0, minalt, 0));
    }

    /// <summary>
    /// Prend les données d'un terrain et renvoie d'autres données modifiées.
    /// </summary>
    /// <param name="terrainData">Données sur lesquelles la fonction se base pour apporter des modifications.</param>
    /// <returns>
    /// Type : terrainData
    /// Données modifiées pour correspondre aux paramètre choisis dans l'inspecteur.
    /// </returns>
    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;

        terrainData.size = new Vector3(width, height, depth);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    /// <summary>
    /// Calcul l'altitude de chaque point du terraint.
    /// </summary>
    /// <returns>
    /// Type : float[,]
    /// Altitude de chaque point du terrain</returns>
    float[,] GenerateHeights()
    {
        float[,] heights = new float[width + 1, depth + 1];
        if (altitudes.Length != 0)
        {
            for (int z = 0; z <= depth; z++)
            {
                for (int x = 0; x <= width; x++)
                {
                    if (x == width && z == depth)
                    {
                        //Debug.Log("heights["+z+", "+x+"] : " + Mathf.InverseLerp(minalt, maxalt, altitudes[(x - 1) * 256 + z - 1]));
                        heights[z, x] = Mathf.InverseLerp(minalt, maxalt, altitudes[(x - 1) * 256 + z - 1]);
                    }
                    else if (x == width)
                    {
                        //Debug.Log("heights[" + z + ", " + x + "] : " + Mathf.InverseLerp(minalt, maxalt, altitudes[(x - 1) * 256 + z]));

                        heights[z, x] = Mathf.InverseLerp(minalt, maxalt, altitudes[(x - 1) * 256 + z]);
                    }
                    else if (z == depth)
                    {
                        //Debug.Log("heights[" + z + ", " + x + "] : " + Mathf.InverseLerp(minalt, maxalt, altitudes[x * 256 + z - 1]));

                        heights[z, x] = Mathf.InverseLerp(minalt, maxalt, altitudes[x * 256 + z - 1]);
                    }
                    else
                    {
                        //Debug.Log("heights[" + z + ", " + x + "] : " + Mathf.InverseLerp(minalt, maxalt, altitudes[x * 256 + z]));

                        heights[z, x] = Mathf.InverseLerp(minalt, maxalt, altitudes[x * 256 + z]);

                    }
                }
            }
            
        }
        return heights;
    }

    //Inutilisée.
    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / depth * scale;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    /// <summary>
    /// Lisse les bordures du terrain.
    /// </summary>
    public void SmouthEdge()
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData data = terrain.terrainData;
        if (terrain.bottomNeighbor != null)
        {
            for (int x = 0; x < width; x++)
            {
                //altitudes[(x - 1) * 256 + 0] = terrain.gameObject.GetComponent<TerrainGenerator>().HeightmapToAlt(heights_neighbor[depth, x]);
                altitudes[x * 256] = terrain.bottomNeighbor.gameObject.GetComponent<Tile>().altitudes[255 + (x * 256)];
            }
        }

        if (terrain.leftNeighbor != null)
        {
            for (int z = 0; z < depth; z++)
            {
                //Debug.Log(terrain.leftNeighbor.gameObject.name);
                //Debug.Log("altitude du voisin : " + terrain.leftNeighbor.gameObject.GetComponent<Tile>().altitudes[65280 + z]);
                //Debug.Log("altitude de la tuile : " + altitudes[z]);
                //altitudes[0 * 256 + z] = terrain.gameobject.getcomponent<terraingenerator>().heightmaptoalt(heights_neighbor[z, width]);
                altitudes[z] = terrain.leftNeighbor.gameObject.GetComponent<Tile>().altitudes[65280 + z];
            }
        }

        if (terrain.topNeighbor != null)
        {
            for (int x = 0; x < width; x++)
            {
                //altitudes[x * 256 + depth - 1] = terrain.gameObject.GetComponent<TerrainGenerator>().HeightmapToAlt(heights_neighbor[0, x]);
                altitudes[255 + x * 256] = terrain.topNeighbor.gameObject.GetComponent<Tile>().altitudes[x * 256];
            }
        }

        if (terrain.rightNeighbor != null)
        {
            for (int z = 0; z < depth; z++)
            {
                //altitudes[(width - 1) * 256 - z] = terrain.gameObject.GetComponent<TerrainGenerator>().HeightmapToAlt(heights_neighbor[z, 0]);
                altitudes[65280 + z] = terrain.rightNeighbor.gameObject.GetComponent<Tile>().altitudes[z];
            }
        }
        GetComponent<Tile>().altitudes = altitudes;
        maxalt = Mathf.Max(altitudes);
        minalt = Mathf.Min(altitudes);
        height = maxalt - minalt;
        data.size = new Vector3(width, height, depth);
        data.SetHeights(0, 0, GenerateHeights());
        transform.position = new Vector3(transform.position.x,minalt,transform.position.z);
        GetComponent<TerrainCollider>().terrainData = data;
    }

    /// <summary>
    /// Initialise les voisins du terrain. 
    /// Se fait normalement automatiquement lorsqu'on est en mode scène et qu'on utilise l'outil terrain.
    /// </summary>
    /// <param name="size">Nombre de tuiles d'un côté de la carte (cf xSize)</param>
    public static void FindAndSetNeighbors(int size)
    {
        GameObject[] terrains = TriPosition(GameObject.FindGameObjectsWithTag("Terrain_tag")); 

        for (int i = 0; i < terrains.Length; i++)
        {
                if ((i < size) && (i % size == 0)) //coin inférieur droit
                {
                    terrains[i].GetComponent<Terrain>().SetNeighbors(terrains[i + size].GetComponent<Terrain>(), terrains[i + 1].GetComponent<Terrain>(), null, null);
                }
                else if ((i < size) && ((i + 1) % size == 0)) //coin supérieur droit
                {
                    terrains[i].GetComponent<Terrain>().SetNeighbors(terrains[i + size].GetComponent<Terrain>(), null, null, terrains[i - 1].GetComponent<Terrain>());

                }
                else if ((i >= terrains.Length - size) && (i % size == 0))//coin inférieur gauche
                {
                    terrains[i].GetComponent<Terrain>().SetNeighbors(null, terrains[i + 1].GetComponent<Terrain>(), terrains[i - size].GetComponent<Terrain>(), null);

                }
                else if ((i >= terrains.Length - size) && ((i + 1) % size == 0)) //coin supérieur gauche
                {
                    terrains[i].GetComponent<Terrain>().SetNeighbors(null, null, terrains[i - size].GetComponent<Terrain>(), terrains[i - 1].GetComponent<Terrain>());

                }
                else if (i < size) //pas de voisin à droite
                {
                    terrains[i].GetComponent<Terrain>().SetNeighbors(terrains[i + size].GetComponent<Terrain>(), terrains[i + 1].GetComponent<Terrain>(), null, terrains[i - 1].GetComponent<Terrain>());

                }
                else if (i >= terrains.Length - size) //pas de voisin à gauche
                {
                    terrains[i].GetComponent<Terrain>().SetNeighbors(null, terrains[i + 1].GetComponent<Terrain>(), terrains[i - size].GetComponent<Terrain>(), terrains[i - 1].GetComponent<Terrain>());

                }
                else if (i % size == 0) //pas de voisin en bas
                {
                    terrains[i].GetComponent<Terrain>().SetNeighbors(terrains[i + size].GetComponent<Terrain>(), terrains[i + 1].GetComponent<Terrain>(), terrains[i - size].GetComponent<Terrain>(), null);
                }
                else if ((i + 1) % size == 0) //pas de voisin en haut
                {
                    terrains[i].GetComponent<Terrain>().SetNeighbors(terrains[i + size].GetComponent<Terrain>(), null, terrains[i - size].GetComponent<Terrain>(), terrains[i - 1].GetComponent<Terrain>());
                }
                else
                {
                    terrains[i].GetComponent<Terrain>().SetNeighbors(terrains[i + size].GetComponent<Terrain>(), terrains[i + 1].GetComponent<Terrain>(), terrains[i - size].GetComponent<Terrain>(), terrains[i - 1].GetComponent<Terrain>());
                }
        }
    }
    /// <summary>
    /// Tri les terrains selon leurs coordonnées dans la scène.
    /// Cette fonction est seulement utilisée pour l'attribution des voisins.
    /// </summary>
    /// <param name="mnts">Ensemble des terrains de la scène</param>
    /// <returns>liste triée contenant l'ensemble des terrains composant la scène</returns>
    static GameObject[] TriPosition(GameObject[] mnts)
    {
        int length = mnts.Length;
        int sqrt_length = (int)Mathf.Sqrt(length);
        GameObject[] ret = new GameObject[length];
        foreach (GameObject mnt in mnts)
        {
            int x = mnt.GetComponent<Tile>().position_x;
            int z = mnt.GetComponent<Tile>().position_z;
            ret[x * sqrt_length + z] = mnt;
        }
        return ret;
    }

    //Inutilisée
    public float HeightmapToAlt(float h)
    {
        return (minalt + h * height);
    }

    //Inutilisée
    public float AltToHeightmap(float a)
    {
        return Mathf.InverseLerp(minalt, maxalt, a);
    }

}
