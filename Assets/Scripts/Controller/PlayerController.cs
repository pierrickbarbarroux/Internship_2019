using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe gérant quelques paramètres relatif au joueur
/// </summary>
public class PlayerController : MonoBehaviour
{
    //Un dialogue qui sera lancé quand le joueur trouvera la fiche géodésique
    Dialogue dialogue;

    //True si le joueur a trouvé la fiche géodésique, false sinon
    public bool hasMap;

    //true si le joueur se trouve dans le géoroom, false sinon
    public bool in_georoom;

    // Start is called before the first frame update
    void Start()
    {
        //Initialisation des variables et construction du dialogue
        hasMap = false;
        dialogue = new Dialogue();
        dialogue.name = "post_chest";
        dialogue.sentences = new string[4];
        dialogue.sentences[0] = "Bravo, vous avez la fiche géodésique.";
        dialogue.sentences[1] = "Déterminez maintenant la commune rattachée à cette fiche.";
        dialogue.sentences[2] = "Une fois que vous aurez le nom de la commune, rendez-vous sous la flèche verte pour sélectionner la clé correspondante.";
        dialogue.sentences[3] = "Cette clé vous donne accès à l'ordinateur de la boite blanche.";
    }

   /// <summary>
   /// Lance le dialogue une fois la fiche géodésique obtenue. 
   /// Cette méthode est référencée dans l'envent system du script 'Throwable' de la fiche géodésique (dans le coffre)
   /// </summary>
    public void GetMap()
    {
        hasMap = true;
        Destroy(GameObject.Find("FicheGeodesique"));
        GameObject.Find("LeftHand").GetComponent<HandController>().ShowMap();
        StartCoroutine(GameObject.Find("DialogueManager").GetComponent<DialogueManager>().StartDialogue(dialogue));
    }

}
