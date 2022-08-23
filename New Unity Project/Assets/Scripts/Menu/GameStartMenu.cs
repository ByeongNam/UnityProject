using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class GameStartMenu : MonoBehaviour
{
    [SerializeField] Image background = null;
    [SerializeField] TMP_Text startMent = null;
    [SerializeField] GameObject button = null;

    public SoftMaskScript softMaskScript = null;

    public static event Action OnGameStartSetting;  

    string temp = "Hint: ";
    
    private void Start() 
    {
        startMent.text = temp; 
    }

    public void GameStartSetting()
    {
        button.SetActive(false);
        OnGameStartSetting?.Invoke();

        StartCoroutine(Swirl());
        //StartCoroutine(FadeOut());
    }
    IEnumerator Swirl()
    {
        while (softMaskScript.CutOff > 0)
        {
            softMaskScript.CutOff -= 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
        gameObject.SetActive(false);
    }
    IEnumerator FadeOut()
    {
        float count = 0;
        while (count < 1.0f)
        {
            count += 0.01f;
            yield return new WaitForSeconds(0.01f);
            background.color = new Color(0, 0, 0, 1.0f - count);
        }
        gameObject.SetActive(false);
    }
}
