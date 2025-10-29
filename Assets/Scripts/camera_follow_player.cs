using UnityEngine;
public class FollowCam : MonoBehaviour
{
    public Transform target; // Player_TP or a CameraPivot child
    public Vector3 offset = new Vector3(0f, 1.8f, -3.5f);
    public float smooth = 8f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smooth);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
