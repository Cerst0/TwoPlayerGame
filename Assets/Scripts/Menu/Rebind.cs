using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Rebind : MonoBehaviour
{
    private InputS InputScript;
    private KeyBindings bindingsS;
    private Menu MenuS;
    private Status status;
    private Notificaton n;
    private InputAction action;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;
    public InputManagerGamepad img;

    PlayerInputHandler pihBlue;
    PlayerInputHandler pihRed;

    public Text BindingT;
    public string ActionName;
    public bool isMovement;
    public Image Icon;

    public Sprite LSUp;
    public Sprite LSDown;
    public Sprite LSRight;
    public Sprite LSLeft;

    public Sprite RSUp;
    public Sprite RSDown;
    public Sprite RSRight;
    public Sprite RSLeft;

    public Sprite DPUp;
    public Sprite DPDown;
    public Sprite DPRight;
    public Sprite DPLeft;

    int bindingIndex = 0;
    string bindingName;

    private void Awake()
    {
        img = new InputManagerGamepad();
    }

    private void Start()
    {
        n = FindObjectOfType<Notificaton>();
        InputScript = FindObjectOfType<InputS>();
        bindingsS = FindObjectOfType<KeyBindings>();
        MenuS = FindObjectOfType<Menu>();
        status = FindObjectOfType<Status>();

        Icon.gameObject.SetActive(false);
        GetAction();
        InputScript.LoadUserRebinds(action);
    }

    private void Update()
    {
        if (status.useGamepads)
        {
            pihBlue = GameObject.FindGameObjectWithTag("InputPlayer0").GetComponent<PlayerInputHandler>();
            pihRed = GameObject.FindGameObjectWithTag("InputPlayer1").GetComponent<PlayerInputHandler>();
        }

        GetAction();
        if (status.inputViewingType == 2) { bindingIndex = 0; }
        bindingName = InputControlPath.ToHumanReadableString(action.bindings[bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

        if (rebindingOperation == null)
        {
            if (bindingName == "Left Stick/Up" | bindingName == "Left Stick/Down" | bindingName == "Left Stick/Right" | bindingName == "Left Stick/Left" | bindingName == "Right Stick/Up" | bindingName == "Right Stick/Down" | bindingName == "Right Stick/Right" | bindingName == "Right Stick/Left" | bindingName == "D-Pad/Up" | bindingName == "D-Pad/Down" | bindingName == "D-Pad/Right" | bindingName == "D-Pad/Left")
            {
                BindingT.text = "";
                Icon.gameObject.SetActive(true);

                if (bindingName == "Left Stick/Up") { Icon.sprite = LSUp; }
                if (bindingName == "Left Stick/Down") { Icon.sprite = LSDown; }
                if (bindingName == "Left Stick/Right") { Icon.sprite = LSRight; }
                if (bindingName == "Left Stick/Left") { Icon.sprite = LSLeft; }

                if (bindingName == "Right Stick/Up") { Icon.sprite = RSUp; }
                if (bindingName == "Right Stick/Down") { Icon.sprite = RSDown; }
                if (bindingName == "Right Stick/Right") { Icon.sprite = RSRight; }
                if (bindingName == "Right Stick/Left") { Icon.sprite = RSLeft; }

                if (bindingName == "D-Pad/Up") { Icon.sprite = DPUp; }
                if (bindingName == "D-Pad/Down") { Icon.sprite = DPDown; }
                if (bindingName == "D-Pad/Right") { Icon.sprite = DPRight; }
                if (bindingName == "D-Pad/Left") { Icon.sprite = DPLeft; }
            }


            else { BindingT.text = bindingName; Icon.gameObject.SetActive(false); }
        }



        Scene Sname = SceneManager.GetActiveScene();
        if (Sname.name == "Menu")
        {
            if (MenuS.panelName == "bindings" && bindingsS.listen && rebindingOperation == null && bindingsS.BName == ActionName)
            {
                StartInteractivRebind();
            }
        }
    }

    private void StartInteractivRebind()
    {
        GetComponent<Image>().color = new Color(1f, 1f, 0.5f, 1);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        BindingT.text = "listening...";
        Icon.gameObject.SetActive(false);
        print("RebindStart for " + action);
        action.Disable();
        if (status.inputViewingType == 0)
        {
            if (ActionName != "enter1" && ActionName != "enter2")
            {
                print("1");
                rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .WithCancelingThrough(InputScript.ip.Player.Esc.bindings[0].effectivePath)
                .OnCancel(c => { RebindClompleted(); })
                .OnComplete(c => { RebindClompleted(); /*print(c.action.bindings[bindingIndex].id); print(c.action.bindings[bindingIndex].action);*/ });
            }
            else
            {
                print("2");
                rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnCancel(c => { RebindClompleted(); })
                .OnComplete(c => { RebindClompleted(); /*print(c.action.bindings[bindingIndex].id); print(c.action.bindings[bindingIndex].action);*/ });
            }
        }

        if (status.inputViewingType == 1)
        {
            if (isMovement)
            {
                print("3");
                rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .WithControlsExcluding("Keyboard")
                .OnMatchWaitForAnother(0.1f)
                .WithCancelingThrough(InputScript.ip.Player.Esc.bindings[0].effectivePath)
                .OnCancel(c => { RebindClompleted(); })
                .OnComplete(c => { RebindClompleted(); /*print(c.action.bindings[bindingIndex].id); print(c.action.bindings[bindingIndex].action);*/ });
            }
            else
            {
                if (ActionName != "enter1" && ActionName != "enter2" && !isMovement)
                {
                    print("4");
                    rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .WithCancelingThrough(InputScript.ip.Player.Esc.bindings[0].effectivePath)
                    .OnCancel(c => { RebindClompleted(); })
                    .OnComplete(c => { RebindClompleted(); /*print(c.action.bindings[bindingIndex].id); print(c.action.bindings[bindingIndex].action);*/ });
                }
                else
                {
                    print("5");
                    rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .OnCancel(c => { RebindClompleted(); })
                    .OnComplete(c => { RebindClompleted(); /*print(c.action.bindings[bindingIndex].id); print(c.action.bindings[bindingIndex].action);*/ });
                }
            }
        }


        if (status.inputViewingType == 2)
        {
            print("is2");
            if (isMovement)
            {
                print("6");
                rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .WithControlsExcluding("<Gamepad>")
                .OnMatchWaitForAnother(0.1f)
                .WithCancelingThrough(InputScript.ip.Player.Esc.bindings[0].effectivePath)
                .OnCancel(c => { RebindClompleted(); })
                .OnComplete(c => { RebindClompleted(); /*print(c.action.bindings[bindingIndex].id); print(c.action.bindings[bindingIndex].action);*/ });
            }
            else
            {
                if (ActionName != "enter1" && ActionName != "enter2" && !isMovement)
                {
                    print("7");
                    rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .WithCancelingThrough(InputScript.ip.Player.Esc.bindings[0].effectivePath)
                    .OnCancel(c => { RebindClompleted(); })
                    .OnComplete(c => { RebindClompleted(); /*print(c.action.bindings[bindingIndex].id); print(c.action.bindings[bindingIndex].action);*/ });
                }
                else
                {
                    print("8");
                    rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .OnCancel(c => { RebindClompleted(); })
                    .OnComplete(c => { RebindClompleted(); /*print(c.action.bindings[bindingIndex].id); print(c.action.bindings[bindingIndex].action);*/ });
                }
            }

        }

        rebindingOperation.Start();
    }

    private void RebindClompleted()
    {
        if (rebindingOperation != null)
        {
            if (action.bindings[0].effectivePath.Contains("Gamepad") && isMovement && !status.useGamepads && InputScript.twoGamepads)
            {
                n.Notification(new Color(1, 0, 0, 0), "Pair your controllers to avoid problems", 2f);
            }
            rebindingOperation?.Dispose();
            bindingsS.listen = false;
            action.Enable();
            InputScript.SaveUserRebinds(action);
            rebindingOperation.Dispose();
            rebindingOperation = null;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            print("RebindEnd");
        }
    }


    private void GetAction()
    {
        if (status.inputViewingType != 1)
        {
            if (ActionName == "upBlue") { action = InputScript.ip.Player.UpBlue; isMovement = true; }
            if (ActionName == "downBlue") { action = InputScript.ip.Player.DownBlue; isMovement = true; }
            if (ActionName == "leftBlue") { action = InputScript.ip.Player.LeftBlue; isMovement = true; }
            if (ActionName == "rightBlue") { action = InputScript.ip.Player.RightBlue; isMovement = true; }

            if (ActionName == "upRed") { action = InputScript.ip.Player.UpRed; isMovement = true; }
            if (ActionName == "downRed") { action = InputScript.ip.Player.DownRed; isMovement = true; }
            if (ActionName == "leftRed") { action = InputScript.ip.Player.LeftRed; isMovement = true; }
            if (ActionName == "rightRed") { action = InputScript.ip.Player.RightRed; isMovement = true; }
        }
        else
        {
            if (ActionName == "upBlue") { action = pihBlue.MVB; bindingIndex = 2; }
            if (ActionName == "downBlue") { action = pihBlue.MVB; bindingIndex = 1; }
            if (ActionName == "leftBlue") { action = pihBlue.MHB; bindingIndex = 1; }
            if (ActionName == "rightBlue") { action = pihBlue.MHB; bindingIndex = 2; }

            if (ActionName == "upRed") { action = pihRed.MVR; bindingIndex = 2; }
            if (ActionName == "downRed") { action = pihRed.MVR; bindingIndex = 1; }
            if (ActionName == "leftRed") { action = pihRed.MHR; bindingIndex = 1; }
            if (ActionName == "rightRed") { action = pihRed.MHR; bindingIndex = 2; }
        }

        if (ActionName == "enter1") { action = InputScript.ip.Player.Enter; }
        if (ActionName == "enter2") { action = InputScript.ip.Player.Entertwo; }

        if (ActionName == "esc1") { action = InputScript.ip.Player.Esc; }
        if (ActionName == "esc2") { action = InputScript.ip.Player.Esctwo; }
    }
}
