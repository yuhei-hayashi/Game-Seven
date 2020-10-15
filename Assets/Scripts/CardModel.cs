using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class CardModel
    {
        public int id;
        public string color;
        public string mark;
        public int number;
        public int owner_id;
        public Sprite icon;

        public CardModel(int cardID,int ownerID)
        {
            CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/Card " + cardID);
            id = cardEntity.id;
            color = cardEntity.color;
            mark = cardEntity.mark;
            number = cardEntity.number;
            icon = cardEntity.icon;
            owner_id = ownerID;
        }
    }

