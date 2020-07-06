using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Valve.VR.InteractionSystem;

public class TestTP : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.AddComponent<TeleportArea>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
