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
    public GameObject cameraOverlayUI; // Your camcorder/lens UI screen
    public AudioSource panicAudio; // Plays when the battery dies

    private int defaultMask;
    private int revealMask;
    private bool isCameraUp = false;
    private bool isOverheated = false;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        defaultMask = playerCamera.cullingMask;

        // This is the magic line that lets the camera see the ghosts!
        int hiddenLayerIndex = LayerMask.NameToLayer("Hidden Anomaly");
        revealMask = defaultMask | (1 << hiddenLayerIndex);

        currentBattery = maxBattery;
        if (batteryUI != null) batteryUI.maxValue = maxBattery;

        TurnOffCamera();
    }

    void Update()
    {
        // 1. Recharge the battery when the camera is down
        if (!isCameraUp)
        {
            currentBattery += rechargeRate * Time.deltaTime;
            if (currentBattery > maxBattery) currentBattery = maxBattery;

            // Cooldown: Camera won't work again until it hits 20% charge
            if (currentBattery > 20f) isOverheated = false;
        }

        if (batteryUI != null) batteryUI.value = currentBattery;

        // 2. Turn camera ON
        if (Input.GetKeyDown(cameraButton) && !isOverheated)
        {
            TurnOnCamera();
        }

        // 3. Drain battery while holding Right-Click
        if (Input.GetKey(cameraButton) && isCameraUp)
        {
            currentBattery -= drainRate * Time.deltaTime;

            if (currentBattery <= 0)
            {
                currentBattery = 0;
                CameraDied();
            }
        }

        // 4. Turn camera OFF when letting go
        if (Input.GetKeyUp(cameraButton) && isCameraUp)
        {
            TurnOffCamera();
        }
    }

    void TurnOnCamera()
    {
        isCameraUp = true;
        playerCamera.cullingMask = revealMask; // Reveal ghosts
        if (cameraOverlayUI != null) cameraOverlayUI.SetActive(true);
        if (panicAudio != null) panicAudio.Stop();
    }

    void TurnOffCamera()
    {
        isCameraUp = false;
        playerCamera.cullingMask = defaultMask; // Hide ghosts again
        if (cameraOverlayUI != null) cameraOverlayUI.SetActive(false);
    }

    void CameraDied()
    {
        isOverheated = true;
        TurnOffCamera();
        if (panicAudio != null) panicAudio.Play();
    }
}