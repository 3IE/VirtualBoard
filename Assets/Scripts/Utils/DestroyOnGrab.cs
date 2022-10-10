using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Utility class used to destroy an object when grabbed
    /// </summary>
    public class DestroyOnGrab : MonoBehaviour
    {
        /// <summary>
        /// Called when the object is grabbed
        /// </summary>
        public void OnGrab()
        {
            Destroy(gameObject);
        }
    }
}
