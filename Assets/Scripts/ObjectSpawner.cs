using UnityEngine;
using System.Collections;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Prefabs (set in Inspector)")]
    public GameObject[] fruitPrefabs; // multiple fruits (apple, banana, etc.)
    public GameObject bombPrefab;
    public GameObject medkitPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 1.2f;

    [Header("Spawn Area / Follow")]
    public bool followPlayer = true;
    public Transform playerTarget;
    public Vector3 areaOffset = new Vector3(0f, 1.5f, 6f); // offset from player
    public Vector3 spawnCenter = Vector3.zero;
    public Vector3 spawnRange = new Vector3(6f, 2f, 6f); // X, Y (height variation), Z

    [Header("Launch Settings (impulse)")]
    public float minLaunchForce = 140f;
    public float maxLaunchForce = 220f;
    public float horizontalForceRange = 3f;

    [Header("Direction Bias (0..1)")]
    [Tooltip("How strongly to push outward away from player vs purely upward")]
    [Range(0f, 1f)] public float outwardBias = 0.75f;
    [Range(0f, 2f)] public float verticalBias = 1.0f;

    [Header("Rotation Settings")]
    public float minTorque = -5f;
    public float maxTorque = 5f;

    [Header("Spawn Probabilities")]
    [Range(0f, 1f)] public float bombChance = 0.25f;
    [Range(0f, 1f)] public float medkitChance = 0.12f;

    bool canSpawn = true;

    void Reset()
    {
        spawnCenter = transform.position;
        areaOffset = new Vector3(0f, 1.5f, 6f);
    }

    void Update()
    {
        // keep spawnCenter following player if needed
        if (followPlayer && playerTarget != null)
        {
            Vector3 offset = playerTarget.TransformDirection(areaOffset);
            spawnCenter = playerTarget.position + offset;
        }

        if (canSpawn) StartCoroutine(SpawnAndShoot());
    }

    IEnumerator SpawnAndShoot()
    {
        canSpawn = false;

        // pick a spawn pos within XZ range; for Y pick spawnCenter.y + Random.Range(0, spawnRange.y)
        Vector3 spawnPos = new Vector3(
            spawnCenter.x + Random.Range(-spawnRange.x, spawnRange.x),
            spawnCenter.y + Random.Range(0f, spawnRange.y),
            spawnCenter.z + Random.Range(-spawnRange.z, spawnRange.z)
        );

        // decide prefab
        float r = Random.value;
        GameObject prefabToSpawn;

        if (r <= medkitChance && medkitPrefab != null)
            prefabToSpawn = medkitPrefab;
        else if (r <= medkitChance + bombChance && bombPrefab != null)
            prefabToSpawn = bombPrefab;
        else
        {
            // pick a random fruit from array
            if (fruitPrefabs != null && fruitPrefabs.Length > 0)
                prefabToSpawn = fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
            else
            {
                Debug.LogWarning("[ObjectSpawner] No fruit prefabs assigned!");
                yield return new WaitForSeconds(spawnInterval);
                canSpawn = true;
                yield break;
            }
        }

        // spawn the object
        GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // ensure collider exists
        Collider objCollider = obj.GetComponent<Collider>();
        if (objCollider == null)
        {
            if (prefabToSpawn == medkitPrefab)
                objCollider = obj.AddComponent<BoxCollider>();
            else
                objCollider = obj.AddComponent<SphereCollider>();
        }
        objCollider.isTrigger = false;

        // ensure Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) rb = obj.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.mass = (rb.mass <= 0f) ? 0.2f : rb.mass;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // vertical launch
        float launchForce = Random.Range(minLaunchForce, maxLaunchForce);
        Vector3 launchDir = Vector3.up;
        rb.AddForce(launchDir * launchForce, ForceMode.Impulse);

        // add random spin torque
        Vector3 randomTorque = new Vector3(
            Random.Range(minTorque, maxTorque),
            Random.Range(minTorque, maxTorque),
            Random.Range(minTorque, maxTorque)
        );
        rb.AddTorque(randomTorque, ForceMode.VelocityChange);

        // convert to trigger after delay for fruit/medkit (so they can be shot/picked up)
        if (prefabToSpawn != bombPrefab)
            StartCoroutine(ConvertToTriggerNextFrame(objCollider));

        yield return new WaitForSeconds(spawnInterval);
        canSpawn = true;
    }

    IEnumerator ConvertToTriggerNextFrame(Collider col)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        if (col != null) col.isTrigger = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (followPlayer && playerTarget != null)
            ? playerTarget.position + playerTarget.TransformDirection(areaOffset)
            : spawnCenter;
        Gizmos.DrawWireCube(center, new Vector3(spawnRange.x * 2, 0.1f, spawnRange.z * 2));
    }
}
