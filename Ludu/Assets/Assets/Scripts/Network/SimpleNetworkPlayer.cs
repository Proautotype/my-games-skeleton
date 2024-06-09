using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SimpleNetworkPlayer : NetworkBehaviour
{
    public GameObject networkPlayerUI_prefab;
    private GameObject layoutGroup_Presence;
    private GameObject inputPlayerName;
    private Button changeUserNameBtn;
    GameObject ui_prefab;

    string _playerName = string.Empty;

    [Tooltip("The name identifier used for this custom message handler.")]
    public string changeUserNameMessage = "changePlayerName";

    Dictionary<ulong, string> players = new Dictionary<ulong, string>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        layoutGroup_Presence = GameObject.FindGameObjectWithTag("NetworkPlayersPanel");
        inputPlayerName = GameObject.FindGameObjectWithTag("inputPlayerName");
        changeUserNameBtn = GameObject.FindGameObjectWithTag("changeUserNameBtn").GetComponent<Button>();

        AddButtonEvent(changeUserNameBtn);

        NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(changeUserNameMessage, ReceiveMessage);

        if(IsServer)
        {
            // Server broadcasts to all clients when a new client connects
            print($"is server owner {IsOwner}" );
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
        else
        {
            print($"is client owner {IsOwner}");
            _playerName = inputPlayerName.GetComponent<TMP_InputField>().text;
            SendMessage(NetworkObjectId, _playerName);
        }     

    }

    private void OnClientConnectedCallback(ulong clientId)
    {  
        players.Add(clientId, _playerName);       
    }

    private void SendMessage(ulong clientId, string userName)
    {
        var writer = new FastBufferWriter(1024, Allocator.Temp);
        using (writer)
        {
            writer.WriteValueSafe((FixedString128Bytes)userName);

            NetworkManager.CustomMessagingManager.SendNamedMessage(changeUserNameMessage, clientId, writer);
        }
    }

    /**
     * <summary>
     *  Invoked when a custom message of type <see cref="changeUserNameMessage"/>
     * </summary>
     */
    private void ReceiveMessage(ulong senderClientId, FastBufferReader messagePayload)
    {
        try
        {
            var receivedMessageContent = string.Empty;

            messagePayload.ReadValueSafe(out receivedMessageContent, true);

            print("client unique name " +  receivedMessageContent);

        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }

    private void InstantiatePlayerUI(string username)
    {
        ui_prefab = Instantiate(networkPlayerUI_prefab, layoutGroup_Presence.transform);
        //set the player name txt on the ui_prefab
        ui_prefab.GetComponent<PlayerTemplateScript>().playerName
                = $"{username}";
        
        ui_prefab.transform.SetParent(layoutGroup_Presence.transform, false);

    }

    void PrintAllClient()
    {
        print("players --> ");
        for (int i = 0; i < players.ToList().Count; i++)
        {
            print($"player {players.ToList()[i]}");
        }
    }
   
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        try
        {
            if (ui_prefab != null)
            {
                Destroy(ui_prefab);
            }
            NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(changeUserNameMessage);
            NetworkManager.OnClientDisconnectCallback -= OnClientConnectedCallback;
        }
        catch (Exception e)
        {
            print(e);
        }
    }

    public void AddButtonEvent(Button aBtn)
    {
        aBtn.onClick.AddListener(() => {
            _playerName = inputPlayerName.GetComponent<TMP_InputField>().text;
            SendMessage(NetworkObjectId, _playerName);
        });
    }

}
