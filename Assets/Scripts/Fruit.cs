using UnityEngine;

public class Fruit : MonoBehaviour
{
    public GameObject whole;
    public GameObject sliced;

    private Rigidbody fruitRigidbody;
    private Collider fruitCollider;
    private ParticleSystem juiceEffect;

    [Header("Points")]
    public int points = 1;

    [Header("Missed Fruit Detection")]
    [SerializeField] private float missedYPosition = -10f;

    private bool hasBeenSliced = false;
    private bool hasMissed = false;

    private void Awake()
    {
        fruitRigidbody = GetComponent<Rigidbody>();
        fruitCollider = GetComponent<Collider>();
        juiceEffect = GetComponentInChildren<ParticleSystem>();
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
        if (hasBeenSliced) return;
        hasBeenSliced = true;

        GameManager.Instance.IncreaseScore(points);

        // Disable the whole fruit
        fruitCollider.enabled = false;
        whole.SetActive(false);

        // Enable the sliced fruit
        sliced.SetActive(true);

        // Play juice effect
        if (juiceEffect != null)
        {
            juiceEffect.Play();
        }

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Blade blade = other.GetComponent<Blade>();
            Slice(blade.direction, blade.transform.position, blade.sliceForce);
        }
    }
}
