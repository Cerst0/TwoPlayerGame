using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HighScore : MonoBehaviour
{
    Status status;
    InputS input;

    TMP_Text[] texts;
    GameObject ps;
    GameObject UIPs;

    public int levelID = -1;
    int levelIDB = -1;
    float highScore;
    float currentScore;
    string template = "<b>second</b><size=70%>.mili";
    bool isGame;
    bool isNewHighScore;
    bool finishedNewHighScore;

    private void Start()
    {
        status = FindObjectOfType<Status>();
        input = FindObjectOfType<InputS>();
    }

    private void OnLevelWasLoaded(int level)
    {
        isGame = FindObjectOfType<Methods>().IsSceneGame();

        currentScore = 0;
        finishedNewHighScore = false;
        isNewHighScore = false;

        if (isGame)
        {
            if (status.playerNumber == 1)
            {
                GameObject.Find("Name1").transform.GetChild(0).gameObject.SetActive(true);
                GameObject.Find("Name1").transform.GetChild(1).gameObject.SetActive(true);

                GameObject.Find("Name1").GetComponent<TMP_Text>().text = string.Empty;

                texts = new TMP_Text[2];
                texts[0] = GameObject.Find("Name1").transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
                texts[1] = GameObject.Find("Name1").transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>();

                ps = GameObject.Find("Name1").transform.GetChild(2).gameObject;
                UIPs = GameObject.Find("PlayerNames").transform.GetChild(2).GetChild(0).gameObject;

                highScore = PlayerPrefs.GetFloat("highscore" + status.gameIndex + "level" + levelID);
            }
            else
            {
                GameObject.Find("Name1").transform.GetChild(0).gameObject.SetActive(false);
                GameObject.Find("Name1").transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (isGame && status.playerNumber == 1)
        {
            if (levelID != levelIDB)
            {
                highScore = PlayerPrefs.GetFloat("highscore" + status.gameIndex + "level" + levelID);
                levelIDB = levelID;
            }

            if (!status.countdown && !status.scoredPoint && !status.scoredScore)
            {
                currentScore += Time.deltaTime;

                if (currentScore > highScore && highScore != 0 && texts[1].color == Color.white)
                {
                    LeanTween.value(texts[1].gameObject, texts[1].color, new Color(1, .4f, .4f), .25f).setEaseInOutQuad().setOnUpdate((Color col) =>
                    {
                        texts[1].color = col;
                    });
                }
            }
            if (status.countdown)
            {
                currentScore = 0;
            }
            if (status.scoredPoint && (currentScore < highScore || highScore == 0))
            {
                isNewHighScore = true;

                if (!finishedNewHighScore)
                {
                    NewHighScore();
                    finishedNewHighScore = true;
                }
            }

            texts[0].text = ConstructString(highScore, true);
            texts[1].text = ConstructString(currentScore);
        }
    }

    void NewHighScore()
    {
        PlayerPrefs.SetFloat("highscore" + status.gameIndex + "level" + levelID, currentScore);

        input.ClickEvent += (s, args) =>
        {
            Click();
        };

        for (int i = 0; i < 5; i++)
        {
            Vector2 vector = new(UnityEngine.Random.Range(Screen.width * .75f, Screen.width), UnityEngine.Random.Range(Screen.height * .85f, Screen.height));
            SpawnConfetti(vector);
        }

        LeanTween.value(gameObject, highScore, currentScore, 10).setEaseOutQuart().setOnUpdate((float val) =>
        {
            highScore = val;
        });

        float oldHighScore = highScore;

        LeanTween.value(gameObject, highScore, currentScore, 10).setEaseOutQuart().setOnUpdate((float val) =>
        {
            highScore = val;
        });

        texts[1].transform.parent.Find("Description").GetComponent<TMP_Text>().text = "Old High Score";
        currentScore = oldHighScore;

        FindObjectOfType<Notificaton>().Notification(new Color(1, 0.5884774f, 0), "New High Score!", 3f);
    }

    public void Click()
    {
        if (isNewHighScore)
        {
            Vector2 vector = Mouse.current.position.ReadValue();
            SpawnConfetti(vector);
        }
    }

    private void SpawnConfetti(Vector2 vector)
    {
        if (GameObject.Find("Name1").transform.childCount > 25) { return; }
        UIPs.GetComponent<RectTransform>().position = vector;
        ps.transform.GetChild(1).GetComponent<ParticleSystem>().Play();

        Transform UIPsParent = UIPs.transform.parent;
        UIPs = Instantiate(UIPs);
        UIPs.transform.parent = UIPsParent;

        Transform psParent = ps.transform.parent;
        ps = Instantiate(ps);
        ps.transform.parent = psParent;

        RenderTexture texture = ps.transform.GetChild(0).GetComponent<Camera>().targetTexture;
        RenderTexture newTexture = new(texture);
        ps.transform.GetChild(0).GetComponent<Camera>().targetTexture = newTexture;
        UIPs.GetComponent<RawImage>().texture = newTexture;
    }

    string ConstructString(float score, bool isHighScore = false)
    {
        string s = template;


        s = s.Replace("second", Mathf.Floor(score).ToString());
        s = s.Replace("mili", GetMs(score));

        if ((isHighScore || isNewHighScore) && score == 0)
        {
            return "<b><color=green>X</b><size=70%>.x";
        }
        else
        {
            return s;
        }
    }

    string GetMs(float score)
    {
        float f = score % 1;
        f = (float)Math.Round(f, 2);
        f *= 100;

        string s = f.ToString();
        if (s.Length == 1) { s = s.Insert(0, "0"); }
        if (s.Length == 3) { s = "99"; }

        return s;
    }

    public void ResetHighScores()
    {
        for (int i = 0; i < 100; i++)
        {
            for (int level = -1; level < 50; level++)
            {
                string key = "highscore" + i + "level" + level;
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.SetFloat(key, 0);
                }
            }
        }
    }
}
