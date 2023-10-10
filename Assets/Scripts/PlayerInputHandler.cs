using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MovementDebug;

    public PlayerInput playerInput;
    private InputS inputS;
    private Status status;
    private Notificaton n;
    public InputAction MHB;
    public InputAction MVB;

    public InputAction MHR;
    public InputAction MVR;

    int id;

    private void Start()
    {
        inputS = FindObjectOfType<InputS>();
        status = FindObjectOfType<Status>();
        n = FindObjectOfType<Notificaton>();

        id = playerInput.playerIndex - 1;

        print("Player " + id + "has joined");
        inputS.bindingsS.currenPlayerIndex = playerInput.playerIndex;

        gameObject.transform.tag = "InputPlayer" + id;
        name = "PlayerInputGamepad" + id;

        if (id == 0)
        {
            MHB = playerInput.actions["MHB"];
            MVB = playerInput.actions["MVB"];
        }
        else
        {
            MHR = playerInput.actions["MHR"];
            MVR = playerInput.actions["MVR"];
        }
    }

    private void Update()
    {
        playerInput.actions.Enable();

        if (id == 0)
        {
            if (inputS.reset == true)
            {
                MHB.ApplyBindingOverride(default);
                MVB.ApplyBindingOverride(default);
                inputS.resetCounter = 1;
                inputS.SaveUserRebinds(MHB);
                inputS.SaveUserRebinds(MVB);
            }
        }
        else
        {
            status.useGamepads = true;

            if (inputS.reset == true && inputS.resetCounter == 1)
            {
                MHR.ApplyBindingOverride(default);
                MVR.ApplyBindingOverride(default);
                inputS.resetCounter = 0;
                inputS.SaveUserRebinds(MHR);
                inputS.SaveUserRebinds(MVR);
                inputS.reset = false;
            }
        }
    }

    public void SaveUserRebinds(InputAction action)
    {
        print("Saved binding: " + action.name + "New Binding is: " + action.SaveBindingOverridesAsJson());
        var rebinds = action.SaveBindingOverridesAsJson();

        PlayerPrefs.SetString("rebinds" + action.name, rebinds);
        print("Save: " + PlayerPrefs.GetString("rebinds" + action.name));
    }

    public void LoadUserRebinds(InputAction action)
    {
        var rebinds = PlayerPrefs.GetString("rebinds" + action.name);
        action.LoadBindingOverridesFromJson(rebinds);
        print("Load: " + PlayerPrefs.GetString("rebinds" + action.name));
    }

    public void OnDisconnect()
    {
        n.NotificationWith2Colors(new Color(1, 0, 0, 0), 2f, FindObjectOfType<Methods>().GetPlayerName(id), "Controller disconnected", FindObjectOfType<Methods>().GetPlayerColor(id));
    }

    public void OnReconnect()
    {
        n.NotificationWith2Colors(new Color(0, 1, 0, 0), 2f, FindObjectOfType<Methods>().GetPlayerName(id), "Controller reconnected", FindObjectOfType<Methods>().GetPlayerColor(id));
    }

    //public void OnX(CallbackContext contex)
    //{
    //    if (contex.action.triggered)
    //    {
    //        print("X Button was presed");
    //    }
    //}

    public void OnMoveBlueHorizontal(CallbackContext contex)
    {
        if (id == 0)
        {
            inputS.MHBlueG = contex.action.ReadValue<float>();

            inputS.rightB = contex.ReadValue<float>();
            inputS.leftB = contex.ReadValue<float>();

            MovementDebug.x = contex.action.ReadValue<float>();
        }
        print(contex.action.ReadValue<float>() + id);
    }

    public void OnMoveBlueVertical(CallbackContext contex)
    {
        if (id == 0)
        {
            inputS.MVBlueG = contex.action.ReadValue<float>();

            inputS.upB = contex.ReadValue<float>();
            inputS.downB = contex.ReadValue<float>();

            MovementDebug.y = contex.action.ReadValue<float>();
        }
    }


    public void OnMoveRedHorizontal(CallbackContext contex)
    {
        if (id == 1)
        {
            inputS.MHRedG = contex.action.ReadValue<float>();

            inputS.rightR = contex.ReadValue<float>();
            inputS.leftR = contex.ReadValue<float>();
        }
    }

    public void OnMoveRedVertical(CallbackContext contex)
    {
        if (id == 1)
        {
            inputS.MVRedG = contex.action.ReadValue<float>();

            inputS.upR = contex.ReadValue<float>();
            inputS.downR = contex.ReadValue<float>();
        }
    }

}
