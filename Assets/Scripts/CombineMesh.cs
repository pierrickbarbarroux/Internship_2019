using UnityEngine;
using System.Collections;


// Copy meshes from children into the parent's Mesh.
// CombineInstance stores the list of meshes.  These are combined
// and assigned to the attached Mesh.
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

/// <summary>
/// Classe de test (n'est plus utilisée). Je voulais voir si en combinant les meshes du MNT je pouvais éviter les bugs de bordures.
/// Déjà, cela ne marchait pas correctement. Ensuite, la fusion de mesh ne résout pas le problème.
/// Finalement, je suis passé par des terrains et non des meshes classiques.
/// </summary>
public class CombineMesh : MonoBehaviour
{
    void Start()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);
    }
}
