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
                isMulliganTurn;
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
        isPlayerTurn = !isPlayerTurn;
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
    }

     void Spade1()
    {
        photonView.RPC(nameof(ExtraTurn), RpcTarget.All);
    }

    void Spade2()
    {
        CardController[] playerCardList = playerHandTransform.GetComponentsInChildren<CardController>();
        int n = playerCardList.Length;
        while(n < 7)
        {
            GiveCardToHand(playerDeck,playerHandTransform);
            n = playerCardList.Length;
        }
    }

    void Spade3()
    {
        EffectCanvas.SetActive(true);
        List<int> deck = playerDeck;
        while(deck.Count > 0)
        {
            int cardID = deck[0];
            CreateCard(cardID,EffectFieldTransform,PhotonNetwork.LocalPlayer.ActorNumber);
            deck.RemoveAt(0);
        }
        StartCoroutine(CardSelect(()=>
        {
            CardSearch();
            EffectCanvas.SetActive(false);
        }));
    }


        [PunRPC]
    void ExtraTurn()
    {
        isPlayerTurn = !isPlayerTurn;
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
        returnDeck(playerDeck);
        Shuffle(playerDeck);
    }

    void returnDeck(List<int> deck)
    {
        CardController[] CardList = EffectFieldTransform.GetComponentsInChildren<CardController>();
        while(CardList.Length > 0)
        {
            deck.Add(CardList[0].model.id);
            Destroy(CardList[0].gameObject);
        }
    }
}
