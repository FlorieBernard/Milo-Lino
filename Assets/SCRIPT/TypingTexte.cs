using System.Collections;
using TMPro;
using UnityEngine;

public class TypingTexte : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string[] originaleTexte;
    [SerializeField] private TextMeshProUGUI uiText;

    [Tooltip("Délai entre chaque lettre (secondes)")]
    [SerializeField] private float delaiEntreLettre = 0.05f;

    [Tooltip("Délai entre chaque phrase (secondes)")]
    [SerializeField] private float delaiEntrePhrase = 1f;

    private void Start()
    {
        //  Récupérer le composant si non assigné
        if (uiText == null)
        {
            uiText = GetComponent<TextMeshProUGUI>();
        }

        //  Lancer l'affichage
        StartCoroutine(ShowPhraseByPhrase());
    }

    /// <summary>
    /// Affiche chaque phrase l'une après l'autre
    /// </summary>
    private IEnumerator ShowPhraseByPhrase()
    {
        for (int i = 0; i < originaleTexte.Length; i++)
        {
            //  Vider le texte avant d'afficher la nouvelle phrase
            uiText.text = "";

            //  Afficher la phrase lettre par lettre (et ATTENDRE qu'elle soit terminée)
            yield return StartCoroutine(ShowLetterByLetter(originaleTexte[i]));

            //  Attendre avant de passer à la phrase suivante
            yield return new WaitForSeconds(delaiEntrePhrase);
        }

        Debug.Log(" Toutes les phrases ont été affichées");
    }

    /// <summary>
    /// Affiche une phrase lettre par lettre
    /// </summary>
    private IEnumerator ShowLetterByLetter(string phrase)
    {
        for (int i = 0; i <= phrase.Length; i++) //  <= pour afficher la phrase complète
        {
            //  Afficher les lettres de 0 à i
            uiText.text = phrase.Substring(0, i);

            //  Attendre entre chaque lettre
            yield return new WaitForSeconds(delaiEntreLettre);
        }
    }
}   