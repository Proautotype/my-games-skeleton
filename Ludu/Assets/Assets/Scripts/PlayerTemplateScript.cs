using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerTemplateScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI txtPlayerName;

    public ulong networkId { get; set; } = new ulong();

    public string playerName { 
        get => txtPlayerName.text;
        set => txtPlayerName.text = value;
    }


}
