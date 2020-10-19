using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CardController : MonoBehaviourPunCallbacks
{       public CardView view;
        public CardModel model;
            public GameObject clickedGameObject;
        private void Awake()
        {
            view = GetComponent<CardView>();
        }

        void Update ()
        {
            if (Input.GetMouseButtonDown(0))
            {
                clickedGameObject = null;

                Vector2 tapPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D collition2d = Physics2D.OverlapPoint(tapPoint);

                if (collition2d)
                {
                    clickedGameObject = collition2d.transform.gameObject;
                    Debug.Log("OK");
                }
                Debug.Log(clickedGameObject);
            }
        }
    public void Init(int cardID,int ownerID)
    {
        model = new CardModel(cardID,ownerID);
        view.Show(model.icon);
    }
}
