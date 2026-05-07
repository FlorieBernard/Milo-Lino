using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueZone : MonoBehaviour
{
    [Header("Dialogue")]
    public Sprite portraitMilo;
    public Sprite portraitLino;
    [TextArea(2, 4)]
    public string[] lignes;
    public string[] nomPersonnages;

    [Header("UI")]
    public GameObject panneauDialogue;
    public Image portrait;
    public TextMeshProUGUI nomTexte;
    public TextMeshProUGUI dialogueTexte;

    private int indexLigne = 0;
    private bool enCours = false;
    private bool dejaJoue = false;
    private int loopInt = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Start dialogue");
        if (!other.CompareTag("Milo")) return;
        Debug.Log("PlayerDetected");
        if (dejaJoue || enCours) return;

        StartCoroutine(LancerDialogue());
    }

    private IEnumerator LancerDialogue()
    {
        enCours = true;
        dejaJoue = true;
        panneauDialogue.SetActive(true);
        
        loopInt = 0;

        foreach (string ligne in lignes)
        {
            nomTexte.text = nomPersonnages[loopInt];
            if (nomPersonnages[loopInt] == "Milo") 
            {
                portrait.sprite = portraitMilo;
            }
            else 
            {
                portrait.sprite = portraitLino;
            }
                dialogueTexte.text = "";
            foreach (char c in ligne)
            {
                dialogueTexte.text += c;
                
                yield return new WaitForSeconds(0.04f);
            }

            loopInt++;
            yield return new WaitForSeconds(2f); // pause entre les lignes
        }

        panneauDialogue.SetActive(false);
        enCours = false;
    }
}

