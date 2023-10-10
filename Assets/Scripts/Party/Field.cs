using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : NetworkBehaviour
{
    [SyncVar] public Type type;

    public Texture2D doubleTexture;
    public Texture2D diceTexture;
    public Texture2D powerUpTexture;

    Status status;
    Party party;

    private void Start()
    {
        party = FindObjectOfType<Party>();
        status = FindObjectOfType<Status>();

        AssignTypeAttributes();
    }

    public void AssignFieldType()
    {
        type = Type.normal;
        int random = Random.Range(0, 100);

        if (random <= 10)
        {
            type = Type.doublePoints;
        }
        if (random > 10 && random <= 25)
        {
            type = Type.dice;
        }
        if (random > 25 && random <= 40)
        {
            type = Type.powerUp;
        }
    }

    private void AssignTypeAttributes()
    {
        switch (type)
        {
            case Type.doublePoints:
                {
                    SetUpFieldMesh(new Color(0, 3f, 4f), doubleTexture, "DoubleText");
                    break;
                }
            case Type.dice:
                {
                    SetUpFieldMesh(Color.red * 2, diceTexture, "DiceText");
                    break;
                }

            case Type.powerUp:
                {
                    SetUpFieldMesh(new Color(1.6f, 1.1f, .08f), powerUpTexture, "PowerUpImage");
                    break;
                }
        }
    }

    private void SetUpFieldMesh(Color GapColor, Texture2D BlendTexture, string UIElemntName)
    {
        GetComponent<MeshRenderer>().material.SetInt("_UseBlendTex", 1);
        GetComponent<MeshRenderer>().material.SetInt("_UseGapNoise", 1);
        GetComponent<MeshRenderer>().material.SetColor("_GapColor", GapColor);
        GetComponent<MeshRenderer>().material.SetTexture("_BlendTexture", BlendTexture);

        transform.GetChild(0).Find(UIElemntName).gameObject.SetActive(true);
    }

    public enum Type
    {
        normal,
        doublePoints,
        powerUp,
        dice
    }
}
