using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CardController : MonoBehaviourPunCallbacks
{       public CardView view;
        public CardModel model;
        private void Awake()
        {
            view = GetComponent<CardView>();
        }

    public void Init(int cardID,int ownerID)
    {
        model = new CardModel(cardID,ownerID);
        view.Show(model.icon);
    }

}
