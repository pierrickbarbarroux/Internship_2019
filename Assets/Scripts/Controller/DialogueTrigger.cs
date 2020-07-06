using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ce script ne sert pas à grand chose, mis à part à stocker un dialogue qui se trouve donc dans l'éditeur
/// au niveau de l'inspector de l'objet sur lequel est attaché ce script.
/// Il ne faut donc pas le supprimer, sauf si on a déplacé le dialogue ailleurs
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

//    public void TriggerDialogue()
//    {
//        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
//    }
}
