using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] private Stat stat = null;
    [SerializeField] private GameObject hpBarObject = null;
    [SerializeField] private Image hpBarImage = null;
    [SerializeField] private Image hpBarAfterImage = null;
    private float slowFillAmount = 1f;
    private float currentFillAmount = 1f;
    private float time = 0f;
    private bool delayAfterImage = true;

    private bool updateUPAfterImageBarFlag;
    private void Awake() {
        stat.ClientOnHealthUpdated += updateHPbar;
    }

    private void OnDestroy() {
        stat.ClientOnHealthUpdated -= updateHPbar;
    }
    private void Start(){
        hpBarObject.SetActive(false);
    }
    
    private void OnMouseOver() {
        hpBarObject.SetActive(true);
    }
    private void OnMouseExit() {
        StartCoroutine(HideHPbar());
        
    }
    IEnumerator HideHPbar(){
        yield return new WaitForSeconds(1);
        hpBarObject.SetActive(false);
    }
    private void updateHPbar(int currentHP, int healthPoint){
        currentFillAmount = (float)currentHP / healthPoint;
        hpBarImage.fillAmount = currentFillAmount;
        time = 0f;
        delayAfterImage = true;
    }
    private void Update() {
        slowFillAmount = hpBarAfterImage.fillAmount;

        if(slowFillAmount == currentFillAmount){ return; }

        if(delayAfterImage){
            StartCoroutine(DelayAfterImage());
            return; 
        }
            
        slowFillAmount = Mathf.Lerp(slowFillAmount, currentFillAmount, time);
        time += 1.0f * Time.deltaTime;

        hpBarAfterImage.fillAmount = slowFillAmount;
    }

    IEnumerator DelayAfterImage(){
        yield return new WaitForSeconds(0.8f);
        delayAfterImage = false;
    }
    

}
