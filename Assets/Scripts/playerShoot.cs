using UnityEngine;
using UnityEngine.InputSystem;

public class playerShoot : MonoBehaviour
{
    public Camera playerCamera;         // assign in Inspector
    public float shootRange = 100f;     // ray length
    public LayerMask targetLayer = ~0;  // default everything (set to your target layer)
    public GameObject popEffect;        // optional VFX prefab

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Fire.performed += ctx => TryShoot();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void TryShoot()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("[playerShoot] playerCamera not assigned.");
            return;
        }

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);

        Debug.DrawRay(ray.origin, ray.direction * shootRange, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, shootRange, targetLayer, QueryTriggerInteraction.Collide))
        {
            // try the generic TargetBox first
            TargetBox tb = hit.collider.GetComponent<TargetBox>();
            if (tb != null)
            {
                tb.Pop();
            }
            else
            {
                // fallback: check specific components
                if (hit.collider.TryGetComponent<Fruit>(out var fruit))
                {
                    fruit.OnShot();
                }
                else if (hit.collider.TryGetComponent<Bomb>(out var bomb))
                {
                    // Bomb may explode on shot
                    bomb.OnShot(); // ensured to exist in bomb class below
                }
                else if (hit.collider.TryGetComponent<Medkit>(out var medkit))
                {
                    medkit.OnShot();
                }
                else
                {
                    // nothing recognized
                    Debug.Log($"Hit {hit.collider.name} but no target component found.");
                }
            }

            if (popEffect != null)
                Instantiate(popEffect, hit.point, Quaternion.identity);
        }
    }
}
