using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DestroyAnchor : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        // Destroy when grabbed
        //grabInteractable.selectEntered.AddListener(OnGrabbed);

        grabInteractable.activated.AddListener(OnActivated);
    }

    private void OnDisable()
    {
        //grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.activated.RemoveListener(OnActivated);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (AnchorManager.instance != null)
        {
            AnchorManager.instance.RemoveAnchor(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnActivated(ActivateEventArgs args)
    {
        var assignedUser = UserManager.instance.Users.Find(u => u.Anchor == this.gameObject);
        if (assignedUser != null)
        {
            assignedUser.Anchor = null;
            UserManager.instance.AddWaitingUser(assignedUser);
        }

        AnchorManager.instance.RemoveAnchor(this.gameObject);


    }
}