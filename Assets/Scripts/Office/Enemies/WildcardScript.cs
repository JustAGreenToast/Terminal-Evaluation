using UnityEngine;

public class WildcardScript : EnemyScript
{
    class Card
    {
        protected int color;
        protected int symbol;
        GameObject cardObj;
        Sprite[] colorEmblems;
        Sprite[][] cardSymbols;
        Sprite wildcardIcon;
        SpriteRenderer colorEmblem;
        SpriteRenderer cardSymbol;
        public Card(GameObject _cardObj)
        {
            colorEmblems = Resources.LoadAll<Sprite>("Sprites/Card Parts/ColorEmblems");
            cardSymbols = new Sprite[4][]
            {
                Resources.LoadAll<Sprite>("Sprites/Card Parts/InvadersIcons"),
                Resources.LoadAll<Sprite>("Sprites/Card Parts/PacManIcons"),
                Resources.LoadAll<Sprite>("Sprites/Card Parts/TetrisIcons"),
                new Sprite[] { Resources.Load<Sprite>("Sprites/Card Parts/HalIcon") }
            };
            wildcardIcon = Resources.Load<Sprite>("Sprites/Characters/Wildcard");
            cardObj = _cardObj;
            colorEmblem = cardObj.transform.GetChild(0).GetComponent<SpriteRenderer>();
            cardSymbol = cardObj.transform.GetChild(1).GetComponent<SpriteRenderer>();
        }
        public void Randomize()
        {
            color = Random.Range(0, 4);
            symbol = Random.Range(0, 3);
        }
        public void SetAsWildcard()
        {
            color = 4;
            symbol = 3;
        }
        public bool IsMatch(Card card)
        {
            if (color == 4 || card.color == 4) { return true; }
            return color == card.color || symbol == card.symbol;
        }
        public void Display()
        {
            colorEmblem.sprite = colorEmblems[color];
            Sprite[] symbolSet = cardSymbols[symbol];
            cardSymbol.sprite = symbolSet[Random.Range(0, symbolSet.Length)];
            cardSymbol.transform.localScale = Vector3.one * (symbol == 2 ? 1.25f : 0.75f);
            cardObj.SetActive(true);
        }
        public void DisplayFail()
        {
            colorEmblem.enabled = false;
            cardSymbol.sprite = wildcardIcon;
            cardSymbol.transform.localScale = Vector3.one * 0.75f;
        }
        public void SetActive(bool _enabled) { cardObj.SetActive(_enabled); }
    }
    Card enemyCard;
    Card[] playerCards = new Card[3];
    enum States { Hidden, Waiting, Attack };
    States currentState;
    float stateTimer;
    int missCounter;
    float moveCooldown { get { return Random.value < 0.25f ? Random.Range(8f, 12f) : Random.Range(16f, 24f); } }
    const float patienceTime = 10;
    AudioClip deckShuffleClip;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Tournament_Wildcard; }
    protected override void OnStart()
    {
        enemyCard = new Card(transform.GetChild(0).gameObject);
        for (int i = 0; i < 3; i++) { playerCards[i] = new Card(transform.GetChild(i + 1).gameObject); }
        deckShuffleClip = Resources.Load<AudioClip>("SFX/card_shuffle");
        currentState = States.Hidden;
        stateTimer = moveCooldown;
        //IncreaseAI(10);
    }
    protected override void OnUpdate()
    {
        switch (currentState)
        {
            case States.Hidden:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (Random.Range(0, 10) < aiLevel + missCounter && manager.IsLocationAvailable(Locations.Behind))
                    {
                        currentLocation = Locations.Behind;
                        missCounter = 0;
                        PlayCard();
                    }
                    else
                    {
                        stateTimer = moveCooldown;
                        if (aiLevel > 0) { missCounter++; }
                    }
                }
                break;
            case States.Waiting:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0) { Attack(); }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { manager.ExamFailed("Wildcard will hand you 3 cards and play an extra card of her own: pick a card that either matches the extra card's family (Invaders, Tetris, Pac-Man) or color. If you pick a wrong card or take too long to pick, you lose."); }
                break;
        }
    }
    void PlayCard()
    {
        enemyCard.Randomize();
        foreach (Card card in playerCards) { card.Randomize(); }
        if (!HasAnyMatch()) { playerCards[Random.Range(0, playerCards.Length)].SetAsWildcard(); }
        manager.TriggerRoomOverlay();
        manager.TriggerRoomOverlay();
        manager.PlaySound(deckShuffleClip, "Deck Shuffle", true, true);
        enemyCard.Display();
        foreach (Card card in playerCards) { card.Display(); }
        currentState = States.Waiting;
        stateTimer = patienceTime;
    }
    bool HasAnyMatch()
    {
        foreach (Card card in playerCards) { if (card.IsMatch(enemyCard)) { return true; } }
        return false;
    }
    public void CardSelected(int _cardIndex)
    {
        if (playerCards[_cardIndex].IsMatch(enemyCard)) { Retreat(); }
        else
        {
            foreach (Card card in playerCards) { if (!card.IsMatch(enemyCard)) { card.DisplayFail(); } }
            Attack();
        }
    }
    void Retreat()
    {
        manager.TriggerRoomOverlay();
        enemyCard.SetActive(false);
        foreach (Card card in playerCards) { card.SetActive(false); }
        currentState = States.Hidden;
        stateTimer = patienceTime;
        currentLocation = Locations.None;
    }
    void Attack()
    {
        manager.TriggerRoomOverlay();
        manager.LockEnemies(this);
        manager.LockPlayer();
        manager.LockCamera(180);
        manager.FadeOutMusic();
        foreach (Card card in playerCards) { if (!card.IsMatch(enemyCard)) { card.DisplayFail(); } }
        currentState = States.Attack;
        stateTimer = 1.5f;
    }
}