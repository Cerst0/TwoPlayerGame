using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerCountManager : MonoBehaviour
{
    Status status;
    public GameObject[] Players = new GameObject[4];
    GameObject[] UIPlayers = new GameObject[4];

    bool isMenu;
    bool isParty;
    int playerNum;
    int activePlayerNum;
    int activeUIPlayerNum;
    string SceneNameB;

    // Start is called before the first frame update
    void Start()
    {
        status = GameObject.FindGameObjectWithTag("Status").GetComponent<Status>();
        RotateToLight();
        isMenu = !FindObjectOfType<Methods>().IsSceneGame();
        isParty = FindObjectOfType<Methods>().IsSceneParty();
    }
    private void OnLevelWasLoaded(int level)
    {
        isMenu = !FindObjectOfType<Methods>().IsSceneGame();
        isParty = FindObjectOfType<Methods>().IsSceneParty();

        foreach (GameObject go in Players)
        {
            go.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        }

        RotateToLight();
    }



    void Update()
    {
        if (!isParty)
        {
            playerNum = status.playerNumber;

            bool menuEnabled = false;

            if (isMenu)
            {
                menuEnabled = FindObjectOfType<Menu>().enabled;
            }

            if (Players[0] == null)
            {
                for (int i = 0; i < 4; i++)
                {
                    Players[i] = transform.GetChild(i).gameObject;
                    Players[i].SetActive(false);
                }
            }
            if (UIPlayers[0] == null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (menuEnabled) { UIPlayers[i] = GameObject.FindGameObjectWithTag("PlayerLayout").transform.GetChild(i).gameObject; }
                    if (!isMenu)
                    {
                        foreach (GameObject go in FindObjectOfType<Functions>().PlayerUIs)
                        {
                            go.SetActive(true);
                        }
                        UIPlayers[i] = GameObject.Find("PlayerUI" + i);
                        foreach (GameObject go in FindObjectOfType<Functions>().PlayerUIs)
                        {
                            go.SetActive(false);
                        }
                    }
                }
                if (!isMenu)
                {
                    if (status.playerNumber == 2)
                    {
                        //Change UIPlayers order
                        (UIPlayers[2], UIPlayers[1]) = (UIPlayers[1], UIPlayers[2]);
                        UIPlayers[1].transform.GetComponentInChildren<UniversalButton>().buttonName = "Player1";
                        UIPlayers[1].transform.GetComponentInChildren<RawImage>().texture = UIPlayers[2].transform.GetComponentInChildren<RawImage>().texture;
                    }
                }
                activeUIPlayerNum = UIPlayers.Length;
            }

            if (SceneManager.GetActiveScene().name != SceneNameB)
            {
                UpdatePlayerCount();
                SceneNameB = SceneManager.GetActiveScene().name;
            }

            if (activePlayerNum != playerNum | (activePlayerNum != activeUIPlayerNum) && (menuEnabled | !isMenu))
            {
                UpdatePlayerCount();
            }
        }
    }
    private void UpdatePlayerCount()
    {
        activePlayerNum = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i > playerNum - 1) { Players[i].SetActive(false); }
            else { Players[i].SetActive(true); activePlayerNum++; }
        }

        if (UIPlayers[0] != null)
        {
            activeUIPlayerNum = 0;
            for (int i = 0; i < 4; i++)
            {
                if (i > playerNum - 1) { UIPlayers[i].SetActive(false); }
                else { UIPlayers[i].SetActive(true); activeUIPlayerNum++; }
            }
        }

        if (!isMenu)
        {
            if (status.playerNumber == 1)
            {
                FindObjectOfType<Scoreboard>().SetScoreboardSingleScore();
            }

            if (status.playerNumber > 2)
            {
                FindObjectOfType<Scoreboard>().SetScoreboardMultiScore();
            }

            if (status.playerNumber == 1)
            {
                GameObject.Find("Score0").SetActive(true);
                GameObject.Find("Score1").SetActive(false);
                GameObject.Find("Score2").SetActive(false);
                GameObject.Find("Score3").SetActive(false);
            }

            if (status.playerNumber == 2)
            {
                GameObject.Find("Score0").SetActive(false);
                GameObject.Find("Score1").SetActive(true);
                GameObject.Find("Score2").SetActive(true);
                GameObject.Find("Score3").SetActive(false);
            }

            if (status.playerNumber == 3)
            {
                GameObject.Find("Score0").SetActive(true);
                GameObject.Find("Score1").SetActive(true);
                GameObject.Find("Score2").SetActive(true);
                GameObject.Find("Score3").SetActive(false);
            }
        }
        print("updated PlayerCount to UI and 3DView");
    }
    private void RotateToLight()
    {
        transform.rotation = GameObject.Find("Directional Light").transform.rotation;
        transform.Rotate(Vector3.up, 180);
    }
}
