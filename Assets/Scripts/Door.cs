using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] GameConfig config;
    [SerializeField] Transform target;
    [SerializeField] Transform leftHinge;
    [SerializeField] Transform rightHinge;
    [SerializeField] float openAngle = 100f;
    [SerializeField] float openSpeed = 2f;

    bool open;
    float progress;

    void Update()
    {
        if (!open && FlatDistanceToTarget() <= config.doorOpenDistance)
        {
            open = true;
        }

        progress = Mathf.MoveTowards(progress, open ? 1f : 0f, openSpeed * Time.deltaTime);

        float angle = Mathf.SmoothStep(0f, openAngle, progress);

        leftHinge.localRotation = Quaternion.Euler(0f, -angle, 0f);
        rightHinge.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    float FlatDistanceToTarget()
    {
        Vector3 diff = target.position - transform.position;
        diff.y = 0f;
        return diff.magnitude;
    }
}
