using NUnit.Framework;
using UnityEngine;

public class LookAtCameraTests
{
    private GameObject cameraGO;
    private GameObject targetGO;
    private LookAtCamera lookAtCamera;

    [SetUp]
    public void Setup()
    {
        // Creiamo e marchiamo la Camera come MainCamera
        cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        cameraGO.AddComponent<Camera>();

        // Creiamo l'oggetto target
        targetGO = new GameObject("Target");
        lookAtCamera = targetGO.AddComponent<LookAtCamera>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(cameraGO);
        Object.DestroyImmediate(targetGO);
    }

    [Test]
    public void LateUpdate_RotatesTowardsCamera()
    {
        cameraGO.transform.position = new Vector3(0, 0, 10);
        targetGO.transform.position = Vector3.zero;

        lookAtCamera.LateUpdate();

        // Forward del target deve puntare (circa) verso la camera
        Vector3 forward = targetGO.transform.forward;
        Vector3 toCamera = (cameraGO.transform.position - targetGO.transform.position).normalized;

        float dot = Vector3.Dot(forward, toCamera);
        Assert.Greater(dot, 0.99f, "Target is not facing the camera correctly.");
    }

    [Test]
    public void LateUpdate_DoesNothing_WhenNoCamera()
    {
        Object.DestroyImmediate(cameraGO); // rimuoviamo la camera

        Quaternion initialRotation = targetGO.transform.rotation;

        lookAtCamera.LateUpdate();

        // La rotazione non deve cambiare
        Assert.AreEqual(initialRotation, targetGO.transform.rotation);
    }

    [Test]
    public void LateUpdate_PreservesUpDirection()
    {
        // Ruotiamo l'oggetto così che abbia un "up" inclinato
        targetGO.transform.rotation = Quaternion.Euler(0, 0, 45);

        cameraGO.transform.position = new Vector3(0, 0, 10);
        targetGO.transform.position = Vector3.zero;

        lookAtCamera.LateUpdate();

        // Dopo l'allineamento, l'up deve essere (circa) lo stesso
        Vector3 originalUp = targetGO.transform.up;
        Vector3 newUp = targetGO.transform.up;

        Assert.AreEqual(originalUp.normalized, newUp.normalized);
    }
}
