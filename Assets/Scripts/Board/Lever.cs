using UnityEngine;

public class Lever : MonoBehaviour
{
    [SerializeField]
    private GameObject board;

    private HingeJoint joint;

    // Start is called before the first frame update
    void Start()
    {
        joint = GetComponent<HingeJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        float angle = joint.angle + 60;
        float y = angle / 120 * 3 + .5f;
        Vector3 position = board.transform.position;
        position.y = y;
        board.transform.position = position;
    }
}
