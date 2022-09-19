using UnityEngine;

public class BoardCollider : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tool"))
            other.GetComponent<WritingTool>().rotationLocked = true;
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tool"))
            other.GetComponent<WritingTool>().rotationLocked = false;
    }
}
