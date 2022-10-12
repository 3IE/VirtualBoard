using System;
using TMPro;
using UnityEngine;
using DeviceType = Utils.DeviceType;

public class PlayerListEntry : MonoBehaviour
{
    private TMP_Text nameText;
    private TMP_Text typeText;
    public  int      playerID { get; private set; }

    private void OnEnable()
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        Debug.Log($"texts: {texts}");
        nameText = texts[0];
        typeText = texts[1];
    }

    public void Initialize(DeviceType type, int id, string user)
    {
        playerID      = id;
        nameText.text = user;

        typeText.text = type switch
                        {
                            DeviceType.VR       => "VR",
                            DeviceType.AR       => "AR",
                            DeviceType.Hololens => "MR",
                            _                   => throw new ArgumentOutOfRangeException(nameof(type), type, null),
                        };
    }
}