using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cette classe devait gérer le LOD pour certains objets (sans doute)...
/// Dans tous les cas il s'agit simplement d'une classe de test.
/// </summary>
public class GenerateMeshLODGroup : MonoBehaviour
{
    void CreateLODGroup()
    {
        GameObject lod_parent = new GameObject();
        lod_parent.name = "MNT_X :" + this.GetComponent<Tile>().position_x + " Z :" + this.GetComponent<Tile>().position_z; // à modifier
        lod_parent.transform.position = this.GetComponent<MeshRenderer>().bounds.center;
        lod_parent.AddComponent<LODGroup>();
        LODGroup mylodgroup = lod_parent.GetComponent<LODGroup>();
        mylodgroup.size = mylodgroup.size / 10;

        LOD[] lods = new LOD[5];

        for (int i = 0; i < lods.Length; i++)
        {
            this.gameObject.transform.parent = lod_parent.transform;
            MeshRenderer[] mnt_renderer = new MeshRenderer[1];
            mnt_renderer[0] = this.GetComponent<MeshRenderer>();
            lods[i] = new LOD(1.0F / (i + 1), mnt_renderer);
        }
        mylodgroup.SetLODs(lods);
        mylodgroup.RecalculateBounds();
    }
}
