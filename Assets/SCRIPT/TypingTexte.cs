using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TypingTexte : MonoBehaviour
{
    string originaleTexte;
    Text uiText;
    public float delai = 0.2f;
    void Start()
    {
        uiText = GetComponent<Text>();
        originaleTexte = uiText.text;
        uiText.text = null;
        StartCoroutine(ShowLetterByLetter());
    }


    IEnumerator ShowLetterByLetter()
    {
        for (int i = 0; i <= originaleTexte.Length; i++)
        {
            uiText.text = originaleTexte.Substring(0, i);
            yield return new WaitForSeconds(delai);

        }
    }
}
