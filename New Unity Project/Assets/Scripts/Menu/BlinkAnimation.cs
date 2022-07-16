using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlinkAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float time;
    private Color defaultColor = Color.white;
    private bool mouseEnter = false;
    private void Update() {
        if(!mouseEnter){ return; }

        if(time < 0.5f)
        {
            gameObject.GetComponentInChildren<TMP_Text>().color = new Color(1 ,1 ,1 , 1 - time);
        }
        else
        {
            gameObject.GetComponentInChildren<TMP_Text>().color = new Color(1 ,1 ,1 , time);
            if(time > 1f)
            {
                time = 0;
            }
        }
        time += Time.deltaTime;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEnter = true;    
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEnter = false;
        gameObject.GetComponentInChildren<TMP_Text>().color = defaultColor;
    }
}
