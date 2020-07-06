using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe gérant l'ouverture de la boite contenant l'ordinateur. 
/// Ce script s'occupe également de faire la transition entre la partie géoroom et la partie drone
/// </summary>
public class OpenableBoxController : MonoBehaviour
{
    [Tooltip("Clé ouvrant la boite")]
    public GameObject key;

    [Tooltip("Position de la clé lorsque l'animation se met en marche")]
    public Transform key_parent;

    [Tooltip("Point où l'on téléporte le joueur (à l'exterieur du georoom)")]
    public Transform point_tp_transition;

    [Tooltip("Ecran de transition (voir canvas)")]
    public GameObject transition_screen;

    [Tooltip("L'objet drone qui est désactivé dans la scène (drone_red)")]
    public GameObject drone;

    [Tooltip("Le modèle de la boussole")]
    public GameObject compass;

    [Tooltip("La main droite")]
    public GameObject rightHand;

    //True si la boite est ouverte, false sinon
    bool is_open;

    //True si la transition a été effectué, false sinon
    bool transitionned;

    Color transparent;
    Dialogue dialogue;

    //La boussole
    GameObject myCompass;


    // Start is called before the first frame update
    void Start()
    {
        //initialisation des variables et du dialogue de transition
        transitionned = false;
        is_open = false;
        transparent.a = 0;
        dialogue = new Dialogue();
        dialogue.sentences = new string[3];
        dialogue.sentences[0] = "L'ordinateur est déverouillé.";
        dialogue.sentences[1] = "Une fois ce dialogue passé, vous prendrez les commandes d'un drone dans la commune de Saint Mandé.";
        dialogue.sentences[2] = "Retrouvez la borne géodésique avec l’aide de vos amis qui ont accès à ses coordonnées.";
    }

    // Update is called once per frame
    void Update()
    {
        if (is_open && !GameObject.Find("DialogueManager").GetComponent<DialogueManager>().is_dialogue && !transitionned)
        {
            StartCoroutine(Transition());
            transitionned = true;
        }
    }

    /// <summary>
    /// On détecte si la bonne clé entre en contact avec la boîte
    /// </summary>
    /// <param name="other">Objet avec lequel entre en contact la boite</param>
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "key_silver Variant(Clone)")
        {
            if (other.gameObject.GetComponent<KeyController2>().is_good_key)
            {
                GetComponent<AudioSource>().Play();

                //On détruit la clé qui entre en contact et on en cré une nouvelle au bon endroit, puis on
                //lance l'animation correspondante
                GameObject new_key = Instantiate(key, key_parent);
                Destroy(other.gameObject);
                //new_key.GetComponent<KeyAnimationController>().enabled = true;
                new_key.GetComponent<Animator>().enabled = true;
                new_key.GetComponent<Animator>().Play("Key_disapear_Animation");

                //On lance également l'animation de la boite
                GetComponent<Animator>().Play("BoxOpen");
                //On détruit la clé 
                StartCoroutine(DestroyAfterSeconds(new_key, 1.5f));
                //on lance le dialogue de transition
                StartCoroutine(GameObject.Find("DialogueManager").GetComponent<DialogueManager>().StartDialogue(dialogue));
                //other.gameObject.GetComponent<KeyAnimationController>().enabled = true;
                is_open = true;
            }
        }
    }

    /// <summary>
    /// Détruit un objet après quelques secondes
    /// </summary>
    /// <param name="go">Objet à détruire</param>
    /// <param name="sec">Nombre de secondes avant destruction</param>
    /// <returns></returns>
    IEnumerator DestroyAfterSeconds(GameObject go, float sec)
    {
        yield return new WaitForSecondsRealtime(sec);
        Destroy(go);
    }

    /// <summary>
    /// Méthode établissant la transition entre le georoom et la partie en drone
    /// </summary>
    /// <returns></returns>
    IEnumerator Transition()
    {
        GameObject player = GameObject.Find("Player");
        GameObject HUD = GameObject.Find("HUD");
        yield return new WaitForSecondsRealtime(1.5f);

        //Lancement de l'animation de l'écran de transition
        transition_screen.GetComponent<Animator>().Play("Transition_screen_Animation");
        yield return new WaitForSecondsRealtime(1.98f);

        //téléportation du joueur
        player.transform.position = point_tp_transition.position;
        player.GetComponent<PlayerController>().in_georoom = false;
        GameObject.Find("LeftHand").GetComponent<HandController>().HideMap();

        //activation du drone
        drone.SetActive(true);
        drone.GetComponent<MyDroneController>().enabled = true;

        //Affichage des objets dont on avait supprimé le renderer
        ChangePositionGeoroom.EnableFarRenderer(GameObject.Find("Georoom_real_axes").transform);

        //Activation du HUD easting, northing et alitude
        HUD.transform.GetChild(0).gameObject.SetActive(true);
        HUD.transform.GetChild(1).gameObject.SetActive(true);
        HUD.transform.GetChild(2).gameObject.SetActive(true);
        StartCoroutine(DestroyAfterSeconds(gameObject, 2f));

        //On supprime le géoroom pour gagner en performance (le géoroom fait au moins 1 mliions de vertices)
        Destroy(GameObject.Find("Georoom_real_axes"));
        GameObject.Find("bati_indifferencie.1658549").GetComponent<MeshRenderer>().enabled = true;
        GameObject.Find("bati_indifferencie.1658549").GetComponent<MeshCollider>().enabled = true;
        GameObject.Find("Toit de bati_indifferencie.1658549").GetComponent<MeshRenderer>().enabled = true;
        GameObject.Find("bati_indifferencie.1658555").GetComponent<MeshRenderer>().enabled = true;
        GameObject.Find("bati_indifferencie.1658555").GetComponent<MeshCollider>().enabled = true;
        GameObject.Find("Toit de bati_indifferencie.1658555").GetComponent<MeshRenderer>().enabled = true;

        myCompass = Instantiate(compass, rightHand.transform);
        myCompass.name = "compass";
    }
}
