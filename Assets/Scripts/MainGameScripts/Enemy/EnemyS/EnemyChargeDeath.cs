using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public sealed class EnemyChargeDeath : MonoBehaviour
{
    [Header("Charged-Death Explosion")]
    [SerializeField] private LayerMask _explodeOnHitMask;
    [SerializeField] private float _chargedDeathTimeout = 4.0f;

    [Header("Explosion Radius (0이면 미사용)")]
    [SerializeField] private float _explosionRadius = 0f;

    private bool _armed;
    private Rigidbody _rigid;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();
    }

    public void StartFlight(Vector3 dir, float force)
    {
        _armed = true;

        Vector3 n = (dir.sqrMagnitude > 1e-6f) ? dir.normalized : Vector3.right;

        _rigid.isKinematic = false;
        _rigid.useGravity = true;
        _rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rigid.velocity = new Vector3(0f, _rigid.velocity.y, 0f);
        _rigid.AddForce(n * force, ForceMode.Impulse);

        StartCoroutine(TimeoutRoutine());
    }

    private IEnumerator TimeoutRoutine()
    {
        float t0 = Time.time;
        while (_armed && Time.time - t0 < _chargedDeathTimeout)
            yield return null;

        _armed = false;
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision c)
    {
        if (!_armed) return;

        int otherLayer = c.gameObject.layer;
        if (((1 << otherLayer) & _explodeOnHitMask) == 0)
            return;

        if (c.collider.CompareTag("Ground"))
            return;

        _armed = false;

        if (_explosionRadius > 0f)
            Explode();
        else
            gameObject.SetActive(false);
    }

    private void Explode()
    {
        var hits = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<IExplosionInteract>(out var r))
                r.OnExplosionInteract(ChannelManager.Instance.CurrentChannel);
        }

        Destroy(gameObject);
    }
}
