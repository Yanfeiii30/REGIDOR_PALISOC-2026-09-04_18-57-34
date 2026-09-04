using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class BounceBullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifeTime = 7f;
    public LayerMask hitLayers = ~0;

    private Rigidbody rb;
    private SphereCollider bulletCollider;
    private Renderer bulletRenderer;
    private Vector3 direction;
    private Color bulletColor;
    private string bulletColorName;
    private bool launched;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bulletCollider = GetComponent<SphereCollider>();
        bulletRenderer = GetComponentInChildren<Renderer>();

        rb.useGravity = false;
        rb.isKinematic = true;

        bulletColor = ColorSelector.CurrentColor;
        bulletColorName = ColorSelector.CurrentColorName;

        if (bulletRenderer != null)
        {
            Material material = bulletRenderer.material;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", bulletColor);

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", bulletColor);
        }
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);

        if (!launched)
            Launch(transform.forward);
    }

    public void Launch(Vector3 newDirection)
    {
        direction = newDirection.normalized;
        launched = true;
    }

    void FixedUpdate()
    {
        if (!launched) return;

        float distance = speed * Time.fixedDeltaTime;
        float radius = bulletCollider.radius *
            Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

        RaycastHit[] hits = Physics.SphereCastAll(
            rb.position,
            radius,
            direction,
            distance + 0.02f,
            hitLayers,
            QueryTriggerInteraction.Ignore
        );

        RaycastHit closestHit = new RaycastHit();
        bool foundHit = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            if (!foundHit || hit.distance < closestHit.distance)
            {
                closestHit = hit;
                foundHit = true;
            }
        }

        if (foundHit)
        {
            ColorHitObject(closestHit.collider);
            Destroy(gameObject);
            return;
        }

        rb.MovePosition(rb.position + direction * distance);
    }

    void ColorHitObject(Collider hitObject)
    {
        Renderer[] targets = hitObject.GetComponentsInChildren<Renderer>(true);

        if (targets.Length == 0)
            targets = hitObject.GetComponentsInParent<Renderer>(true);

        foreach (Renderer target in targets)
        {
            Material targetMaterial = target.material;
            Color solidColor = bulletColor;
            solidColor.a = 1f;

            if (targetMaterial.HasProperty("_BaseColor"))
                targetMaterial.SetColor("_BaseColor", solidColor);

            if (targetMaterial.HasProperty("_Color"))
                targetMaterial.SetColor("_Color", solidColor);
        }

        ColorTarget colorTarget = hitObject.GetComponentInParent<ColorTarget>();

        if (colorTarget != null)
        {
            colorTarget.CheckColor(bulletColorName);
        }
    }
}