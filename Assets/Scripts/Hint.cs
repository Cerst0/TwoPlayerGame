using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Hint : MonoBehaviour
{
    [TextArea]
    public string tip = "0-UpB, 1-DownB, 2-RightB, 3- LeftB, 4-UpR, 5-DownR, 6-RightR, 7-LeftR";

    public Sprite[] KeySprites;
    public string[] KeySpritesNames;

    int index;
    int keyCount;
    float animationSpeed = 1;
    float timer;
    float targetXPos;
    float startXPos;

    TMP_Text[] Texts;
    TMP_Text HintNameText;
    GameObject PlusSign;

    InputS input;
    Functions functions;
    Status status;


    private void Start()
    {
        SetUp();

        if (index == 0)
        {
            startXPos = -100;
            targetXPos = 100;
        }
        else if (index == 1)
        {
            startXPos = 100;
            targetXPos = -100;
        }

        GetComponent<RectTransform>().anchoredPosition = new(startXPos, -50);
        GetComponent<CanvasGroup>().alpha = 0;
    }

    private void Update()
    {
        timer += -Time.deltaTime;

        foreach (TMP_Text text in Texts)
        {
            float width = text.bounds.extents.x;
            if (width > 20)
            {
                text.transform.parent.GetComponent<RectTransform>().sizeDelta = new(200 / (keyCount * 1.35f), text.transform.parent.GetComponent<RectTransform>().sizeDelta.y);
            }
        }
    }

    void SetUp()
    {
        status = FindObjectOfType<Status>();
        functions = FindObjectOfType<Functions>();
        input = FindObjectOfType<InputS>();

        if (name == "Hint0")
        {
            index = 0;
        }
        else if (name == "Hint1") { index = 1; }

        Texts = new TMP_Text[2] { transform.Find("Keys/Key0/Key0Text").GetComponent<TMP_Text>(), transform.Find("Keys/Key1/Key1Text").GetComponent<TMP_Text>() };
        PlusSign = transform.Find("Keys/+").gameObject;
        HintNameText = transform.Find("Text").GetComponent<TMP_Text>();

        foreach (TMP_Text t in Texts)
        {
            t.transform.parent.gameObject.SetActive(false);
        }
        PlusSign.SetActive(false);
        HintNameText.gameObject.SetActive(false);
    }

    public void ActivateHint(string hintName, int[] key, bool animation = true, float time = -1)
    {
        SetUp();

        if (key.Length == 1)
        {
            Texts[0].transform.parent.gameObject.SetActive(true);
            Texts[1].transform.parent.gameObject.SetActive(false);
            PlusSign.SetActive(false);
        }
        else
        {
            Texts[0].transform.parent.gameObject.SetActive(true);
            Texts[1].transform.parent.gameObject.SetActive(true);
            PlusSign.SetActive(true);
        }
        HintNameText.gameObject.SetActive(true);


        string[] keyNames = GetKeyName(key);

        for (int i = 0; i < key.Length; i++)
        {
            Texts[i].SetText(keyNames[i]);
        }
        HintNameText.text = hintName;

        int playerindex = index;
        if (NetworkClient.active)
        {
            playerindex = 0;
        }
        HintNameText.color = FindObjectOfType<Methods>().GetPlayerColor(playerindex);

        CancelAnimations();

        if (animation)
        {
            Animation(true);
        }
        else
        {
            RectTransform rect = functions.PlayerNames[index].GetComponent<RectTransform>();
            rect.offsetMax = new Vector2(-220, rect.offsetMax.y);
            GetComponent<RectTransform>().anchoredPosition = new(targetXPos, -50);
            GetComponent<CanvasGroup>().alpha = 1;
        }

        for (int keyIndex = 0; keyIndex < key.Length; keyIndex++)
        {
            Sprite keySprite = null;
            for (int spriteIndex = 0; spriteIndex < KeySpritesNames.Length; spriteIndex++)
            {
                if (KeySpritesNames[spriteIndex] == keyNames[keyIndex])
                {
                    keySprite = KeySprites[spriteIndex];
                }
            }
            if (keySprite == null)
            {
                Texts[keyIndex].transform.parent.Find("Key" + keyIndex + "Image").GetComponent<Image>().color = Color.clear;
            }
            else
            {
                Texts[keyIndex].transform.parent.Find("Key" + keyIndex + "Image").GetComponent<Image>().sprite = keySprite;
                Texts[keyIndex].SetText(string.Empty);
            }
        }

        if (time != -1)
        {
            timer = time + animationSpeed;
            StartCoroutine(CheckTimer());
        }

        keyCount = key.Length;
    }


    public void DeactivateHint(bool animation = true)
    {
        CancelAnimations();

        if (animation)
        {
            Animation(false);
        }
        else
        {
            RectTransform rect = functions.PlayerNames[index].GetComponent<RectTransform>();
            rect.offsetMax = new Vector2(-15, rect.offsetMax.y);
            GetComponent<RectTransform>().anchoredPosition = new(startXPos, -50);
        }
    }



    void Animation(bool activate)
    {
        if (activate)
        {
            LeanTween.move(GetComponent<RectTransform>(), new(targetXPos, -50), animationSpeed).setEaseInOutQuad();

            LeanTween.value(functions.PlayerNames[index], functions.PlayerNames[index].GetComponent<RectTransform>().offsetMax.x, -220, animationSpeed).setEaseInOutQuad().setOnUpdate((float val) =>
            {
                RectTransform rect = functions.PlayerNames[index].GetComponent<RectTransform>();
                rect.offsetMax = new Vector2(val, rect.offsetMax.y);
            });

            LeanTween.value(gameObject, GetComponent<CanvasGroup>().alpha, 1, animationSpeed).setEaseInOutQuad().setOnUpdate((float alpha) =>
             {
                 GetComponent<CanvasGroup>().alpha = alpha;
             });
        }
        else
        {
            LeanTween.move(GetComponent<RectTransform>(), new(startXPos, -50), animationSpeed).setEaseInOutQuad();

            LeanTween.value(functions.PlayerNames[index], functions.PlayerNames[index].GetComponent<RectTransform>().offsetMax.x, -15, animationSpeed).setEaseInOutQuad().setOnUpdate((float val) =>
            {
                RectTransform rect = functions.PlayerNames[index].GetComponent<RectTransform>();
                rect.offsetMax = new Vector2(val, rect.offsetMax.y);
            });

            LeanTween.value(gameObject, GetComponent<CanvasGroup>().alpha, 0, animationSpeed).setEaseInOutQuad().setOnUpdate((float alpha) =>
             {
                 GetComponent<CanvasGroup>().alpha = alpha;
             });
        }

    }

    private void CancelAnimations()
    {
        LeanTween.cancel(functions.PlayerNames[index].GetComponent<RectTransform>());
        LeanTween.cancel(gameObject.GetComponent<RectTransform>());
    }

    string[] GetKeyName(int[] key)
    {
        string[] keyNames = new string[key.Length];

        for (int i = 0; i < key.Length; i++)
        {
            int currentKey = key[i];

            if (status.useGamepads)
            {
                int index = 0;
                if (currentKey >= 4) // decides wether blue or red player is used
                {
                    index = 1;
                }
                PlayerInputHandler handler = GameObject.FindGameObjectWithTag("InputPlayer" + index).GetComponent<PlayerInputHandler>();

                int[] actionKey = { 0, 0, 1, 1, 2, 2, 3, 3 };

                InputAction[] actions = { handler.MVB, handler.MHB, handler.MVR, handler.MHR };
                InputAction action = actions[actionKey[currentKey]];

                int[] bindingIndexs = { 2, 1, 2, 1, 2, 1, 2, 1 };
                InputBinding binding = action.bindings[bindingIndexs[currentKey]];

                keyNames[i] = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.UseShortNames);
                try { keyNames[i] = keyNames[i].Replace(" [Gamepad]", string.Empty); }
                catch { }
            }
            else
            {
                InputAction action = input.ip.Player.Get().actions[currentKey];
                keyNames[i] = InputControlPath.ToHumanReadableString(action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            }
        }

        return keyNames;
    }

    IEnumerator CheckTimer()
    {
        yield return new WaitUntil(() => timer <= 0);
        DeactivateHint(true);
        StopCoroutine(CheckTimer());
    }
}
