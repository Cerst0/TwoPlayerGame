using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputS : MonoBehaviour, InputManager.IPlayerActions
{
    [Header("Inputs")]

    public InputManager ip;
    public KeyBindings bindingsS;
    public EventHandler ClickEvent;

    Status status;

    public bool menu;
    bool menuB;
    public bool reset;
    public bool twoGamepads;
    bool clickB;

    public bool enter;
    public bool esc;

    int enterCounter = 0;
    int escCounter = 0;
    public int resetCounter;
    float enterValue;
    float enterValue2;

    public bool left;
    public bool right;
    public bool up;
    public bool down;

    bool waitUp;
    bool waitDown;

    bool waitLeft;
    bool waitRight;

    public float leftB;
    public float rightB;
    public float upB;
    public float downB;

    public float leftR;
    public float rightR;
    public float upR;
    public float downR;

    public string upBlueBT;

    public float MHBlue;
    public float MVBlue;
    public float MHRed;
    public float MVRed;

    public float MHBlueK;
    public float MVBlueK;
    public float MHRedK;
    public float MVRedK;

    public float MHBlueG;
    public float MVBlueG;
    public float MHRedG;
    public float MVRedG;

    private void Awake()
    {
        ip = new InputManager();
    }

    private void Start()
    {
        LoadAllUserRebinds();

        Setup();
    }

    private void OnLevelWasLoaded(int level)
    {
        Setup();
    }

    private void Setup()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        menu = (sceneName == "Menu" || sceneName == "Party") ? true : false;

        if (Gamepad.all.Count > 1)
        {
            twoGamepads = true;
        }
        else { print("one controller was detected"); }

        if (menu) { bindingsS = FindObjectOfType<KeyBindings>(); }
    }

    void Update()
    {
        status = FindObjectOfType<Status>();
        //if(enter == true) { print(enter); print(enterCounter); }

        twoGamepads = Gamepad.all.Count >= 2;

        if (menu)
        {
            if (waitUp && (ip.Player.RightBlue.ReadValue<float>() > 0.75f) | (ip.Player.RightRed.ReadValue<float>() > 0.75f | rightB > 0.75f | rightR > 0.75f)) { waitUp = true; }
            else { waitUp = false; }

            if (waitDown && (ip.Player.DownBlue.ReadValue<float>() > 0.75f | ip.Player.DownRed.ReadValue<float>() > 0.75f | downB < -0.75f | downR < -0.75f)) { waitDown = true; }
            else { waitDown = false; }

            if (waitLeft && (ip.Player.LeftBlue.ReadValue<float>() > 0.75f | ip.Player.LeftRed.ReadValue<float>() > 0.75f | leftB < -0.75f | leftR < -0.75f)) { waitLeft = true; }
            else { waitLeft = false; }

            if (waitRight && (ip.Player.RightBlue.ReadValue<float>() > 0.75f | ip.Player.RightRed.ReadValue<float>() > 0.75f | rightB > 0.75f | rightR > 0.75f)) { waitRight = true; }
            else { waitRight = false; }

            switch (menuB)
            {
                case false:
                    {
                        if (ip.Player.UpBlue.ReadValue<float>() > 0.75f | ip.Player.UpRed.ReadValue<float>() > 0.75f | upB > 0.75f | upR > 0.75f) { waitUp = true; }
                        if (ip.Player.DownBlue.ReadValue<float>() > 0.75f | ip.Player.DownRed.ReadValue<float>() > 0.75f | downB < -0.75f | downR < -0.75f) { waitDown = true; }

                        if (ip.Player.LeftBlue.ReadValue<float>() > 0.75f | ip.Player.LeftRed.ReadValue<float>() > 0.75f | leftB < -0.75f | leftR < -0.75f) { waitLeft = true; }
                        if (ip.Player.RightBlue.ReadValue<float>() > 0.75f | ip.Player.RightRed.ReadValue<float>() > 0.75f | rightB > 0.75f | rightR > 0.75f) { waitRight = true; }

                        break;
                    }

                case true:
                    {
                        if (!waitUp) { up = (ip.Player.UpBlue.ReadValue<float>() > 0.75f) | (ip.Player.UpRed.ReadValue<float>() > 0.75f | upB > 0.75f | upR > 0.75f); }
                        if (!waitDown) { down = (ip.Player.DownBlue.ReadValue<float>() > 0.75f) | (ip.Player.DownRed.ReadValue<float>() > 0.75f | downB < -0.75f | downR < -0.75f); }
                        if (!waitLeft) { left = (ip.Player.LeftBlue.ReadValue<float>() > 0.75f) | (ip.Player.LeftRed.ReadValue<float>() > 0.75f | leftB < -0.75f | leftR < -0.75f); }
                        if (!waitRight) { right = (ip.Player.RightBlue.ReadValue<float>() > 0.75f) | (ip.Player.RightRed.ReadValue<float>() > 0.75f | rightB > 0.75f | rightR > 0.75f); }

                        enterValue = ip.Player.Enter.ReadValue<float>();
                        enterValue2 = ip.Player.Entertwo.ReadValue<float>();

                        break;
                    }
            }

        }
        else
        {
            MHBlueK = ip.Player.RightBlue.ReadValue<float>() - ip.Player.LeftBlue.ReadValue<float>();
            MVBlueK = ip.Player.UpBlue.ReadValue<float>() - ip.Player.DownBlue.ReadValue<float>();
            MHRedK = ip.Player.RightRed.ReadValue<float>() - ip.Player.LeftRed.ReadValue<float>();
            MVRedK = ip.Player.UpRed.ReadValue<float>() - ip.Player.DownRed.ReadValue<float>();
        }

        bool click = ip.Player.Click.ReadValue<float>() == 1;
        if (click && !clickB)
        {
            ClickEvent?.Invoke(this, EventArgs.Empty);
        }
        clickB = click;
    }

    void LateUpdate()
    {
        if (enterValue > 0f | enterValue2 > 0f) { enterCounter++; }
        else { enterCounter = 0; }
        if (enterCounter == 1) { enter = true; print("enter"); }
        else { enter = false; }

        if (ip.Player.Esc.ReadValue<float>() > 0f | ip.Player.Esctwo.ReadValue<float>() > 0f) { escCounter++; }
        else { escCounter = 0; }
        if (escCounter == 1) { esc = true; }
        else { esc = false; }



        if (MHBlueK != 0 | (MHBlueG != 0 && status.useGamepads))
        {
            if (MHBlueK != 0) { MHBlue = MHBlueK; }
            else { MHBlue = MHBlueG; }
        }
        else { MHBlue = 0f; }

        if (MVBlueK != 0 | (MVBlueG != 0 && status.useGamepads))
        {
            if (MVBlueK != 0) { MVBlue = MVBlueK; }
            else { MVBlue = MVBlueG; }
        }
        else { MVBlue = 0f; }


        if (MHRedK != 0 | (MHRedG != 0 && status.useGamepads))
        {
            if (MHRedK != 0) { MHRed = MHRedK; }
            else { MHRed = MHRedG; }
        }
        else { MHRed = 0f; }

        if (MVRedK != 0 | (MVRedG != 0 && status.useGamepads))
        {
            if (MVRedK != 0) { MVRed = MVRedK; }
            else { MVRed = MVRedG; }
        }
        else { MVRed = 0f; }

        if (Mathf.Abs(MHBlue) < 0.1f) { MHBlue = 0; }
        if (Mathf.Abs(MVBlue) < 0.1f) { MVBlue = 0; }

        if (Mathf.Abs(MHRed) < 0.1f) { MHRed = 0; }
        if (Mathf.Abs(MVRed) < 0.1f) { MVRed = 0; }


        menuB = menu;
    }

    private void OnEnable()
    {
        ip.Enable();
    }

    private void OnDisable()
    {
        ip.Disable();
    }

    public void SaveUserRebinds(InputAction action)
    {
        var rebinds = action.SaveBindingOverridesAsJson();
        print("Saved binding: " + action.name + " New Binding is: " + action.SaveBindingOverridesAsJson());
        PlayerPrefs.SetString("rebinds" + action.name, rebinds);
    }

    public void LoadUserRebinds(InputAction action)
    {
        var rebinds = PlayerPrefs.GetString("rebinds" + action.name);
        //print("Loaded binding: " + action.name + " Binding is: " + action.SaveBindingOverridesAsJson());
        action.LoadBindingOverridesFromJson(rebinds);
    }

    public void SaveAllUserRebinds()
    {

        SaveUserRebinds(ip.Player.UpBlue);
        SaveUserRebinds(ip.Player.DownBlue);
        SaveUserRebinds(ip.Player.LeftBlue);
        SaveUserRebinds(ip.Player.RightBlue);

        SaveUserRebinds(ip.Player.UpRed);
        SaveUserRebinds(ip.Player.DownRed);
        SaveUserRebinds(ip.Player.LeftRed);
        SaveUserRebinds(ip.Player.RightRed);

        SaveUserRebinds(ip.Player.Enter);
        SaveUserRebinds(ip.Player.Entertwo);

        SaveUserRebinds(ip.Player.Esc);
        SaveUserRebinds(ip.Player.Esctwo);
    }

    public void LoadAllUserRebinds()
    {

        LoadUserRebinds(ip.Player.UpBlue);
        LoadUserRebinds(ip.Player.DownBlue);
        LoadUserRebinds(ip.Player.LeftBlue);
        LoadUserRebinds(ip.Player.RightBlue);

        LoadUserRebinds(ip.Player.UpRed);
        LoadUserRebinds(ip.Player.DownRed);
        LoadUserRebinds(ip.Player.LeftRed);
        LoadUserRebinds(ip.Player.RightRed);

        LoadUserRebinds(ip.Player.Enter);
        LoadUserRebinds(ip.Player.Entertwo);

        LoadUserRebinds(ip.Player.Esc);
        LoadUserRebinds(ip.Player.Esctwo);
    }

    public void ResetBindings()
    {
        reset = true;

        ip.Player.UpBlue.ApplyBindingOverride(default);
        ip.Player.DownBlue.ApplyBindingOverride(default);
        ip.Player.RightBlue.ApplyBindingOverride(default);
        ip.Player.LeftBlue.ApplyBindingOverride(default);

        ip.Player.UpRed.ApplyBindingOverride(default);
        ip.Player.DownRed.ApplyBindingOverride(default);
        ip.Player.RightRed.ApplyBindingOverride(default);
        ip.Player.LeftRed.ApplyBindingOverride(default);

        ip.Player.Enter.ApplyBindingOverride(default);
        ip.Player.Entertwo.ApplyBindingOverride(default);

        ip.Player.Esc.ApplyBindingOverride(default);
        ip.Player.Esctwo.ApplyBindingOverride(default);

        SaveAllUserRebinds();
    }

    public void OnUpBlue(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnTest(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnDownBlue(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnRightBlue(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnLeftBlue(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnUpRed(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnDownRed(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnRightRed(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnLeftRed(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnEnter(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnEntertwo(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnEsc(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnEsctwo(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
}
