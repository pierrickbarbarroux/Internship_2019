using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;
using UnityEngine.SceneManagement;


/// <summary>
/// Gère la borne que le joueur doit trouver. 
/// </summary>
public class BorneController : MonoBehaviour
{

    Dialogue dialogue;

    //Faux si le jeu n'est pas terminé, vrai sinon
    bool end_game;

    [Tooltip("Ecran de transition entre les deux scène (voir canvas)")]
    public GameObject transition_screen;

    // Start is called before the first frame update
    void Start()
    {
        end_game = false;
        gameObject.GetComponent<VibrationNearObject>().mygo = GameObject.Find("Player");

        //Dialogue de la fin du jeu, s'active normalement quand on attrape la borne
        dialogue = new Dialogue();
        dialogue.name = "post_borne";
        dialogue.sentences = new string[3];
        dialogue.sentences[0] = "Félicitations ! ";
        dialogue.sentences[1] = "Vous venez de trouver la borne géodésique";        
        dialogue.sentences[2] = "Le code que vous cherchez correspond à " + ParameterManager.Question + ".";

        Throwable script = GetComponent<Throwable>();
        script.onPickUp.AddListener(GrabBorne);

        transition_screen = GameObject.Find("Transition_screen");
        //script.onPickUp = new UnityEvent();

        //UnityAction methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), , "GrabBorne") as UnityAction;
        //UnityEventTools.AddPersistentListener(script.onPickUp, methodDelegate);
    }

    /// <summary>
    /// Lance le dialogue de fin. Doit être référencé dans l'event system du script 'Throwable' de la borne
    /// (ce qui est fait dans la fonction start de ce scipt lignes 38-39)
    /// </summary>
    public void GrabBorne()
    {
        if (end_game == false)
        {
            end_game = true;
            Debug.Log("fin");
            StartCoroutine(GameObject.Find("DialogueManager").GetComponent<DialogueManager>().StartDialogue(dialogue));
            StartCoroutine(EndGame());
        }
    }

    /// <summary>
    /// Lance la fin du jeu, c'est à dire un écran de transition vers le menu principal.
    /// </summary>
    /// <returns></returns>
    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(15f);
        transition_screen.GetComponent<Animator>().Play("Transition_screen_Animation_EndGame");
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }

}
