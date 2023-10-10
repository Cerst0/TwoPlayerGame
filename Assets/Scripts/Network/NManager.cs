using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NManager : NetworkManager // Use to ovveride Functions of the original NetworkMager script
{
    Status status;
    int currentId;

    private new void Start()
    {
        status = FindObjectOfType<Status>();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (FindObjectOfType<Methods>().stopListnening)
        {
            NetworkServer.RemoveConnection(conn.connectionId);
        }
        else
        {
            base.OnServerConnect(conn);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        string name = conn.identity.GetComponent<Sync>().playerName;
        int id = conn.identity.GetComponent<Sync>().playerID;

        GameObject.Find("NetworkPlayer0").GetComponent<Sync>().ServerNetworkNotification(Color.red, name, "disconnected", 4f, id);

        status.points[conn.identity.GetComponent<Sync>().playerID] = 0;
        currentId--;

        int missingID = conn.identity.playerID; //Updates Players IDs, so there isn't a gap in between all the id's;

        base.OnServerDisconnect(conn);

        foreach (NetworkIdentity nd in FindObjectsOfType<NetworkIdentity>())
        {
            if (nd.playerID > missingID)
            {
                print("Server update player id; new ID is: " + (nd.playerID - 1) + " old ID is: " + nd.playerID);
                nd.gameObject.GetComponent<Sync>().UpdatePlayerID(nd.playerID - 1);
            }
        }

        if (SceneManager.GetActiveScene().name != "Menu")
        {
            FindObjectOfType<Methods>().LoadMenu();
        }
        else
        {
            if (singleton.numPlayers == 1)
            {
                FindObjectOfType<PlayerCountSelector>().SkipPlayerCountSelection();
            }
        }
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        print("client disconnected");
        FindObjectOfType<NetworkManager>().StopClient();

        foreach (DontDestroyOnLoad DDOL in FindObjectsOfType<DontDestroyOnLoad>())
        {
            GameObject go = DDOL.gameObject.gameObject;
            Destroy(go);
        }

        string name = conn.identity.GetComponent<Sync>().playerName;

        GameObject obj = Instantiate(new GameObject());
        obj.name = name;
        obj.tag = "SelfDisconnect";
        DontDestroyOnLoad(obj);

        base.OnClientDisconnect(conn);

        FindObjectOfType<Methods>().LoadMenu();
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        print("New Client connecting; Server will assaign ID: " + currentId);

        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);

        // instantiating a "Player" prefab gives it the name "Player(clone)"
        // => appending the connectionId is WAY more useful for debugging!
        player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        player.GetComponent<NetworkIdentity>().playerID = currentId;

        NetworkServer.AddPlayerForConnection(conn, player, currentId);

        int i = conn.identity.playerID;
        StartCoroutine(OnConnect(i, .5f, true));

        currentId++;
    }

    public override void OnClientError(Exception exception)
    {
        FindObjectOfType<Notificaton>().Notification(Color.red, "NetworkError - Look at log", 5f);
        base.OnClientError(exception);
    }

    IEnumerator OnConnect(int id, float delayTime, bool onServer)
    {
        yield return new WaitForSeconds(delayTime);

        if (GameObject.Find("NetworkPlayer" + id) != null)
        {
            string name = GameObject.Find("NetworkPlayer" + id).GetComponent<Sync>().playerName;
            GameObject.Find("NetworkPlayer0").GetComponent<Sync>().ServerNetworkNotification(Color.green, name, "connected", 2f, id);
        }
        else
        {
            FindObjectOfType<Sync>().ServerNetworkNotification(Color.green, "new Player", "connected", 2f, id);
        }
    }
}