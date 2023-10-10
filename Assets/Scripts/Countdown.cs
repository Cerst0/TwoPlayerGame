using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : MonoBehaviour
{
    public float time;
    int currentNumber = -1;

    GameObject[] numbers;
    Material BorderMat;

    Functions functions;
    Status status;

    private void Awake()
    {
        numbers = new GameObject[4];
        for (int i = 3; i >= 0; i--)
        {
            numbers[i] = transform.GetChild(i).gameObject;
        }
    }

    private void Start()
    {
        functions = FindObjectOfType<Functions>();
        status = FindObjectOfType<Status>();

        if (functions.splitScreen)
        {
            BorderMat = GameObject.Find("SplitScreenBorder").transform.GetChild(0).GetComponent<Image>().material;
            BorderMat.SetColor("_OldColor", Color.black);
            BorderMat.SetFloat("_Progress", 0);
        }
    }

    private void Update()
    {
        foreach (GameObject go in numbers)
        {
            go.SetActive(LeanTween.isTweening(go));
        }
    }

    public void StartCountdown()
    {
        if (NetworkClient.active && !NetworkServer.active) { return; }
        foreach (GameObject go in numbers)
        {
            LeanTween.cancel(go);
            LeanTween.cancel(go.GetComponent<RectTransform>());
        }
        StartCoroutine(CountdownCoroutine());
    }

    public void UpdateCountdown(int index)
    {
        //print("update Countdown " + index);
        BorderColor(index);

        switch (index)
        {
            case 0:
                {
                    numbers[index].GetComponent<RectTransform>().localScale = new Vector2(5, 5);
                    LeanTween.scale(numbers[index].GetComponent<RectTransform>(), new Vector2(0, 0), time * 1.25f).setEaseInSine();

                    LeanTween.value(numbers[index].GetComponent<TMP_Text>().color.a, 1, time / 4).setOnUpdate((float val) =>
                    {
                        Color c = numbers[index].GetComponent<TMP_Text>().color;
                        c.a = val;
                        numbers[index].GetComponent<TMP_Text>().color = c;
                    });

                    LeanTween.value(numbers[index].GetComponent<TMP_Text>().color.a, 0, time / 4).setDelay(time).setOnUpdate((float val) =>
                    {
                        Color c = numbers[index].GetComponent<TMP_Text>().color;
                        c.a = val;
                        numbers[index].GetComponent<TMP_Text>().color = c;
                    });
                    break;
                }

            case 1:
                {
                    numbers[index].GetComponent<RectTransform>().anchoredPosition = new Vector2(1000, 0);
                    Color color = numbers[index].GetComponent<TMP_Text>().color;
                    color.a = 0;

                    LeanTween.moveX(numbers[index].GetComponent<RectTransform>(), 0, time * 1.25f).setEaseOutExpo();

                    LeanTween.value(numbers[index].GetComponent<TMP_Text>().color.a, 1, time / 2).setOnUpdate((float val) =>
                    {
                        Color c = numbers[index].GetComponent<TMP_Text>().color;
                        c.a = val;
                        numbers[index].GetComponent<TMP_Text>().color = c;
                    });
                    LeanTween.value(numbers[index].GetComponent<TMP_Text>().color.a, 0, time / 4).setDelay(time).setOnUpdate((float val) =>
                    {
                        Color c = numbers[index].GetComponent<TMP_Text>().color;
                        c.a = val;
                        numbers[index].GetComponent<TMP_Text>().color = c;
                    });
                    break;
                }
            case 2:
                {
                    numbers[index].GetComponent<RectTransform>().anchoredPosition = new Vector2(-1000, 0);
                    Color color = numbers[index].GetComponent<TMP_Text>().color;
                    color.a = 0;

                    LeanTween.moveX(numbers[index].GetComponent<RectTransform>(), 0, time * 1.25f).setEaseOutExpo();

                    LeanTween.value(numbers[index].GetComponent<TMP_Text>().color.a, 1, time / 2).setOnUpdate((float val) =>
                    {
                        Color c = numbers[index].GetComponent<TMP_Text>().color;
                        c.a = val;
                        numbers[index].GetComponent<TMP_Text>().color = c;
                    });
                    LeanTween.value(numbers[index].GetComponent<TMP_Text>().color.a, 0, time / 4).setDelay(time).setOnUpdate((float val) =>
                    {
                        Color c = numbers[index].GetComponent<TMP_Text>().color;
                        c.a = val;
                        numbers[index].GetComponent<TMP_Text>().color = c;
                    });
                    break;
                }
            case 3:
                {
                    functions.OnCoundownFinished(); // Game Starts Now 

                    numbers[index].GetComponent<RectTransform>().localScale = new Vector2(0, 0);
                    LeanTween.scale(numbers[index].GetComponent<RectTransform>(), new Vector2(5, 5), time * 1.25f).setEaseInSine();

                    LeanTween.value(numbers[index].GetComponent<TMP_Text>().color.a, 1, time / 4).setOnUpdate((float val) =>
                    {
                        Color c = numbers[index].GetComponent<TMP_Text>().color;
                        c.a = val;
                        numbers[index].GetComponent<TMP_Text>().color = c;
                    });

                    LeanTween.value(numbers[index].GetComponent<TMP_Text>().color.a, 0, time * .75f).setDelay(time * .5f).setEaseOutSine().setOnUpdate((float val) =>
                     {
                         Color c = numbers[index].GetComponent<TMP_Text>().color;
                         c.a = val;
                         numbers[index].GetComponent<TMP_Text>().color = c;
                     });
                    break;
                }
            case 4:
                {
                    LeanTween.moveY(functions.WiningConditionGO, 0, .25f).setEaseInBack().setDelay(time * 6);

                    break;
                }
        }
    }

    void BorderColor(int index)
    {
        if (!functions.splitScreen) { return; }

        float blendTime = time;

        BorderMat.SetFloat("_Progress", 0);

        switch (index)
        {
            case 0:
                {
                    BorderMat.SetColor("_BlendColor", Color.red);
                    BorderMat.SetColor("_OldColor", Color.black);
                    break;
                }

            case 1:
                {
                    BorderMat.SetColor("_BlendColor", new Color(1, .5f, 0));
                    BorderMat.SetColor("_OldColor", Color.red);
                    break;
                }

            case 2:
                {
                    BorderMat.SetColor("_BlendColor", Color.yellow);
                    BorderMat.SetColor("_OldColor", new Color(1, .5f, 0));
                    break;
                }

            case 3:
                {
                    BorderMat.SetColor("_BlendColor", new Color(0, .7f, 0));
                    BorderMat.SetColor("_OldColor", Color.yellow);
                    break;
                }

            case 4:
                {
                    BorderMat.SetColor("_BlendColor", Color.black);
                    BorderMat.SetColor("_OldColor", new Color(0, .7f, 0));

                    blendTime *= 6;

                    break;
                }
        }

        LeanTween.value(0, 1.75f, blendTime).setEaseOutQuad().setOnUpdate((float val) =>
        {
            BorderMat.SetFloat("_Progress", val);
        });
    }
    IEnumerator CountdownCoroutine()
    {
        //yield return new WaitForSeconds(2);

        for (int i = 0; i <= 4; i++)
        {
            currentNumber = i;
            if (NetworkServer.active) { status.sync.SetCountdown(i); }
            UpdateCountdown(i);

            yield return new WaitForSeconds(time);

        }
    }
}
