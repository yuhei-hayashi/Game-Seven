using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class FieldDropPlace : MonoBehaviourPunCallbacks , IDropHandler , IPointerEnterHandler ,IPointerExitHandler
{
  public void OnDrop(PointerEventData eventData)
    {
        FieldController[] fieldList = this.transform.parent.GetComponentsInChildren<FieldController>();
        CardController card = eventData.pointerDrag.GetComponent<CardController>();
        CardMovement cardMovement = eventData.pointerDrag.GetComponent<CardMovement>();
        FieldController field = GetComponent<FieldController>();
        if((card != null) && (field.model.color == card.model.color) && (card.model.number == field.model.num))
        {
          if(field.model.num == 13)
          {
            if(fieldList[0].model.cards >= 1 || fieldList[11].model.cards >= 1)
            {
              card.clickedGameObject = null;
              this.transform.localScale = new Vector3( 1 , 1 , 0 );
              int cardID = card.model.id;
              GameManager.instance.CreateFieldCardNetwork(cardID,field.model.id,field.model.color);
              Destroy(card.gameObject);
              GameManager.instance.DiscardEnemyHandNetwork(cardMovement.siblingIndex);
              GameManager.instance.CheckWin(this.transform.parent);
              GameManager.instance.ChangeTurnNetwork();
            }
          }
          else if(field.model.num == 1)
          {
            if(fieldList[12].model.cards >= 1 || fieldList[1].model.cards >= 1)
            {
              card.clickedGameObject = null;
              this.transform.localScale = new Vector3( 1 , 1 , 0 );
              int cardID = card.model.id;
              GameManager.instance.CreateFieldCardNetwork(cardID,field.model.id,field.model.color);
              Destroy(card.gameObject);
              GameManager.instance.DiscardEnemyHandNetwork(cardMovement.siblingIndex);
              GameManager.instance.CheckWin(this.transform.parent);
              GameManager.instance.ChangeTurnNetwork();
            }
          }
          else
          {
            if(fieldList[field.model.num].model.cards >= 1 || fieldList[(field.model.num - 2)].model.cards >= 1)
            {
              card.clickedGameObject = null;
              this.transform.localScale = new Vector3( 1 , 1 , 0 );
              int cardID = card.model.id;
              GameManager.instance.CreateFieldCardNetwork(cardID,field.model.id,field.model.color);
              Destroy(card.gameObject);
              GameManager.instance.DiscardEnemyHandNetwork(cardMovement.siblingIndex);
              GameManager.instance.CheckWin(this.transform.parent);
              GameManager.instance.ChangeTurnNetwork();
            }
          }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
      if(eventData.pointerDrag != null )
      {
        FieldController[] fieldList = this.transform.parent.GetComponentsInChildren<FieldController>();
        CardController card = eventData.pointerDrag.GetComponent<CardController>();
        FieldController field = GetComponent<FieldController>();
        if(card != null)
        {
          if(field.model.color == card.model.color && card.model.number == field.model.num)
          {
            if(field.model.num == 13)
            {
              if(fieldList[0].model.cards >= 1 || fieldList[11].model.cards >= 1)
              {
                if(field.model.cards == 0)
                {
                  this.transform.localScale = new Vector3( 2 , 2 , 0 );
                  this.gameObject.GetComponent<Image>().color = Color.red;
                }
                else if(field.model.cards == 1)
                {
                  this.transform.localScale = new Vector3( 2 , 2 , 0 );
                }
              }
            }
            else if(field.model.num == 1)
            {
              if(fieldList[0].model.cards >= 1 || fieldList[11].model.cards >= 1)
              {
                if(field.model.cards == 0)
                {
                  this.transform.localScale = new Vector3( 2 , 2 , 0 );
                  this.gameObject.GetComponent<Image>().color = Color.red;
                }
                else if(field.model.cards == 1)
                {
                  this.transform.localScale = new Vector3( 2 , 2 , 0 );
                }
              }
            }
            else
            {
              if(fieldList[field.model.num].model.cards >= 1 || fieldList[(field.model.num - 2)].model.cards >= 1)
              {
                if(field.model.cards == 0)
                {
                  this.transform.localScale = new Vector3( 2 , 2 , 0 );
                  this.gameObject.GetComponent<Image>().color = Color.red;
                }
                else if(field.model.cards == 1)
                {
                  this.transform.localScale = new Vector3( 2 , 2 , 0 );
                }
              }
            }
          }
        }
      }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      this.transform.localScale = new Vector3( 1 , 1 , 0 );
      this.gameObject.GetComponent<Image>().color = new Color(0,0,0,0);
    }
}
