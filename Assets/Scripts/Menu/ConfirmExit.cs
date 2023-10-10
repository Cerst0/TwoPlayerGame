using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfirmExit : MonoBehaviour
{
    InputS input;
    Status status;
    Menu menu;

    public AudioSource ChangeSelection;

    public GameObject panel;
    public Image PanelCloseColor;

    bool upB;
    bool downB;
    bool leftB;
    bool rightB;
    bool isOpen;
    bool isOpenB;
    public Vector2 ButtonNumber;
    Vector2 ButtonNumberB;

    [Header("StartScreen")]
    public GameObject StartScreenDescriptionImage;
    public GameObject StartScreenImage;

    [Header("Exit")]
    public GameObject ExitDescriptionImage;
    public GameObject ExitScreenImage;

    private void Start()
    {
        status = GameObject.FindGameObjectWithTag("Status").GetComponent<Status>();
        input = FindObjectOfType<InputS>();
        menu = GetComponent<Menu>();

        panel.SetActive(false);
    }

    private void Update()
    {
        if (isOpen && !isOpenB)
        {
            SetButton(StartScreenImage, StartScreenDescriptionImage, new Vector2(0, 0));
            SetButton(ExitScreenImage, ExitDescriptionImage, new Vector2(0, -1), true);
        }
        isOpenB = isOpen;

        if (menu.ConfirmExitStat == 2)
        {
            isOpen = true;
        }

        if (isOpen) { ChangeSelectedVector(); KeyPressed(); }
    }

    private void KeyPressed()
    {
        if ((input.enter && ButtonNumber.x == 1) | status.buttonName == "CloseExit" | input.esc)
        {
            input.enter = false;
            isOpen = false;
            GetComponent<Menu>().ConfirmExitStat = 3;
            Cursor.SetCursor(menu.cursorNormal, Vector2.zero, CursorMode.Auto);
            ButtonNumber = Vector2.zero;
        }

        if ((input.enter && ButtonNumber.Equals(new Vector2(0, 0))) | status.buttonName == "StartScreen")
        {
            if (NetworkManager.singleton.isNetworkActive)
            {
                NetworkManager.singleton.StopHost();
            }
            else
            {
                foreach (DontDestroyOnLoad DDOL in FindObjectsOfType<DontDestroyOnLoad>())
                {
                    GameObject go = DDOL.gameObject.gameObject;
                    Destroy(go);
                }
                SceneManager.LoadScene("Menu");
            }
        }

        if ((input.enter && ButtonNumber.Equals(new Vector2(0, -1))) | status.buttonName == "ExitApp")
        {
            FindObjectOfType<NetworkManager>().StopClient();
            PlayerPrefs.Save();
            Application.Quit();
            Debug.LogError("Couldn't close Application; Is Game running in Editor?");
        }
    }

    private void ChangeSelectedVector()
    {
        if (ButtonNumber != ButtonNumberB)
        {
            ChangeSelection.Play();

            SetButton(StartScreenImage, StartScreenDescriptionImage, new Vector2(0, 0));
            SetButton(ExitScreenImage, ExitDescriptionImage, new Vector2(0, -1));

            if (ButtonNumber.x == 1 && ButtonNumberB.x != 1) { menu.SetCloseColor(PanelCloseColor, true); }
            if (ButtonNumberB.x == 1) { menu.SetCloseColor(PanelCloseColor, false); }
        }
        ButtonNumberB = ButtonNumber;


        if (input.up && (input.up != upB) && ButtonNumber.y < 0)
        {
            ButtonNumber.y += 1;
        }

        if (input.down && (input.down != downB) && ButtonNumber.y > -1)
        {
            ButtonNumber.y -= 1;
        }

        if (input.right && (input.right != rightB) && ButtonNumber.x < 1)
        {
            ButtonNumber.x += 1;
        }

        if (input.left && (input.left != leftB) && ButtonNumber.x > 0)
        {
            ButtonNumber.x -= 1;
        }


        upB = input.up;
        downB = input.down;
        leftB = input.left;
        rightB = input.right;
    }

    public void SetButton(GameObject Image, GameObject DescriptionImage, Vector2 Pos, bool isStartDeselector = false)
    {
        if (ButtonNumberB == Pos | isStartDeselector)
        {
            Image.GetComponent<UniversalButton>().active[1] = false;
            if (Image.GetComponent<UniversalButton>().active.All(active => active == false))
            {
                LeanTween.scale(Image.GetComponent<RectTransform>(), Image.GetComponent<UniversalButton>().startSize, .25f);

            }
            Image.GetComponent<Image>().color = menu.ButtonUnSelected;
            DescriptionImage.GetComponent<Image>().color = menu.ButtonDescriptionUnSelected;
        }
        if (ButtonNumber == Pos)
        {
            Image.GetComponent<UniversalButton>().active[1] = true;
            LeanTween.scale(Image.GetComponent<RectTransform>(), Image.GetComponent<UniversalButton>().selectedSize, .25f);
            Image.GetComponent<Image>().color = menu.ButtonSelected;
            DescriptionImage.GetComponent<Image>().color = menu.ButtonDescriptionSelected;
        }
    }
}
