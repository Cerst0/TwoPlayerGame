using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerEditor : MonoBehaviour
{
    public GameObject CharacterModel;
    public GameObject CharacterRotModel;
    public int playerIndex;
    Functions functions;
    public float textOverflowX;

    public bool isMenu;
    public TMP_Text text;
    IdDataApply IDA;
    Vector2 Offset;
    Vector3 pointerPosB;
    EventTrigger trigger;
    Status status;

    void Start()
    {
        status = FindObjectOfType<Status>();
        if (!isMenu && status.playerNumber == 2 && playerIndex == 2)
        {
            playerIndex = 1;
        }

        if (isMenu)
        {
            trigger = GetComponent<EventTrigger>();
        }
        else
        {
            trigger = transform.GetChild(2).GetComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry();
        EventTrigger.Entry entryBegin = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Drag;
        entryBegin.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener((data) =>
        {
            OnDragDelegate((PointerEventData)data);
        });
        entryBegin.callback.AddListener((data) => { OnDragBegin((PointerEventData)data); });
        trigger.triggers.Add(entry);
        trigger.triggers.Add(entryBegin);
    }

    public void OnDragBegin(PointerEventData data)
    {
        Offset = data.position;

        pointerPosB = CharacterRotModel.transform.localEulerAngles;
    }

    public void OnDragDelegate(PointerEventData data)
    {
        Vector2 pos = data.position - Offset;
        pos = -pos;
        CharacterRotModel.transform.localEulerAngles = new Vector3(pos.y + pointerPosB.x, pos.x + pointerPosB.y, CharacterRotModel.transform.rotation.z);
    }

    private void Update()
    {
        if (!isMenu)
        {
            if (CharacterModel == null | CharacterRotModel == null)
            {
                try
                {
                    functions = GameObject.FindGameObjectWithTag("Brain").GetComponent<Functions>();

                    CharacterModel = functions.CharacterModels[playerIndex];
                    CharacterRotModel = functions.CharacterRotModels[playerIndex];
                }
                catch { }
            }
        }
        if (isMenu)
        {
            if (CharacterModel == null | CharacterRotModel == null)
            {
                try
                {
                    CharacterRotModel = GameObject.Find("RotateCharacter" + playerIndex);
                }
                catch { }
            }

            if (IDA == null) { IDA = GameObject.FindGameObjectWithTag("IDA" + playerIndex).GetComponent<IdDataApply>(); }
            text.text = IDA.playerName;
            textOverflowX = text.bounds.extents.x;
        }
    }
}
