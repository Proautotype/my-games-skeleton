using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RPCTest : NetworkBehaviour
{
    public GameObject networkPlayerUI_prefab;
    private GameObject layoutGroup_Presence;
    GameObject ui_prefab;
    private Button changeUserNameBtn;
    private GameObject inputPlayerName;
    string _playerName = string.Empty;

    private NetworkVariable<PlayerData> playerData 
        = new NetworkVariable<PlayerData>(
                new PlayerData(){ playerName = "" },
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Owner
            );

    private Dictionary<ulong, string> playersNames = new Dictionary<ulong, string>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        layoutGroup_Presence = GameObject.FindGameObjectWithTag("NetworkPlayersPanel");
        changeUserNameBtn = GameObject.FindGameObjectWithTag("changeUserNameBtn").GetComponent<Button>();


        playerData.OnValueChanged += onPlayerDataChanged;

        //AddButtonEvent(changeUserNameBtn);
        if (IsClient && IsOwner )
        {
            inputPlayerName = GameObject.FindGameObjectWithTag("inputPlayerName");
            _playerName = inputPlayerName.GetComponent<TMP_InputField>().text;
            ChangeNameServerRpc(_playerName);
            playersNames.Add(this.NetworkObjectId, _playerName);
            //ChangeNameClientRpc(_playerName);
            //InstantiatePlayerUI(_playerName);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        Destroy(ui_prefab);
    }

    private void onPlayerDataChanged(PlayerData previousValue, PlayerData newValue)
    {
        //InstantiatePlayerUI($"{newValue.playerName}");
        //print($"Player Data {newValue.playerName} ");
        //InstantiatePlayerUI($"{newValue.playerName}");
    }

    private void InstantiatePlayerUI(string username)
    {
        ui_prefab = Instantiate(networkPlayerUI_prefab, layoutGroup_Presence.transform);
        //set the player name txt on the ui_prefab
        ui_prefab.GetComponent<PlayerTemplateScript>().playerName
                = $"{username}";


        ui_prefab.transform.SetParent(layoutGroup_Presence.transform, false);

        //ui_prefab.GetComponent<NetworkObject>().Spawn(true);

    }

    [ServerRpc]
    private void ChangeNameServerRpc(string playerName_)
    {
        print("server received this detail -> " +playerName_);
        //ChangeNameClientRpc(playersNames);
    }


    [ClientRpc]
    private void ChangeNameClientRpc(string playerName_)
    {
        print("client u copy -> " + playerName_);
    }

    struct PlayerData : INetworkSerializable
    {
        public FixedString4096Bytes playerName;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerName);
        }
    }

    public void AddButtonEvent(Button aBtn)
    {
        inputPlayerName = GameObject.FindGameObjectWithTag("inputPlayerName");
        _playerName = inputPlayerName.GetComponent<TMP_InputField>().text;
        aBtn.onClick.AddListener(() => {
            print("called");
            playerData.Value = new PlayerData()
            {
                playerName = _playerName
            };
        });
    }
}


