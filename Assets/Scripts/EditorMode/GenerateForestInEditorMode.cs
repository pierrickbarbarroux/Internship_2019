using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GenerateForestInEditorMode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Génération des forêts");
        GenerateAllForest();
        SpawnAllForest();

        Debug.Log("Done");
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
}
