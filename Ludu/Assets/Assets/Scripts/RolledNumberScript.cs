using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RolledNumberScript : MonoBehaviour, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    // Start is called before the first frame update
    public TextMeshProUGUI text;
    public int number;
    private float lastClickTime;
    public float doubleClickTimeThreshold = 0.3f; // Adjust as needed
    public bool trigger = false;

    public void SetNumber(int number)
    {
        this.number = number;
        text.text = number.ToString();
    }

    public void SetText(int number)
    {
        text.text = number.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.gmInstance.doHint)
        {
            GameManager.gmInstance.RolledNumberRequest(this);
        }
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        Debug.Log("UI element selected");
        trigger = true;
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        Debug.Log("UI element de selected");
        trigger = false;
    }

    public int Number
    {
        get => this.number; 
    }

}
