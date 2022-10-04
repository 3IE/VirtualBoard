using UnityEngine;

public class Console : MonoBehaviour
{
    private void Awake()
    {
        #if !UNITY_EDITOR
        Destroy(gameObject);
        #endif
    }
}
