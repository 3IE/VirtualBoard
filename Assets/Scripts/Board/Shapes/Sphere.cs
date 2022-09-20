using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Board.Shapes
{
    public class Sphere : Shape
    {
        protected override void Move()
        {
            XRRayInteractor interactor = Interactors[0] as XRRayInteractor;

            if (interactor!.TryGetCurrent3DRaycastHit(out RaycastHit hit) &&
                hit.colliderInstanceID != ColliderId &&
                hit.distance < InitialDistance)
                transform.position = hit.point;
            else
                transform.position = interactor.transform.position + interactor.transform.forward * InitialDistance;
            //throw new System.NotImplementedException();
        }

        protected override void Resize()
        {
            if (Interactors[0].transform.position == Interactors[1].transform.position)
                return;
            
            transform.localScale = 
                InitialScale / InitialDistance
                * UnityEngine.Vector3.Distance(Interactors[0].transform.position, Interactors[1].transform.position);
        }
    }
}