using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SimpleNetworkManager : MonoBehaviour
{    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void LeaveGame()
    {
        NetworkManager.Singleton.Shutdown();
    }


}
