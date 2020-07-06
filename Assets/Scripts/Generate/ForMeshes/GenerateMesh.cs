using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Valve.VR.InteractionSystem;
using System.IO;

/// <summary>
/// Cette classe n'est plus utilisée. Elle servait pour la génération de mesh.
/// Cette classe génère les terrains en utilisant des meshes et non des terrains. Vers la fin du projet, 
/// je n'ai utilisé exclusivement que les terrains et non les meshes (même si les deux strctures son très similaires).
/// </summary>
public class GenerateMesh : MonoBehaviour
{

    //Mont saint michel
    //367573
    //6846560

    public Camera mycamera;

    public int rapport_réduction;

    public int xSize;
    public int zSize;
    public int lim_x;
    public int lim_z;

    public Material myMat;
    public Gradient gradient;
    public static float minTerrainHeight;
    public static float maxTerrainHeight;

    float minHeight;
    float maxHeight;

    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    Color[] colors;
    int[] triangles;

    //public Gradient gradient;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.Optimize();
        StartCoroutine(GetAlt());

    }

    public void GenerateMNT()
    {
        minTerrainHeight = GenerateTiles.GetAltitudeExtrema().Item1;
        maxTerrainHeight = GenerateTiles.GetAltitudeExtrema().Item2;

        minHeight = Mathf.Min(this.GetComponent<Tile>().altitudes);
        maxHeight = Mathf.Max(this.GetComponent<Tile>().altitudes);

        if (Mathf.Abs(minHeight - maxHeight) < 7)
        {
            //this.GetComponent<MeshFilter>().mesh = CreateShapeLOD(64, this.gameObject).GetComponent<MeshFilter>().mesh;
            CreateShapeRapport(64);
        }

        else if (Mathf.Abs(minHeight - maxHeight) < 25)
        {
            //this.GetComponent<MeshFilter>().mesh = CreateShapeLOD(32, this.gameObject).GetComponent<MeshFilter>().mesh;
            CreateShapeRapport(32);

        }

        else if (Mathf.Abs(minHeight - maxHeight) < 50)
        {
            //this.GetComponent<MeshFilter>().mesh = CreateShapeLOD(16, this.gameObject).GetComponent<MeshFilter>().mesh;
            CreateShapeRapport(16);

        }

        else if (Mathf.Abs(minHeight - maxHeight) < 80)
        {
            //this.GetComponent<MeshFilter>().mesh = CreateShapeLOD(8, this.gameObject).GetComponent<MeshFilter>().mesh;
            CreateShapeRapport(8);

        }

        else if (Mathf.Abs(minHeight - maxHeight) < 120)
        {
            //this.GetComponent<MeshFilter>().mesh = CreateShapeLOD(4, this.gameObject).GetComponent<MeshFilter>().mesh;
            CreateShapeRapport(4);

        }
        else
        {
            CreateShape();
        }
        GetComponent<MeshCollider>().sharedMesh = mesh;
        GetComponent<MeshCollider>().enabled = false;
        GameObject tparea = new GameObject();
        tparea.name = "TP_Area_X:" + GetComponent<Tile>().position_x + "_Z:" + GetComponent<Tile>().position_z;
        tparea.tag = "Tpzone_tag";
        tparea.transform.position = this.transform.position;
        tparea.AddComponent<MeshFilter>();
        tparea.AddComponent<MeshRenderer>();
        tparea.GetComponent<MeshFilter>().mesh = this.GetComponent<MeshFilter>().mesh;
        tparea.GetComponent<MeshRenderer>().material = myMat;
        tparea.AddComponent<TeleportArea>();
        tparea.transform.Translate(new Vector3(0, 0.001f, 0));
        tparea.GetComponent<MeshRenderer>().enabled = false;
        //tparea.transform.Translate(new Vector3(0,30f,0));
        tparea.AddComponent<MeshCollider>();
    }


    void UpdateVisibleTiles()
    {

        int xdiff;
        int zdiff;

        xdiff = Mathf.Abs(this.GetComponent<Tile>().position_x - mycamera.GetComponent<CameraController>().position_x);
        zdiff = Mathf.Abs(this.GetComponent<Tile>().position_z - mycamera.GetComponent<CameraController>().position_z);
        if (xdiff > (lim_x - 1) / 2 || zdiff > (lim_z - 1) / 2)
        {
            if (this != mycamera.GetComponent<GenerateTiles>().mesh_ref)
            {
                Destroy(this.gameObject);
            }
        }
    }

    IEnumerator GetAlt()
    {

        string path = "Assets/Data/MNTs/" + "mnt_xbot_" + this.GetComponent<Tile>().left_down_x + "_ybot_" + this.GetComponent<Tile>().left_down_y + "_xtop_" + this.GetComponent<Tile>().right_up_x + "_ytop_" + this.GetComponent<Tile>().right_up_y + ".bil";
        if (!File.Exists(path))
        {
            string url = DataController.GetWmsRequest(this.GetComponent<Tile>().layer, this.GetComponent<Tile>().format, this.GetComponent<Tile>().height, this.GetComponent<Tile>().width, (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y), (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y));
            StartCoroutine(DataController.WriteDataFile(url, path));
            Debug.Log(url);
            yield return null;
        }
        //CoroutineResult cd = new CoroutineResult(this, DataController.GetData(DataController.GetRequest(this.GetComponent<Tile>().layer, this.GetComponent<Tile>().tileMatrix, this.GetComponent<Tile>().tileRow, this.GetComponent<Tile>().tileCol)));

        //CoroutineResult cd = new CoroutineResult(this, DataController.GetWmsData(DataController.GetWmsRequest(this.GetComponent<Tile>().layer, this.GetComponent<Tile>().format, this.GetComponent<Tile>().height, this.GetComponent<Tile>().width, (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y), (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y)), int.Parse(this.GetComponent<Tile>().height), int.Parse(this.GetComponent<Tile>().width)));
        //yield return cd.coroutine;
        //float[] altitude = (float[])cd.result;


        float[] altitude = DataController.GetMNTFromFile(path, 65536);


        //for (int k = 0; k < (Mathf.Log(rapport_réduction, 2)); k++)
        //{
        //    altitude = DataController.Reduce(altitude);
        //}
        //float[] altitude_large = (float[])cd.result;
        //float[] altitude_mlarge = DataController.Reduce(altitude_large);
        //float[] altitude = DataController.Reduce(altitude_mlarge);

        this.GetComponent<Tile>().altitudes = altitude;
    }

    void CreateShape()
    {
        float[] altitude = this.GetComponent<Tile>().altitudes;
        int i = 0;
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                if (z == zSize && x == xSize)
                {
                    vertices[i] = new Vector3(x * rapport_réduction, altitude[z - 1 + ((x - 1) * zSize)], z * rapport_réduction);
                    i++;
                }
                else if (z == zSize)
                {

                    vertices[i] = new Vector3(x * rapport_réduction, altitude[z - 1 + (x * zSize)], z * rapport_réduction);
                    i++;
                }
                else if (x == xSize)
                {
                    vertices[i] = new Vector3(x * rapport_réduction, altitude[z + ((x - 1) * zSize)], z * rapport_réduction);
                    i++;
                }
                else
                {
                    vertices[i] = new Vector3(x * rapport_réduction, (altitude[z + (x * zSize)]), z * rapport_réduction); //changer y pour le relief et la mise a niveau (si réimplentée)
                    i++;
                }
                //if (i < 350)
                //{
                //    vertices[i] = new Vector3(x * rapport_réduction, 0, z * rapport_réduction);
                //}
            }
        }


        triangles = new int[xSize * zSize * 6];

        int vert_index = 0;
        int tri_index = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tri_index + 0] = vert_index + 0;
                triangles[tri_index + 1] = vert_index + xSize + 1;
                triangles[tri_index + 2] = vert_index + 1;
                triangles[tri_index + 3] = vert_index + 1;
                triangles[tri_index + 4] = vert_index + xSize + 1;
                triangles[tri_index + 5] = vert_index + xSize + 2;

                vert_index++;
                tri_index += 6;

            }
            vert_index++;
        }

        uvs = new Vector2[vertices.Length];
        colors = new Color[vertices.Length];

        int j = 0;
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                uvs[j] = new Vector2((float)x / xSize, (float)z / zSize);
                //Debug.Log( minTerrainHeight);
                //Debug.Log( maxTerrainHeight);

                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[j].y);
                //float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[j].y);
                //Debug.Log(height);
                colors[j] = gradient.Evaluate(height);
                //colors[j] = Color.green;
                j++;
            }
        }

        UpdateMesh();
        //Debug.Log( minTerrainHeight);
        //Debug.Log( maxTerrainHeight);

    }

    void CreateShapeRapport(int rapport)
    {
        int i = 0;
        int new_xSize = this.GetComponent<GenerateMesh>().xSize / rapport;
        int new_zSize = this.GetComponent<GenerateMesh>().zSize / rapport;
        float[] altitude = this.GetComponent<Tile>().altitudes;

        for (int k = 0; k < (Mathf.Log(rapport, 2)); k++)
        {
            altitude = DataController.RemoveSome(altitude);
        }

        vertices = new Vector3[(new_xSize + 1) * (new_zSize + 1)];
        for (int z = 0; z <= new_zSize; z++)
        {
            for (int x = 0; x <= new_xSize; x++)
            {
                if (z == new_zSize && x == new_xSize)
                {
                    float a = (altitude[z - 1 + ((x - 1) * new_zSize)] - altitude[z - 2 + ((x - 2) * new_zSize)])/2;
                    vertices[i] = new Vector3(x * rapport, altitude[z - 1 + ((x - 1) * new_zSize)]+a, z * rapport);
                    i++;
                }
                else if (z == new_zSize)
                {
                    float a = (altitude[z - 1 + (x * new_zSize)] - altitude[z - 2 + (x * new_zSize)])/2;
                    vertices[i] = new Vector3(x * rapport, altitude[z - 1 + (x * new_zSize)]+a, z * rapport);
                    i++;
                }
                else if (x == new_xSize)
                {
                    float a = (altitude[z + ((x-1) * new_zSize)] - altitude[z + ((x-2) * new_zSize)])/2;
                    vertices[i] = new Vector3(x * rapport, altitude[z + ((x - 1) * new_zSize)]+a, z * rapport);
                    i++;
                }
                else
                {
                    vertices[i] = new Vector3(x * rapport, (altitude[z + (x * new_zSize)]), z * rapport); //changer y pour le relief et la mise a niveau (si réimplentée)
                    i++;
                }

            }
        }


        triangles = new int[new_xSize * new_zSize * 6];

        int vert_index = 0;
        int tri_index = 0;

        for (int z = 0; z < new_zSize; z++)
        {
            for (int x = 0; x < new_xSize; x++)
            {
                triangles[tri_index + 0] = vert_index + 0;
                triangles[tri_index + 1] = vert_index + new_xSize + 1;
                triangles[tri_index + 2] = vert_index + 1;
                triangles[tri_index + 3] = vert_index + 1;
                triangles[tri_index + 4] = vert_index + new_xSize + 1;
                triangles[tri_index + 5] = vert_index + new_xSize + 2;

                vert_index++;
                tri_index += 6;

            }
            vert_index++;
        }

        uvs = new Vector2[vertices.Length];
        colors = new Color[vertices.Length];

        int j = 0;
        for (int z = 0; z <= new_zSize; z++)
        {
            for (int x = 0; x <= new_xSize; x++)
            {
                uvs[j] = new Vector2((float)x / new_xSize, (float)z / new_zSize);

                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[j].y);
                colors[j] = gradient.Evaluate(height);
                j++;
            }
        }

        UpdateMesh();
    }

    void CreateLODGroup()
    {
        GameObject lod_parent = new GameObject();
        lod_parent.name = "MNT_X :" + this.GetComponent<Tile>().position_x + " Z :" + this.GetComponent<Tile>().position_z; // à modifier
        lod_parent.transform.position = this.GetComponent<MeshRenderer>().bounds.center;
        lod_parent.AddComponent<LODGroup>();
        LODGroup mylodgroup = lod_parent.GetComponent<LODGroup>();


        LOD[] lods = new LOD[4];
        float[] srth = { 0.95f, 0.90f, 0.80f, 0.01f }; //screenRelativeTransitionHeight

        for (int i = 0; i < lods.Length; i++)
        {
            if (i == 0)
            {
                this.gameObject.transform.parent = lod_parent.transform;
                MeshRenderer[] mnt_renderer = new MeshRenderer[1];
                mnt_renderer[0] = this.GetComponent<MeshRenderer>();
                lods[i] = new LOD(srth[i], mnt_renderer);
            }
            else
            {
                GameObject to_add = CreateShapeLOD((int)Mathf.Pow(2f, (float)(i + 2)), this.gameObject);
                to_add.transform.parent = lod_parent.transform;
                MeshRenderer[] mnt_renderer = new MeshRenderer[1];
                mnt_renderer[0] = to_add.GetComponent<MeshRenderer>();
                lods[i] = new LOD(srth[i], mnt_renderer);
            }
        }
        mylodgroup.SetLODs(lods);
        mylodgroup.RecalculateBounds();
        //mylodgroup.size = mylodgroup.size / 2;
    }

    GameObject CreateShapeLOD(int rapport, GameObject mymesh)
    {
        GameObject new_mnt = new GameObject();
        new_mnt.tag = "MNT_tag";
        new_mnt.AddComponent<MeshFilter>();
        new_mnt.AddComponent<MeshRenderer>();
        new_mnt.transform.position = this.transform.position;

        Mesh new_mesh;
        Vector3[] new_vertices;
        Vector2[] new_uvs;
        int[] new_triangles;

        int new_xSize = mymesh.GetComponent<GenerateMesh>().xSize / rapport;
        int new_zSize = mymesh.GetComponent<GenerateMesh>().zSize / rapport;
        float[] altitude = mymesh.GetComponent<Tile>().altitudes;

        for (int k = 0; k < (Mathf.Log(rapport, 2)); k++)
        {
            altitude = DataController.Reduce(altitude);
        }

        int i = 0;
        new_vertices = new Vector3[(new_xSize + 1) * (new_zSize + 1)];
        for (int z = 0; z <= new_zSize; z++)
        {
            for (int x = 0; x <= new_xSize; x++)
            {
                if (z == new_zSize && x == new_xSize)
                {
                    new_vertices[i] = new Vector3(x * rapport, altitude[z - 1 + ((x - 1) * new_zSize)], z * rapport);
                    i++;
                }
                else if (z == new_zSize)
                {
                    new_vertices[i] = new Vector3(x * rapport, altitude[z - 1 + (x * new_zSize)], z * rapport);
                    i++;
                }
                else if (x == new_xSize)
                {
                    new_vertices[i] = new Vector3(x * rapport, altitude[z + ((x - 1) * new_zSize)], z * rapport);
                    i++;
                }
                else
                {
                    new_vertices[i] = new Vector3(x * rapport, (altitude[z + (x * new_zSize)]), z * rapport); //changer y pour le relief et la mise a niveau (si réimplentée)
                    i++;
                }

            }
        }


        new_triangles = new int[new_xSize * new_zSize * 6];
        //colors = new Color[vertices.Length];

        int vert_index = 0;
        int tri_index = 0;

        for (int z = 0; z < new_zSize; z++)
        {
            for (int x = 0; x < new_xSize; x++)
            {
                new_triangles[tri_index + 0] = vert_index + 0;
                new_triangles[tri_index + 1] = vert_index + new_xSize + 1;
                new_triangles[tri_index + 2] = vert_index + 1;
                new_triangles[tri_index + 3] = vert_index + 1;
                new_triangles[tri_index + 4] = vert_index + new_xSize + 1;
                new_triangles[tri_index + 5] = vert_index + new_xSize + 2;

                vert_index++;
                tri_index += 6;

            }
            vert_index++;
        }

        new_uvs = new Vector2[new_vertices.Length];
        int j = 0;
        for (int z = 0; z <= new_zSize; z++)
        {
            for (int x = 0; x <= new_xSize; x++)
            {
                new_uvs[j] = new Vector2((float)x / new_xSize, (float)z / new_zSize);
                //colors[j] = Color.green;

                j++;
            }
        }

        new_mesh = new Mesh();
        new_mnt.GetComponent<MeshFilter>().mesh = new_mesh;

        new_mesh.Clear();
        new_mesh.vertices = new_vertices;
        new_mesh.triangles = new_triangles;
        new_mesh.uv = new_uvs;
        new_mesh.colors = colors;
        new_mnt.GetComponent<MeshRenderer>().material = mymesh.GetComponent<MeshRenderer>().material;
        new_mesh.RecalculateNormals();

        return new_mnt;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }





    //Quasiment bon, il reste que le bug du dernier triangle
    public void UpdateAlt()
    {
        Debug.Log(vertices.Length);
        Debug.Log(GetComponent<Tile>().altitudes.Length);
        int i = 0;
        GetComponent<Tile>().Smooth();
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                if (z == zSize && x == xSize)
                {
                    vertices[i] = new Vector3(x * rapport_réduction, GetComponent<Tile>().altitudes[z - 1 + ((x - 1) * zSize)], z * rapport_réduction);
                    i++;
                }
                else if (z == zSize)
                {
                    vertices[i] = new Vector3(x * rapport_réduction, GetComponent<Tile>().altitudes[z - 1 + (x * zSize)], z * rapport_réduction);
                    i++;
                }
                else if (x == xSize)
                {
                    vertices[i] = new Vector3(x * rapport_réduction, GetComponent<Tile>().altitudes[z + ((x - 1) * zSize)], z * rapport_réduction);
                    i++;
                }
                else
                {
                    vertices[i] = new Vector3(x * rapport_réduction, (GetComponent<Tile>().altitudes[z + (x * zSize)]), z * rapport_réduction); //changer y pour le relief et la mise a niveau (si réimplentée)
                    i++;
                }


            }
        }
        UpdateMesh();
    }

    public IEnumerator UpdateSkin(GameObject mnt)
    {
        if (mnt.GetComponent<Renderer>().material.name == "mesh_material" + " (Instance)" || mnt.GetComponent<Renderer>().material.name == myMat.name)
        {
            //ortho hr a 20cm => donc 1280*1280

            //CoroutineResult cd = new CoroutineResult(mnt, DataController.GetTexture(DataController.GetWmsRequest("HR.ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", "1500", "1500", (mnt.GetComponent<Tile>().left_down_x, mnt.GetComponent<Tile>().left_down_y), (mnt.GetComponent<Tile>().right_up_x, mnt.GetComponent<Tile>().right_up_y))));
            //yield return cd.coroutine;
            Debug.Log(DataController.GetWmsRequest("ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", "1280", "1280", (mnt.GetComponent<Tile>().left_down_x, mnt.GetComponent<Tile>().left_down_y), (mnt.GetComponent<Tile>().right_up_x, mnt.GetComponent<Tile>().right_up_y)));
            //res = (Texture2D)cd.result;

            string path = "Assets/Data/Textures/" + "ortho_xbot_" + mnt.GetComponent<Tile>().left_down_x + "_ybot_" + mnt.GetComponent<Tile>().left_down_y + "_xtop_" + mnt.GetComponent<Tile>().right_up_x + "_ytop_" + mnt.GetComponent<Tile>().right_up_y + ".jpeg";
            if (!File.Exists(path))
            {
                string url = DataController.GetWmsRequest("ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", "1280", "1280", (mnt.GetComponent<Tile>().left_down_x, mnt.GetComponent<Tile>().left_down_y), (mnt.GetComponent<Tile>().right_up_x, mnt.GetComponent<Tile>().right_up_y));
                StartCoroutine(DataController.WriteDataFile(url, path));
                yield return null;
            }
            Texture2D res = new Texture2D(2560, 2560);
            res.LoadImage(File.ReadAllBytes(path));
            res = rotateTexture(res, false);
            mnt.GetComponent<MeshRenderer>().material.mainTexture = res;
            mnt.GetComponent<MeshRenderer>().material.name = "Texture_Ortho_";
            //Transform trans_parent = mnt.transform.parent;
            //foreach (Transform child in trans_parent)
            //{
            //    child.gameObject.GetComponent<MeshRenderer>().material.mainTexture = res;
            //    child.gameObject.GetComponent<MeshRenderer>().material.name = "newmat";

            //}
            //yield return new WaitForSeconds(0.1f);
        }
    }

    void UpdateTransform()
    {
        Vector3 new_transform = this.GetComponent<MeshRenderer>().bounds.center;
    }

    IEnumerator UpdateRoad()
    {
        Texture res;
        if (this.GetComponent<Renderer>().material.name == myMat.name + " (Instance)" || this.GetComponent<Renderer>().material.name == myMat.name)
        {
            //string layer_test = "HR.ORTHOIMAGERY.ORTHOPHOTOS";
            //CoroutineResult cd = new CoroutineResult(this, DataController.GetTexture(DataController.GetWmsRequest("HR.ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", (int.Parse(this.GetComponent<Tile>().height)*4).ToString(), (int.Parse(this.GetComponent<Tile>().width)*4).ToString(), (this.GetComponent<Tile>().left_down_lon, this.GetComponent<Tile>().left_down_lat), (this.GetComponent<Tile>().right_up_lon, this.GetComponent<Tile>().right_up_lat))));
            CoroutineResult cd = new CoroutineResult(this, DataController.GetTexture(DataController.GetWmsRequest("HR.ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", "1500", "1500", (this.GetComponent<Tile>().left_down_x, this.GetComponent<Tile>().left_down_y), (this.GetComponent<Tile>().right_up_x, this.GetComponent<Tile>().right_up_y))));
            //Debug.Log(DataController.GetWmsRequest("HR.ORTHOIMAGERY.ORTHOPHOTOS", "image/jpeg", this.GetComponent<Tile>().height, this.GetComponent<Tile>().width, (this.GetComponent<Tile>().left_down_1, this.GetComponent<Tile>().left_down_2), (this.GetComponent<Tile>().right_up_1, this.GetComponent<Tile>().right_up_2)));
            yield return cd.coroutine;
            //this.GetComponent<MeshRenderer>().material.mainTexture = (Texture)cd.result;
            res = (Texture)cd.result;
            this.GetComponent<MeshRenderer>().material.mainTexture = res;
            this.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(1, -1);
            this.GetComponent<MeshRenderer>().material.name = "newmat";
        }
    }

    public static Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
    {
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                iRotated = (i + 1) * h - j - 1;
                iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }

}
