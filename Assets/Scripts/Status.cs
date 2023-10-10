#if UNITY_EDITOR
using UnityEditor;
#endif
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Status : MonoBehaviour
{
    public Functions functions;
    public Sync sync;

    public string buttonName;
    public bool scoredPoint;
    public bool scoredScore;
    public bool countdown;
    public bool enteredMenu;
    public bool useGamepads;
    public bool movementLock;
    public bool isParty;
    public bool isSceneGame;
    public bool isSceneParty;
    public int inputViewingType;
    public int SceneEnters;
    public int gameIndex;
    public int clientPlayerID = -1;
    public int[] points = new int[4];
    public int playerNumber = 2;

    public string buttonNamePreview;

    private void Start()
    {
        StartSetup();
    }

    private void OnLevelWasLoaded(int level)
    {
        StartSetup();

        SceneEnters++;
    }

    private void StartSetup()
    {
#if UNITY_EDITOR
        Debug.Log("<color=green>Loaded Scene succesfully</color>");
        if (EditorSettings.enterPlayModeOptionsEnabled)
        {
            Debug.LogError("Enter Play Mode Options are not supported by Mirror. Please disable 'ProjectSettings -> Editor -> Enter Play Mode Settings (Experimental)'.");
            //EditorApplication.isPlaying = false;
        }
#endif
        isParty = FindObjectOfType<Party>().isParty;
        isSceneGame = FindObjectOfType<Methods>().IsSceneGame();
        isSceneParty = FindObjectOfType<Methods>().IsSceneParty();

        if (isSceneGame)
        {
            functions = FindObjectOfType<Functions>();
        }


        if (NetworkClient.active)
        {
            GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>();
            sync = GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>();
        }

        FunctionVars();
    }

    private void LateUpdate()
    {
        FunctionVars();

        if (NetworkClient.active)
        {
            try
            {
                playerNumber = GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>().playersConnected;
            }
            catch { playerNumber = 1; }
        }


        buttonName = string.Empty;
        if (buttonNamePreview != string.Empty)
        {
            buttonName = buttonNamePreview;
            buttonNamePreview = string.Empty;
        }
    }

    private void FunctionVars()
    {
        if (isSceneGame)
        {
            movementLock = functions.movementLock;
            scoredPoint = functions.scoredPoint;
            scoredScore = functions.scoredScore;
            countdown = !functions.countdownFinished;
        }
    }
}
