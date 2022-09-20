using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class Sphere : Shape
    {
        private float radius = 1;

        /// <summary>
        /// Moves the object to where the controller points
        /// TODO: Check potential collisions at the new position
        /// </summary>
        protected override void Move()
        {
            RaycastHit[] hits = Physics.RaycastAll(Interactors[0].transform.position,
                Interactors[0].transform.forward, InitialDistance,
                LayerMask.GetMask("Default", "Player"));

            bool positionFound = false;

            for (int i = hits.Length - 1; i >= 0 && !positionFound; i--)
            {
                Vector3 position = hits[i].point + hits[i].normal * radius / 2;

                if (!Physics.CheckSphere(position, radius / 2, LayerMask.GetMask("Default", "Player")))
                {
                    transform.position = position;
                    positionFound = true;
                }
            }

            if (!positionFound)
                transform.position = Interactors[0].transform.position +
                                     Interactors[0].transform.forward * InitialDistance;

            SendTransform();
        }

        protected override void Resize()
        {
            if (Interactors[0].transform.position == Interactors[1].transform.position)
                return;

            transform.localScale =
                InitialScale / InitialDistance
                * Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);
            radius = transform.localScale.x;

            SendTransform();
        }
    }
}