using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public void SetScoreboardMultiScore()
    {
        transform.Find("Edge").GetComponent<RectTransform>().sizeDelta = new Vector2(200, 33.6f);
        transform.Find("ScoreBoardColor0").GetComponent<RectTransform>().anchoredPosition = new Vector2(-125, -121.5f);
        transform.Find("ScoreBoardColor1").GetComponent<RectTransform>().rect.Set(0, 0, 150, 120);
        transform.Find("ScoreBoardColor2").GetComponent<RectTransform>().rect.Set(0, 0, 150, 120);
        transform.Find("ScoreBoardColor3").GetComponent<RectTransform>().anchoredPosition = new Vector2(125, -121.5f);
        GameObject.Find("Name0").GetComponent<RectTransform>().offsetMax = new Vector2(-375, GameObject.Find("Name0").GetComponent<RectTransform>().offsetMax.y);
        GameObject.Find("Name1").GetComponent<RectTransform>().offsetMin = new Vector2(375, GameObject.Find("Name1").GetComponent<RectTransform>().offsetMin.y);

        if (FindObjectOfType<Status>().playerNumber == 3)
        {
            transform.Find("Score2").GetComponent<RectTransform>().anchoredPosition = new Vector2(152.5f, -60);
        }
    }

    public void SetScoreboardSingleScore()
    {
        GetComponent<Canvas>().sortingOrder = 75;
        transform.Find("SplitMiddle").gameObject.SetActive(false);
        transform.Find("Score0").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, transform.Find("Score0").GetComponent<RectTransform>().anchoredPosition.y);
    }
}
