using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Scrollbar;

public class GameManager : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Board boardManager;
    [SerializeField] private GameObject floatingText;
    [SerializeField] private GameObject Dice;
    [SerializeField] private GameObject DiceRestPostion;
    [SerializeField] private List<GameObject> seedList;

    //ui elements
    [SerializeField] private GameObject InteruptPanel;
    [SerializeField] private GameObject GameplayPanel;
    [SerializeField] private GameObject StatusPanel;
    [SerializeField] private GameObject PauseMenu;

    [SerializeField] private Toggle forwardToggle;
    [SerializeField] private Toggle backwardToggle;
    //end of ui elements

    //player names objects
    [SerializeField] private TMPro.TextMeshPro blueTextMesh;
    [SerializeField] private TMPro.TextMeshPro greenTextMesh;
    [SerializeField] private TMPro.TextMeshPro redTextMesh;
    [SerializeField] private TMPro.TextMeshPro yellowTextMesh;
    //end of player names

    //cheat
    [SerializeField] private int cheatDiceNumber = 1;
    public bool allowCheat = false;
    public bool cumulateDiceRoll = false;
    public List<int> accumulatedDices;
    public bool allowToMoveAfterAccumulate = false;
    //end of cheat


    private GameObject activeSeed;
    Vector3 activeSeedPreviousPosition;
    public static GameManager gmInstance;
    public UiManager uiManager;

    private List<String> players = new();

    public List<LuduPlayer> basePlayers = new();
    private LuduPlayer currentPlayer;

    private string activePlayer;
    public int moveNumber = -1;

    private bool isOnMove = false;
    private bool isCounting = false;
    //used to set the direction of movement
    public bool goClockWise = true;

    //Game Management

    //event to trigger when active user changes
    public delegate void activeUserEvt(LuduPlayer player);
    public event activeUserEvt onActiveUser;

    //event to trigger to initiate rolling
    public delegate void tossEvt(bool tossState);
    public event tossEvt processOnGoing;

    public delegate void ActiveSeedChangedEvt(GameObject seeed);
    public event ActiveSeedChangedEvt ActiveSeedChanged;

    public delegate void ChangeAction(int num);
    public ChangeAction changePlayerEvent;

    //accumulated list events
    public delegate void AccumulatedEvent(AccumulatedListMessage accumulatedListMessage);
    public AccumulatedEvent acculatedDiceEvent;

    //directional mvemnt event
    [HideInInspector] public UnityEvent directionChangeEvent;

    private LayerMask seedLayerMask;
    private LayerMask cellLayerMask;
    private MeinPhysics meinPhysics;
    private PlayerManager playerManager;
    private HelperUtils helperUtils;

    //very important
    private Transform _cellTransform;
    private bool specialOperation = false;
    //private int _specialOperation_init_number = 0;
    private GameObject _specialOperationInitiator;
    private Vector3 _specialOperation_init_position;
    //used to check if side kick option is up
    private bool IsAboutSideKick = false;
    //end of very important

    //hint
    public bool doHint = false;
    public GameObject hintEffect;
    public GameObject kickoutEffect;

    private void Awake()
    {
        Time.timeScale = 1.5f;
        Physics.gravity = new Vector3(0, -55f, 0);
        if (gmInstance == null)
        {
            gmInstance = this;
        }
        boardManager.rollEvent += DoAccumulation;
        meinPhysics = new MeinPhysics();
        helperUtils = new HelperUtils();
        accumulatedDices = new();
    }
    private void Start()
    {
        seedLayerMask = LayerMask.GetMask("seed");
        cellLayerMask = LayerMask.GetMask("cell");
        //setup background manager
    }
    private void Update()
    {
        if (goClockWise)
        {
            forwardToggle.isOn = true;
        }
        else
        {
            backwardToggle.isOn = true;
        }
    }
    private void OnDestroy()
    {
        boardManager.rollEvent -= DoAccumulation;
    }

    public void SetHint(bool hinter)
    {
        doHint = hinter;
    }

    public void SeedPlayers(List<LuduPlayer> players)
    {
        playerManager = new PlayerManager(players);
        //start assigning player names
        foreach (LuduPlayer luduPlayer in players)
        {
            switch (luduPlayer.Color)
            {
                case "blue":
                    blueTextMesh.text = luduPlayer.Name;
                    break;
                case "yellow":
                    yellowTextMesh.text = luduPlayer.Name;
                    break;
                case "red":
                    redTextMesh.text = luduPlayer.Name;
                    break;
                case "green":
                    greenTextMesh.text = luduPlayer.Name;
                    break;
            }
        }
        //end of assigning player names

        StartCoroutine(NewToss());
        StartCoroutine(audioManager.PlayAudio(0, 1));
    }

    IEnumerator NewToss()
    {
        yield return new WaitForSeconds(2);
        float tossTimeOut = 0.5f;
        int selected;

        selected = UnityEngine.Random.Range(0, playerManager.luduPlayers.Count);
        currentPlayer = playerManager.GetPlayer(selected);
        currentPlayer.Active = true;
        onActiveUser?.Invoke(currentPlayer);
        yield return new WaitForSeconds(tossTimeOut);

        activePlayer = currentPlayer.Color;
        processOnGoing?.Invoke(false);
    }

    public void DoAccumulation(int number)
    {
        if (allowCheat)
        {
            number = cheatDiceNumber;
        }
        bool shouldTurnOver = ShouldTurnOver(number);

        if (shouldTurnOver && SeedsOnFinishingLane() > 0 && (moveNumber != 6 && AllSeedsHome().Count == 0)) { 
            ChangePosition(DiceRestPostion, Dice);
            processOnGoing?.Invoke(true);
            ChangeActivePlayer();
            return;
        }
        // if the accumulated list is not empty
        if (accumulatedDices.Count > 0)
        {
            // check for the last item in the accumulated list
            // if it's 6 and the incoming number is also 6
            // we continue to append to the accumulated list
            // else add the new number to the list and stop dice roll for the current player
            if (accumulatedDices[^1] == 6 && number == 6)
            {
                AppendToAccumulatedList(number);
                processOnGoing?.Invoke(false);
                //allowToMoveAfterAccumulate = false;
                ChangePosition(DiceRestPostion, Dice);
            }
            else
            {
                AppendToAccumulatedList(number);
                processOnGoing?.Invoke(true);
                //allowToMoveAfterAccumulate = true;
                ChangePosition(DiceRestPostion, Dice);
            }
        }
        else
        {
            if (number == 6)
            {
                AppendToAccumulatedList(number);
                processOnGoing?.Invoke(false);
                //allowToMoveAfterAccumulate = false;
            }
            else
            {
                //here represents one-off roll
                AppendToAccumulatedList(number);
                processOnGoing?.Invoke(true);
                //allowToMoveAfterAccumulate = true;
                ChangePosition(DiceRestPostion, Dice);
                // if is hint is on, don't commit the rolled number to move
                if (!doHint)
                {
                    SetToMove(number, true);
                }
            }
        }
    }

    //called when the rolled number card is pressed or clicked
    public void RolledNumberRequest(RolledNumberScript rolledNumberScript)
    {
        int number = rolledNumberScript.Number;
        SetToMove(number);
        RemoveFromAccumulatedList(number);
    }
    
    /// <summary>
    /// To be used by seed to play when none is highlighted but seed is eligible to move and player eligible to play
    /// </summary>
    /// <param name="number"></param>
    public void SeedToMoveByFirstAccumulatedDice(GameObject seed)
    {
        if(accumulatedDices.Count > 0)
        {
            int number;
            if (currentPlayer.playerSettings.orderOfDiceDispensary == PlayerSettings.OrderOfDiceDispensary.LR)
            {
                number = accumulatedDices[0];
            }
            else
            {
                number = accumulatedDices[accumulatedDices.Count - 1];
            }
            this.moveNumber = number;
            StartCoroutine(SetAndMoveActiveSeed(seed));
            RemoveFromAccumulatedList(number);
        }
    }

    //event subscribe to board.rollEvent: invoked when player roll dice
    public void SetToMove(int moveNumber)
    {
        bool shouldTurnOver = ShouldTurnOver(moveNumber);
        List<GameObject> allSeedsPlayable = AllSeedsPlayable();

        if (shouldTurnOver)
        {
            List<GameObject> allSeedsHome = AllSeedsHome();
            int numberOfSeedsOnFinishingLane = SeedsOnFinishingLane();
            if (allSeedsHome.Count != 0 && moveNumber != 6)
            {
                //ChangeActivePlayer();
                processOnGoing?.Invoke(false);
                return;
            }
            if (allSeedsHome.Count == 0 && numberOfSeedsOnFinishingLane > 0)
            {
                //ChangeActivePlayer();
                processOnGoing?.Invoke(false);
                return;
            }
            ChangePosition(DiceRestPostion, Dice);
        }
        else
        {
            //for cases where last number is 6
            if (moveNumber == 6 && accumulatedDices.Count == 0)
            {
                ChangeActivePlayer();
                processOnGoing?.Invoke(false);
            }
        }


        if (!isOnMove)
        {
            isOnMove = true;
            activeSeed = null;
            isCounting = false;
            this.moveNumber = moveNumber;
            //choose a seed before move is initiated
            HighLightActivePlayerSeedsBaseDieNumber(activePlayer, true, moveNumber);
        }
        ////reset dice position
        ChangePosition(DiceRestPostion, Dice);
        if (accumulatedDices.Count <= 1)
        {
            RemoveFromAccumulatedList(moveNumber);
        }
    }
    public void SetToMove(int moveNumber, bool clean)
    {
        bool shouldTurnOver = ShouldTurnOver(moveNumber);
        List<GameObject> allSeedsPlayable = AllSeedsPlayable();

        if (shouldTurnOver)
        {
            List<GameObject> allSeedsHome = AllSeedsHome();
            int numberOfSeedsOnFinishingLane = SeedsOnFinishingLane();



            if (allSeedsHome.Count != 0 && moveNumber != 6)
            {
                //ChangeActivePlayer();
                processOnGoing?.Invoke(false);
                ChangePosition(DiceRestPostion, Dice);
                if (clean)
                {
                    StartCoroutine(IRemoveFromListAndChangePlayer(moveNumber));
                }
                return;
            }
            else if (allSeedsHome.Count == 0 && numberOfSeedsOnFinishingLane > 0)
            {
                ChangeActivePlayer();
                StartCoroutine(IRemoveFromListAndChangePlayer(moveNumber));
                processOnGoing?.Invoke(false);
                ChangePosition(DiceRestPostion, Dice);
                return;
            }
        }


        if (!isOnMove)
        {
            isOnMove = true;
            activeSeed = null;
            isCounting = false;
            this.moveNumber = moveNumber;
            //choose a seed before move is initiated
            HighLightActivePlayerSeedsBaseDieNumber(activePlayer, true, moveNumber);
        }
        ////reset dice position
        ChangePosition(DiceRestPostion, Dice);
        if (accumulatedDices.Count <= 1)
        {
            RemoveFromAccumulatedList(moveNumber);
        }
    }

    public IEnumerator IRemoveFromListAndChangePlayer(int moveNumber)
    {
        yield return new WaitForSeconds(1);
        RemoveFromAccumulatedList(moveNumber);
        ChangeActivePlayer();
    }

    //when seed is clicked 
    // it registers the current seed
    // active and moving
    public void ClearAccumulatedList()
    {
        accumulatedDices = new();
        acculatedDiceEvent.Invoke(new AccumulatedListMessage(true));
    }

    public void AppendToAccumulatedList(int number)
    {
        accumulatedDices.Add(number);
        acculatedDiceEvent.Invoke(new AccumulatedListMessage(number, true));
    }

    public void RemoveFromAccumulatedList(int number)
    {
        accumulatedDices.Remove(number);
        acculatedDiceEvent.Invoke(new AccumulatedListMessage(number, false));
    }

    public IEnumerator SetAndMoveActiveSeed(GameObject selectedSeed)
    {
        print("movenumber -> "+ this.moveNumber);
        if (moveNumber == -1)
        {
            yield return null;
        };
        activeSeedPreviousPosition = selectedSeed.transform.position;
        string playerSeed = selectedSeed.name;
        //if active player selects its own seed, then move it
        //we compare the name with seed name on their first index
        if (activePlayer != null && playerSeed.StartsWith(activePlayer[0]) && isOnMove)
        {
            activeSeed = selectedSeed;
            ActiveSeedChanged?.Invoke(activeSeed);
            RaycastHit hit;
            if (Physics.Raycast(activeSeed.transform.position, Vector3.down * 50, out hit))
            {
                //print(hit.collider.gameObject.name);
                string seedInteract = hit.collider.gameObject.tag.ToLower();
                //if player is not on board, but at home? then move player to board at position 
                //{activePlayer[0]}40
                if (seedInteract.Contains("home") && moveNumber == 6)
                {
                    //print("preparing home normal procedure ");
                    GameObject startPoint = GameObject.Find($"{activePlayer[0]}40");
                    PlaceSeedToPosition(startPoint);
                    //set move-number to disable it from moving 
                    this.moveNumber = -1;

                    //print("preparing home normal procedure 1");
                    //play sound
                    StartCoroutine(audioManager.PlayAudio(3, 0));
                    //print("preparing home normal procedure 2");
                    //check if you can remove an opponent as soon as start at start-point;

                    //pause before raycasting, good to wait a while
                    yield return new WaitForSeconds(1);
                    RaycastHit cellHit;
                    if (Physics.Raycast(activeSeed.transform.position, Vector3.down, out cellHit, Mathf.Infinity, cellLayerMask))
                    {
                        HomeNormalProcedure(cellHit.transform);
                    }
                    else
                    {
                        print("hit  " + cellHit.IsUnityNull());
                    }
                    //clear all highlighting
                    HighLightActivePlayerSeeds(playerSeed, false);
                }
                else
                {
                    //if seed is not home, then move seeed
                    if (!seedInteract.Contains("home"))
                    {
                        if (goClockWise)
                        {
                            // moving
                            StartCoroutine(MoveForwardRoutine());
                        }
                        else
                        {
                            if (!specialOperation)
                            {
                                specialOperation = true;
                                _specialOperation_init_position = activeSeed.transform.position;
                                _specialOperationInitiator = activeSeed;
                            }
                            else if (specialOperation && (activeSeed.name != _specialOperationInitiator.name)) yield break;
                            StartCoroutine(MoveBackRoutine());
                        }
                    }
                    //yet then remove highlights from seeds
                    HighLightActivePlayerSeeds(playerSeed, false);

                }
            }
        }
        else
        {
            print("player " + playerSeed + " activePlayer " + activePlayer);
        }
    }
    private IEnumerator MoveForwardRoutine()
    {
        int moveCounter = 0;
        string cellCoordinateXY = "";
        bool breakRoutine = false;
        if (!isCounting)
        {
            isCounting = true;
            for (int i = 1; i <= this.moveNumber; i++)
            {
                int x = 0; int z = 0;
                Ray ray = new(activeSeed.transform.position, Vector3.down);
                float maxDistance = Mathf.Infinity;
                Vector3 oldPosition = activeSeed.transform.position;
                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, cellLayerMask))
                {
                    string cellName = hit.collider.gameObject.name;
                    //get the seed coordinates(x,y~z)
                    cellCoordinateXY = cellName[1..];
                    //activeSeed.GetComponent<Rigidbody>().useGravity = true;
                    oldPosition.y = 0.9f;

                    int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
                    int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());

                    switch (cellName[0])
                    {
                        case 'b':
                            //check if seed is on the last column then opt to move downwards
                            if (cordY == 2)
                            {
                                //moving downwards
                                z = -1;
                                //however if the seed is at position starting with 5
                                //then stop moving down and move left
                                if (cordX == 5)
                                {
                                    z = 0;
                                    x = -1;
                                }
                            }
                            else if (cordY == 0 || cordY == 1)
                            {
                                z = 1;
                                if (cordX == 5)
                                {
                                    z = 0;
                                    x = -1;
                                    //if seed is on 50 then we move up
                                    if (cordY == 0)
                                    {
                                        z = 1;
                                        x = 0;
                                    }
                                    //ends with 51
                                    else if (cordY == 1)
                                    {
                                        if (activeSeed.name[0] == 'b')
                                        {
                                            z = 1;
                                            x = 0;
                                            print("in b lane");
                                            {
                                                //start checking if seed is eligible to complete 
                                                int midValue = cordX + 1;
                                                //if moving starts for the first time;
                                                // and positionValue | straightValue
                                                // and this.movenNumber cancels each other out to 0?
                                                if ((midValue - this.moveNumber == 0 && moveCounter == 0) || (midValue > this.moveNumber))
                                                {
                                                    print("can move to home ~ because it is eligible to move difference is none -> " + (midValue - this.moveNumber));
                                                    if ((midValue - this.moveNumber == 0 && moveCounter == 0))
                                                    {
                                                        GameObject seedOutHome = GameObject.Find($"{activeSeed.name}OutHome");
                                                        activeSeed.GetComponent<Rigidbody>().useGravity = false;
                                                        //move seed out of board
                                                        ChangePosition(seedOutHome, activeSeed);
                                                        yield return new WaitForSeconds(.5f);
                                                        if (AllComplete())
                                                        {
                                                            playerManager.RemovePlayer(this.currentPlayer);
                                                            StartCoroutine(audioManager.PlayAudio(5, 0));
                                                        }
                                                        else
                                                        {
                                                            StartCoroutine(audioManager.PlayAudio(4, 0));
                                                        }
                                                        //check if that number is the last if so change player
                                                        if (accumulatedDices.Count == 0)
                                                        {
                                                            ChangeActivePlayer();
                                                        }
                                                        PartialReset();
                                                        yield break;
                                                    }
                                                }
                                                else if (midValue - this.moveNumber != 0 && moveCounter == 0)
                                                {
                                                    print("cannot move to home ~ because difference is avaiable -> " + (midValue - this.moveNumber));
                                                    ChangeActivePlayer();
                                                    PartialReset();
                                                    yield break;

                                                }
                                                //end of checking if seed is eligible to complete 
                                            }
                                        }
                                    }
                                }
                                else if (cordX == 0 && cordY == 0)
                                {
                                    PlaceSeedToPosition("y02");
                                    yield return new WaitForSeconds(0.3f);
                                    continue;
                                }
                                else if (cordX >= 1 || cordX <= 4)
                                {
                                    if (activeSeed.name[0].Equals('b'))
                                    {
                                        z = 1;
                                        x = 0;
                                        if (cordY == 1)
                                        {
                                            //start checking if seed is eligible to complete 
                                            int midValue = cordX + 1;
                                            //if moving starts for the first time;
                                            // and positionValue | straightValue
                                            // and this.movenNumber cancels each other out to 0?
                                            if ((midValue - this.moveNumber == 0 && moveCounter == 0) || (midValue > this.moveNumber))
                                            {
                                                if ((midValue - this.moveNumber == 0 && moveCounter == 0))
                                                {
                                                    GameObject seedOutHome = GameObject.Find($"{activeSeed.name}OutHome");
                                                    activeSeed.GetComponent<Rigidbody>().useGravity = false;
                                                    ChangePosition(seedOutHome, activeSeed);
                                                    yield return new WaitForSeconds(.5f);
                                                    if (AllComplete())
                                                    {
                                                        //print(playerManager.luduPlayers.Count);
                                                        playerManager.RemovePlayer(this.currentPlayer);
                                                        //print("base players");
                                                        StartCoroutine(audioManager.PlayAudio(5, 0));
                                                    }
                                                    else
                                                    {
                                                        StartCoroutine(audioManager.PlayAudio(4, 0));
                                                    }
                                                    //check if that number is the last if so change player
                                                    if (accumulatedDices.Count == 0)
                                                    {
                                                        ChangeActivePlayer();
                                                    }
                                                    PartialReset();
                                                    yield break;
                                                }
                                            }
                                            else if (midValue - this.moveNumber != 0 && moveCounter == 0)
                                            {
                                                ChangeActivePlayer();
                                                PartialReset();
                                                yield break;

                                            }
                                            //end of checking if seed is eligible to complete 
                                        }
                                    }
                                }
                            }
                            break;
                        case 'y':
                            if (cordY == 2)
                            {
                                x = -1;
                                if (cellCoordinateXY[0] == '5')
                                {
                                    x = 0;
                                    z = 1;
                                }
                            }
                            else if (cordY == 0 || cordY == 1)
                            {
                                x = 1;
                                if (cordX == 5)
                                {
                                    x = 0;
                                    z = 1;
                                    //ends with 50
                                    if (cordY == 0)
                                    {
                                        z = 0;
                                        x = 1;
                                    }
                                    //ends with 51
                                    else if (cordY == 1)
                                    {
                                        if (activeSeed.name[0].Equals('y'))
                                        {
                                            z = 0;
                                            x = 1;
                                            if (cordY == 1)
                                            {
                                                //start checking if seed is eligible to complete 
                                                int midValue = cordX + 1;
                                                //if moving starts for the first time;
                                                // and positionValue | straightValue
                                                // and this.movenNumber cancels each other out to 0?
                                                if ((midValue - this.moveNumber == 0 && moveCounter == 0) || (midValue > this.moveNumber))
                                                {
                                                    if ((midValue - this.moveNumber == 0 && moveCounter == 0))
                                                    {
                                                        GameObject seedOutHome = GameObject.Find($"{activeSeed.name}OutHome");
                                                        activeSeed.GetComponent<Rigidbody>().useGravity = false;
                                                        ChangePosition(seedOutHome, activeSeed);
                                                        //play complete sound
                                                        yield return new WaitForSeconds(.5f);
                                                        if (AllComplete())
                                                        {
                                                            StartCoroutine(audioManager.PlayAudio(5, 0));
                                                            playerManager.RemovePlayer(this.currentPlayer);
                                                        }
                                                        else
                                                        {
                                                            StartCoroutine(audioManager.PlayAudio(4, 0));
                                                        }
                                                        //check if that number is the last if so change player
                                                        if (accumulatedDices.Count == 0)
                                                        {
                                                            ChangeActivePlayer();
                                                        }
                                                        PartialReset();
                                                        yield break;
                                                    }
                                                }
                                                else if (midValue - this.moveNumber != 0 && moveCounter == 0)
                                                {
                                                    ChangeActivePlayer();
                                                    PartialReset();
                                                    yield break;

                                                }
                                                //end of checking if seed is eligible to complete 
                                            }
                                        }
                                    }
                                }
                                else if (cordX == 0 && cordY == 0)
                                {

                                    PlaceSeedToPosition("g02");
                                    yield return new WaitForSeconds(0.3f);
                                    continue;
                                }
                                else if (cordX >= 1 || cordX <= 4)
                                {
                                    if (cordY == 1)
                                    {
                                        //start checking if seed is eligible to complete 
                                        int midValue = cordX + 1;
                                        //if moving starts for the first time;
                                        // and positionValue | straightValue
                                        // and this.movenNumber cancels each other out to 0?
                                        if ((midValue - this.moveNumber == 0 && moveCounter == 0) || (midValue > this.moveNumber))
                                        {
                                            if ((midValue - this.moveNumber == 0 && moveCounter == 0))
                                            {
                                                GameObject seedOutHome = GameObject.Find($"{activeSeed.name}OutHome");
                                                activeSeed.GetComponent<Rigidbody>().useGravity = false;
                                                ChangePosition(seedOutHome, activeSeed);
                                                //play complete sound
                                                yield return new WaitForSeconds(.5f);
                                                if (AllComplete())
                                                {
                                                    StartCoroutine(audioManager.PlayAudio(5, 0));
                                                    playerManager.RemovePlayer(this.currentPlayer);
                                                }
                                                else
                                                {
                                                    StartCoroutine(audioManager.PlayAudio(4, 0));
                                                }
                                                //check if that number is the last if so change player
                                                if (accumulatedDices.Count == 0)
                                                {
                                                    ChangeActivePlayer();
                                                }
                                                PartialReset();
                                                yield break;
                                            }
                                        }
                                        else if (midValue - this.moveNumber != 0 && moveCounter == 0)
                                        {
                                            ChangeActivePlayer();
                                            PartialReset();
                                            yield break;

                                        }
                                        //end of checking if seed is eligible to complete 
                                    }
                                }
                            }
                            break;
                        case 'g':
                            //check if seed is on the last column then opt to move downwards
                            if (cordY == 2)
                            {
                                //moving downwards
                                z = 1;
                                //however if the seed is at position starting with 5
                                //then stop moving down and move left
                                if (cordX == 5)
                                {
                                    z = 0;
                                    x = 1;
                                }
                            }
                            else if (cordY == 0 || cordY == 1)
                            {
                                z = -1;
                                if (cordX == 5)
                                {
                                    z = 0;
                                    x = 1;
                                    //if seed is on 50 then we move up
                                    if (cordY == 0)
                                    {
                                        z = -1;
                                        x = 0;
                                    }
                                    //ends with 51
                                    else if (cordY == 1)
                                    {
                                        if (activeSeed.name[0].Equals('g'))
                                        {
                                            z = -1;
                                            x = 0;

                                            {
                                                //start checking if seed is eligible to complete 
                                                int midValue = cordX + 1;
                                                //if moving starts for the first time;
                                                // and positionValue | straightValue
                                                // and this.movenNumber cancels each other out to 0?
                                                if ((midValue - this.moveNumber == 0 && moveCounter == 0) || (midValue > this.moveNumber))
                                                {
                                                    if ((midValue - this.moveNumber == 0 && moveCounter == 0))
                                                    {
                                                        GameObject seedOutHome = GameObject.Find($"{activeSeed.name}OutHome");
                                                        activeSeed.GetComponent<Rigidbody>().useGravity = false;
                                                        ChangePosition(seedOutHome, activeSeed);
                                                        //play complete sound
                                                        yield return new WaitForSeconds(.5f);
                                                        if (AllComplete())
                                                        {
                                                            StartCoroutine(audioManager.PlayAudio(5, 0));
                                                            playerManager.RemovePlayer(this.currentPlayer);
                                                        }
                                                        else
                                                        {
                                                            StartCoroutine(audioManager.PlayAudio(4, 0));
                                                        }
                                                        if (accumulatedDices.Count == 0)
                                                        {
                                                            ChangeActivePlayer();
                                                        }
                                                        PartialReset();
                                                        yield break;
                                                    }
                                                }
                                                else if (midValue - this.moveNumber != 0 && moveCounter == 0)
                                                {
                                                    ChangeActivePlayer();
                                                    PartialReset();
                                                    yield break;

                                                }
                                                //end of checking if seed is eligible to complete 
                                            }
                                        }
                                    }
                                }
                                else if (cordX == 0 && cordY == 0)
                                {

                                    PlaceSeedToPosition("r02");
                                    yield return new WaitForSeconds(0.3f);
                                    continue;
                                }
                                else if (cordX >= 1 || cordX <= 4)
                                {
                                    if (cordY == 1)
                                    {
                                        //start checking if seed is eligible to complete 
                                        int midValue = cordX + 1;
                                        //if moving starts for the first time;
                                        // and positionValue | straightValue
                                        // and this.movenNumber cancels each other out to 0?
                                        if ((midValue - this.moveNumber == 0 && moveCounter == 0) || (midValue > this.moveNumber))
                                        {
                                            if ((midValue - this.moveNumber == 0 && moveCounter == 0))
                                            {
                                                GameObject seedOutHome = GameObject.Find($"{activeSeed.name}OutHome");
                                                activeSeed.GetComponent<Rigidbody>().useGravity = false;
                                                ChangePosition(seedOutHome, activeSeed);
                                                //play complete sound
                                                yield return new WaitForSeconds(0.5f);
                                                if (AllComplete())
                                                {
                                                    StartCoroutine(audioManager.PlayAudio(5, 0));
                                                    playerManager.RemovePlayer(this.currentPlayer);
                                                }
                                                else
                                                {
                                                    StartCoroutine(audioManager.PlayAudio(4, 0));
                                                }
                                                if (accumulatedDices.Count == 0)
                                                {
                                                    ChangeActivePlayer();
                                                }
                                                PartialReset();
                                                yield break;
                                            }
                                        }
                                        else if (midValue - this.moveNumber != 0 && moveCounter == 0)
                                        {
                                            ChangeActivePlayer();
                                            PartialReset();
                                            yield break;

                                        }
                                        //end of checking if seed is eligible to complete 
                                    }
                                }
                            }
                            break;
                        case 'r':
                            if (cordY == 2)
                            {
                                x = 1;
                                if (cellCoordinateXY[0] == '5')
                                {
                                    x = 0;
                                    z = -1;
                                }
                            }
                            else if (cordY == 0 || cordY == 1)
                            {
                                x = -1;
                                if (cordX == 5)
                                {
                                    x = 0;
                                    z = -1;
                                    //ends with 50
                                    if (cellCoordinateXY.EndsWith("0"))
                                    {
                                        z = 0;
                                        x = -1;
                                    }
                                    //ends with 51
                                    else if (cellCoordinateXY.EndsWith("1"))
                                    {
                                        if (activeSeed.name[0] == 'r')
                                        {
                                            z = 0;
                                            x = -1;

                                            {
                                                //start checking if seed is eligible to complete 
                                                int midValue = cordX + 1;
                                                //if moving starts for the first time;
                                                // and positionValue | straightValue
                                                // and this.movenNumber cancels each other out to 0?
                                                if ((midValue - this.moveNumber == 0 && moveCounter == 0) || (midValue > this.moveNumber))
                                                {
                                                    if ((midValue - this.moveNumber == 0 && moveCounter == 0))
                                                    {
                                                        GameObject seedOutHome = GameObject.Find($"{activeSeed.name}OutHome");
                                                        activeSeed.GetComponent<Rigidbody>().useGravity = false;
                                                        ChangePosition(seedOutHome, activeSeed);

                                                        //play complete sound
                                                        yield return new WaitForSeconds(.5f);
                                                        if (AllComplete())
                                                        {
                                                            StartCoroutine(audioManager.PlayAudio(5, 0));
                                                            playerManager.RemovePlayer(this.currentPlayer);
                                                        }
                                                        else
                                                        {
                                                            StartCoroutine(audioManager.PlayAudio(4, 0));
                                                        }
                                                        if (accumulatedDices.Count == 0)
                                                        {
                                                            ChangeActivePlayer();
                                                        }
                                                        PartialReset();
                                                        yield break;
                                                    }
                                                }
                                                else if (midValue - this.moveNumber != 0 && moveCounter == 0)
                                                {
                                                    ChangeActivePlayer();
                                                    PartialReset();
                                                    yield break;

                                                }
                                                //end of checking if seed is eligible to complete 
                                            }
                                        }
                                    }
                                }
                                else if (cordX == 0 && cordY == 0)
                                {

                                    PlaceSeedToPosition("b02");
                                    yield return new WaitForSeconds(0.3f);
                                    continue;
                                }
                                else if (cordX >= 1 || cordX <= 4)
                                {
                                    if (cordY == 1)
                                    {
                                        //start checking if seed is eligible to complete 
                                        int midValue = cordX + 1;
                                        //if moving starts for the first time;
                                        // and positionValue | straightValue
                                        // and this.movenNumber cancels each other out to 0?
                                        if ((midValue - this.moveNumber == 0 && moveCounter == 0) || (midValue > this.moveNumber))
                                        {
                                            if ((midValue - this.moveNumber == 0 && moveCounter == 0))
                                            {
                                                GameObject seedOutHome = GameObject.Find($"{activeSeed.name}OutHome");
                                                activeSeed.GetComponent<Rigidbody>().useGravity = false;
                                                ChangePosition(seedOutHome, activeSeed);
                                                //play complete sound
                                                yield return new WaitForSeconds(1);
                                                if (AllComplete())
                                                {
                                                    StartCoroutine(audioManager.PlayAudio(5, 0));
                                                    playerManager.RemovePlayer(this.currentPlayer);
                                                }
                                                else
                                                {
                                                    StartCoroutine(audioManager.PlayAudio(4, 0));
                                                }
                                                if (accumulatedDices.Count == 0)
                                                {
                                                    ChangeActivePlayer();
                                                }
                                                PartialReset();
                                                yield break;
                                            }
                                        }
                                        else if (midValue - this.moveNumber != 0 && moveCounter == 0)
                                        {
                                            ChangeActivePlayer();
                                            PartialReset();
                                            yield break;

                                        }
                                        //end of checking if seed is eligible to complete 
                                    }
                                }
                            }
                            break;
                    }
                    if (!breakRoutine)
                    {
                        audioManager.PlaySimpleAudio(3);
                        Vector3 newPosition = new(oldPosition.x + x, oldPosition.y, oldPosition.z + z);
                        activeSeed.transform.position = newPosition;

                        //check if the partial position has seeds more than 2: if it is, don't allow skip
                        Collider[] colliders = meinPhysics.ProjectOverlap(activeSeed.transform, seedLayerMask);
                        if (colliders.Length >= 2)
                        {
                            if (colliders[0].name[0] != activeSeed.name[0])
                            {
                                this.moveNumber = 0;
                                breakRoutine = true;
                                ForceChangeActivePlayer();
                                ResetToPreviousPostion();
                                isOnMove = false;

                                //play sound
                                audioManager.PlayAudio(6, 0);

                                //display the number seeds there
                                GameObject floatedText = Instantiate(floatingText, newPosition, Quaternion.identity);
                                floatedText.transform.GetChild(0).GetComponent<TextMeshPro>().text = colliders.Length.ToString();
                                Destroy(floatedText, 5);
                            }

                        }
                    }
                }
                yield return new WaitForSeconds(0.3f);

                moveCounter++; ;
            }
        }
        if (!breakRoutine)
        {
            //seed is finally at rest
            GameObject cell = meinPhysics.WhatIsBelow(activeSeed.transform, cellLayerMask);
            if (cell != null)
            {
                //fit activeSeed to the center of the cell
                Vector3 cellPos = cell.transform.position;
                cellPos = new(cellPos.x, cellPos.y + 1.5f, cellPos.z);
                activeSeed.transform.position = cellPos;

                //push it up 
                //cellPos = new(cellPos.x, cellPos.y + 1.5f, cellPos.z);
                //activeSeed.transform.position = cellPos;
                //activeSeed.GetComponent<Rigidbody>().useGravity = true;

                //proceed to normal procedure
                _cellTransform = cell.transform;
                NormalProcedure(cell.transform);
                //after normal procedure check the accumulated list
                //if one is left, then automatical commit that to move

                if (accumulatedDices.Count == 1 && !IsAboutSideKick)
                {
                    SetToMove(accumulatedDices[^1]);
                    moveNumber = accumulatedDices[^1];
                    yield return null;
                }
            }
        }
        //reset movenumber
        moveNumber = -1;
    }

    private IEnumerator MoveBackRoutine()
    {
        int moveCounter = 0;
        string cellCoordinateXY = "";
        bool breakRoutine = false;
        if (!isCounting)
        {
            isCounting = true;
            for (int i = 1; i <= this.moveNumber; i++)
            {
                int x = 0; int z = 0;
                Ray ray = new(activeSeed.transform.position, Vector3.down);
                float maxDistance = Mathf.Infinity;
                Vector3 oldPosition = activeSeed.transform.position;
                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, cellLayerMask))
                {
                    string cellName = hit.collider.gameObject.name;
                    //get the seed coordinates(x,y~z)
                    cellCoordinateXY = cellName[1..];
                    //activeSeed.GetComponent<Rigidbody>().useGravity = true;
                    oldPosition.y = 0.9f;

                    int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
                    int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());
                    switch (cellName[0])
                    {
                        case 'b':
                            //check if seed is on the first column then opt to move downwards
                            if (cordY == 0)
                            {
                                //moving downwards
                                z = -1;
                                //however if the seed is at position starting with 5
                                //then stop moving down and move right
                                if (cordX == 5)
                                {
                                    z = 0;
                                    x = 1;
                                }
                            }
                            else if (cordY == 1 || cordY == 2)
                            {
                                z = 1;
                                if (cordX == 5)
                                {
                                    z = 0;
                                    x = 1;//move further right
                                    //if seed is on 50 then we move up
                                    if (cordY == 2)
                                    {
                                        z = 1;
                                        x = 0;
                                    }
                                }
                                else if (cordX == 0 && cordY == 2)
                                {
                                    PlaceSeedToPosition("r00");
                                    yield return new WaitForSeconds(0.3f);
                                    continue;
                                }

                            }
                            break;
                        case 'y':
                            if (cordY == 0)
                            {
                                x = -1;
                                if (cordX == 5)
                                {
                                    x = 0;
                                    z = -1;
                                }
                            }
                            else if (cordY == 1 || cordY == 2)
                            {
                                x = 1;
                                if (cordX == 5)
                                {
                                    x = 0;
                                    z = -1;
                                    //ends with 50
                                    if (cordY == 2)
                                    {
                                        z = 0;
                                        x = 1;
                                    }
                                    //ends with 51
                                    else if (cordY == 1)
                                    {

                                    }
                                }
                                else if (cordX == 0 && cordY == 2)
                                {

                                    PlaceSeedToPosition("b00");
                                    yield return new WaitForSeconds(0.3f);
                                    continue;
                                }
                                else if (cordX >= 1 || cordX <= 4)
                                {

                                }
                            }
                            break;
                        case 'g':
                            //check if seed is on the last column then opt to move downwards
                            if (cordY == 0)
                            {
                                //moving downwards
                                z = 1;
                                //however if the seed is at position starting with 5
                                //then stop moving down and move left
                                if (cordX == 5)
                                {
                                    z = 0;
                                    x = -1;
                                }
                            }
                            else if (cordY == 1 || cordY == 2)
                            {
                                z = -1;
                                if (cordX == 5)
                                {
                                    z = 0;
                                    x = -1;
                                    //if seed is on 50 then we move up
                                    if (cordY == 2)
                                    {
                                        z = -1;
                                        x = 0;
                                    }
                                    //ends with 51
                                    else if (cordY == 1)
                                    {

                                    }
                                }
                                else if (cordX == 0 && cordY == 2)
                                {

                                    PlaceSeedToPosition("y00");
                                    yield return new WaitForSeconds(0.3f);
                                    continue;
                                }
                            }
                            break;
                        case 'r':
                            if (cordY == 0)
                            {
                                x = 1;
                                if (cordX == 5)
                                {
                                    x = 0;
                                    z = 1;
                                }
                            }
                            else if (cordY == 1 || cordY == 2)
                            {
                                x = -1;
                                if (cordX == 5)
                                {
                                    x = 0;
                                    z = 1;
                                    //ends with 50
                                    if (cordY == 2)
                                    {
                                        z = 0;
                                        x = -1;
                                    }
                                    //ends with 51
                                    else if (cordY == 1)
                                    {

                                    }
                                }
                                else if (cordX == 0 && cordY == 2)
                                {

                                    PlaceSeedToPosition("g00");
                                    yield return new WaitForSeconds(0.3f);
                                    continue;
                                }
                                else if (cordX >= 1 || cordX <= 4)
                                {

                                }
                            }
                            break;
                    }
                    if (!breakRoutine)
                    {
                        audioManager.PlaySimpleAudio(3);
                        Vector3 newPosition = new(oldPosition.x + x, oldPosition.y, oldPosition.z + z);
                        activeSeed.transform.position = newPosition;

                        //check if the partial position has seeds more than 2: if it is, don't allow skip
                        Collider[] colliders = meinPhysics.ProjectOverlap(activeSeed.transform, seedLayerMask);
                        if (colliders.Length >= 2)
                        {
                            if (colliders[0].name[0] != activeSeed.name[0])
                            {
                                this.moveNumber = 0;
                                breakRoutine = true;
                                ForceChangeActivePlayer();
                                ResetToPreviousPostion();
                                isOnMove = false;

                                //play sound
                                audioManager.PlayAudio(6, 0);

                                //display the number seeds there
                                GameObject floatedText = Instantiate(floatingText, newPosition, Quaternion.identity);
                                floatedText.transform.GetChild(0).GetComponent<TextMeshPro>().text = colliders.Length.ToString();
                                Destroy(floatedText, 5);
                            }

                        }
                    }
                }
                yield return new WaitForSeconds(0.3f);

                moveCounter++; ;
            }
        }
        if (!breakRoutine)
        {
            //seed is finally at rest
            GameObject cell = meinPhysics.WhatIsBelow(activeSeed.transform, cellLayerMask);
            if (cell != null)
            {
                //fit activeSeed to the center of the cell
                activeSeed.transform.position = cell.transform.position;
                //proceed to normal procedure
                _cellTransform = cell.transform;
                if (!NormalProcedure(cell.transform))
                {
                    print("no normal procedure");
                    if (accumulatedDices.Count == 0)
                    {
                        ResetToPreviousPostion(_specialOperation_init_position);
                        SetMoveDirection(true);

                        _specialOperation_init_position = new();
                        specialOperation = false;
                    }
                }
                else
                {
                    print("there was normal procedure");
                    if (!specialOperation)
                    {
                        _specialOperation_init_position = new();
                    }

                    specialOperation = false;
                    SetMoveDirection(true);
                    //if(accumulatedDices.Count == 0)
                    //{
                    //    ChangeActivePlayer();
                    //}
                }
                //SetMoveDirection(true);
            }
        }
        //reset movenumber
        moveNumber = -1;

    }

    /**
        i.  | tracerNumber[] the number of traces 
        ii. | pather: the way point to find all points 
        iii.| tracer: the dot and pathway of a trace
     **/
    public IEnumerator SimpleForwardTrace(List<int> traceNumber, GameObject pather, GameObject tracer)
    {
        print("doing forward routine");
        if(moveNumber != -1)
        {
            traceNumber = new()
            {
                moveNumber
            };
        }
        else
        {
            traceNumber ??= accumulatedDices;
        }

        foreach(int i in traceNumber)
        {
            print("the "+ i);
        }

        Vector3 tracerDefaultPosition = pather.transform.position;
        if(traceNumber.Count > 0)
        {
            for (int j = 0; j < traceNumber.Count; j++)
            {
                string cellCoordinateXY = "";
                for (int i = 1; i <= traceNumber[j]; i++)
                {
                    int x = 0; int z = 0;
                    Ray ray = new(pather.transform.position, Vector3.down);
                    float maxDistance = Mathf.Infinity;
                    Vector3 oldPosition = pather.transform.position;
                    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, cellLayerMask))
                    {
                        string cellName = hit.collider.gameObject.name;
                        //get the seed coordinates(x,y~z)
                        cellCoordinateXY = cellName[1..];
                        //activeSeed.GetComponent<Rigidbody>().useGravity = true;
                        oldPosition.y = 0.4f;

                        int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
                        int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());

                        switch (cellName[0])
                        {
                            case 'b':
                                //check if seed is on the last column then opt to move downwards
                                if (cordY == 2)
                                {
                                    //moving downwards
                                    z = -1;
                                    //however if the seed is at position starting with 5
                                    //then stop moving down and move left
                                    if (cordX == 5)
                                    {
                                        z = 0;
                                        x = -1;
                                    }
                                }
                                else if (cordY == 0 || cordY == 1)
                                {
                                    z = 1;
                                    if (cordX == 5)
                                    {
                                        z = 0;
                                        x = -1;
                                        //if seed is on 50 then we move up
                                        if (cordY == 0)
                                        {
                                            z = 1;
                                            x = 0;
                                        }
                                        //ends with 51
                                        else if (cordY == 1)
                                        {
                                            if (pather.name[0] == 'b')
                                            {
                                                z = 1;
                                                x = 0;
                                            }
                                        }
                                    }
                                    else if (cordX == 0 && cordY == 0)
                                    {
                                        PlaceSeedToPosition("y02", pather.transform);
                                        GameObject aTracerA = Instantiate(tracer);
                                        aTracerA.transform.position = pather.transform.position;
                                        if (i == traceNumber[j])
                                        {
                                            GameObject canvas = aTracerA.transform.GetChild(0).gameObject;
                                            canvas.gameObject.SetActive(true);
                                            print("cell name " + cellName + " cordY " + cordY);
                                            switch (cellName[0])
                                            {
                                                case 'b':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'y':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'g':
                                                    break;
                                                case 'r':
                                                    break;
                                            }
                                            Destroy(aTracerA, 10f);
                                        }
                                        else
                                        {
                                            Destroy(aTracerA, 3f);
                                        }
                                        yield return new WaitForSeconds(0.1f);
                                        continue;
                                    }
                                    else if (cordX >= 1 || cordX <= 4)
                                    {
                                        if (pather.name[0].Equals('b'))
                                        {
                                            z = 1;
                                            x = 0;
                                        }
                                    }
                                }
                                break;
                            case 'y':
                                if (cordY == 2)
                                {
                                    x = -1;
                                    if (cellCoordinateXY[0] == '5')
                                    {
                                        x = 0;
                                        z = 1;
                                    }
                                }
                                else if (cordY == 0 || cordY == 1)
                                {
                                    x = 1;
                                    if (cordX == 5)
                                    {
                                        x = 0;
                                        z = 1;
                                        //ends with 50
                                        if (cordY == 0)
                                        {
                                            z = 0;
                                            x = 1;
                                        }
                                        //ends with 51
                                        else if (cordY == 1)
                                        {
                                            if (pather.name[0].Equals('y'))
                                            {
                                                z = 0;
                                                x = 1;
                                            }
                                        }
                                    }
                                    else if (cordX == 0 && cordY == 0)
                                    {

                                        PlaceSeedToPosition("g02", pather.transform);
                                        GameObject aTracerA = Instantiate(tracer);
                                        aTracerA.transform.position = pather.transform.position;
                                        if (i == traceNumber[j])
                                        {
                                            GameObject canvas = aTracerA.transform.GetChild(0).gameObject;
                                            canvas.gameObject.SetActive(true);
                                            print("char -> " + cellName[0]);
                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                            Destroy(aTracerA, 10f);
                                        }
                                        else
                                        {
                                            Destroy(aTracerA, 3f);
                                        }
                                        yield return new WaitForSeconds(0.1f);
                                        continue;
                                    }
                                    else if (cordX >= 1 || cordX <= 4)
                                    {
                                        if (cordY == 1)
                                        {
                                            // end of checking if seed is eligible to complete 
                                        }
                                    }
                                }
                                break;
                            case 'g':
                                //check if seed is on the last column then opt to move downwards
                                if (cordY == 2)
                                {
                                    //moving downwards
                                    z = 1;
                                    //however if the seed is at position starting with 5
                                    //then stop moving down and move left
                                    if (cordX == 5)
                                    {
                                        z = 0;
                                        x = 1;
                                    }
                                }
                                else if (cordY == 0 || cordY == 1)
                                {
                                    z = -1;
                                    if (cordX == 5)
                                    {
                                        z = 0;
                                        x = 1;
                                        //if seed is on 50 then we move up
                                        if (cordY == 0)
                                        {
                                            z = -1;
                                            x = 0;
                                        }
                                        //ends with 51
                                        else if (cordY == 1)
                                        {
                                            if (pather.name[0].Equals('g'))
                                            {
                                                z = -1;
                                                x = 0;
                                            }
                                        }
                                    }
                                    else if (cordX == 0 && cordY == 0)
                                    {

                                        PlaceSeedToPosition("r02", pather.transform);
                                        GameObject aTracerA = Instantiate(tracer);
                                        aTracerA.transform.position = pather.transform.position;
                                        if (i == traceNumber[j])
                                        {
                                            GameObject canvas = aTracerA.transform.GetChild(0).gameObject;
                                            canvas.gameObject.SetActive(true);
                                            print("cell name " + cellName);
                                            switch (cellName[0])
                                            {
                                                case 'b':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'y':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'g':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'r':
                                                    break;
                                            }
                                            Destroy(aTracerA, 10f);
                                        }
                                        else
                                        {
                                            Destroy(aTracerA, 3f);
                                        }
                                        yield return new WaitForSeconds(0.1f);
                                        continue;
                                    }
                                    else if (cordX >= 1 || cordX <= 4)
                                    {
                                        if (cordY == 1)
                                        {
                                            //end of checking if seed is eligible to complete 
                                        }
                                    }
                                }
                                break;
                            case 'r':
                                if (cordY == 2)
                                {
                                    x = 1;
                                    if (cellCoordinateXY[0] == '5')
                                    {
                                        x = 0;
                                        z = -1;
                                    }
                                }
                                else if (cordY == 0 || cordY == 1)
                                {
                                    x = -1;
                                    if (cordX == 5)
                                    {
                                        x = 0;
                                        z = -1;
                                        //ends with 50
                                        if (cellCoordinateXY.EndsWith("0"))
                                        {
                                            z = 0;
                                            x = -1;
                                        }
                                        //ends with 51
                                        else if (cellCoordinateXY.EndsWith("1"))
                                        {
                                            if (pather.name[0] == 'r')
                                            {
                                                z = 0;
                                                x = -1;
                                            }
                                        }
                                    }
                                    else if (cordX == 0 && cordY == 0)
                                    {

                                        PlaceSeedToPosition("b02", pather.transform);
                                        GameObject aTracerA = Instantiate(tracer);
                                        aTracerA.transform.position = pather.transform.position;
                                        if (i == traceNumber[j])
                                        {
                                            GameObject canvas = aTracerA.transform.GetChild(0).gameObject;
                                            canvas.gameObject.SetActive(true);
                                            print("cell name " + cellName);
                                            switch (cellName[0])
                                            {
                                                case 'b':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'y':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'g':
                                                    break;
                                                case 'r':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                            }
                                            Destroy(aTracerA, 10f);
                                        }
                                        else
                                        {
                                            Destroy(aTracerA, 3f);
                                        }
                                        yield return new WaitForSeconds(0.1f);
                                        continue;
                                    }
                                    else if (cordX >= 1 || cordX <= 4)
                                    {
                                        if (cordY == 1)
                                        {
                                            //start checking if seed is eligible to complete 
                                            //end of checking if seed is eligible to complete 
                                        }
                                    }
                                }
                                break;
                        }
                        Vector3 newPosition = new(oldPosition.x + x, oldPosition.y, oldPosition.z + z);
                        pather.transform.position = newPosition;
                        GameObject aTracer = Instantiate(tracer);
                        aTracer.transform.position = newPosition;

                        if (i == traceNumber[j])
                        {
                            GameObject canvas = aTracer.transform.GetChild(0).gameObject;
                            canvas.gameObject.SetActive(true);
                            switch (cellName[0])
                            {
                                case 'b':
                                    switch (cordY)
                                    {
                                        case 0:
                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                            break;
                                        case 1:
                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                            break;
                                        case 2:
                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                            break;
                                    }
                                    break;
                                case 'y':
                                    print("down -- " + cordY + $" {cordY}, {cellName[0]} " + cellName[0]);
                                    switch (cordY)
                                    {
                                        case 0:
                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                            break;
                                        case 1:
                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                            break;
                                        case 2:
                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                            break;
                                    }
                                    break;
                                case 'g':
                                    switch (cordY)
                                    {
                                        case 0:
                                            print("the gnm break");
                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                            break;
                                        case 1:
                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                            break;
                                        case 2:
                                            print("the g break");
                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                            break;
                                    }
                                    break;
                                case 'r':
                                    switch (cordY)
                                    {
                                        case 0:
                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                            break;
                                        case 1:
                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                            break;
                                        case 2:
                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                            break;
                                    }
                                    break;
                            }
                            Destroy(aTracer, 10f);
                        }
                        else
                        {
                            Destroy(aTracer, 3f);
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                if (j == traceNumber.Count - 1)
                {
                    Destroy(pather);
                }
            }
        }
    }

    public IEnumerator SimpleBackwardTrace(List<int> traceNumber, GameObject pather, GameObject tracer)
    {
        if (moveNumber != -1)
        {
            traceNumber = new()
            {
                moveNumber
            };
        }
        else
        {
            traceNumber ??= accumulatedDices;
        }
        Vector3 tracerDefaultPosition = pather.transform.position;
        if (traceNumber.Count > 0)
        {
            for (int j = 0; j < traceNumber.Count(); j++)
            {
                string cellCoordinateXY = "";
                for (int i = 1; i <= traceNumber[j]; i++)
                {
                    int x = 0; int z = 0;
                    Ray ray = new(pather.transform.position, Vector3.down);
                    float maxDistance = Mathf.Infinity;
                    Vector3 oldPosition = pather.transform.position;
                    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, cellLayerMask))
                    {
                        string cellName = hit.collider.gameObject.name;
                        //get the seed coordinates(x,y~z)
                        cellCoordinateXY = cellName[1..];
                        //activeSeed.GetComponent<Rigidbody>().useGravity = true;
                        oldPosition.y = 0.4f;

                        int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
                        int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());

                        switch (cellName[0])
                        {
                            case 'b':
                                //check if seed is on the first column then opt to move downwards
                                if (cordY == 0)
                                {
                                    //moving downwards
                                    z = -1;
                                    //however if the seed is at position starting with 5
                                    //then stop moving down and move right
                                    if (cordX == 5)
                                    {
                                        z = 0;
                                        x = 1;
                                    }
                                }
                                else if (cordY == 1 || cordY == 2)
                                {
                                    z = 1;
                                    if (cordX == 5)
                                    {
                                        z = 0;
                                        x = 1;//move further right
                                        //if seed is on 50 then we move up
                                        if (cordY == 2)
                                        {
                                            z = 1;
                                            x = 0;
                                        }
                                    }
                                    else if (cordX == 0 && cordY == 2)
                                    {
                                        PlaceSeedToPosition("r00", pather.transform);
                                        GameObject aTracerA = Instantiate(tracer);
                                        aTracerA.transform.position = pather.transform.position;
                                        if (i == traceNumber[j])
                                        {
                                            GameObject canvas = aTracerA.transform.GetChild(0).gameObject;
                                            canvas.gameObject.SetActive(true);
                                            print("cell name " + cellName);
                                            switch (cellName[0])
                                            {
                                                case 'b':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'y':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'g':
                                                    break;
                                                case 'r':
                                                    break;
                                            }
                                            Destroy(aTracerA, 10f);
                                        }
                                        else
                                        {
                                            Destroy(aTracerA, 3f);
                                        }
                                        yield return new WaitForSeconds(0.3f);
                                        continue;
                                    }

                                }
                                break;
                            case 'y':
                                if (cordY == 0)
                                {
                                    x = -1;
                                    if (cordX == 5)
                                    {
                                        x = 0;
                                        z = -1;
                                    }
                                }
                                else if (cordY == 1 || cordY == 2)
                                {
                                    x = 1;
                                    if (cordX == 5)
                                    {
                                        x = 0;
                                        z = -1;
                                        //ends with 50
                                        if (cordY == 2)
                                        {
                                            z = 0;
                                            x = 1;
                                        }
                                        //ends with 51
                                        else if (cordY == 1)
                                        {

                                        }
                                    }
                                    else if (cordX == 0 && cordY == 2)
                                    {

                                        PlaceSeedToPosition("b00", pather.transform);
                                        GameObject aTracerA = Instantiate(tracer);
                                        aTracerA.transform.position = pather.transform.position;
                                        if (i == traceNumber[j])
                                        {
                                            GameObject canvas = aTracerA.transform.GetChild(0).gameObject;
                                            canvas.gameObject.SetActive(true);
                                            print("cell name " + cellName + " ~ " + cordY);
                                            switch (cellName[0])
                                            {
                                                case 'b':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'y':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'g':
                                                    break;
                                                case 'r':
                                                    break;
                                            }
                                            Destroy(aTracerA, 10f);
                                        }
                                        else
                                        {
                                            Destroy(aTracerA, 3f);
                                        }
                                        yield return new WaitForSeconds(0.3f);
                                        continue;
                                    }
                                    else if (cordX >= 1 || cordX <= 4)
                                    {

                                    }
                                }
                                break;
                            case 'g':
                                //check if seed is on the last column then opt to move downwards
                                if (cordY == 0)
                                {
                                    //moving downwards
                                    z = 1;
                                    //however if the seed is at position starting with 5
                                    //then stop moving down and move left
                                    if (cordX == 5)
                                    {
                                        z = 0;
                                        x = -1;
                                    }
                                }
                                else if (cordY == 1 || cordY == 2)
                                {
                                    z = -1;
                                    if (cordX == 5)
                                    {
                                        z = 0;
                                        x = -1;
                                        //if seed is on 50 then we move up
                                        if (cordY == 2)
                                        {
                                            z = -1;
                                            x = 0;
                                        }
                                        //ends with 51
                                        else if (cordY == 1)
                                        {

                                        }
                                    }
                                    else if (cordX == 0 && cordY == 2)
                                    {

                                        PlaceSeedToPosition("y00", pather.transform);
                                        GameObject aTracerA = Instantiate(tracer);
                                        aTracerA.transform.position = pather.transform.position;
                                        if (i == traceNumber[j])
                                        {
                                            GameObject canvas = aTracerA.transform.GetChild(0).gameObject;
                                            canvas.gameObject.SetActive(true);
                                            print("cell name " + cellName);
                                            switch (cellName[0])
                                            {
                                                case 'b':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'y':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'g':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'r':

                                                    break;
                                            }
                                            Destroy(aTracerA, 10f);
                                        }
                                        else
                                        {
                                            Destroy(aTracerA, 3f);
                                        }
                                        yield return new WaitForSeconds(0.3f);
                                        continue;
                                    }
                                }
                                break;
                            case 'r':
                                if (cordY == 0)
                                {
                                    x = 1;
                                    if (cordX == 5)
                                    {
                                        x = 0;
                                        z = 1;
                                    }
                                }
                                else if (cordY == 1 || cordY == 2)
                                {
                                    x = -1;
                                    if (cordX == 5)
                                    {
                                        x = 0;
                                        z = 1;
                                        //ends with 50
                                        if (cordY == 2)
                                        {
                                            z = 0;
                                            x = -1;
                                        }
                                        //ends with 51
                                        else if (cordY == 1)
                                        {

                                        }
                                    }
                                    else if (cordX == 0 && cordY == 2)
                                    {

                                        PlaceSeedToPosition("g00", pather.transform);
                                        GameObject aTracerA = Instantiate(tracer);
                                        aTracerA.transform.position = pather.transform.position;
                                        if (i == traceNumber[j])
                                        {
                                            GameObject canvas = aTracerA.transform.GetChild(0).gameObject;
                                            canvas.gameObject.SetActive(true);
                                            print("cell name " + cellName);
                                            switch (cellName[0])
                                            {
                                                case 'b':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'y':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                                case 'g':
                                                    break;
                                                case 'r':
                                                    switch (cordY)
                                                    {
                                                        case 0:
                                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                                            break;
                                                        case 1:
                                                            break;
                                                        case 2:
                                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                                            break;
                                                    }
                                                    break;
                                            }
                                            Destroy(aTracerA, 10f);
                                        }
                                        else
                                        {
                                            Destroy(aTracerA, 3f);
                                        }
                                        yield return new WaitForSeconds(0.3f);
                                        continue;
                                    }
                                    else if (cordX >= 1 || cordX <= 4)
                                    {

                                    }
                                }
                                break;
                        }
                        Vector3 newPosition = new(oldPosition.x + x, oldPosition.y, oldPosition.z + z);
                        pather.transform.position = newPosition;
                        GameObject aTracer = Instantiate(tracer);
                        aTracer.transform.position = newPosition;
                        print($"cord-x {cordX} cord-y {cordX} name-{cellCoordinateXY} - {cellName[0]}");
                        if (i == traceNumber[j])
                        {
                            GameObject canvas = aTracer.transform.GetChild(0).gameObject;
                            canvas.gameObject.SetActive(true);
                            switch (cellName[0])
                            {
                                case 'b':
                                    switch (cordY)
                                    {
                                        case 0:
                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                            break;
                                        case 1:
                                            if(cordX == 5)
                                            {
                                                canvas.transform.GetChild(2).gameObject.SetActive(true);
                                            }
                                            else
                                            {
                                                canvas.transform.GetChild(3).gameObject.SetActive(true);
                                            }
                                            break;
                                        case 2:
                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                            break;
                                    }
                                    break;
                                case 'y':
                                    switch (cordY)
                                    {
                                        case 0:
                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                            break;
                                        case 1:
                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                            break;
                                        case 2:
                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                            break;
                                    }
                                    break;
                                case 'g':
                                    switch (cordY)
                                    {
                                        case 0:
                                            canvas.transform.GetChild(2).gameObject.SetActive(true);
                                            break;
                                        case 1:
                                            print($"cordY {cordY}");
                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                            break;
                                        case 2:
                                            canvas.transform.GetChild(3).gameObject.SetActive(true);
                                            break;
                                    }
                                    break;
                                case 'r':
                                    switch (cordY)
                                    {
                                        case 0:
                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                            break;
                                        case 1:
                                            canvas.transform.GetChild(0).gameObject.SetActive(true);
                                            break;
                                        case 2:
                                            canvas.transform.GetChild(1).gameObject.SetActive(true);
                                            break;
                                    }
                                    break;
                            }
                            Destroy(aTracer, 10f);
                        }
                        else
                        {
                            Destroy(aTracer, 3f);
                        }
                    }
                    yield return new WaitForSeconds(0.1f);
                }
                if (j == traceNumber.Count - 1)
                {
                    Destroy(pather);
                }
            }
        }
    }

    private void PartialReset()
    {
        isCounting = false;
        isOnMove = false;
    }

    bool NormalProcedure(Transform cellTransform)
    {
        bool _worked = false;
        //print("initiating normal procedure");
        //look at current cell to see if contains some seeds
        Collider[] colliders = new MeinPhysics().ProjectOverlap(cellTransform, seedLayerMask);
        // cellTransform.GetComponent<MeshRenderer>().material.color = Color.black;
        // remove current seed from colliders
        List<Collider> seivedColliders = new();
        foreach (Collider collider in colliders)
        {
            //print("collider " + collider.name); 
            string tag = collider.tag?.ToLower();
            if ((tag.ToLower().Contains("seed")) && (collider.name[0] != activeSeed.name[0]))
            {
                seivedColliders.Add(collider);
            }
        }
        /*
         * begin procedure
         */
        /*check if is a valid move*/

        //print(seivedColliders.Count);

        print(seivedColliders.Count);
        if (ValidMove(seivedColliders))
        {
            //if number is not 6 don't kick out || but this should change be because it doesn't make too much sense
            //well there were some seeds at this cell transform position
            if (seivedColliders.Count > 0)
            {
                //hasDonePositionKick = PositionKickOut(seivedColliders);
                // has completed positional kickout
                if (!PositionKickOut(seivedColliders))
                {
                    //used to check if opposite cell has seeds
                    //for sideKickin
                    _worked = TestBeforeSwapUI(cellTransform);
                }
                else
                {
                    _worked = true;
                    //change player 
                    ForceChangeActivePlayer();
                }
            }
            else
            {
                //if no seeds are at current posstion 
                // check to your row to see possible moves
                _worked = TestBeforeSwapUI(cellTransform);
            }

            isCounting = false;
            isOnMove = false;
        }
        else
        {
            //print("is not a valid move");
            /** reset position of activeSeed */
            ResetToPreviousPostion();
            /** rehighlight activeSeed to indicate player has another chance to play*/
            //HighLightActivePlayerSeedsBaseDieNumber(this.activePlayer, true, this.moveNumber);

            //update onMove to allow player to move by setting isOnMove to true
            ForceChangeActivePlayer();
        }
        return _worked;
    }
    void HomeNormalProcedure(Transform cellTransform)
    {
        _cellTransform = cellTransform;
        Collider[] colliders = new MeinPhysics().ProjectOverlap(cellTransform, seedLayerMask);
       
        // cellTransform.GetComponent<MeshRenderer>().material.color = Color.black;
        // remove current seed from colliders
        List<Collider> seivedColliders = new(colliders);
        /*
         * begin procedure
         */
        /*check if is a valid move*/
        //print("number of colliders : " +seivedColliders.Count);
        foreach (Collider collider in colliders)
        {
            //print("collider " + collider.name); 
            string tag = collider.tag?.ToLower();
            if ((tag.ToLower().Contains("seed")) && (collider.name[0] != activeSeed.name[0]))
            {
                seivedColliders.Add(collider);
            }
        }

        if (ValidMove(seivedColliders))
        {
            if (!HomePositionKickOut(seivedColliders))
            {
                TestBeforeSwapUI(cellTransform);
            }
            PartialReset();
        }
        else
        {
            print("not valid move");
            /** reset position of activeSeed */
            ResetToPreviousPostion();
            /** rehighlight activeSeed to indicate player has another chance to play*/
            HighLightActivePlayerSeedsBaseDieNumber(this.activePlayer, true, this.moveNumber);

            //update onMove to allow player to move by setting isOnMove to true
            PartialReset();
            //ChangeActivePlayer();
        }
    }
    private void ForceChangeActivePlayer()
    {
        if (accumulatedDices.Count == 0)
        {
            ChangeActivePlayer();
        }
        PartialReset();
        isOnMove = true;
        isCounting = false;
        SetMoveDirection(true);
    }

    void ChangeActivePlayer()
    {
        this.currentPlayer = playerManager.SequenceChangePlayer(currentPlayer);
        this.activePlayer = currentPlayer.Color;
        onActiveUser?.Invoke(this.currentPlayer);
        processOnGoing?.Invoke(false);
        IsAboutSideKick = false;
        this.moveNumber = -1;
    }

    //used to select a seed to move
    private void HighLightActivePlayerSeeds(string player, bool activate)
    {
        GameObject[] playerSeeds = GameObject.FindGameObjectsWithTag($"{player[0]}seed");

        foreach (GameObject seed in playerSeeds)
        {
            Transform transform = seed.transform;
            //activating flash to highlight active player seeds
            transform.GetChild(0).gameObject.SetActive(activate);
        }
    }
    private void HighLightActivePlayerSeedsBaseDieNumber(string player, bool activate, int dieNumber)
    {
        GameObject[] playerSeeds = GameObject.FindGameObjectsWithTag($"{player[0]}seed");
        if (dieNumber == 6)
        {
            foreach (GameObject seed in playerSeeds)
            {
                Transform transform = seed.transform;
                //activating flash to highlight active player seeds
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit))
                {
                    string tag = hit.transform.tag;
                    if (tag.Equals("cell") || tag.Contains("Home"))
                    {
                        //transform.GetChild(0).gameObject.SetActive(activate);
                        //highlight possible moves only


                        string cellName = hit.collider.gameObject.name;
                        //get the seed coordinates(x,y~z)
                        string cellCoordinateXY = cellName[1..];

                        int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
                        int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());
                        //check if seed is in the middle lane
                        if (cordY == 1 && cordX != 5)
                        {
                            //start checking if seed is eligible to complete 
                            int midValue = cordX + 1;
                            //if moving starts for the first time;
                            // and positionValue | straightValue
                            // and this.movenNumber cancels each other out to 0?

                            //checking validity | when is true that means move is not good, skip.
                            bool _c = (midValue - dieNumber) < 0;
                            if (_c)
                            {
                                transform.GetChild(0).gameObject.SetActive(!_c);//mark it active to move
                                transform.GetComponent<SeedScript>().active = true;
                                //transform.GetComponent<SeedScript>().active = !_c;
                            }
                            else
                            {
                                transform.GetComponent<SeedScript>().active = true;
                            }
                        }
                        else
                        {
                            transform.GetChild(0).gameObject.SetActive(activate);
                            transform.GetComponent<SeedScript>().active = true;
                        }
                    }
                }
            }

        }
        else
        {
            foreach (GameObject seed in playerSeeds)
            {
                Transform transform = seed.transform;
                //activating flash to highlight active player seeds
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit))
                {
                    string tag = hit.transform.tag;
                    if (tag.Equals("cell"))
                    {
                        //highlight possible moves only

                        string cellName = hit.collider.gameObject.name;
                        //get the seed coordinates(x,y~z)
                        string cellCoordinateXY = cellName[1..];

                        int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
                        int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());
                        //check if seed is in the middle lane
                        if (cordY == 1 && cordX != 5)
                        {
                            //start checking if seed is eligible to complete 
                            int midValue = cordX + 1;
                            //if moving starts for the first time;
                            // and positionValue | straightValue
                            // and this.movenNumber cancels each other out to 0?

                            //checking validity | when is true that means move is not good, skip.
                            bool canMove = (midValue - dieNumber) < 0;
                            transform.GetChild(0).gameObject.SetActive(!canMove);
                            transform.GetComponent<SeedScript>().active = true;
                        }
                        else
                        {
                            transform.GetChild(0).gameObject.SetActive(activate);
                            //mark it active to move
                            transform.GetComponent<SeedScript>().active = true;
                        }
                    }
                }

            }
        }

    }

    public void PlaceSeedToPosition(string positionName)
    {
        isOnMove = false;
        GameObject yEntry = GameObject.Find(positionName);
        Vector3 yEntryPosition = yEntry.transform.position;
        yEntryPosition.y = activeSeed.transform.position.y;
        activeSeed.transform.position = yEntryPosition;
    }
    
    public void PlaceSeedToPosition(string positionName, Transform transport)
    {
        isOnMove = false;
        GameObject yEntry = GameObject.Find(positionName);
        Vector3 yEntryPosition = yEntry.transform.position;
        yEntryPosition.y = transport.position.y;
        transport.position = yEntryPosition;
    }

    private void PlaceSeedToPosition(GameObject opposite)
    {
        isOnMove = false;
        Vector3 OppositePosition = opposite.transform.position;
        //OppositePosition.y = activeSeed.transform.position.y;
        activeSeed.transform.position = OppositePosition;
    }

    //get seeds on home and board itself
    private List<GameObject> AllSeedsPlayable()
    {
        if(activePlayer != null)
        {
            GameObject[] playerSeeds = GameObject.FindGameObjectsWithTag($"{activePlayer[0]}seed");
            List<GameObject> found = new();
            foreach (GameObject seed in playerSeeds)
            {
                Transform transform = seed.transform;
                //activating flash to highlight active player seeds
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit))
                {
                    string tag = hit.transform.tag;
                    if (tag.ToLower().Contains("home") || !(tag.ToLower().Contains("home") || tag.ToLower().Contains("tray")))
                    {
                        found.Add(seed);
                    }
                }

            }
            return found;
        }
        return null;    
    }

    //get seeds on board itself
    public List<GameObject> AllSeedsOnBoard()
    {
        GameObject[] playerSeeds = GameObject.FindGameObjectsWithTag($"{activePlayer[0]}seed");
        List<GameObject> found = new();
        foreach (GameObject seed in playerSeeds)
        {
            Transform transform = seed.transform;
            //activating flash to highlight active player seeds
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                string tag = hit.transform.tag;
                if (tag.ToLower().Contains("cell"))
                {
                    found.Add(seed);
                }
            }

        }
        return found;
    }

    private List<GameObject> AllSeedsHome()
    {
        GameObject[] playerSeeds = GameObject.FindGameObjectsWithTag($"{activePlayer[0]}seed");
        List<GameObject> found = new();
        foreach (GameObject seed in playerSeeds)
        {
            Transform transform = seed.transform;
            //activating flash to highlight active player seeds
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                string tag = hit.transform.tag;
                if (tag.ToLower().Contains("home"))
                {
                    found.Add(seed);
                }
            }

        }
        return found;
    }

    private bool AllComplete()
    {
        GameObject[] playerSeeds = GameObject.FindGameObjectsWithTag($"{activePlayer[0]}seed");
        bool outBool = false;
        foreach (GameObject seed in playerSeeds)
        {
            Transform transform = seed.transform;
            //activating flash to highlight active player seeds
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                string tag = hit.transform.tag;
                outBool = tag.Equals("tray") || !seed.active;
                if (!outBool)
                {
                    break;
                }
            }

        }
        return outBool;
    }

    /** capacity: change positions*/
    bool ValidMove(List<Collider> allColliders)
    {
        if (allColliders.Count > 1)
        {
            /*
             * check if activeseed different from the first item
             * **/
            string collided = allColliders[0].name;
            string active = activeSeed.name;
            return collided[0] == active[0];
        }
        return true;
    }

    void ResetToPreviousPostion()
    {
        activeSeed.transform.position = activeSeedPreviousPosition;
    }

    void ResetToPreviousPostion(Vector3 position)
    {
        activeSeed.transform.position = position;
    }

    bool PositionKickOut(List<Collider> colliders)
    {
        GameObject oppositeSeed = colliders[0].gameObject;
        string oppositeSeedName = oppositeSeed.name;
        //print(" oppositeSeed -> " + oppositeSeed);
        //print("opposite name -> " + oppositeSeedName);
        string activeName = activeSeed.name;
        bool isOppositeSeed = oppositeSeed.CompareTag("bseed") || oppositeSeed.CompareTag("yseed") || oppositeSeed.CompareTag("rseed") || oppositeSeed.CompareTag("gseed");
        if (isOppositeSeed && (oppositeSeedName[0] != activeName[0]))
        {
            GameObject oppositeSeedHome = GameObject.Find($"{oppositeSeedName}Home");
            ChangePosition(oppositeSeedHome, oppositeSeed);
            StartCoroutine(audioManager.PlayAudio(2, 0));
            //instantiate the kick out effect
            GameObject effect = Instantiate(kickoutEffect, activeSeed.transform.position, Quaternion.Euler(new Vector3(0, 90, 0)));
            Destroy(effect, 3f);
            /**** change to differenct player ***/
            return true;
        }
        return false;
    }

    bool PositionKickOut(List<Collider> colliders, GameObject replacer)
    {
        if (colliders.Count == 0)
        {
            return false;
        };
        GameObject oppositeSeed = colliders[0].gameObject;
        //get the opp seed position, at this point, to be occupied by the replacer 
        Vector3 oppositeSeedFOrmalPosition = oppositeSeed.transform.position;

        string oppositeSeedName = oppositeSeed.name;
        string activeName = activeSeed.name;
        bool kickedOut = false;
        if (oppositeSeedName[0] != activeName[0])
        {
            GameObject oppositeSeedHome = GameObject.Find($"{oppositeSeedName}Home");
            ChangePosition(oppositeSeedHome, oppositeSeed);
            replacer.transform.position = oppositeSeedFOrmalPosition;
            StartCoroutine(audioManager.PlayAudio(2, 0));
            /**** change to differenct player ***/
            kickedOut = true;
            //instantiate the kick out effect
            GameObject effect = Instantiate(kickoutEffect, activeSeed.transform.position, Quaternion.identity);
            Destroy(effect, 1.5f);
        }

        if (accumulatedDices.Count == 0)
        {
            ChangeActivePlayer();
        }
        return kickedOut;
    }

    bool HomePositionKickOut(List<Collider> colliders)
    {
        if (colliders.Count == 0) return false;
        GameObject oppositeSeed = colliders[0].gameObject;
        string oppositeSeedName = oppositeSeed.name;
        string activeName = activeSeed.name;
        if ((oppositeSeedName[0] == activeName[0]) && colliders.Count == 2)
        {
            oppositeSeed = colliders[1].gameObject;
            oppositeSeedName = oppositeSeed.name;
        }
        if (oppositeSeedName[0] != activeName[0])
        {
            GameObject oppositeSeedHome = GameObject.Find($"{oppositeSeedName}Home");
            ChangePosition(oppositeSeedHome, oppositeSeed);
            StartCoroutine(audioManager.PlayAudio(2, 0));
            /**** change to differenct player ***/
            //instantiate the kick out effect
            GameObject effect = Instantiate(kickoutEffect, activeSeed.transform.position, Quaternion.Euler(Vector3.up));
            Destroy(effect, 1.5f);
            return true;
        }
        return false;
    }

    bool TestBeforeSwapUI(Transform cellTransform)
    {
        //used to track if swap has occured
        bool _worked = false;
        string cellName = cellTransform.name;
        string cellCoordinateXY = cellName[1..];

        //int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
        int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());
        //find the opposite cell to sidekick on

        //get the mid cell
        GameObject oppositeCell01 = O_1_Opposite(cellTransform, cordY);
        //do a mid cell to check if seeds are there
        Collider[] oppositeCell01colliders 
            = new MeinPhysics().ProjectOverlap(oppositeCell01.transform, seedLayerMask);

        //if no seed separates or in the middle of both seeds
        if (oppositeCell01colliders.Length == 0)
        {
            // get 0_2 cell
            GameObject oppositeCell02 = O_2_Opposite(cellTransform, cordY);
            // check if the 0_2 cell, contains seeds
            Collider[] colliders = new MeinPhysics().ProjectOverlap(oppositeCell02.transform, seedLayerMask);
          
            // if it contains seeds
            if (colliders.Length == 1)
            {
                bool isSeed = colliders[0].CompareTag("bseed") || colliders[0].CompareTag("yseed") || colliders[0].CompareTag("rseed") || colliders[0].CompareTag("gseed");
                if (isSeed && (colliders[0].name[0] != activePlayer[0]))
                {
                    //set IsAboutSideKick to true, this helps for automatic commit of the last dice number
                    IsAboutSideKick = true;

                    processOnGoing?.Invoke(true);
                    //play the announcer audio before kick out this is essential (fixed index btn 14-16)
                    audioManager.PlaySimpleAudio(UnityEngine.Random.Range(14, 17));

                    SwapUI(GameplayPanel, InteruptPanel);

                    _worked = true;
                }
                else
                {

                    //if the accumulated list is empty after, the 
                    if (accumulatedDices.Count == 0)
                    {
                        ChangeActivePlayer();
                        processOnGoing?.Invoke(false);
                    }
                }
            }
            else
            {

                //if the accumulated list is empty after, the 
                if (accumulatedDices.Count == 0)
                {
                    ChangeActivePlayer();
                    processOnGoing?.Invoke(false);
                }
            }
        }
        else
        {
            //if the accumulated list is empty after, the 
            if (accumulatedDices.Count == 0)
            {
                ChangeActivePlayer();
                processOnGoing?.Invoke(false);
            }
        }
        return _worked;
    }

    void SwapUI(GameObject current, GameObject next)
    {
        next.SetActive(true);
        current.SetActive(false);
    }

    bool SideKickOut(Transform cellTransform)
    {
        string cellName = cellTransform.name;
        string cellCoordinateXY = cellName[1..];

        //int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
        int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());
        //find the opposite cell to sidekick on
        GameObject oppositeCell = O_2_Opposite(cellTransform, cordY);

        Collider[] colliders = new MeinPhysics().ProjectOverlap(oppositeCell.transform, seedLayerMask);
        if (colliders.Length > 1) return false;
        PositionKickOut(colliders.ToList(), activeSeed);

        return false;
    }

    public void ShouldSideKick(bool delta)
    {
        if (delta)
        {
            if (_cellTransform != null)
            {
                IsAboutSideKick = false;
                SideKickOut(this._cellTransform);
                SwapUI(InteruptPanel, GameplayPanel);
                SetMoveDirection(true);
                //if (accumulatedDices.Count == 0)
                //{
                //    ChangeActivePlayer();
                //}
                if (accumulatedDices.Count == 1)
                {
                    print("forced to move");
                    SetToMove(accumulatedDices[^1]);
                }
            }
        }
        else
        {
            IsAboutSideKick = false;
            if (backwardToggle.isOn)
            {
                ResetToPreviousPostion(_specialOperation_init_position);
            }
            specialOperation = false;
            SwapUI(InteruptPanel, GameplayPanel);
            if (accumulatedDices.Count == 0)
            {
                ChangeActivePlayer();
            }
        }
        processOnGoing?.Invoke(false);
    }

    void ChangePosition(GameObject target, GameObject current)
    {
        if(target == null) return;
        if (current == null) return;
        current.transform.position = target.transform.position;
    }

    private bool ShouldTurnOver(int _movenumber)
    {
        if(activePlayer != null)
        {
            string cellCoordinateXY = "";
            List<bool> validSeeds = new();
            GameObject[] playerSeeds = GameObject.FindGameObjectsWithTag($"{activePlayer[0]}seed");
            bool outBool = false;
            foreach (GameObject seed in playerSeeds)
            {
                Transform transform = seed.transform;
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit))
                {
                    string tag = hit.transform.tag;
                    //look on to the board lane
                    if (!(tag.ToLower().Contains("home") || tag.Equals("tray")))
                    {
                        string cellName = hit.collider.gameObject.name;
                        //get the seed coordinates(x,y~z)
                        cellCoordinateXY = cellName[1..];
                        int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
                        int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());
                        //check if seed is in the middle lane
                        if (cordY == 1)
                        {
                            //start checking if seed is eligible to complete 
                            int midValue = cordX + 1;
                            //if moving starts for the first time;
                            // and positionValue | straightValue
                            // and this.movenNumber cancels each other out to 0?

                            //checking validity | when is true that means move is not good, skip.
                            bool _c = (midValue - _movenumber) < 0;
                            //print(_c + " mid " + midValue + " move " + _movenumber);
                            {
                                //print(midValue + " - " + _movenumber + " = " + (midValue - _movenumber));
                            }
                            validSeeds.Add(_c);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

            }
            outBool = helperUtils.AreAllTrue(validSeeds);
            return outBool;
        }
        return false;
    }

    private int SeedsOnFinishingLane()
    {
        string cellCoordinateXY = "";
        int count = 0;
        GameObject[] playerSeeds = GameObject.FindGameObjectsWithTag($"{activePlayer[0]}seed");
        foreach (GameObject seed in playerSeeds)
        {
            Transform transform = seed.transform;
            //activating flash to highlight active player seeds
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit))
            {
                string tag = hit.transform.tag;
                if (!(tag.ToLower().Contains("home") || tag.Equals("tray")))
                {
                    string cellName = hit.collider.gameObject.name;
                    //get the seed coordinates(x,y~z)
                    cellCoordinateXY = cellName[1..];
                    int cordX = helperUtils.StringToIntConverter(cellCoordinateXY[0].ToString());
                    int cordY = helperUtils.StringToIntConverter(cellCoordinateXY[1].ToString());
                    //check if seed is in the middle lane
                    if (cordY == 1)
                    {
                        count++;
                    }
                }
            }

        }
        return count;
    }

    private GameObject O_2_Opposite(Transform cellTransform, int cordY)
    {
        GameObject oppositeCell;
        if (cordY == 0)
        {
            string sideKickOpposite = cellTransform.name[..2] + 2;
            oppositeCell = GameObject.Find(sideKickOpposite);
        }
        else
        {
            string sideKickOpposite = cellTransform.name[..2] + 0;
            oppositeCell = GameObject.Find(sideKickOpposite);
        }
        return oppositeCell;
    }

    private GameObject O_1_Opposite(Transform cellTransform, int cordY)
    {
        string sideKickOpposite = cellTransform.name[..2] + 1;
        return GameObject.Find(sideKickOpposite);
    }

    public void SetMoveDirection(bool direction)
    {
        //get the directioin type: if is changing to clockwise
        if (direction && this.specialOperation) return;
        if (direction != goClockWise)
        {
            directionChangeEvent?.Invoke();
        }
        goClockWise = direction;
        
    }

    public void ForceEjectSeeds()
    {
        foreach (GameObject seed in seedList)
        {
            seed.transform.GetComponent<Rigidbody>().useGravity = false;
            Vector3 oldPosition = seed.transform.position;
            oldPosition.y = 0.5f;
            seed.transform.position = oldPosition;
            seed.transform.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    public void DoPause()
    {
        SwapUI(GameplayPanel, PauseMenu);
    }

    public void DoUnPause()
    {
        SwapUI(PauseMenu, GameplayPanel);
    }

}
