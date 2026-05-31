using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingSequence : MonoBehaviour
{
    [Header("Settings")]
    public float fadeSpeed = 0.3f;

    // Hidden variables
    private Image voidScreen;
    private FPSController playerScript;
    private AudioSource playerFootsteps;

    private bool isEnding = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isEnding)
        {
            isEnding = true;

            // 1. Grab Player
            playerScript = other.GetComponent<FPSController>();
            playerFootsteps = other.GetComponent<AudioSource>();

            // 2. Grab the UI directly from the GameManager!
            if (GameManager.instance != null && GameManager.instance.voidScreenUI != null)
            {
                GameManager.instance.voidScreenUI.SetActive(true);
                voidScreen = GameManager.instance.voidScreenUI.GetComponent<Image>();
            }
            else
            {
                Debug.LogWarning("The GameManager is missing the Void Screen UI!");
            }

            StartCoroutine(SwallowedByTheVoid());
        }
    }

    private IEnumerator SwallowedByTheVoid()
    {
        // 1. Paralyze the player
        if (playerScript != null) playerScript.canMove = false;
        if (playerFootsteps != null) playerFootsteps.Stop();

        // 2. FORCE the screen to be visible and reset Alpha to 0
        if (voidScreen != null)
        {
            voidScreen.enabled = true; // Guarantees the image component is on

            Color screenColor = voidScreen.color;
            screenColor.a = 0f; // Force it to start completely transparent!
            voidScreen.color = screenColor;

            // Slowly fade to solid black
            while (voidScreen.color.a < 1f)
            {
                screenColor.a += Time.deltaTime * fadeSpeed;
                voidScreen.color = screenColor;
                yield return null;
            }
        }

        Debug.Log("The Loop has collapsed. Game Over.");
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}