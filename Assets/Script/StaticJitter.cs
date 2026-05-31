using UnityEngine;

public class StaticJitter : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("Settings")]
    public float jitterSpeed = 0.05f; // How fast it shakes
    public float shakeAmount = 25f; // How far it jumps around

    private float timer;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // We make the image 20% larger than the screen so when it shakes, 
        // you don't see the edges of the image peeking out!
        rectTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Every few milliseconds, randomly snap the image to a new location
        if (timer >= jitterSpeed)
        {
            timer = 0;

            float randomX = Random.Range(-shakeAmount, shakeAmount);
            float randomY = Random.Range(-shakeAmount, shakeAmount);

            // Move the image
            rectTransform.anchoredPosition = new Vector2(randomX, randomY);

            // Randomly flip the image left/right and up/down to make it look even more chaotic
            float scaleX = Random.value > 0.5f ? 1.2f : -1.2f;
            float scaleY = Random.value > 0.5f ? 1.2f : -1.2f;
            rectTransform.localScale = new Vector3(scaleX, scaleY, 1.2f);
        }
    }
}