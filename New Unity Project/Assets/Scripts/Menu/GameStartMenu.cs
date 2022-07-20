using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [SerializeField] Image background = null;
    [SerializeField] TMP_Text startMent = null;
    [SerializeField] GameObject button = null;

    string temp = "They are coming";
    
    private void Start() 
    {
        startMent.text = temp; 
    }

    public void FadeOutBackground()
    {
        button.SetActive(false);
        StartCoroutine(FadeOut());
        Destroy(this.gameObject);
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
    }
}
