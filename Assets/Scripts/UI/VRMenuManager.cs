using System.Collections.Generic;
using UnityEngine;
using DeviceType = Utils.DeviceType;

public class VRMenuManager : MonoBehaviour
{
    [Header("PlayerList")] public GameObject playerListLayout;

    public  GameObject            playerListEntryPrefab;
    private List<PlayerListEntry> playerListEntries;


    public void NewPlayer(DeviceType deviceType, int id, string user)
    {
        GameObject entry = Instantiate(playerListEntryPrefab, playerListLayout.transform);
        var        cmp   = entry.GetComponent<PlayerListEntry>();
        cmp.Initialize(deviceType, id, user);
        playerListEntries.Add(cmp);
    }

    public void RemovePlayer(int id)
    {
        PlayerListEntry entry = playerListEntries.Find(x => x.playerID == id);
        playerListEntries.Remove(entry);
        Destroy(entry.gameObject);
    }
}