using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Classe de test non utilisée par la suite.
/// </summary>
public class TestFusionRail : MonoBehaviour
{
    Mesh ground1;
    Mesh ground2;

    Mesh rail11;
    Mesh rail12;
    Mesh rail21;
    Mesh rail22;

    Vector3[] new_vert;

    // Start is called before the first frame update
    void Start()
    {
        int k = 0;
        ground1 = this.transform.GetChild(0).GetChild(3).gameObject.GetComponent<MeshFilter>().mesh;
        ground2 = this.transform.GetChild(1).GetChild(3).gameObject.GetComponent<MeshFilter>().mesh;

        rail11 = this.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshFilter>().mesh;
        rail12 = this.transform.GetChild(0).GetChild(2).gameObject.GetComponent<MeshFilter>().mesh;

        rail21 = this.transform.GetChild(1).GetChild(1).gameObject.GetComponent<MeshFilter>().mesh;
        rail22 = this.transform.GetChild(1).GetChild(2).gameObject.GetComponent<MeshFilter>().mesh;

        foreach (Vector3 vert in ground1.vertices)
        {
            k++;
        }
        new_vert = new Vector3[ground2.vertices.Length];

        for (int i = 0; i < 24; i++)
        {
            if (i < 4)
            {
                new_vert[i] = ground1.vertices[i] + new Vector3(0, 0, 1);
            }
            else
            {
                new_vert[i] = ground1.vertices[i];
            }
        }
        ground1.vertices = new_vert;
        ground1.RecalculateBounds();
        ground1.RecalculateNormals();

    }
}
