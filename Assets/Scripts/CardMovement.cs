using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class CardMovement : MonoBehaviour , IDragHandler, IBeginDragHandler , IEndDragHandler
{
    public Transform defaultParent;
    public int siblingIndex;
    public void OnBeginDrag(PointerEventData eventData)
    {
        CardController card = eventData.pointerDrag.GetComponent<CardController>();
        if((GameManager.instance.isPlayerTurn || GameManager.instance.isPlayerMulligan) && card.model.owner_id == PhotonNetwork.LocalPlayer.ActorNumber)
        {
        siblingIndex = transform.GetSiblingIndex();
        defaultParent = transform.parent;
        transform.SetParent(defaultParent.parent , false);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        CardController card = eventData.pointerDrag.GetComponent<CardController>();
        if((GameManager.instance.isPlayerTurn || GameManager.instance.isPlayerMulligan) && card.model.owner_id == PhotonNetwork.LocalPlayer.ActorNumber)
        {
        transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CardController card = eventData.pointerDrag.GetComponent<CardController>();
        if((GameManager.instance.isPlayerTurn || GameManager.instance.isPlayerMulligan) && card.model.owner_id == PhotonNetwork.LocalPlayer.ActorNumber)
        {
        transform.SetParent(defaultParent , false);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        transform.SetSiblingIndex(siblingIndex);
        }
    }
}
