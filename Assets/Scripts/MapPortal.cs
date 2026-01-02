using UnityEngine;

public class MapPortal : MonoBehaviour
{
    public string targetMapId;
    public int targetSpawnId;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Moving to {targetMapId}...");
            // マネージャーに移動を依頼
            MapManager.Instance.LoadMap(targetMapId, targetSpawnId);
        }
    }
}