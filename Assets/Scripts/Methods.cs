using Mirror;
using System;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Methods : MonoBehaviour
{
    public Color Gold;
    public Color Silver;
    public Color Bronze;
    public Color LastPlace;

    Status status;

    private void Start()
    {
        status = FindObjectOfType<Status>();
    }

    public bool stopListnening;

    public void SetPointsColorOrder(TMP_Text[] texts, bool WithScoreAnimation = false)
    {
        status = FindObjectOfType<Status>();

        int[] points = new int[4];

        if (NetworkClient.active)
        {
            for (int i = 0; i < 4; i++)
            {
                points[i] = status.points[i];
            }
        }
        else
        {
            if (status.playerNumber == 1)
            {
                points = new int[] { PlayerPrefs.GetInt("SinglePlayerPoint0"), 0, 0, 0 };
            }
            else
            {
                points = new int[] { PlayerPrefs.GetInt("Point0"), PlayerPrefs.GetInt("Point1"), 0, 0 };
            }
        }

        if (WithScoreAnimation)
        {
            for (int i = 0; i < status.playerNumber; i++)
            {
                points[i] = Convert.ToInt16(FindObjectOfType<Functions>().PlayerPoints[i].GetComponent<TMP_Text>().text);
            }
        }

        for (int i = 0; i < status.playerNumber; i++)
        {
            texts[i].color = GetPointsOrder(points)[i];
            if (!WithScoreAnimation)
            {
                texts[i].text = points[i].ToString();
            }
        }
    }

    public Color[] GetPointsOrder(int[] points)
    {
        Color[] colors = { Gold, Silver, Bronze, LastPlace };
        Color[] rankedColors = new Color[4];
        int[] sorted;
        int[] ranks;
        int currentRank;
        int currentValue;
        string[] pointsString;

        sorted = new int[] { points[0], points[1], points[2], points[3], };
        Array.Sort(sorted);
        Array.Reverse(sorted);
        currentRank = 1;
        currentValue = sorted[0];
        ranks = new int[status.playerNumber];
        pointsString = new string[status.playerNumber];

        for (int i = 0; i < status.playerNumber; i++)
        {
            string s = i.ToString() + points[i].ToString();
            pointsString[i] = s;
        }

        for (int i = 0; i < status.playerNumber; i++)
        {
            bool[] scoreIsAtIndex = new bool[status.playerNumber];

            for (int i2 = 0; i2 < status.playerNumber; i2++)
            {
                string s1 = pointsString[i2].ToString();
                string s2 = sorted[i].ToString();
                char c1 = s1[1];
                char c2 = s2[0];
                if (c1 == c2)
                {
                    scoreIsAtIndex[i2] = true;
                }
            }
            if (sorted[i] != currentValue)
            {
                currentRank++;
            }

            for (int i3 = 0; i3 < status.playerNumber; i3++)
            {
                if (scoreIsAtIndex[i3]) { ranks[i3] = currentRank; }
            }
            currentValue = sorted[i];
        }

        if (points.All(isNull => isNull == 0))
        {
            for (int i = 0; i < status.playerNumber; i++)
            {
                ranks[i] = 4;
            }
        }

        for (int i = 0; i < status.playerNumber; i++)
        {
            int index = ranks[i];
            index += -1;
            rankedColors[i] = colors[index];
        }

        return rankedColors;
    }

    public void SetServerListnening(bool state)
    {
        stopListnening = !state;
    }

    public bool IsSceneGame()
    {
        return FindObjectOfType<Functions>() != null;
    }

    public bool IsSceneParty()
    {
        return SceneManager.GetActiveScene().name == "Party";
    }

    public void SetPSColorFromPlayerColor(ParticleSystem ps, int playerID, bool includeRandomDestroy = false)//dont run in menu
    {
        if (includeRandomDestroy)
        {
            int i = UnityEngine.Random.Range(0, FindObjectOfType<Quality>().psLODMaxRandomNumber());
            if (i != 0) { return; }
        }

        var main = ps.main;
        Gradient gr = new();
        GradientColorKey[] keys = new GradientColorKey[3];
        GradientAlphaKey[] aKeys = new GradientAlphaKey[1];

        Color.RGBToHSV(GetPlayerColor(playerID), out float H, out float S, out float V);

        H += -0.05f;
        if (H < 0) { H += 1; }
        keys[0].color = Color.HSVToRGB(H, S, V);
        keys[0].time = 0f;
        keys[1].color = GetPlayerColor(playerID);
        keys[1].time = .5f;
        H += 0.1f;
        if (H > 1) { H += -1; }
        keys[2].color = Color.HSVToRGB(H, S, V);
        keys[2].time = 1f;
        aKeys[0].alpha = 1;

        gr.SetKeys(keys, aKeys);
        main.startColor = gr;

        ps.Clear();
        ps.Play();
    }

    public void SetScoreOnAllOpponents(int team, int maxScoreCount)
    {
        Movement[] movements = FindObjectsOfType<Movement>();
        foreach (Movement move in movements)
        {
            if (team != move.team)
            {
                Functions functions = FindObjectOfType<Functions>();
                int scoreCount = maxScoreCount - functions.scores[move.playerID];

                if (status.clientPlayerID < 1) { functions.AddScore(0, maxScoreCount, move.playerID, false, false, scoreCount); }
                else { status.sync.AddScore(0, maxScoreCount, move.playerID, false, scoreCount); }
            }
        }
    }

    public void SetHint(bool acivate, string hintName, int[] keys, int playerID, bool[] players = null, bool animation = true, float time = -1) //assign either Player Array or single target Player ID
    {
        if (players == null)
        {
            players = new bool[4];
            for (int i1 = 0; i1 < 4; i1++)
            {
                if (playerID == i1) { players[i1] = true; }
            }
        }

        for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
        {
            if (players[playerIndex])
            {
                int hintIndex = playerIndex;
                if (NetworkClient.active) { hintIndex = 0; }
                Hint hint = GameObject.Find("Hint" + hintIndex).GetComponent<Hint>();

                int[] currentKeys = keys;

                if (acivate)
                {
                    if (NetworkClient.active)
                    {
                        if (status.clientPlayerID == playerIndex)
                        {
                            hint.ActivateHint(hintName, keys, animation, time);
                        }
                    }
                    else
                    {
                        if (playerIndex == 1)
                        {
                            for (int i2 = 0; i2 < currentKeys.Length; i2++) { currentKeys[i2] += 4; }
                        }

                        hint.ActivateHint(hintName, currentKeys, animation, time);
                    }
                }
                else { hint.DeactivateHint(animation); }
            }
        }
    }

    public void SetGameData(GameDataManager.GameData gameData)
    {
        FindObjectOfType<GameDataManager>().WriteGameData(gameData);
    }

    public Material[] GetDefaultMaterials(int meshIndex)
    {
        Material[] DefaultMaterials = Resources.LoadAll<Material>("DefaultCharacterMaterials");
        Material[] ExtraDefaultMaterials = Resources.LoadAll<Material>("ExtraMat" + meshIndex);
        DefaultMaterials = DefaultMaterials.Concat(ExtraDefaultMaterials).ToArray();

        return DefaultMaterials;
    }

    public Color GetPlayerColor(int id)
    {
        Color color = Color.magenta;

        if (NetworkClient.active)
        {
            color = GameObject.Find("NetworkPlayer" + id).GetComponent<Sync>().currentMeshColors[0];
        }
        else
        {
            if (ColorUtility.TryParseHtmlString(GameDataManager.GetGameData().PlayerDatas[id].colors[0], out color)) { }
            else { print("Color not found " + id); }
        }

        return color;
    }

    public string GetPlayerName(int id) //ONLY USE FOR LOCAL PLAYER!
    {
        string name = "ErrorGetName";

        if (NetworkClient.active)
        {
            name = GameObject.Find("NetworkPlayer" + id).GetComponent<Sync>().playerName;
        }
        else
        {
            name = GameDataManager.GetGameData().PlayerDatas[id].name;
        }

        return name;
    }

    public GameObject FindInactiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }

    public void DeleteAllNetworkPlayers()
    {
        if (NetworkClient.active)
        {
            foreach (Sync sync in FindObjectsOfType<Sync>())
            {
                if (sync.transform.GetChild(0) != null)
                {
                    Destroy(sync.transform.GetChild(0).gameObject);
                }
            }
        }
    }

    public void PlayNetworkedPs(string psName, bool setStartColor, Color startColor, Gradient startGradient = null, bool syncPos = false)
    {
        if (GameObject.Find(psName) != null)
        {
            ParticleSystem ps = GameObject.Find(psName).GetComponent<ParticleSystem>();
            var main = ps.main;

            if (setStartColor)
            {
                main.startColor = startColor;
            }
            if (startGradient != null) //Transfer a Gradient to clients isn't possible so it pics the middle color of the gradient and sets it as startColor
            {
                main.startColor = startGradient;
                int colorKeysNum = startGradient.colorKeys.Length;
                startColor = startGradient.colorKeys[(int)Mathf.Round(colorKeysNum / 2)].color;
            }

            if (NetworkClient.active)
            {
                status.sync.PlayPsOnClients(psName, setStartColor, startColor);
                if (syncPos)
                {
                    status.sync.SetGameObjectPos(psName, ps.transform.position);
                }
            }
            else
            {
                ps.Clear();
                ps.Play();
            }
        }
        else
        {
            Debug.LogWarning("NoPsFound");
            return;
        }
    }

    public void LoadMenu()
    {
        Time.timeScale = 1;
        status.gameIndex = -1;
        FindObjectOfType<LoadingScreen>().SetLoadingScreen(-1);
        FindObjectOfType<Party>().ResetParty();

        if (NetworkClient.active)
        {
            Sync sync = GameObject.FindGameObjectWithTag("PlayerInstance").GetComponent<Sync>();
            sync.LoadScene(0, true, -1);
        }
        else { SceneManager.LoadSceneAsync(0); }
    }
}
public static class Extensions
{
    public static int FindIndex<T>(this T[] array, T item)
    {
        return Array.FindIndex(array, val => val.Equals(item));
    }
}
