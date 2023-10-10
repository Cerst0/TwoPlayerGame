using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IdDataApply : MonoBehaviour
{
    public int PlayerID;

    Mesh[] meshes = new Mesh[2];
    public int meshNumber;
    public string playerName;
    Functions functions;
    TMP_Text WorldSpacaName;
    public bool isMenu;
    public bool isSpectator;
    SkinnedMeshRenderer rend;
    public TMP_Text nameText;
    public Color PlayerColor;
    Image[] ScoreBoardImages = new Image[4];
    Status status;
    bool appliedTexts;
    bool isParty;
    bool doneSetup;

    private void Start()
    {
        Setup();
    }

    private void Setup(bool calledByDataApply = false)
    {
        status = FindObjectOfType<Status>();

        isParty = FindObjectOfType<Methods>().IsSceneParty();

        if (!isMenu && !isSpectator)
        {
            if (isParty)
            {
                PlayerID = transform.parent.parent.GetComponent<MovementParty>().playerID;
            }
            else
            {
                PlayerID = transform.parent.parent.GetComponent<Movement>().playerID;
            }
        }

        gameObject.tag = "IDA" + PlayerID;

        rend = GetComponent<SkinnedMeshRenderer>();

        if (meshes[0] == null)
        {
            meshes[0] = Resources.Load<Mesh>("BlueMesh");
            meshes[1] = Resources.Load<Mesh>("RedMesh");
        }

        if (!isMenu && !isSpectator && !isParty)
        {
            for (int i = 0; i < 4; i++)
            {
                ScoreBoardImages[i] = GameObject.Find("ScoreBoardColor" + i).GetComponent<Image>();
            }
        }

        if (isSpectator)
        {
            PlayerID = Random.Range(0, status.playerNumber);
        }

        if (PlayerID < status.playerNumber && !calledByDataApply) { IDDataApply(); }

        doneSetup = true;
    }

    private void OnLevelWasLoaded(int level)
    {
        appliedTexts = false;
    }

    public void IDDataApply()
    {
        if (!doneSetup) { Setup(true); }
        // CODES:
        // mesh = Player ID + "mesh"
        // color = Player ID + "colorExtra" + colorIndex + meshNumber
        // extra color = Player ID + "color" + colorIndex + meshNumber

        int id = PlayerID;
        if (NetworkClient.active && PlayerID == status.clientPlayerID) { id = 0; }

        bool network = NetworkClient.active && (status.clientPlayerID != PlayerID);

        if (NetworkClient.active && PlayerID >= status.playerNumber) { Destroy(this); }
        Sync sync = null;
        if (network)
        {
            sync = GameObject.FindGameObjectWithTag("Player" + PlayerID).GetComponent<Sync>();

            playerName = sync.playerName;
            meshNumber = sync.meshNumber;
            //print(gameObject.name + " with the ID: " + id + "With name: " + playerName + "With MeshNumber: " + meshNumber + "Got Data of sync: " + sync.gameObject.name);
        }
        else
        {
            playerName = GameDataManager.GetGameData().PlayerDatas[id].name;
            meshNumber = GameDataManager.GetGameData().PlayerDatas[id].mesh;
            //print(gameObject.name + " with the ID: " + id + "With name: " + playerName + "With MeshNumber: " + meshNumber + "Got Data of sync: " + "  NO NETWORK");
        }

        rend.sharedMesh = meshes[meshNumber]; // Applyes Mesh;

        Material[] defaultMaterials = FindObjectOfType<Methods>().GetDefaultMaterials(meshNumber); //Aplyes Materials with Colors
        GameDataManager.GameData GD = GameDataManager.GetGameData();

        rend.materials = defaultMaterials;


        for (int i = 0; i < rend.materials.Length; i++) // Applyes Colors
        {
            Color color = Color.magenta;

            if (network)
            {
                color = sync.currentMeshColors[i];
            }
            else
            {
                if (i <= 8)
                {
                    ColorUtility.TryParseHtmlString(GD.PlayerDatas[id].colors[i], out color);
                }
                else
                {
                    ColorUtility.TryParseHtmlString(GD.PlayerDatas[id].extraColors[meshNumber].colors[i - 9], out color);
                }
            }

            rend.materials[i].color = color;

            if (i == 0 && !isMenu)
            {
                if (isMenu) { PlayerColor = color; }
                else
                {
                    if (isSpectator)
                    {
                        try { transform.parent.GetChild(0).GetChild(3).GetComponentInChildren<SkinnedMeshRenderer>().material.color = color; }
                        catch { }
                    }
                    PlayerColor = color;
                    try
                    {
                        if (PlayerColor != null && !isSpectator)
                        {
                            if (status.playerNumber == 1)
                            {
                                foreach (Image image in ScoreBoardImages)
                                {
                                    image.color = PlayerColor;
                                }
                            }
                            if (status.playerNumber == 2)
                            {
                                ScoreBoardImages[PlayerID * 2].color = PlayerColor;
                                ScoreBoardImages[PlayerID * 2 + 1].color = PlayerColor;
                            }
                            if (status.playerNumber == 3)
                            {
                                ScoreBoardImages[PlayerID].color = PlayerColor;
                                if (PlayerID == 2) { ScoreBoardImages[3].color = PlayerColor; }
                            }
                            if (status.playerNumber == 4)
                            {
                                ScoreBoardImages[PlayerID].color = PlayerColor;
                            }
                        }
                    }
                    catch { }
                }
            }
        }
    }

    private void Update()
    {
        if (!isMenu && !isSpectator)
        {
            bool namesLength = true;

            if (functions == null && !isParty)
            {
                functions = GameObject.FindGameObjectWithTag("Brain").GetComponent<Functions>();
                namesLength = functions.PlayerNames.Length != 0;
            }

            if (!appliedTexts && namesLength)
            {
                ApplyTexts();
            }
        }
    }

    private void ApplyTexts()
    {
        try { WorldSpacaName = GameObject.Find("NameWorldSpace" + PlayerID).GetComponent<TMP_Text>(); WorldSpacaName.text = playerName; }
        catch { }

        if (!isParty)
        {
            if (status.playerNumber > 2 && (PlayerID == 1 || PlayerID == 3))
            {
                float i = .5f * (PlayerID - 1); //Math xD
                nameText = functions.PlayerNames[(int)i].GetComponent<TMP_Text>();
                nameText.text = GameObject.FindGameObjectWithTag("IDA" + (PlayerID - 1).ToString()).GetComponent<IdDataApply>().playerName + " <color=#ffff>|</color> " + playerName;
            }
            if (status.playerNumber <= 2 && PlayerID <= 2)
            {
                nameText = functions.PlayerNames[PlayerID].GetComponent<TMP_Text>();
                nameText.text = playerName;
            }
            if (status.playerNumber == 3 && PlayerID == 2)
            {
                nameText = functions.PlayerNames[1].GetComponent<TMP_Text>();
                nameText.text = playerName;
            }
        }

        appliedTexts = true;
    }
}
