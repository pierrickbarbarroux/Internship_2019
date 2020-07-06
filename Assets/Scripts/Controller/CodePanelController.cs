using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

/// <summary>
/// Gère le digicode se trouvant dans le géoroom.
/// </summary>
public class CodePanelController : MonoBehaviour
{
    //Ensemble des buttons qui composent le digicode
    [HideInInspector]
    public GameObject[] buttons;

    //Ensemble des 'boutons' se trouvant derrière les boutons que l'on presse
    [HideInInspector]
    public GameObject[] back_buttons;

    //Combinaison que le joueur tape sur le digicode
    [HideInInspector]
    public List<int> combinaison;

    [Tooltip("Combinaison qu'il faut entrer pour ouvrir le coffre")]
    public List<int> true_code;

    public static int k;

    [Tooltip("Collider de la tête du joueur")]
    public Collider Head;

    [Tooltip("Coffre contenant la fiche géodésique")]
    public GameObject Chest;

    [Tooltip("Il s'agit de la fiche géodésique")]
    public GameObject map;

    [Tooltip("Cadena situé au niveau du coffre")]
    public GameObject padlock;

    [Tooltip("Material des boutons si la combinaison est fausse")]
    public Material wrong;

    [Tooltip("Matériel des boutons si la combinaison est bonne")]
    public Material right;

    //Anciens material des boutons (pour faire les transitions)
    Material[] old_mat;


    // Start is called before the first frame update
    void Start()
    {
        //On initialise les boutons et les materials pour pouvoir les récupérer facilement après
        buttons = new GameObject[10];
        back_buttons = new GameObject[10];
        old_mat = new Material[10];
        for (int i = 0; i < 10; i++)
        {
            buttons[i] = transform.GetChild(0).GetChild(i).gameObject;
            back_buttons[i] = transform.GetChild(0).GetChild(i + 10).gameObject;
            old_mat[i] = buttons[i].GetComponent<Renderer>().material;
        }
        k = 0;
        combinaison = new List<int>();
    }

    /// <summary>
    /// Méthode appelée quand le code tapé est faux. Elle réinitialise le digicode pour pouvoir retaper le code
    /// </summary>
    /// <returns></returns>
    public IEnumerator FalseCode()
    {

        combinaison = new List<int>();
        k = 0;
        transform.GetChild(2).gameObject.GetComponent<AudioSource>().Play(0);
        for (int i = 0; i < 10; i++)
        {
            buttons[i].GetComponent<MeshRenderer>().material = wrong;
        }
        yield return new WaitForSecondsRealtime(2f);

        for (int i = 0; i < 10; i++)
        {
            buttons[i].GetComponent<MeshRenderer>().material = old_mat[i];
        }

        for (int i = 0; i < 10; i++)
        {
            back_buttons[i].GetComponent<BoxCollider>().enabled = true;
        }

    }

    /// <summary>
    /// Méthode appelée lorsque le code tapé est le bon. Elle réinitialise le digicode est dévérouille le coffre
    /// </summary>
    /// <returns></returns>
    public IEnumerator Validate()
    {
        combinaison = new List<int>();
        k = 0;
        GetComponent<AudioSource>().Play(0);
        for (int i = 0; i < 10; i++)
        {
            buttons[i].GetComponent<MeshRenderer>().material = right;
        }
        yield return new WaitForSecondsRealtime(2f);
        for (int i = 0; i < 10; i++)
        {
            back_buttons[i].GetComponent<BoxCollider>().enabled = true;
        }
        padlock.GetComponent<AudioSource>().Play(0);

        for (int i = 0; i < 10; i++)
        {
            buttons[i].GetComponent<MeshRenderer>().material = old_mat[i];
        }

        Chest.transform.GetChild(5).gameObject.GetComponent<Interactable>().enabled = true;
        Chest.transform.GetChild(5).gameObject.GetComponent<CircularDrive>().enabled = true;
        padlock.GetComponent<Animator>().Play("Padlock_Animation");
        //Destroy(padlock);
        map.SetActive(true);
    }

    /// <summary>
    /// Active les collider des mains lorsque le joueur se trouve devant le digicode.
    /// Cela permet au joueur de 'pousser' les boutons sans les attraper
    /// </summary>
    /// <param name="other">Collider avec lequel l'objet rentre en contact</param>
    void OnTriggerEnter(Collider other)
    {
        if (other == Head)
        {
            other.gameObject.transform.parent.GetChild(1).gameObject.GetComponent<SphereCollider>().enabled = true;
            other.gameObject.transform.parent.GetChild(2).gameObject.GetComponent<SphereCollider>().enabled = true;
        }
    }

    /// <summary>
    /// Désactive les collider des mains lorsque le joueur s'éloigne du digicode
    /// Cela permet au joueur de pouvoir de nouveau attraper des objets
    /// </summary>
    /// <param name="other">Collider avec lequel l'objet rentre en contact</param>
    void OnTriggerExit(Collider other)
    {
        if (other == Head)
        {
            other.gameObject.transform.parent.GetChild(1).gameObject.GetComponent<SphereCollider>().enabled = false;
            other.gameObject.transform.parent.GetChild(2).gameObject.GetComponent<SphereCollider>().enabled = false;
        }
    }

}
