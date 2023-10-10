
using System.IO;
using UnityEngine;

// EDITOR: https://jsonformatter.org/json-editor/

public class GameDataManager : MonoBehaviour
{
    private static void CreateGameDataFile()
    {
        string file = Resources.Load<TextAsset>("defaultGamedata").text;
        string path = Application.persistentDataPath + "/gamedata.json";

        File.WriteAllText(path, file);
        print("gamedata.json was saved to: " + Application.persistentDataPath + "/gamedata.json");
    }

    public static GameData GetGameData()
    {
        if (File.Exists(Application.persistentDataPath + "/gamedata.json"))
        {
            return ReadGameData();
        }
        else
        {
            Debug.LogWarning("Couldn't find gamedata.json file in AppData. Now creating default");

            CreateGameDataFile();
            return ReadGameData();
        }
    }

    private static GameData ReadGameData()
    {
        string json = File.ReadAllText(Application.persistentDataPath + "/gamedata.json");
        GameData GD = JsonUtility.FromJson<GameData>(json);
        return GD;
    }

    public void WriteGameData(GameData gameData)
    {
        string newGameData = JsonUtility.ToJson(gameData);
        File.WriteAllText(Application.persistentDataPath + "/gamedata.json", newGameData);
    }

    [System.Serializable]
    public class GameData
    {
        public PlayerData[] PlayerDatas = new PlayerData[2];
        public Mesh[] meshes = new Mesh[2];
        public SettingsData settingsData;
    }

    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public int mesh;

        public string[] colors = new string[9];

        public ExtraColors[] extraColors = new ExtraColors[2];

        [System.Serializable]
        public class ExtraColors
        {
            public string[] colors = new string[3];
        }
    }

    [System.Serializable]
    public class Mesh
    {
        public string[] Defaultcolors = new string[12];
    }


    [System.Serializable]
    public class SettingsData
    {
        public float musicVolume;
        public float effectsVolume;

        public float windowMode;
        public float shadowDistance;
        public float antiAliasing;
        public float textureRes;
        public float natureLOD;
        public float psLOD;
        public float standLOD;
    }
}