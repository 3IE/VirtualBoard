using UnityEngine;

namespace Board
{
    /// <summary>
    ///     Utility class tied to an object used to adjust the height of the board
    /// </summary>
    public class Lever : MonoBehaviour
    {
        [SerializeField] private GameObject board;

        private HingeJoint _joint;

        // Start is called before the first frame update
        private void Start()
        {
            _joint = GetComponent<HingeJoint>();
        }

        // Update is called once per frame
        private void Update()
        {
            float   angle    = _joint.angle    + 60;
            float   y        = angle / 120 * 3 - 1.5f;
            Vector3 position = board.transform.position;

            position.y               = y;
            board.transform.position = position;
        }
    }
}