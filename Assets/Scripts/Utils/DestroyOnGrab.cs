using UnityEngine;

public class DestroyOnGrab : MonoBehaviour
{
    public void OnGrab()
    {
        Destroy(gameObject);
    }
}
