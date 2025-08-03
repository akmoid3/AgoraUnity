using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AnchorSpawner : MonoBehaviour
{
    [SerializeField] private ARAnchorManager arAnchorManager;
    [SerializeField] private XRRayInteractor xrRayInteractor;

    //[SerializeField] private XRBaseInteractable simpleInteractable;

    private void Awake()
    {
        //simpleInteractable = GetComponent<XRSimpleInteractable>();
        xrRayInteractor = GetComponent<XRRayInteractor>();

    }

    private void Start()
    {
        //simpleInteractable.selectEntered.AddListener(SpawnAnchor);
        xrRayInteractor.selectEntered.AddListener(SpawnAnchor);
    }

    private async void SpawnAnchor(SelectEnterEventArgs arg0)
    {
        Debug.Log("SpawnAnchor");
        xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
        hit.collider.TryGetComponent(out ARPlane arPlane);
        
        if (!arPlane)
            return;
        
        Pose hitPose = new Pose(hit.point, Quaternion.LookRotation(-hit.normal));

        // if (arAnchorManager == null)
        // {
        //     GameObject prefabSpawned = Instantiate(UserManager.instance.UserPrefab, hitPose.position,
        //         hitPose.rotation);
        //     Debug.Log("ARAnchorManager is null");
        //     return;
        // }

        var result = await arAnchorManager.TryAddAnchorAsync(hitPose);

        if (result.status.IsSuccess())
        {
            ARAnchor anchor = result.value;
            
            GameObject prefabSpawned = Instantiate(UserManager.instance.UserPrefab, anchor.transform.position,
                anchor.transform.rotation);
            prefabSpawned.transform.SetParent(anchor.transform);
            
            AnchorManager.instance.AddAnchor(prefabSpawned);
        }
    }

    // public async void SpawnAnchor(SelectEnterEventArgs args)
    // {
    //     
    //     var interactor = args.interactorObject;
    //
    //     if (interactor is XRRayInteractor rayInteractor)
    //     {
    //         Debug.Log("SpawnAnchor");
    //         rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);
    //         hit.collider.TryGetComponent(out ARPlane arPlane);
    //         if (!arPlane)
    //             return;
    //         Pose hitPose = new Pose(hit.point, Quaternion.LookRotation(-hit.normal));
    //
    //         if (arAnchorManager == null)
    //         {
    //             Debug.Log("ARAnchorManager is null");
    //             return;
    //         }
    //         var result = await arAnchorManager.TryAddAnchorAsync(hitPose);
    //     
    //         if (result.status.IsSuccess())
    //         {
    //             ARAnchor anchor = result.value;
    //             GameObject prefabSpawned = Instantiate(UserManager.instance.UserPrefab, anchor.transform.position, anchor.transform.rotation);
    //             prefabSpawned.transform.SetParent(anchor.transform);
    //         }
    //     }
}