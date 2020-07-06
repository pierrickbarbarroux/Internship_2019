using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe de test pour l'extrusion de mesh 
/// </summary>
public class TestExtrusion : MonoBehaviour
{

    Mesh srcMesh;
    MeshExtrusion.Edge[] precomEdges;

    // Start is called before the first frame update
    void Start()
    {
        srcMesh = (GetComponent<MeshFilter>()).sharedMesh;
        precomEdges = MeshExtrusion.BuildManifoldEdges(srcMesh);

        Matrix4x4[] sections = new Matrix4x4[2];

        sections[0] = transform.worldToLocalMatrix * Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
        sections[1] = transform.worldToLocalMatrix * Matrix4x4.TRS(transform.position + 4f * Vector3.up, Quaternion.identity, Vector3.one);
        //sections[2] = transform.worldToLocalMatrix * Matrix4x4.TRS(transform.position + 6f * Vector3.up, Quaternion.identity, Vector3.one);

        MeshExtrusion.ExtrudeMesh(srcMesh, (GetComponent<MeshFilter>()).mesh, sections, precomEdges, true);
    }

    //This code simply elevates the plane(creates a box), with two sections.You have use the transformation matrix(Matrix4x4) to tell the 
    //script where your sections end, so first you get a transformation matrix to convert your world position to local 
    //coordinates(transform.worldToLocalMatrix), then you apply(by multiplying) another transformation  to tell where you want to end up
    //from your local point of view(Matrix4x4.TRS (transform.position+4f*Vector3.up, Quaternion.identity, Vector3.one))

    // Update is called once per frame
    void Update()
    {
        
    }
}
