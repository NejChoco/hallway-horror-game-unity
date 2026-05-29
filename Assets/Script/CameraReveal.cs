using UnityEngine;
using UnityEngine.UI;

public class CameraReveal : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera playerCamera;
    public KeyCode cameraButton = KeyCode.Mouse1; // Right-Click

    [Header("Battery System")]
    public float maxBattery = 100f;
    public float currentBattery;
    public float drainRate = 30f; // Drains completely in ~3.3 seconds
    public float rechargeRate = 10f; // Takes 10 seconds to fully recharge
    public Slider batteryUI;

    [Header("UI & Audio Effects")]
    public GameObject cameraOverlayUI;
    public AudioSource panicAudio;

    private int defaultMask;
    private int revealMask;
    private bool isCameraUp = false;
    private bool isOverheated = false;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        defaultMask = playerCamera.cullingMask;
        int hiddenLayerIndex = LayerMask.NameToLayer("Hidden Anomaly");
        revealMask = defaultMask | (1 << hiddenLayerIndex);

        currentBattery = maxBattery;
        if (batteryUI != null) batteryUI.maxValue = maxBattery;

        TurnOffCamera();
    }

    void Update()
    {
        if (!isCameraUp)
        {
            currentBattery += rechargeRate * Time.deltaTime;
            if (currentBattery > maxBattery) currentBattery = maxBattery;
            if (currentBattery > 20f) isOverheated = false;
        }

        if (batteryUI != null) batteryUI.value = currentBattery;

        if (Input.GetKeyDown(cameraButton) && !isOverheated)
        {
            TurnOnCamera();
        }

        if (Input.GetKey(cameraButton) && isCameraUp)
        {
            currentBattery -= drainRate * Time.deltaTime;

            if (currentBattery <= 0)
            {
                currentBattery = 0;
                CameraDied();
            }
        }

        if (Input.GetKeyUp(cameraButton) && isCameraUp)
        {
            TurnOffCamera();
        }
    }

    void TurnOnCamera()
    {
        isCameraUp = true;
        playerCamera.cullingMask = revealMask;
        if (cameraOverlayUI != null) cameraOverlayUI.SetActive(true);
        if (panicAudio != null) panicAudio.Stop();
    }

    void TurnOffCamera()
    {
        isCameraUp = false;
        playerCamera.cullingMask = defaultMask;
        if (cameraOverlayUI != null) cameraOverlayUI.SetActive(false);
    }

    void CameraDied()
    {
        isOverheated = true;
        TurnOffCamera();
        if (panicAudio != null) panicAudio.Play();
    }
}