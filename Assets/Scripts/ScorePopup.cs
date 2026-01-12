using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Floating score popup that appears when fruit is sliced.
/// Shows points earned and floats upward before fading out.
/// </summary>
public class ScorePopup : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private float lifetime = 1f;

    private Color originalColor;
    private float timer = 0f;

    public void Initialize(int score, Vector3 worldPosition, bool isCombo = false)
    {
        if (scoreText != null)
        {
            scoreText.text = "+" + score;

            if (isCombo)
            {
                scoreText.color = Color.yellow;
                scoreText.fontSize = (int)(scoreText.fontSize * 1.5f);
            }

            originalColor = scoreText.color;
        }

        // Convert world position to screen position
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
            transform.position = screenPos;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Float upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime * 50f;

        // Fade out
        if (scoreText != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
            scoreText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        // Destroy after lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
