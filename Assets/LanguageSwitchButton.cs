using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSwitchButton : MonoBehaviour
{
    public static LanguageSwitchButton languageSwitchButton;

    private void Awake()
    {
        languageSwitchButton = this;
    }
    //A very crappy script for language swapping
    private string english = "English";
    private string chinese = "中文";

    public Font englishFont;
    public Font chineseFont;

    private bool isEnglish = true;

    private Text text;
    private void Start()
    {
        text = gameObject.GetComponent<Text>();
        text.font = chineseFont;
        text.text = chinese;
    }

    public event Action<bool> OnLanguageSwap;

    public void SwapLanguages()
    {
        if(isEnglish)
        {
            isEnglish = false;
            text.font = englishFont;
            text.text = english;
        }
        else
        {
            isEnglish = true;
            text.font = chineseFont;
            text.text = chinese;
        }
        OnLanguageSwap?.Invoke(isEnglish);
    }
}
