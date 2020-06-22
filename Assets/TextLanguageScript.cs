using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TextLanguageScript : MonoBehaviour
{
    public Font englishFont;
    public Font chineseFont;

    public string englishText;
    public string chineseText;

    private Text text;

    public bool isInMainMenu = true;//To subscribe to language switch event or not


    private void Start()
    {
        text = gameObject.GetComponent<Text>();
        if(isInMainMenu)
        {
            LanguageSwitchButton.languageSwitchButton.OnLanguageSwap += UpdateLanguage;
        }
    }

    void UpdateLanguage(bool isEnglish)
    {
        if (!isEnglish)
        {
            text.font = chineseFont;
            text.text = chineseText;
        }
        else
        {
            text.font = englishFont;
            text.text = englishText;
        }
    }

}
