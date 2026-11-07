using System.Collections;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] Color infectedColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] ParticleSystem popEffect;

    Renderer rend;
    Collider body;
    bool infected;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        body = GetComponent<Collider>();
    }

    public void Infect(float delay)
    {
        if (infected)
        {
            return;
        }

        infected = true;
        StartCoroutine(PopRoutine(delay));
    }

    IEnumerator PopRoutine(float delay)
    {
        rend.material.color = infectedColor;
        yield return new WaitForSeconds(delay);

        body.enabled = false;

        if (popEffect != null)
        {
            Instantiate(popEffect, transform.position, Quaternion.identity);
        }

        const float duration = 0.2f;
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            transform.localScale = startScale * (1f + 0.5f * k) * (1f - k * k);
            yield return null;
        }

        Destroy(gameObject);
    }
}
