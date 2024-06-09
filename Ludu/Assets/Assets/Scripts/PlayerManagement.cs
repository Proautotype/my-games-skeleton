using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManagement : MonoBehaviour
{
    private int NOP = 0;    

    [SerializeField]
    private ToggleGroup toggleNumberOfPlayers;

    [SerializeField] private GameObject PlayersSelectionBoard;
    [SerializeField] private GameObject PlayerEditBoard;
    [SerializeField] private GameObject PlayerTemplate;

    [SerializeField] private GameObject GameControlsUI;
    [SerializeField] private GameObject SelectionUI;

    Animator animator;

    public void GotoEditPlayer()
    {
        StartCoroutine(SwapUI(PlayerEditBoard, PlayersSelectionBoard));
        Toggle toggle =  GetToggleActivity(toggleNumberOfPlayers);
        if (toggle != null)
        {
            NOP = new HelperUtils().StringToIntConverter(toggle.name[1].ToString());
        }
    }
    
    public void GotoPlayersSelection()
    {
        StartCoroutine(SwapUI(PlayersSelectionBoard, PlayerEditBoard));
    }

    public void SeedPlayers()
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        GameManager.gmInstance.SeedPlayers(StripDetailsFromListView(PlayerEditBoard.transform.GetChild(0).gameObject.transform));
        animator = PlayerEditBoard.GetComponent<Animator>();
        animator.Play("exit");
        yield return new WaitForSeconds(1f);
        GameControlsUI.SetActive(true);
        yield return new WaitForEndOfFrame();
        SelectionUI.SetActive(false);
    }

    IEnumerator SwapUI(GameObject next, GameObject current)
    {        
        current.SetActive(true);
        animator = current.GetComponent<Animator>();
        animator.Play("exit");
        yield return new WaitForSeconds(2);
        next.SetActive(true);
        animator = next.GetComponent<Animator>();
        animator.Play("enter");
        current.SetActive(false);

        if (next.CompareTag("playersEdit"))
        {
            GameObject listView = next.transform.GetChild(0).gameObject;
            GameObject listItem;
            for (int i  = 0 ; i < NOP; i++)
            {
                string defaultPlayerName = "Player " + (i + 1);
                listItem = Instantiate(PlayerTemplate, listView.transform);
                Transform listItemTransform = listItem.transform;

                //get each color toggle turn it on base on the current index of the prob; by this variant color is assigned to different players on start
                GameObject colorTog = listItemTransform.GetChild(0).transform.GetChild(i).gameObject;
                colorTog.GetComponent<Toggle>().isOn = true;

                Toggle colorToggle = listItemTransform.GetChild(0).GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();                
                //Toggle playerTypeToggle = listItemTransform.GetChild(2).GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();                
                TextMeshProUGUI playerName = listItemTransform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
                //set player name text
                playerName.SetText(defaultPlayerName);

                GameObject inputPanel = listItemTransform.GetChild(2).gameObject;
                TMP_InputField inputFieldGO = inputPanel.transform.GetChild(0).gameObject.GetComponent<TMP_InputField>();
                inputFieldGO.onValueChanged.AddListener(e =>
                {
                    playerName.SetText(e);
                    if(e.Equals(""))
                    {
                        playerName.SetText(defaultPlayerName);
                    }
                });
                //used to regulate the rate players are displayed before seeded into game
                yield return new WaitForSeconds(0.5f);
            }
        }

        if (current.CompareTag("playersEdit"))
        {
            Transform listView = current.transform.GetChild(0).gameObject.transform;
            ClearListView(listView);
        }
    }

    public void ClearListView(Transform listView)
    {
        int childCount = listView.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = listView.GetChild(i).gameObject;
            Destroy(child);
        }
    }

    public List<LuduPlayer> StripDetailsFromListView(Transform listView)
    {
        int childCount = listView.childCount;
        List<LuduPlayer> playersList = new();
        for (int i = 0; i < childCount; i++)
        {
            GameObject listItem = listView.GetChild(i).gameObject;
            Transform listItemTransform = listItem.transform;

            //used to get the current color
            GameObject colorToggleGO = listItemTransform.GetChild(0).gameObject;
            GameObject background = colorToggleGO.transform.GetChild(0).gameObject;
            //print(background.transform.GetChild(0).gameObject.GetComponent<Image>().material.color);
             Toggle colorToggle = colorToggleGO.GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
            //Toggle playerTypeToggle = listItemTransform.GetChild(2).GetComponent<ToggleGroup>().ActiveToggles().FirstOrDefault();
            string playerName = listItemTransform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text;
           
            LuduPlayer player = new()
            {
                Active = false,
                Type = "Human", //should change in the future
                Color = colorToggle.name,
                //Color = background.GetComponent<Image>().material.color.ToHexString(),
                Name = playerName
            };

            playersList.Add(player);
        }
        return playersList;
    }

    public Toggle GetToggleActivity(ToggleGroup activity)
    {
        return activity.ActiveToggles().FirstOrDefault();
    }

}
