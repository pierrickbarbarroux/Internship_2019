using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère les interactions avec les boutons du digicode
/// </summary>
public class BackButtonController : MonoBehaviour
{
    int number;
    GameObject parent;
    [Tooltip("Material du bouton lorsqu'on a bien appuyé")]
    public Material other_mat;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.parent.gameObject;
        number = int.Parse(name.Substring(name.Length - 1, 1));
    }

    /// <summary>
    /// Détecte la collision entre le bouton qu'on pousse et le bouton dans le fond 
    /// </summary>
    /// <param name="other">le collider avec lequel le bouton rentre en contact</param>
    void OnTriggerEnter(Collider other)
    {
        //si on pousse un bouton
        if (other.gameObject.name == "button"+number)
        {
            transform.parent.parent.gameObject.GetComponent<CodePanelController>().combinaison.Add(number);
            other.gameObject.GetComponent<Renderer>().material = other_mat;
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<AudioSource>().Play(0);
        }

        //Test de la combinaison
        if (parent.GetComponent<CodePanelController>().combinaison.Count == parent.GetComponent<CodePanelController>().true_code.Count)
        {
            for (int i = 0; i < parent.GetComponent<CodePanelController>().true_code.Count; i++)
            {
                if (parent.GetComponent<CodePanelController>().combinaison[i] == parent.GetComponent<CodePanelController>().true_code[i])
                {
                    CodePanelController.k++;
                }
            }
            if (CodePanelController.k == 4) //bonne combinaison
            {
                Debug.Log("combinaison trouvé");
                StartCoroutine(parent.GetComponent<CodePanelController>().Validate());
            }
            else //pas la bonne combianaison
            {
                StartCoroutine(parent.GetComponent<CodePanelController>().FalseCode());
            }
        }

        if (parent.GetComponent<CodePanelController>().combinaison.Count > parent.GetComponent<CodePanelController>().true_code.Count) //pas la bonne combinaison
        {
            StartCoroutine(parent.GetComponent<CodePanelController>().FalseCode());
        }
    }

}
