using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AllGenerateInEditorMode : MonoBehaviour
{
    public GameObject terrain_ref;
    public int Size;
    public Material default_mat;
    public GameObject[] myCrops;


    GameObject terrain_bas_gauche;
    float lon_diff;
    float lat_diff;
    int xSize;
    int zSize;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("it works !!!");
        lon_diff = terrain_ref.GetComponent<Tile>().right_up_x - terrain_ref.GetComponent<Tile>().left_down_x;
        lat_diff = terrain_ref.GetComponent<Tile>().right_up_y - terrain_ref.GetComponent<Tile>().left_down_y;

        zSize = 2 * Size - 1;
        xSize = 2 * Size - 1;
        terrain_ref.GetComponent<Tile>().position_x = Size - 1;
        terrain_ref.GetComponent<Tile>().position_z = Size - 1;
        terrain_ref.name = "Terrain_X:" + (Size - 1) + "_Z:" + (Size - 1);

        terrain_ref.transform.Translate(new Vector3(-(Size - 1) * int.Parse(terrain_ref.GetComponent<Tile>().height), 0, (Size - 1) * int.Parse(terrain_ref.GetComponent<Tile>().width)));

        terrain_bas_gauche = Instantiate(terrain_ref, new Vector3(0, 0, 0), Quaternion.identity);
        terrain_bas_gauche.GetComponent<Terrain>().terrainData = new TerrainData();
        terrain_bas_gauche.GetComponent<Terrain>().materialTemplate = default_mat;
        terrain_bas_gauche.GetComponent<TerrainGenerator>().width = terrain_ref.GetComponent<TerrainGenerator>().width;
        terrain_bas_gauche.GetComponent<TerrainGenerator>().depth = terrain_ref.GetComponent<TerrainGenerator>().depth;
        terrain_bas_gauche.GetComponent<TerrainGenerator>().height = terrain_ref.GetComponent<TerrainGenerator>().height;
        terrain_bas_gauche.GetComponent<TerrainGenerator>().scale = terrain_ref.GetComponent<TerrainGenerator>().scale;
        terrain_bas_gauche.GetComponent<Tile>().is_ref = false;
        terrain_bas_gauche.GetComponent<Tile>().left_down_x = terrain_ref.GetComponent<Tile>().left_down_x - ((Size - 1) * lon_diff);
        terrain_bas_gauche.GetComponent<Tile>().left_down_y = terrain_ref.GetComponent<Tile>().left_down_y - ((Size - 1) * lat_diff);
        terrain_bas_gauche.GetComponent<Tile>().right_up_x = terrain_ref.GetComponent<Tile>().right_up_x - ((Size - 1) * lon_diff);
        terrain_bas_gauche.GetComponent<Tile>().right_up_y = terrain_ref.GetComponent<Tile>().right_up_y - ((Size - 1) * lat_diff);
        terrain_bas_gauche.GetComponent<Tile>().position_z = 0;
        terrain_bas_gauche.GetComponent<Tile>().position_x = 0;


        terrain_bas_gauche.name = "Terrain_X:" + 0 + "_Z:" + 0;
        terrain_bas_gauche.GetComponent<Tile>().GetAlt2();
        LoadEverything();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LoadEverything()
    {
        //StartCoroutine(CreateTiles());
        //yield return null;
        //StartCoroutine(GenerateAllTerrain());
        //yield return null;

        //StartCoroutine(SmoothAll());
        //yield return null;

        //StartCoroutine(GenerateAllOrtho());
        //yield return null;

        //StartCoroutine(GenerateAllField());
        //yield return null;

        //StartCoroutine(GenerateAllForest());
        //yield return null;

        //StartCoroutine(SpawnAllForest());
        //yield return null;

        //StartCoroutine(SpawnAllVege());
        //yield return null;

        //StartCoroutine(GenerateField.SpawnField(myCrops));
        CreateTiles();
        GenerateAllTerrain();
        SmoothAll();
        GenerateAllOrtho();
        GenerateAllField();
        GenerateAllForest();
        SpawnAllForest();
        SpawnAllVege();
        Debug.Log("Done");
    }

    void CreateTiles()
    {
        for (int z = 0; z < xSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (z == Size - 1 && x == Size - 1)
                {
                    StartCoroutine(terrain_ref.GetComponent<Tile>().GetAlt());
                }
                else if (x != 0 || z != 0)
                {
                    GameObject new_terrain = Instantiate(terrain_bas_gauche, new Vector3(-z * (int.Parse(terrain_bas_gauche.GetComponent<Tile>().height)), 0, x * (int.Parse(terrain_bas_gauche.GetComponent<Tile>().width))) + terrain_bas_gauche.transform.position, Quaternion.identity);
                    new_terrain.tag = "Terrain_tag";
                    new_terrain.GetComponent<Terrain>().terrainData = new TerrainData();
                    new_terrain.GetComponent<Terrain>().materialTemplate = default_mat;
                    new_terrain.GetComponent<Tile>().layer = terrain_ref.GetComponent<Tile>().layer;
                    new_terrain.GetComponent<Tile>().height = terrain_ref.GetComponent<Tile>().height;
                    new_terrain.GetComponent<Tile>().width = terrain_ref.GetComponent<Tile>().width;
                    new_terrain.GetComponent<Tile>().format = terrain_ref.GetComponent<Tile>().format;
                    new_terrain.GetComponent<Tile>().is_ref = false;
                    new_terrain.GetComponent<Tile>().left_down_x = terrain_bas_gauche.GetComponent<Tile>().left_down_x + (x * lon_diff);
                    new_terrain.GetComponent<Tile>().left_down_y = terrain_bas_gauche.GetComponent<Tile>().left_down_y + (z * lat_diff);
                    new_terrain.GetComponent<Tile>().right_up_x = terrain_bas_gauche.GetComponent<Tile>().right_up_x + (x * lon_diff);
                    new_terrain.GetComponent<Tile>().right_up_y = terrain_bas_gauche.GetComponent<Tile>().right_up_y + (z * lat_diff);
                    new_terrain.GetComponent<Tile>().position_z = ((int)new_terrain.transform.localPosition.z) / (int.Parse(terrain_ref.GetComponent<Tile>().width) - 1);
                    new_terrain.GetComponent<Tile>().position_x = -((int)new_terrain.transform.localPosition.x) / (int.Parse(terrain_ref.GetComponent<Tile>().height) - 1);
                    //StartCoroutine(new_terrain.GetComponent<Tile>().GetAlt());
                    new_terrain.GetComponent<Tile>().GetAlt2();
                    new_terrain.name = "Terrain_X:" + new_terrain.GetComponent<Tile>().position_x + "_Z:" + new_terrain.GetComponent<Tile>().position_z;
                    //yield return null;
                }
            }
        }
    }

    void GenerateAllTerrain()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            mnt.GetComponent<TerrainGenerator>().GenerateTerrainPublic();
            //yield return null;
        }
        TerrainGenerator.FindAndSetNeighbors(xSize);

        //yield return null;
    }
    void SmoothAll()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            Debug.Log(mnt.name);
            mnt.GetComponent<TerrainGenerator>().SmouthEdge();
            //yield return null;
        }
        //yield return null;
    }


    void GenerateAllOrtho()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<Tile>().UpdateSkinTerrain(mnt, default_mat));
            //yield return null;
        }
    }


    void GenerateAllField()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateField>().GetPointsField(mnt));
            //yield return null;
        }
    }

    void GenerateAllForest()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateForest>().GetPointsForest(mnt));
            //yield return null;
        }
    }

    void GenerateAllVege()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateVegetation>().GetPointsVege(mnt));
            //yield return null;
        }
    }

    void SpawnAllForest()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            Debug.Log(mnt.name);
            mnt.GetComponent<GenerateForest>().SpawnTrees(mnt);
            //yield return null;
        }
    }

    void SpawnAllVege()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Terrain_tag");
        foreach (GameObject mnt in MNTs)
        {
            mnt.GetComponent<GenerateVegetation>().SpawnVeges(mnt);
            //yield return null;
        }
    }

}
