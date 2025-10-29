using UnityEngine;

public class TargetBox : MonoBehaviour
{
    // This tries to call the right behaviour depending on the attached component
    public void Pop()
    {
        if (TryGetComponent<Fruit>(out var fruit))
        {
            fruit.OnShot();
            return;
        }

        if (TryGetComponent<Bomb>(out var bomb))
        {
            bomb.OnShot();
            return;
        }

        if (TryGetComponent<Medkit>(out var medkit))
        {
            medkit.OnShot();
            return;
        }

        // fallback: just destroy
        Destroy(gameObject);
    }
}
