using Mirror;
using System.Text;
using TMPro;
using UnityEngine;

public class Notificaton : MonoBehaviour
{
    Status status;

    TMP_Text text;
    public float animSpeed;

    float Mduration;
    int step = 2;
    float step2;
    bool isCustom;
    bool isVisible;

    private void Start()
    {
        status = FindObjectOfType<Status>();

        text = gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        text.gameObject.SetActive(false);

        //Notification(new Color (0, 1, 0, 0), "Start", 1f);
    }

    private void Update()
    {
        if (step == 0)
        {
            LeanTween.value(text.gameObject, 0, 1, animSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, val);
            });
            LeanTween.moveY(text.gameObject.GetComponent<RectTransform>(), 50, Mduration).setIgnoreTimeScale(true);
            step = 1;
        }

        if (step == 1)
        {
            Mduration += -Time.deltaTime;
        }

        if (Mduration < animSpeed && step == 1)
        {
            LeanTween.value(text.gameObject, text.color.a, 0, animSpeed).setEaseInOutQuad().setIgnoreTimeScale(true).setOnUpdate((float val) =>
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, val);
            });
            step = 2;
            step2 = animSpeed;
        }

        if (step == 2)
        {
            step2 += -Time.deltaTime;
        }

        if (step2 < 0f && step == 2)
        {
            isVisible = false;
            isCustom = false;
            step = 3;
        }

        if (!isVisible)
        {
            text.text = "";
        }

        if (isCustom && text.color.a > 0.1f && text.color.a < 0.9f && isVisible)
        {
            StringBuilder sb = new(text.text);
            sb.Remove(14, 2);
            string a;
            a = Mathf.RoundToInt(text.color.a * 100).ToString("X");
            a = a.PadLeft(2, '0');
            sb.Insert(14, a);
            text.text = sb.ToString();
        }

        if (isCustom && text.color.a > 0.9f && isVisible)
        {
            StringBuilder sb = new(text.text);
            sb.Remove(14, 2);
            sb.Insert(14, "ff");
            text.text = sb.ToString();
        }
    }


    public void Notification(Color color, string text, float duration)
    {
        LeanTween.pause(this.text.gameObject);
        this.text.gameObject.SetActive(true);
        this.text.text = text;
        this.text.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 100, 0);
        this.text.color = color;
        step = 0;
        isVisible = true;
        Mduration = duration;
    }

    public void NotificationWith2Colors(Color TextColor, float duration, string playerName, string text, Color playerColor, bool notificateOtherClients = false)
    {
        if (NetworkClient.active && notificateOtherClients)
        {
            status.sync.NotificatonWith2Colors(TextColor, duration, playerName, text, playerColor);
            return;
        }

        string playerColorHex = ColorUtility.ToHtmlStringRGB(playerColor);

        LeanTween.pause(this.text.gameObject);
        isCustom = true;
        this.text.gameObject.SetActive(true);
        this.text.text = "<color=#" + playerColorHex + "00>" + playerName + "</color> " + text;
        this.text.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 100, 0);
        this.text.color = TextColor;
        step = 0;
        isVisible = true;
        Mduration = duration;
    }
}
