using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe non utilisée dans le projet final (Comme CameraRayCast).
/// Ce script est à attacher à tous les objets auxquels on veut appliquer l'occlusion culling (voir CameraRayCast)
/// Attention cependant, l'occlusion culling n'est pas optimiser. Il y a de légers ralentissement 
/// </summary>
public class RendererTimer : MonoBehaviour
{
    public float timer;

    // Start is called before the first frame update
    void Start()
    {
        //Temps avant lequel le renderer de l'objet est disable
        timer = 12f;
    }

    // Update is called once per frame
    void Update()
    {
        //On décroit le timer à chaque frame et on le fait apparaitre/disparaire selon le cas
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            GetComponent<MeshRenderer>().enabled = false;
        }
        else if (GetComponent<MeshRenderer>().enabled == false && timer > 0)
        {
            GetComponent<MeshRenderer>().enabled = true;
        }

    }

}
