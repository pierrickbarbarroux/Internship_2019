using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe n'est plus utilisée.
/// Cette classe servait à générer le MNT pour les tuiles (seulement pour les mesh, pas pour les terrains)
/// </summary>
public class GenerateTiles : MonoBehaviour
{

    public GameObject mesh_ref;
    public Tile tile_ref;
    public Camera mycamera;

    GameObject mesh_bas_gauche;

    public int lim_x;
    public int lim_z;

    float lon_diff;
    float lat_diff;

    int xSize;
    int zSize;

    public int Size;

    public string row_ref;
    public string col_ref;

    public GameObject[] myCrops;
    public GameObject[] myGraves;


    void Start()
    {

        lon_diff = mesh_ref.GetComponent<Tile>().right_up_x - mesh_ref.GetComponent<Tile>().left_down_x;
        lat_diff = mesh_ref.GetComponent<Tile>().right_up_y - mesh_ref.GetComponent<Tile>().left_down_y;

        zSize = 2 * Size - 1;
        xSize = 2 * Size - 1;
        mesh_ref.GetComponent<Tile>().position_x = Size - 1;
        mesh_ref.GetComponent<Tile>().position_z = Size - 1;

        mesh_ref.transform.Translate(new Vector3(-(Size - 1) * int.Parse(mesh_ref.GetComponent<Tile>().height), 0, (Size - 1) * int.Parse(mesh_ref.GetComponent<Tile>().width)));

        mesh_bas_gauche = Instantiate(mesh_ref, new Vector3(0, 0, 0), Quaternion.identity);
        //mesh_bas_gauche = Instantiate(mesh_ref, new Vector3((Size - 1) * int.Parse(mesh_ref.GetComponent<Tile>().height), 0, -(Size - 1) * int.Parse(mesh_ref.GetComponent<Tile>().width)), Quaternion.identity);
        mesh_bas_gauche.GetComponent<Tile>().is_ref = false;
        mesh_bas_gauche.GetComponent<Tile>().left_down_x = mesh_ref.GetComponent<Tile>().left_down_x - ((Size - 1) * lon_diff);
        mesh_bas_gauche.GetComponent<Tile>().left_down_y = mesh_ref.GetComponent<Tile>().left_down_y - ((Size - 1) * lat_diff);
        mesh_bas_gauche.GetComponent<Tile>().right_up_x = mesh_ref.GetComponent<Tile>().right_up_x - ((Size - 1) * lon_diff);
        mesh_bas_gauche.GetComponent<Tile>().right_up_y = mesh_ref.GetComponent<Tile>().right_up_y - ((Size - 1) * lat_diff);
        //new_mesh.GetComponent<GenerateRoad>().enabled = false;
        //new_mesh.GetComponent<GenerateForest>().enabled = false;
        mesh_bas_gauche.GetComponent<Tile>().position_z = 0;
        mesh_bas_gauche.GetComponent<Tile>().position_x = 0;
        mesh_bas_gauche.GetComponent<GenerateMesh>().enabled = true;
        mesh_bas_gauche.name = "Mesh_X:" + mesh_bas_gauche.GetComponent<Tile>().position_x + "_Z:" + mesh_bas_gauche.GetComponent<Tile>().position_z;




        StartCoroutine(CreateTiles());
        //if (Size == 1)
        //{
        //    MNTs = new GameObject[1];
        //}
        //else
        //{
        //    MNTs = new GameObject[((Size * 2) - 1) * ((Size * 2) - 1)];
        //}
        //MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
    }
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            StartCoroutine(GenerateAllMnt());
        }

        if (Input.GetKeyDown("f"))
        {
            StartCoroutine(GenerateAllField());
            StartCoroutine(GenerateAllForest());
        }

        if (Input.GetKeyDown("v"))
        {
            StartCoroutine(GenerateAllVege());
        }
        if (Input.GetKeyDown("p"))
        {
            StartCoroutine(SpawnAllForest());
        }

        if (Input.GetKeyDown("o"))
        {
            StartCoroutine(SpawnAllVege());
        }

        if (Input.GetKeyDown("u"))
        {
            StartCoroutine(GenerateField.SpawnField(myCrops));
        }
        if (Input.GetKeyDown("t"))
        {
            StartCoroutine(GenerateAllOrtho());
        }

        //UpdateRef();
        if (Input.GetKeyDown("l"))
        {
            StartCoroutine(SmoothAll());
        }
        GenerateForest.GroupVege();
        GenerateBat.GroupBat();
        if (Input.GetKeyDown("x"))
        {
            GenerateForest.CleanAround("Batiment_tag", "Vege_tag", 10);
        }
        if (Input.GetKeyDown("c"))
        {
            GameObject[] tp_areas = GameObject.FindGameObjectsWithTag("Tpzone_tag");
            foreach (GameObject tp_area in tp_areas)
            {
                tp_area.AddComponent<MeshCollider>();

            }

            GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
            foreach (GameObject MNT in MNTs)
            {
                MNT.GetComponent<MeshCollider>().enabled = true;

            }

            //ApplyColorMesh(GetAltitudeExtrema());
        }

        if (Input.GetKeyDown("m"))
        {
            GenerateGraves.SpawnGrave(myGraves);
        }

    }

    IEnumerator Waitabit()
    {
        yield return new WaitForSeconds(20);
    }

    IEnumerator GenerateAllMnt()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject mnt in MNTs)
        {
            mnt.GetComponent<GenerateMesh>().GenerateMNT();
            yield return null;
        }
    }

    IEnumerator GenerateAllOrtho()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateMesh>().UpdateSkin(mnt));
            yield return null;
        }
    }

    IEnumerator GenerateAllField()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateField>().GetPointsField(mnt));
            yield return null;
        }
    }

    IEnumerator GenerateAllForest()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateForest>().GetPointsForest(mnt));
            yield return null;
        }
    }

    IEnumerator GenerateAllVege()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject mnt in MNTs)
        {
            StartCoroutine(mnt.GetComponent<GenerateVegetation>().GetPointsVege(mnt));
            yield return null;
        }
    }

    IEnumerator SpawnAllForest()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject mnt in MNTs)
        {
            mnt.GetComponent<GenerateForest>().SpawnTrees(mnt);
            yield return null;
        }
    }

    IEnumerator SpawnAllVege()
    {
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject mnt in MNTs)
        {
            mnt.GetComponent<GenerateVegetation>().SpawnVeges(mnt);
            yield return null;
        }
    }

    IEnumerator SmoothAll()
    {
        GameObject[] all_mesh = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject mesh in all_mesh)
        {
            mesh.GetComponent<Tile>().Smooth2();
            yield return null;
        }
    }

    IEnumerator CreateTiles()
    {
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (z == Size - 1 && x == Size - 1)
                {
                    mesh_ref.GetComponent<GenerateMesh>().enabled = true;
                    mesh_ref.name = "Mesh_X:"+ mesh_ref.GetComponent<Tile>().position_x + "_Z:"+ mesh_ref.GetComponent<Tile>().position_z;
                    GameObject mon_point_de_teleportation = new GameObject();
                    mon_point_de_teleportation.name = "Point_de_TP";
                    mon_point_de_teleportation.transform.position = mesh_ref.GetComponent<MeshRenderer>().bounds.center;
                    mon_point_de_teleportation.transform.Translate(Vector3.up * 100);
                    GameObject.Find("OpenableBox").GetComponent<OpenableBoxController>().point_tp_transition = mon_point_de_teleportation.transform;
                }
                else if (x != 0 || z != 0)
                {
                    //GameObject new_mesh = Instantiate(mesh_ref, new Vector3(-z*(int.Parse(mesh_ref.GetComponent<Tile>().height)-mesh_bas_gauche.GetComponent<GenerateMesh>().rapport_réduction), 0, x*(int.Parse(mesh_bas_gauche.GetComponent<Tile>().width) - mesh_bas_gauche.GetComponent<GenerateMesh>().rapport_réduction)), Quaternion.identity);
                    GameObject new_mesh = Instantiate(mesh_bas_gauche, new Vector3(-z * (int.Parse(mesh_bas_gauche.GetComponent<Tile>().height)), 0, x * (int.Parse(mesh_bas_gauche.GetComponent<Tile>().width))) + mesh_bas_gauche.transform.position, Quaternion.identity);
                    new_mesh.GetComponent<Tile>().is_ref = false;
                    new_mesh.GetComponent<Tile>().left_down_x = mesh_bas_gauche.GetComponent<Tile>().left_down_x + (x * lon_diff);
                    new_mesh.GetComponent<Tile>().left_down_y = mesh_bas_gauche.GetComponent<Tile>().left_down_y + (z * lat_diff);
                    new_mesh.GetComponent<Tile>().right_up_x = mesh_bas_gauche.GetComponent<Tile>().right_up_x + (x * lon_diff);
                    new_mesh.GetComponent<Tile>().right_up_y = mesh_bas_gauche.GetComponent<Tile>().right_up_y + (z * lat_diff);
                    //new_mesh.GetComponent<GenerateRoad>().enabled = false;
                    //new_mesh.GetComponent<GenerateForest>().enabled = false;
                    new_mesh.GetComponent<Tile>().position_z = ((int)new_mesh.transform.localPosition.z) / (int.Parse(new_mesh.GetComponent<Tile>().width) - new_mesh.GetComponent<GenerateMesh>().rapport_réduction);
                    new_mesh.GetComponent<Tile>().position_x = -((int)new_mesh.transform.localPosition.x) / (int.Parse(new_mesh.GetComponent<Tile>().height) - new_mesh.GetComponent<GenerateMesh>().rapport_réduction);
                    new_mesh.GetComponent<GenerateMesh>().enabled = true;
                    new_mesh.name = "Mesh_X:" + new_mesh.GetComponent<Tile>().position_x + "_Z:" + new_mesh.GetComponent<Tile>().position_z;
                    yield return null;
                    //new_mesh.transform.parent = All_MNT.transform;
                }
            }
        }

    }



    void UpdateRef()
    {
        int ref_x = mesh_ref.GetComponent<Tile>().position_x;
        int ref_z = mesh_ref.GetComponent<Tile>().position_z;
        if (mycamera.GetComponent<CameraController>().position_x != ref_x || mycamera.GetComponent<CameraController>().position_z != ref_z)
        {
            GameObject[] all_mesh = GameObject.FindGameObjectsWithTag("Tile_tag");
            foreach (GameObject mesh in all_mesh)
            {
                if (mesh.GetComponent<Tile>().position_x == mycamera.GetComponent<CameraController>().position_x && mesh.GetComponent<Tile>().position_z == mycamera.GetComponent<CameraController>().position_z)
                {
                    mesh_ref = mesh;
                    Debug.Log("X : " + mesh_ref.GetComponent<Tile>().position_x + " Z : " + mesh_ref.GetComponent<Tile>().position_z);
                    //si on se déplace à droite
                    if ((mycamera.GetComponent<CameraController>().position_z - mycamera.GetComponent<CameraController>().old_position_z) == 1)
                    {
                        for (int i = 0; i < lim_x; i++)
                        {
                            GameObject new_mesh = Instantiate(mesh_ref, new Vector3((mesh_ref.GetComponent<Tile>().position_x - (lim_x - 1) / 2 + i) * 63f, 0, (mesh_ref.GetComponent<Tile>().position_z + (lim_z - 1) / 2) * 63f), Quaternion.identity);
                            new_mesh.GetComponent<Tile>().is_ref = false;
                            new_mesh.GetComponent<Tile>().tileRow = (int.Parse(mesh_ref.GetComponent<Tile>().tileRow) + (lim_z - 1) / 2).ToString();
                            new_mesh.GetComponent<Tile>().tileCol = (int.Parse(mesh_ref.GetComponent<Tile>().tileCol) - (lim_x - 1) / 2 + i).ToString();
                            new_mesh.GetComponent<Tile>().position_z = int.Parse(new_mesh.GetComponent<Tile>().tileRow) - int.Parse(tile_ref.tileRow);
                            new_mesh.GetComponent<Tile>().position_x = int.Parse(new_mesh.GetComponent<Tile>().tileCol) - int.Parse(tile_ref.tileCol);
                            new_mesh.GetComponent<GenerateMesh>().enabled = true;
                        }
                    }
                    //si on se déplace à gauche
                    if ((mycamera.GetComponent<CameraController>().position_z - mycamera.GetComponent<CameraController>().old_position_z) == -1)
                    {
                        for (int i = 0; i < lim_x; i++)
                        {
                            GameObject new_mesh = Instantiate(mesh_ref, new Vector3((mesh_ref.GetComponent<Tile>().position_x - (lim_x - 1) / 2 + i) * 63f, 0, (mesh_ref.GetComponent<Tile>().position_z - (lim_z - 1) / 2) * 63f), Quaternion.identity);
                            new_mesh.GetComponent<Tile>().is_ref = false;
                            new_mesh.GetComponent<Tile>().tileRow = (int.Parse(mesh_ref.GetComponent<Tile>().tileRow) - (lim_z - 1) / 2).ToString();
                            new_mesh.GetComponent<Tile>().tileCol = (int.Parse(mesh_ref.GetComponent<Tile>().tileCol) - (lim_x - 1) / 2 + i).ToString();
                            new_mesh.GetComponent<Tile>().position_z = int.Parse(new_mesh.GetComponent<Tile>().tileRow) - int.Parse(tile_ref.tileRow);
                            new_mesh.GetComponent<Tile>().position_x = int.Parse(new_mesh.GetComponent<Tile>().tileCol) - int.Parse(tile_ref.tileCol);
                            new_mesh.GetComponent<GenerateMesh>().enabled = true;
                        }
                    }
                    //si on se déplace vers le haut
                    if ((mycamera.GetComponent<CameraController>().position_x - mycamera.GetComponent<CameraController>().old_position_x) == -1)
                    {
                        for (int i = 0; i < lim_z; i++)
                        {
                            GameObject new_mesh = Instantiate(mesh_ref, new Vector3((mesh_ref.GetComponent<Tile>().position_x - (lim_x - 1) / 2) * 63f, 0, (mesh_ref.GetComponent<Tile>().position_z - (lim_z - 1) / 2 + i) * 63f), Quaternion.identity);
                            new_mesh.GetComponent<Tile>().is_ref = false;
                            new_mesh.GetComponent<Tile>().tileRow = (int.Parse(mesh_ref.GetComponent<Tile>().tileRow) - (lim_z - 1) / 2 + i).ToString();
                            new_mesh.GetComponent<Tile>().tileCol = (int.Parse(mesh_ref.GetComponent<Tile>().tileCol) - (lim_x - 1) / 2).ToString();
                            new_mesh.GetComponent<Tile>().position_z = int.Parse(new_mesh.GetComponent<Tile>().tileRow) - int.Parse(tile_ref.tileRow);
                            new_mesh.GetComponent<Tile>().position_x = int.Parse(new_mesh.GetComponent<Tile>().tileCol) - int.Parse(tile_ref.tileCol);
                            new_mesh.GetComponent<GenerateMesh>().enabled = true;
                        }
                    }
                    //si on se déplace vers le bas
                    if ((mycamera.GetComponent<CameraController>().position_x - mycamera.GetComponent<CameraController>().old_position_x) == 1)
                    {
                        for (int i = 0; i < lim_z; i++)
                        {
                            GameObject new_mesh = Instantiate(mesh_ref, new Vector3((mesh_ref.GetComponent<Tile>().position_x + (lim_x - 1) / 2) * 63f, 0, (mesh_ref.GetComponent<Tile>().position_z - (lim_z - 1) / 2 + i) * 63f), Quaternion.identity);
                            new_mesh.GetComponent<Tile>().is_ref = false;
                            new_mesh.GetComponent<Tile>().tileRow = (int.Parse(mesh_ref.GetComponent<Tile>().tileRow) - (lim_z - 1) / 2 + i).ToString();
                            new_mesh.GetComponent<Tile>().tileCol = (int.Parse(mesh_ref.GetComponent<Tile>().tileCol) + (lim_x - 1) / 2).ToString();
                            new_mesh.GetComponent<Tile>().position_z = int.Parse(new_mesh.GetComponent<Tile>().tileRow) - int.Parse(tile_ref.tileRow);
                            new_mesh.GetComponent<Tile>().position_x = int.Parse(new_mesh.GetComponent<Tile>().tileCol) - int.Parse(tile_ref.tileCol);
                            new_mesh.GetComponent<GenerateMesh>().enabled = true;
                        }
                    }
                }
            }
        }
        //zoom
        /*
        if (mycamera.GetComponent<CameraController>().position_x != ref_x || mycamera.GetComponent<CameraController>().position_z != ref_z)
        {

        }
        */
    }

    void UpdateGrid()
    {
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (z == 0 && x == 0)
                {
                    tile_ref = mesh_ref.GetComponent<Tile>();
                    tile_ref.GetComponent<GenerateMesh>().enabled = true;
                }
                else
                {
                    GameObject new_mesh = Instantiate(mesh_ref, new Vector3(z * 63f, 0, x * 63f), Quaternion.identity);
                    new_mesh.GetComponent<Tile>().is_ref = false;
                    new_mesh.GetComponent<Tile>().tileRow = (int.Parse(tile_ref.tileRow) + x).ToString();
                    new_mesh.GetComponent<Tile>().tileCol = (int.Parse(tile_ref.tileCol) + z).ToString();
                    new_mesh.GetComponent<Tile>().position_z = int.Parse(new_mesh.GetComponent<Tile>().tileRow) - int.Parse(tile_ref.tileRow);
                    new_mesh.GetComponent<Tile>().position_x = int.Parse(new_mesh.GetComponent<Tile>().tileCol) - int.Parse(tile_ref.tileCol);
                    new_mesh.GetComponent<GenerateMesh>().enabled = true;
                }
            }
        }
    }

    public static (float, float) GetAltitudeExtrema()
    {
        float[] altitudes;
        float altmin = 50000;
        float altmax = -100;
        GameObject[] MNTs = GameObject.FindGameObjectsWithTag("Tile_tag");
        foreach (GameObject MNT in MNTs)
        {
            altitudes = MNT.GetComponent<Tile>().altitudes;
            foreach (float alt in altitudes)
            {
                if (alt >= altmax)
                {
                    altmax = alt;
                }
                if (alt <= altmin)
                {
                    altmin = alt;
                }
            }
        }
        return (altmin, altmax);
    }



}
