using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public Transform parentToReturnTo = null;
    [HideInInspector]
    public Transform placeHolderParent = null;

    private GameObject placeHolder = null;

    private Vector2 cardDefaultPostion = Vector2.zero;

    private void Start()
    {
        cardDefaultPostion = GetComponent<RectTransform>().position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        cardDefaultPostion = eventData.position;
        placeHolder = new GameObject();
        placeHolder.transform.SetParent(this.transform.parent);
        LayoutElement le = placeHolder.AddComponent<LayoutElement>();
        le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        placeHolder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

        parentToReturnTo = this.transform.parent;
        placeHolderParent = parentToReturnTo;
        this.transform.SetParent(this.transform.parent.parent);

        this.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 oldPosition = eventData.position;
        oldPosition.y = cardDefaultPostion.y;
        this.transform.position = oldPosition;

        if (placeHolder.transform.parent != placeHolderParent)
        {
            placeHolder.transform.SetParent(placeHolderParent);
        }

        int newSiblingIndex = placeHolderParent.childCount;

        for (int i = 0; i < placeHolderParent.childCount; i++)
        {
            if (this.transform.position.x < placeHolderParent.GetChild(i).position.x)
            {
                newSiblingIndex = i;

                if (placeHolder.transform.GetSiblingIndex() < newSiblingIndex)
                    newSiblingIndex--;

                break;
            }
        }

        placeHolder.transform.SetSiblingIndex(newSiblingIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(parentToReturnTo);
        this.transform.SetSiblingIndex(placeHolder.transform.GetSiblingIndex());
        if (this.GetComponent<CanvasGroup>())
        {
            this.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        Destroy(placeHolder);
        print(transform.parent.name);
        List<int> numbers = new();
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform transform1 = transform.parent.GetChild(i);
            {
                RolledNumberScript rolledNumberScript = transform1.GetComponent<RolledNumberScript>();
                numbers.Add(rolledNumberScript.number);
            }
           
        }

        GameManager.gmInstance.accumulatedDices = numbers;
    }
}
