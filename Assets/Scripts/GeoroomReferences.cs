using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Cette classe permet de re référencer les mesh du georoom qui se serit perdu en route lors d'un transfert
/// de projet. On utilise surtout le fait que les mesh et les objets ont le même nom ce qui simplifie la grandement
/// la tâche. Pour les material, on prendra un material par defaut.
/// </summary>
//[ExecuteInEditMode]
public class GeoroomReferences : MonoBehaviour
{
    public GameObject modele_blender;
    public Material DefaultMaterial;

    // Start is called before the first frame update
    void Start()
    {
        MeshFilter[] mes_mesh = modele_blender.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshfilter in mes_mesh)
        {
            Mesh mesh = meshfilter.sharedMesh;
            GameObject go = GameObject.Find(mesh.name);
            go.GetComponent<MeshFilter>().mesh = mesh;
            go.GetComponent<MeshRenderer>().material = DefaultMaterial;
        }
    }
}
