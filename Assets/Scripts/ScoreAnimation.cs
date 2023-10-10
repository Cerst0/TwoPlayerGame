using System;
using TMPro;
using UnityEngine;

public class ScoreAnimation : MonoBehaviour
{
    float stepTime;
    int step;
    int index;
    float animSpeed = 1f;
    public int ID;
    public bool isParty;

    Status status;
    Party party;

    private void Start()
    {
        status = FindObjectOfType<Status>();
        party = FindObjectOfType<Party>();

        index = ID;
        if (status.playerNumber == 2 && ID == 1 && !isParty) { index = 2; }
    }

    void Update()
    {
        if (step == 0)
        {
            Vector2 target;
            if (isParty)
            {
                target = party.ui.Fields[index].transform.position;
            }
            else
            {
                target = GameObject.Find("Points" + index).transform.position;
            }

            LeanTween.move(gameObject, target, animSpeed).setEaseInQuad();
            LeanTween.scale(GetComponent<RectTransform>(), new Vector2(.6f, .6f), animSpeed).setEaseOutQuart();

            step = 1;
        }

        if (stepTime > animSpeed && step == 1)
        {
            Color color = GetComponent<TMP_Text>().color;
            LeanTween.value(gameObject, color, new Color(color.r, color.g, color.b, 0), animSpeed / 2).setOnUpdate((Color c) =>
            {
                GetComponent<TMP_Text>().color = c;
            });

            if (!isParty)
            {
                TMP_Text text = GameObject.FindGameObjectWithTag("Brain").GetComponent<Functions>().PlayerPoints[ID].GetComponent<TMP_Text>();
                text.text = (Convert.ToInt16(text.text) + 1).ToString();
            }

            step = 2;
            stepTime = 0;
        }

        if (stepTime > animSpeed / 2 && step == 2)
        {
            stepTime = 0;
            step = 3;
        }

        stepTime += Time.deltaTime;
    }
}
