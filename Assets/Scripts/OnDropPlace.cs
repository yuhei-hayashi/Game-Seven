using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnDropPlace : MonoBehaviour ,  IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        CardMovement card = eventData.pointerDrag.GetComponent<CardMovement>();
        if(card != null)
        {
            card.defaultParent = this.transform;
        }
    }
}
