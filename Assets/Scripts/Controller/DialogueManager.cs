using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

/// <summary>
/// Classe gérant les dialogues. 
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Tooltip("Champs de texte où apparaitront les dialogues")]
    public TextMeshProUGUI dialogueText;

    //Les dialogues passant les uns à la suite des autres, on utilise un type Queue 
    public Queue<string> sentences;

    [Tooltip("L'élément de canvas correspondant au dialogue")]
    public GameObject dialogue_box;

    //True si un dialogue est en cours, false sinon
    [HideInInspector]
    public bool is_dialogue;


    // Start is called before the first frame update
    void Start()
    {
        //On initilise nos variables et on lance le premier dialogue
        //D'ailleurs celui-ci devrait attendre la fin du chargement pour apparaitre
        is_dialogue = false;
        sentences = new Queue<string>();
        StartCoroutine(StartDialogue(dialogue_box.GetComponent<DialogueTrigger>().dialogue));
    }

    /// <summary>
    /// On passe les dialogue avec la gachette du controleur VR
    /// </summary>
    void Update()
    {
        if (dialogue_box.GetComponent<RectTransform>().localPosition.y == -194)
        {
            if (SteamVR_Actions._default.GrabPinch.GetStateDown(SteamVR_Input_Sources.Any))
            {
                DisplayNextSentence();
            }
        } 
    }

    /// <summary>
    /// Méthode pour commencer un dialogue.
    /// </summary>
    /// <param name="dialogue">Objet dialogue que l'on souhaite lancer</param>
    /// <returns></returns>
    public IEnumerator StartDialogue(Dialogue dialogue)
    {
        is_dialogue = true;
        dialogue_box.SetActive(true);
        dialogue_box.GetComponent<Animator>().Play("TextGoUp");
        yield return new WaitForSecondsRealtime(2f);
        sentences.Clear();
        foreach(string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    /// <summary>
    /// Permet de passer à la prochaine phrase
    /// </summary>
    public void DisplayNextSentence()
    {
        if (sentences.Count ==0)
        {
            StartCoroutine(EndDialogue());
            return;
        }
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }
    
    /// <summary>
    /// Méthode permettant d'écrire le texte petit à petit et non d'un seul coup.
    /// </summary>
    /// <param name="sentence">Phrase que l'on souhaite écrire</param>
    /// <returns></returns>
    IEnumerator TypeSentence (string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    /// <summary>
    /// Méthode gérant la fin d'un dialogue, quand toutes les phrases sont passées
    /// </summary>
    /// <returns></returns>
    IEnumerator EndDialogue()
    {
        dialogue_box.GetComponent<Animator>().Play("TextGoDown");
        yield return new WaitForSecondsRealtime(1.5f);
        is_dialogue = false;
        dialogueText.text = "";
        dialogue_box.SetActive(false);
        Debug.Log("End of conversation");
    }

}
