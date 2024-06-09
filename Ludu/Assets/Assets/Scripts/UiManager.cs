using Assets.Scripts;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Scrollbar;

public class UiManager : MonoBehaviour
{
    //managers
    //end of managers
    //[SerializeField] private TextMeshProUGUI rolledNumberText;
    //[SerializeField] private TextMeshProUGUI activePlayerText;
    [SerializeField] private GameObject activePlayerGui;
    //the board that displays the numbers that the user has rolled
    [SerializeField] private GameObject Numbers_Board;
    [SerializeField] private GameObject Rolled_Number;

    private Image ImageactivePlayerImage;

    [SerializeField] private Board boardManager;    
    [SerializeField] private List<MeshRenderer> barriers;
    private void Start()
    {
        ImageactivePlayerImage = activePlayerGui.GetComponent<Image>();

        GameManager.gmInstance.onActiveUser += SetActiveUser;
        boardManager.rollEvent += ChangeNumberUi;
        GameManager.gmInstance.acculatedDiceEvent += AlterNumbersBoard;
    }

    private void OnDestroy()
    {
        GameManager.gmInstance.onActiveUser -= SetActiveUser;
        boardManager.rollEvent -= ChangeNumberUi;
        GameManager.gmInstance.acculatedDiceEvent -= AlterNumbersBoard;
    }
    private void AlterNumbersBoard(AccumulatedListMessage accumulatedListMessage)
    {
        if (accumulatedListMessage.Polarity)
        {
            DisplayAccululatedList(accumulatedListMessage.Number);
        }
        else
        {
            RemoveSelectedDiceNumber(accumulatedListMessage.Number);
        }
    }

    private void ChangeNumberUi(int num)
    {
        //ClearBoard();
    }


    private void SetActiveUser(LuduPlayer luduPlayer)
    {
        ClearBoard();
        ImageactivePlayerImage.color = luduPlayer.GetMaterialColor;
        //activePlayerText.SetText(luduPlayer.Name);
        //clear the dice board
        foreach (MeshRenderer meshRenderer in barriers)
        {
            meshRenderer.material.color = luduPlayer.GetMaterialColor;
        }

    }

    private void ClearBoard()
    {
        int childCount = Numbers_Board.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(Numbers_Board.transform.GetChild(i).gameObject);
        }
    }

    public void DisplayAccululatedList(List<int> accumulatedDices)
    {

        for (int i = 0; i < accumulatedDices.Count; i--)
        {            //rolledNumberText.SetText(num == 0 ? "*" : num.ToString());
            GameObject rolledNumber = Instantiate(Rolled_Number, Numbers_Board.transform);
            rolledNumber.GetComponent<RolledNumberScript>().SetNumber(accumulatedDices[i]);
            rolledNumber.transform.SetParent(Numbers_Board.transform, false);
        }
    }

    public void DisplayAccululatedList(int number)
    {
        GameObject rolledNumber = Instantiate(Rolled_Number, Numbers_Board.transform);
        //print(rolledNumber);
        rolledNumber.GetComponent<RolledNumberScript>().SetNumber(number);
        rolledNumber.transform.SetParent(Numbers_Board.transform, false);
    }

    public void RemoveSelectedDiceNumber(int number)
    {
        for (int i = Numbers_Board.transform.childCount - 1; i >= 0; i--)
        {
            GameObject item = Numbers_Board.transform.GetChild(i).gameObject;
            if(item.GetComponent<RolledNumberScript>().number == number)
            {
                GameObject.Destroy(item);
                break;
            }

        }
    }
}