using UnityEngine;

/// <summary>
/// Special fruit that can be sliced multiple times for bonus points.
/// Used for Pomegranate (multi-hit) and Dragonfruit (bonus points).
/// </summary>
public class SpecialFruit : MonoBehaviour
{
    public GameObject whole;
    public GameObject sliced;

    private Rigidbody fruitRigidbody;
    private Collider fruitCollider;
    private ParticleSystem juiceEffect;

    [Header("Points")]
    [SerializeField] private int pointsPerHit = 1;
    [SerializeField] private int maxHits = 10; // For pomegranate-style multi-hit
    [SerializeField] private bool isMultiHit = true; // Can be hit multiple times

    [Header("Visual Feedback")]
    [SerializeField] private float hitScaleMultiplier = 0.95f; // Shrink on each hit
    [SerializeField] private Color hitFlashColor = Color.white;

    [Header("Missed Fruit Detection")]
    [SerializeField] private float missedYPosition = -10f;

    private int currentHits = 0;
    private bool hasBeenSliced = false;
    private bool hasMissed = false;
    private Vector3 originalScale;
    private Renderer fruitRenderer;
    private Color originalColor;

    private void Awake()
    {
        fruitRigidbody = GetComponent<Rigidbody>();
        fruitCollider = GetComponent<Collider>();
        juiceEffect = GetComponentInChildren<ParticleSystem>();
        originalScale = transform.localScale;

        fruitRenderer = whole.GetComponentInChildren<Renderer>();
        if (fruitRenderer != null)
        {
            originalColor = fruitRenderer.material.color;
        }
    }

    private void Update()
    {
        // Check if fruit fell below screen (missed)
        if (!hasBeenSliced && !hasMissed && transform.position.y < missedYPosition)
        {
            hasMissed = true;
            OnMissed();
        }
    }

    private void OnMissed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnFruitMissed();
        }
    }

    private void Slice(Vector3 direction, Vector3 position, float force)
    {
        if (!isMultiHit && hasBeenSliced) return;

        currentHits++;
        GameManager.Instance.IncreaseScore(pointsPerHit);

        // Play juice effect
        if (juiceEffect != null)
        {
            juiceEffect.Play();
        }

        // Visual feedback - flash and shrink
        if (fruitRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }

        if (isMultiHit)
        {
            // Shrink fruit with each hit
            transform.localScale = originalScale * Mathf.Pow(hitScaleMultiplier, currentHits);

            // Check if max hits reached
            if (currentHits >= maxHits)
            {
                FinishSlicing(direction, position, force);
            }
        }
        else
        {
            // Single hit fruit - slice immediately
            hasBeenSliced = true;
            FinishSlicing(direction, position, force);
        }
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        if (fruitRenderer != null)
        {
            fruitRenderer.material.color = hitFlashColor;
            yield return new WaitForSeconds(0.05f);
            fruitRenderer.material.color = originalColor;
        }
    }

    private void FinishSlicing(Vector3 direction, Vector3 position, float force)
    {
        hasBeenSliced = true;

        // Disable the whole fruit
        fruitCollider.enabled = false;
        whole.SetActive(false);

        // Enable the sliced fruit
        if (sliced != null)
        {
            sliced.SetActive(true);

            // Rotate based on the slice angle
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            sliced.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Rigidbody[] slices = sliced.GetComponentsInChildren<Rigidbody>();

            // Add a force to each slice based on the blade direction
            foreach (Rigidbody slice in slices)
            {
                slice.velocity = fruitRigidbody.velocity;
                slice.AddForceAtPosition(direction * force, position, ForceMode.Impulse);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Blade blade = other.GetComponent<Blade>();
            Slice(blade.direction, blade.transform.position, blade.sliceForce);
        }
    }
}
