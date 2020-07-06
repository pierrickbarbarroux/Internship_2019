using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLODGroup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LODGroup mylodgroup = this.GetComponent<LODGroup>();
        mylodgroup.size = 3;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
