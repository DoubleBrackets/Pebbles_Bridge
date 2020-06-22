using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldLanguageScript : MonoBehaviour
{
    public string englishText;
    public string chineseText;

    private InputField text;

    public bool isInMainMenu = true;//To subscribe to language switch event or not


    private void Start()
    {
        text = gameObject.GetComponent<InputField>();
        if (isInMainMenu)
        {
            LanguageSwitchButton.languageSwitchButton.OnLanguageSwap += UpdateLanguage;
        }
    }

    void UpdateLanguage(bool isEnglish)
    {
        if (!isEnglish)
        {
            text.text = chineseText;
        }
        else
        {
            text.text = englishText;
        }
    }
}
