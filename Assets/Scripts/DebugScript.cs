using UnityEngine;
using TMPro;

public class DebugScript : MonoBehaviour
{
    float FPS;
    float FPSB;

    private void Update()
    {
        FPS = 1 / Time.deltaTime;
        FPS = Mathf.Lerp(FPS, FPSB, .99f);

        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = Mathf.RoundToInt(FPS).ToString();

        FPSB = FPS;
    }

    private void OnLevelWasLoaded(int level)
    {
        FPS = 60;
    }

    public void OnReloadButtonPressed()
    {
        try
        {
            FindObjectOfType<Functions>().PlayAgain();

        }
        catch { Debug.LogError("DEBUG: Couldn't reload scene!"); }
    }

    public void OnLogButtonPressed()
    {
        GetComponent<ConsoleInBuild>().enabled = !GetComponent<ConsoleInBuild>().enabled;
    }

    public void OnFPSButtonPressed()
    {
        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
    }
}
