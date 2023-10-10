using Mirror;
using System.Linq;
using UnityEngine;

public class PartyCamera : MonoBehaviour
{
    Party party;
    Status status;

    public State currentState;
    GameObject LastArrivingPlayer;

    float zVelocity;

    public enum State
    {
        startOverview,
        startCamPan,
        playerFocus,
        pillarWalk,
        pillarPan,
        endOverview,
        end,
    }

    private void Start()
    {
        if (NetworkClient.active && !NetworkServer.active) { Destroy(this); }

        party = FindObjectOfType<Party>();
        status = FindObjectOfType<Status>();

        currentState = State.startOverview;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.startOverview:
                {
                    if (!party.doneSetup)
                    {
                        break;
                    }

                    float PlayerPosZ = party.PlayersMovements[0].transform.position.z;
                    float speed = party.fieldCount * 0.4f;
                    LeanTween.moveLocalZ(gameObject, PlayerPosZ, speed).setEaseInOutCubic();

                    currentState = State.startCamPan;
                    break;
                }

            case State.startCamPan:
                {
                    if (!LeanTween.isTweening(gameObject))
                    {
                        float speed = 1f;

                        LeanTween.moveLocalX(gameObject, 7.5f, speed).setEaseInOutCubic();
                        LeanTween.moveLocalY(gameObject, 10, speed).setEaseInOutCubic();
                        LeanTween.rotateX(gameObject, 45, speed).setEaseInOutCubic();

                        currentState = State.playerFocus;
                    }
                    break;
                }

            case State.playerFocus:
                {
                    int currentPlayerMoving = Mathf.Clamp(party.currentPlayerMoving, 0, status.playerNumber - 1);

                    FocusPlayer(party.PlayersMovements[currentPlayerMoving].transform.position.z);

                    if (currentPlayerMoving == status.playerNumber - 1)
                    {
                        if (party.PlayersMovements[currentPlayerMoving].state == MovementParty.State.winMove)
                        {
                            LastArrivingPlayer = party.PlayersMovements.OrderBy(move => move.transform.position.z).First().gameObject;

                            currentState = State.pillarWalk;
                        }
                        if (party.PlayersMovements[currentPlayerMoving].state == MovementParty.State.finishedMove)
                        {
                            currentState = State.endOverview;
                        }
                    }
                    break;
                }

            case State.pillarWalk:
                {
                    if (LastArrivingPlayer.transform.position.z > -15)
                    {
                        LeanTween.moveLocalX(gameObject, 0, 2f).setEaseInOutCubic();
                        LeanTween.moveLocalY(gameObject, 7, 2f).setEaseInOutCubic();
                        LeanTween.moveLocalZ(gameObject, 5, 5f).setEaseOutCubic();
                        LeanTween.rotateX(gameObject, 10, 3.5f).setEaseInOutCubic();
                        LeanTween.rotateY(gameObject, 0, 2f).setEaseInOutCubic();

                        party.InvokeSpawnMenuButton();

                        currentState = State.pillarPan;
                    }
                    else
                    {
                        FocusPlayer(LastArrivingPlayer.transform.position.z);
                    }

                    break;
                }

            case State.endOverview:
                {
                    float speed = 2;
                    LeanTween.moveLocalX(gameObject, 13f, speed).setEaseInOutCubic();
                    LeanTween.moveLocalY(gameObject, 20, speed).setEaseInOutCubic();
                    LeanTween.rotateX(gameObject, 60, speed).setEaseInOutCubic();

                    currentState = State.end;
                    break;
                }
        }
    }

    void FocusPlayer(float playerZPos)
    {
        float cameraZPos = transform.position.z;
        float maxVelo = 40;

        float CamNewZPos = Mathf.SmoothDamp(cameraZPos, playerZPos, ref zVelocity, .1f, maxVelo);

        transform.LeanSetPosZ(CamNewZPos);
    }
}
