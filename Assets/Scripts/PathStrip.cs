using UnityEngine;

public class PathStrip : MonoBehaviour
{
    [SerializeField] PlayerBall player;
    [SerializeField] Transform goal;
    [SerializeField] Material material;

    const float Thickness = 0.02f;

    Transform strip;
    Renderer stripRenderer;

    void Awake()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "RedPath";
        Destroy(go.GetComponent<Collider>());

        strip = go.transform;
        strip.SetParent(transform, false);

        stripRenderer = go.GetComponent<Renderer>();
        stripRenderer.material = material;

        go.SetActive(false);
    }

    void LateUpdate()
    {
        float width = player.Radius * 2f; 

        Vector3 from = player.transform.position;
        Vector3 flat = goal.position - from;
        flat.y = 0f;
        float length = flat.magnitude;

        bool visible = width > 0.01f && length > 0.01f;
        strip.gameObject.SetActive(visible);

        if (!visible)
        {
            return;
        }

        Vector3 mid = from + flat * 0.5f;
        strip.position = new Vector3(mid.x, 0.011f, mid.z);
        strip.rotation = Quaternion.LookRotation(flat.normalized, Vector3.up);
        strip.localScale = new Vector3(width, Thickness, length);
    }
}