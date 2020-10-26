using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] CardController handcardPrefab,
                                    fieldcardPrefab;
    [SerializeField] FieldController fieldPrefab;
        [SerializeField] Transform playerHandTransform,
                               enemyHandTransform,
                               redFieldTransform,
                               blackFieldTransform,
                               MulliganHandTransform,
                               MulliganChangeTransform,
                               EffectFieldTransform;

    [SerializeField] Button MulliganChangeButton;
    [SerializeField] Text TurnText,
                          ResultText;
    [SerializeField]  Image BackImage;

    [SerializeField] GameObject Canvas,
                                MulliganCanvas,
                                ResultCanvas,
                                EffectCanvas;
    public bool isPlayerTurn,
                isPlayerMulligan,
                isEnemyMulligan,
                isMulliganTurn,
                isEffect;

    public int CountExtraTurn,
               CountEnemyExtraTurn;
    public CardController clickedCard;
    List<int> playerDeck = new List<int>();
    List<int> enemyDeck  = new List<int>();

    public Dictionary<int,System.Action> cardEffect = new Dictionary<int,System.Action>();

    public static GameManager instance;
        private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        // PhotonServerSettingsに設定した内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
        Canvas.SetActive(false);
        MulliganCanvas.SetActive(false);
        ResultCanvas.SetActive(false);
        EffectCanvas.SetActive(false);
    }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        // "room"という名前のルームに参加する（ルームが無ければ作成してから参加する）
        PhotonNetwork.JoinOrCreateRoom("room", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        if(PhotonNetwork.PlayerList.Length == 2)
        {
            TurnDecision();
            photonView.RPC(nameof(StartGame), RpcTarget.All);
            SettingSevenFieldcCard();
        }
    }
        public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerLeftRoom");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    [PunRPC]
    private IEnumerator StartGame()
    {
            EffectInstall();
            Canvas.SetActive(true);
            SettingFieldzone(blackFieldTransform);
            SettingFieldzone(redFieldTransform);
            CreateDeck("spade","heart",playerDeck);
            CreateEnemyDeck(enemyDeck);
            Shuffle(playerDeck);
            SettingInitHand();
            yield return StartCoroutine(Mulligan());
            TurnCalc();
    }

    IEnumerator Mulligan()
    {

        isMulliganTurn = isPlayerTurn;
        isPlayerTurn = false;
        isPlayerMulligan = true;
        isEnemyMulligan = true;
        MulliganCanvas.SetActive(true);
        TextChange(TurnText);
        CardController[] HandCardList = playerHandTransform.GetComponentsInChildren<CardController>();
        MulliganHandGet(HandCardList,MulliganHandTransform);
        yield return StartCoroutine( Wait() );
        CardController[] MulliganCardList = MulliganHandTransform.GetComponentsInChildren<CardController>();
        MulliganHandReturn(MulliganCardList,playerHandTransform);
        MulliganCanvas.SetActive(false);
        isPlayerTurn = isMulliganTurn;
        yield return null;
    }

    void TextChange(Text Text)
    {
        if(isMulliganTurn)
        {
            Text.text = "先攻";
        }
        else
        {
            Text.text = "後攻";
        }
    }

    void MulliganHandGet(CardController[] CardList,Transform Parent)
    {
        int n = CardList.Length;
        while(n > 0)
        {
            n--;
            CardList[n].transform.SetParent(Parent,false);
            DiscardEnemyHandNetwork(n);
            enemyDeck.Add(54);
        }
    }

    void MulliganHandReturn(CardController[] CardList,Transform Parent)
    {
        int n = CardList.Length;
        int i = 0;
        while(n > i)
        {
            CardList[i].transform.SetParent(Parent,false);
            int cardID = CardList[i].model.id;
            photonView.RPC(nameof(GiveCardEnemyHand),RpcTarget.Others,PhotonNetwork.LocalPlayer.ActorNumber,cardID);
            i++;
        }
    }

    public void MulliganCard()
    {
        CardController[] MulliganCardList = MulliganChangeTransform.GetComponentsInChildren<CardController>();
        int n = MulliganCardList.Length;
        int i = 0;
        while(n > 0)
            {
                n--;
                i++;
                int k = MulliganCardList[n].model.id;
                playerDeck.Add(k);
                Destroy(MulliganCardList[n].gameObject);
            }
            Shuffle(playerDeck);
            while(i > 0)
            {
                i--;
                int cardID = playerDeck[0];
                playerDeck.RemoveAt(0);
                CreateCard(cardID,MulliganHandTransform,PhotonNetwork.LocalPlayer.ActorNumber);
            }
        isPlayerMulligan = false;
        photonView.RPC(nameof(Mulliganfinish),RpcTarget.Others);
    }

    [PunRPC]
    void Mulliganfinish()
    {
        isEnemyMulligan = false;
    }

    IEnumerator Wait()
    {
        while ( isPlayerMulligan ||  isEnemyMulligan )
        {
            yield return new WaitForEndOfFrame ();
        }
        yield return null;
    }

    void TurnDecision()
    {
        int t = UnityEngine.Random.Range(0,2);
        if(t == 0)
        {
            MyTurn();
        }
        else
        {
            photonView.RPC(nameof(MyTurn),RpcTarget.Others);
        }
    }

    [PunRPC]
    void MyTurn()
    {
            isPlayerTurn = true;
    }

    void SettingFieldzone(Transform field)
    {
        if(field == blackFieldTransform)
        {
            for(int i = 1; i < 14;i++ )
            {
                CreateField(i,"black",field);
            }
        }
        else
        {
            for(int i = 1; i < 14;i++ )
            {
                CreateField(i,"red",field);
            }
        }
    }

    void SettingSevenFieldcCard()
    {
        CreateFieldCardNetwork(7,6,"black");
        CreateFieldCardNetwork(46,6,"red");
        photonView.RPC(nameof(CreateFieldCardNetwork),RpcTarget.Others,7,6,"black");
        photonView.RPC(nameof(CreateFieldCardNetwork),RpcTarget.Others,46,6,"red");
    }
    void CreateField(int fieldID,string fieldColor,Transform field)
    {
        FieldController fieldzone = Instantiate(fieldPrefab,field,false);
        fieldzone.InitField(fieldID,fieldColor);

    }

    void CreateDeck(string Black,string Red,List<int> deck)
    {
        if(Black == "spade")
        {
            InsertMarkCards(0,deck);
        }
        else if(Black == "club")
        {
            InsertMarkCards(1,deck);
        }

        if(Red == "diamond")
        {
            InsertMarkCards(2,deck);
        }
        else
        {
            InsertMarkCards(3,deck);
        }
    }

    void CreateEnemyDeck(List<int> deck)
    {
        for(int i = 1; i < 25; i++)
        {
                deck.Add(54);
        }
    }

    void InsertMarkCards(int marknumber,List<int> deck)
    {
        int n = (marknumber * 13);
        for(int i = 1; i < 14; i++)
        {
            if(i != 7)
            {
                deck.Add((n + i));
            }
        }
    }

    void SettingInitHand()
    {
        for(int i = 0; i < 5; i++)
        {
        GiveCardToHand(playerDeck,playerHandTransform);
        }
    }
    void GiveCardToHand(List<int> deck,Transform hand)
    {
        if(deck.Count == 0)
        {
            return;
        }
        int cardID = deck[0];
        deck.RemoveAt(0);
        CreateCard(cardID,hand,PhotonNetwork.LocalPlayer.ActorNumber);
        photonView.RPC(nameof(GiveCardEnemyHand),RpcTarget.Others,PhotonNetwork.LocalPlayer.ActorNumber,cardID);
    }
    [PunRPC]
    public void GiveCardEnemyHand(int ownerID,int cardID)
    {
        List<int> deck = enemyDeck;
        if(deck.Count == 0)
        {
            return;
        }
        deck.RemoveAt(0);
        CreateCard(cardID,enemyHandTransform,ownerID);
        CardController[] EnemyCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        int n = EnemyCardList.Length - 1;
        CardController card = EnemyCardList[n];
        card.view.Show(BackImage.sprite);
    }
        void CreateCard(int cardID,Transform hand,int ownerID)
    {
        CardController card = Instantiate(handcardPrefab,hand,false);
        card.Init(cardID,ownerID);
    }

    public void DiscardEnemyHandNetwork(int i)
    {
        photonView.RPC(nameof(DiscardEnemyHand),RpcTarget.Others,i);
    }
    [PunRPC]
    public void DiscardEnemyHand(int i)
    {
        CardController card = enemyHandTransform.GetComponentsInChildren<CardController>()[i];
        Destroy(card.gameObject);
    }

    [PunRPC]
    public void CreateFieldCardNetwork(int cardID,int fieldID,string color)
    {
        photonView.RPC(nameof(CreateFieldCard),RpcTarget.All,cardID,fieldID,color,PhotonNetwork.LocalPlayer.ActorNumber);
    }
    [PunRPC]
    public void CreateFieldCard(int cardID,int fieldID,string color,int ownerID)
    {
        if(color == "black")
        {
            FieldController field = blackFieldTransform.GetComponentsInChildren<FieldController>()[fieldID];
            CardController card = Instantiate(fieldcardPrefab,field.transform,false);
            card.Init(cardID,ownerID);
            field.model.cards++;
        }
        else
        {
            FieldController field = redFieldTransform.GetComponentsInChildren<FieldController>()[fieldID];
            CardController card = Instantiate(fieldcardPrefab,field.transform,false);
            card.Init(cardID,ownerID);
            field.model.cards++;
        }
    }

    public void Pass()
    {
        if(isPlayerTurn)
        {
        photonView.RPC(nameof(ChangeTurn), RpcTarget.All);
        }
    }
    public void ChangeTurnNetwork()
    {
        if(isPlayerTurn)
        {
            photonView.RPC(nameof(ChangeTurn), RpcTarget.All);
        }
    }

    [PunRPC]
    public void ChangeTurn()
    {
        if(CountExtraTurn >= 1 && isPlayerTurn)
        {
            CountExtraTurn--;
        }
        else if(CountEnemyExtraTurn >= 1 && !isPlayerTurn)
        {
            CountEnemyExtraTurn--;
        }
        else
        {
            isPlayerTurn = !isPlayerTurn;
        }
        if(isPlayerTurn)
        {
            GiveCardToHand(playerDeck,playerHandTransform);
        }
        TurnCalc();
    }

    void TurnCalc()
    {
        if(isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            EnemyTurn();
        }
    }

    void PlayerTurn()
    {
        Debug.Log("Playerのターン");
    }

    void EnemyTurn()
    {
        Debug.Log("Enemyのターン");
    }

    void Shuffle(List<int> deck)
    {
        int n = deck.Count;
        while(n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0,n + 1);
            int temp = deck[k];
            deck[k] = deck[n];
            deck[n] = temp;
        }
    }

    public void CheckWin(Transform Field)
    {
        FieldController[] FieldList = Field.GetComponentsInChildren<FieldController>();
        int n = 0;
        int count = 0;
        while( n <13 )
        {
            CardController[] CardList = FieldList[n].transform.GetComponentsInChildren<CardController>();
            int i = CardList.Length;
            while( i > 0)
            {
                i--;
                if(CardList[i].model.owner_id == PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    count++;
                }
            }
            n++;
        }
        if(count == 13)
        {
            GameWin();
        }
    }

    void GameWin()
    {
        ResultCanvas.SetActive(true);
        photonView.RPC(nameof(GameLose),RpcTarget.Others);
    }

    [PunRPC]
    void GameLose()
    {
        ResultText.text = "You Lose";
        ResultCanvas.SetActive(true);
    }

    public void EffectInstall()
    {
        cardEffect.Add(1,Spade1);
        cardEffect.Add(2,Spade2);
        cardEffect.Add(3,Spade3);
        cardEffect.Add(4,Spade4);
        cardEffect.Add(5,Spade5);
        cardEffect.Add(6,Spade6);
        cardEffect.Add(8,Spade8);
        cardEffect.Add(9,Spade9);
        cardEffect.Add(10,Spade10);
        cardEffect.Add(11,Spade12);
        cardEffect.Add(12,Spade12);
        cardEffect.Add(13,Spade13);
        cardEffect.Add(40,Heart1);
        cardEffect.Add(41,Heart2);
        cardEffect.Add(42,Heart3);
        cardEffect.Add(43,Heart4);
        cardEffect.Add(44,Heart5);
        cardEffect.Add(45,Heart6);
        cardEffect.Add(47,Heart8);
        cardEffect.Add(48,Heart9);
        cardEffect.Add(49,Heart10);
        cardEffect.Add(50,Heart11);
        cardEffect.Add(51,Heart12);
        cardEffect.Add(52,Heart13);
    }

    public void CallEffect(int cardID)
    {
        System.Action action = cardEffect[cardID];
        action();
    }

     void Spade1()
    {
        CountExtraTurn = 1;
        photonView.RPC(nameof(EnemyExtraTurn),RpcTarget.Others);
        isEffect = false;
    }

    void Spade2()
    {
        CardController[] playerCardList = playerHandTransform.GetComponentsInChildren<CardController>();
        int n = playerCardList.Length;
        while(n < 7)
        {
            GiveCardToHand(playerDeck,playerHandTransform);
            n++;
        }
        isEffect = false;
    }

    void Spade3()
    {
        List<int> deck = playerDeck;
        if(deck.Count)
        {
            EffectCanvas.SetActive(true);
            while(deck.Count > 0)
            {
                int cardID = deck[0];
                CreateCard(cardID,EffectFieldTransform,PhotonNetwork.LocalPlayer.ActorNumber);
                deck.RemoveAt(0);
            }
            StartCoroutine(CardSelect(()=>
            {
                CardSearch();
                returnDeck(playerDeck);
                clickedCard = null;
                EffectCanvas.SetActive(false);
                isEffect = false;
            }));
        }
        else
        {
            isEffect = false;
        }
    }

    void Spade4()
    {
        List<int> deck = playerDeck;
        GiveCardToHand(playerDeck,playerHandTransform);
        GiveCardToHand(playerDeck,playerHandTransform);
        StartCoroutine(CardSelect(()=>
        {
            int HandPosition = clickedCard.transform.GetSiblingIndex();
            HandDestraction(HandPosition);
            clickedCard = null;
            StartCoroutine(CardSelect(()=>
            {
                HandPosition = clickedCard.transform.GetSiblingIndex();
                HandDestraction(HandPosition);
                isEffect = false;
            }));
        }));
    }

    void Spade5()
    {
        List<int> deck = playerDeck;
        GiveCardToHand(playerDeck,playerHandTransform);
        StartCoroutine(CardSelect(()=>
        {
            int HandPosition = clickedCard.transform.GetSiblingIndex();
            HandDestraction(HandPosition);
            isEffect = false;
        }));
    }

    void Spade6()
    {
        List<int> deck = playerDeck;
        if(deck.Count >= 2)
        {
            EffectCanvas.SetActive(true);
            int n = 0;
            while(n < 2)
            {
                int cardID = deck[0];
                CreateCard(cardID,EffectFieldTransform,PhotonNetwork.LocalPlayer.ActorNumber);
                deck.RemoveAt(0);
                n++;
            }
            StartCoroutine(CardSelect(()=>
            {
                deck.Insert(0,clickedCard.model.id);
                int Position = clickedCard.transform.GetSiblingIndex();
                CardController[] CardList = EffectFieldTransform.GetComponentsInChildren<CardController>();
                clickedCard = null;
                if(Position == 0)
                {
                    deck.Add(CardList[1].model.id);
                }
                else
                {
                    deck.Add(CardList[0].model.id);
                }
                EffectFieldCardLost();
                EffectCanvas.SetActive(false);
                isEffect = false;
            }));
        }
        else
        {
            isEffect = false;
        }
    }

    void Spade8()
    {
        List<int> deck = playerDeck;
        if(deck.Count >= 2)
        {
            EffectCanvas.SetActive(true);
            int n = 0;
            while(n < 2)
            {
                int cardID = deck[0];
                CreateCard(cardID,EffectFieldTransform,PhotonNetwork.LocalPlayer.ActorNumber);
                deck.RemoveAt(0);
                n++;
            }
            StartCoroutine(CardSelect(()=>
            {
                deck.Insert(0,clickedCard.model.id);
                int Position = clickedCard.transform.GetSiblingIndex();
                CardController[] CardList = EffectFieldTransform.GetComponentsInChildren<CardController>();
                clickedCard = null;
                if(Position == 0)
                {
                    deck.Add(CardList[1].model.id);
                }
                else
                {
                    deck.Add(CardList[0].model.id);
                }
                EffectFieldCardLost();
                EffectCanvas.SetActive(false);
                isEffect = false;
            }));
        }
        else
        {
            isEffect = false;
        }
    }

    void Spade9()
    {
        List<int> deck = playerDeck;
        GiveCardToHand(playerDeck,playerHandTransform);
        StartCoroutine(CardSelect(()=>
        {
            int HandPosition = clickedCard.transform.GetSiblingIndex();
            HandDestraction(HandPosition);
            isEffect = false;
        }));
    }

    void Spade10()
    {
        GiveCardToHand(playerDeck,playerHandTransform);
        isEffect = false;
    }

    void Spade11()
    {
        GiveCardToHand(playerDeck,playerHandTransform);
        GiveCardToHand(playerDeck,playerHandTransform);
        isEffect = false;
    }

    void Spade12()
    {
        GiveCardToHand(playerDeck,playerHandTransform);
        GiveCardToHand(playerDeck,playerHandTransform);
        GiveCardToHand(playerDeck,playerHandTransform);
        isEffect = false;
    }

    void Spade13()
    {
        EffectCanvas.SetActive(true);
        List<int> deck = playerDeck;
        int cardID = 0;
        while(deck.Count > 0)
        {
            cardID = deck[0];
            CreateCard(cardID,EffectFieldTransform,PhotonNetwork.LocalPlayer.ActorNumber);
            deck.RemoveAt(0);
        }
        StartCoroutine(CardSelect(()=>
        {
            CardSearch();
            clickedCard = null;
            StartCoroutine(CardSelect(()=>
            {
                CardSearch();
                returnDeck(playerDeck);
                clickedCard = null;
                EffectCanvas.SetActive(false);
                isEffect = false;
            }));
        }));
    }

    void Heart1()
    {
        CountExtraTurn = 1;
        photonView.RPC(nameof(EnemyExtraTurn),RpcTarget.Others);
        isEffect = false;
    }

    void Heart2()
    {
        CardController[] EnemyCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        int n = EnemyCardList.Length;
        int i = n;
        while(n > 0)
        {
            n--;
            photonView.RPC(nameof(HandDestraction),RpcTarget.Others,n);
        }
        while(i > 1)
        {
            i--;
            photonView.RPC(nameof(GiveCardToHandNetwork),RpcTarget.Others);
        }
        isEffect = false;
    }

    void Heart3()
    {
        EffectCanvas.SetActive(true);
        CardController[] EnemyCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        int n = EnemyCardList.Length;
        int i = 0;
        while(n > i)
        {
            CreateCard(EnemyCardList[i].model.id,EffectFieldTransform,99);
            i++;
        }
        StartCoroutine(CardSelect(()=>
            {
                PeepingHandDestraction();
                EffectFieldCardLost();
                EffectCanvas.SetActive(false);
                isEffect = false;
            }));
    }

    void Heart4()
    {
        RandomHandDestraction();
        isEffect = false;
    }

    void Heart5()
    {
        RandomHandDestraction();
        photonView.RPC(nameof(GiveCardToHandNetwork),RpcTarget.Others);
        isEffect = false;
    }

    void Heart6()
    {
        if(enemyDeck.Count >= 2)
        {
            EffectCanvas.SetActive(true);
            photonView.RPC(nameof(DecktopSend),RpcTarget.Others);
            photonView.RPC(nameof(DecktopSend),RpcTarget.Others);
            StartCoroutine(CardSelect(()=>
            {
                int cardID = clickedCard.model.id;
                photonView.RPC(nameof(SendDecktop),RpcTarget.Others,cardID);
                int Position = clickedCard.transform.GetSiblingIndex();
                CardController[] CardList = EffectFieldTransform.GetComponentsInChildren<CardController>();
                clickedCard = null;
                if(Position == 0)
                {
                    photonView.RPC(nameof(SendDeckbuttom),RpcTarget.Others,CardList[1].model.id);
                }
                else
                {
                    photonView.RPC(nameof(SendDeckbuttom),RpcTarget.Others,CardList[0].model.id);
                }
                EffectFieldCardLost();
                EffectCanvas.SetActive(false);
                isEffect = false;
            }));
        }
        else
        {
            isEffect = false;
        }
    }

    void Heart8()
    {
        EffectCanvas.SetActive(true);
        photonView.RPC(nameof(DecktopSend),RpcTarget.Others);
        photonView.RPC(nameof(DecktopSend),RpcTarget.Others);
        StartCoroutine(CardSelect(()=>
            {
                int cardID = clickedCard.model.id;
                photonView.RPC(nameof(SendDecktop),RpcTarget.Others,cardID);
                int Position = clickedCard.transform.GetSiblingIndex();
                CardController[] CardList = EffectFieldTransform.GetComponentsInChildren<CardController>();
                clickedCard = null;
                if(Position == 0)
                {
                photonView.RPC(nameof(SendDeckbuttom),RpcTarget.Others,CardList[1].model.id);
                }
                else
                {
                photonView.RPC(nameof(SendDeckbuttom),RpcTarget.Others,CardList[0].model.id);
                }
                EffectFieldCardLost();
                EffectCanvas.SetActive(false);
                isEffect = false;
            }));
    }

    void Heart9()
    {
        RandomHandDestraction();
        photonView.RPC(nameof(GiveCardToHandNetwork),RpcTarget.Others);
        isEffect = false;
    }

    void Heart10()
    {
        EffectCanvas.SetActive(true);
        CardController[] EnemyCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        int n = EnemyCardList.Length;
        int i = 0;
        while(n > i)
        {
            CreateCard(EnemyCardList[i].model.id,EffectFieldTransform,99);
            i++;
        }
        StartCoroutine(CardSelect(()=>
            {
                PeepingHandDestraction();
                EffectFieldCardLost();
                photonView.RPC(nameof(GiveCardToHandNetwork),RpcTarget.Others);
                EffectCanvas.SetActive(false);
                isEffect = false;
            }));
    }

    void Heart11()
    {
        RandomHandDestraction();
        RandomHandDestraction();
        isEffect = false;
    }

    void Heart12()
    {
        RandomHandDestraction();
        RandomHandDestraction();
        RandomHandDestraction();
        isEffect = false;
    }

    void Heart13()
    {
        EffectCanvas.SetActive(true);
        CardController[] EnemyCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        int n = EnemyCardList.Length;
        int i = 0;
        while(n > i)
        {
            CreateCard(EnemyCardList[i].model.id,EffectFieldTransform,99);
            i++;
        }
        StartCoroutine(CardSelect(()=>
        {
            PeepingHandDestraction();
            StartCoroutine(CardSelect(()=>
            {
                PeepingHandDestraction();
                EffectFieldCardLost();
                EffectCanvas.SetActive(false);
                isEffect = false;
            }));
        }));
    }




    [PunRPC]
    void EnemyExtraTurn()
    {
        CountEnemyExtraTurn = 1;
    }

    IEnumerator CardSelect(System.Action action)
    {
        yield return StartCoroutine(SelectWait() );
        action();
    }

    IEnumerator SelectWait()
    {
        while ( clickedCard == null )
        {
            yield return new WaitForEndOfFrame ();
        }
        yield return null;
    }

    void CardSearch()
    {
        clickedCard.transform.SetParent(playerHandTransform,false);
        photonView.RPC(nameof(GiveCardEnemyHand),RpcTarget.Others,PhotonNetwork.LocalPlayer.ActorNumber,clickedCard.model.id);
    }

    void returnDeck(List<int> deck)
    {
        CardController[] CardList = EffectFieldTransform.GetComponentsInChildren<CardController>();
        int n = CardList.Length;
        while(n > 0)
        {
            n--;
            deck.Add(CardList[n].model.id);
            Destroy(CardList[n].gameObject);
        }
        Shuffle(playerDeck);
    }

    [PunRPC]
    void HandDestraction(int i)
    {
            List<int> deck = playerDeck;
            CardController card = playerHandTransform.GetComponentsInChildren<CardController>()[i];
            deck.Add(card.model.id);
            clickedCard = null;
            Destroy(card.gameObject);
            photonView.RPC(nameof(EnemyHandDestraction),RpcTarget.Others,i);
            Shuffle(deck);
    }

    [PunRPC]
    void EnemyHandDestraction(int i)
    {
        CardController card = enemyHandTransform.GetComponentsInChildren<CardController>()[i];
        Destroy(card.gameObject);
        enemyDeck.Add(0);
    }

    [PunRPC]
    void GiveCardToHandNetwork()
    {
        GiveCardToHand(playerDeck,playerHandTransform);
    }

    void EffectFieldCardLost()
    {
        CardController[] CardList = EffectFieldTransform.GetComponentsInChildren<CardController>();
        int n = CardList.Length;
        while(n > 0)
        {
            n--;
            Destroy(CardList[n].gameObject);
        }
    }

    void RandomHandDestraction()
    {
        CardController[] EnemyCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        int n = EnemyCardList.Length;
        int Random_num = UnityEngine.Random.Range(0,n);
        photonView.RPC(nameof(HandDestraction),RpcTarget.Others,Random_num);
    }

    void PeepingHandDestraction()
    {
        int HandPosition = clickedCard.transform.GetSiblingIndex();
        clickedCard = null;
        CardController card = EffectFieldTransform.GetComponentsInChildren<CardController>()[HandPosition];
        Destroy(card.gameObject);
        photonView.RPC(nameof(HandDestraction),RpcTarget.Others,HandPosition);
    }

    [PunRPC]
    void DecktopSend()
    {
        int cardID = playerDeck[0];
        int ownerID = PhotonNetwork.LocalPlayer.ActorNumber;
        playerDeck.RemoveAt(0);
        photonView.RPC(nameof(CreateEnemyDecktop),RpcTarget.Others,ownerID,cardID);
    }

    [PunRPC]
    void CreateEnemyDecktop(int ownerID, int cardID)
    {
        CreateCard(cardID,EffectFieldTransform,ownerID);
    }

    [PunRPC]
    void SendDecktop(int cardID)
    {
        playerDeck.Insert(0,cardID);
    }

    [PunRPC]
    void SendDeckbuttom(int cardID)
    {
        playerDeck.Add(cardID);
    }
}
