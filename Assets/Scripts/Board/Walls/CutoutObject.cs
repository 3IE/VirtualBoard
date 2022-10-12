using UnityEngine;

namespace Board.Walls
{
    /// <summary>
    ///     Utility class used to update the wall's shader
    /// </summary>
    public class CutoutObject : MonoBehaviour
    {
        private static readonly int CutoutPos   = Shader.PropertyToID("_CutoutPos");
        private static readonly int CutoutSize  = Shader.PropertyToID("_CutoutSize");
        private static readonly int FalloffSize = Shader.PropertyToID("_FalloffSize");

        [SerializeField]
        private Transform targetObject;

        [SerializeField]
        private LayerMask wallMask;

        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            Vector2 cutoutPos  = _mainCamera.WorldToViewportPoint(targetObject.position);
            Vector3 offset     = targetObject.position - transform.position;
            var     hitObjects = new RaycastHit[5];

            cutoutPos.y /= Screen.width / Screen.height;

            int size = Physics.RaycastNonAlloc(transform.position, offset, hitObjects,
                                               offset.magnitude, wallMask);

            for (var i = 0; i < size; ++i)
            {
                Material[] materials = hitObjects[i].transform.GetComponent<Renderer>().materials;

                foreach (Material m in materials)
                {
                    m.SetVector(CutoutPos, cutoutPos);
                    m.SetFloat(CutoutSize,  0.1f);
                    m.SetFloat(FalloffSize, 0.05f);
                }
            }
        }
    }
}