using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DeleteHandGesture : MonoBehaviour
{
    [SerializeField] XRRayInteractor rayinteractor;

    public void DeleteAnchor()
    {
        if (rayinteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                GameObject anchor = hit.collider.gameObject;

                var assignedUser = UserManager.instance.Users.Find(u => u.Anchor == anchor);
                if (assignedUser != null)
                {
                    assignedUser.Anchor = null;
                    UserManager.instance.AddWaitingUser(assignedUser);
                }

                AnchorManager.instance.RemoveAnchor(anchor);
            }
        }
    }
}