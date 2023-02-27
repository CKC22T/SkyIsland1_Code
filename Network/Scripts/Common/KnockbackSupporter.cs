using System.Collections;
using UnityEngine;

public class KnockbackSupporter : MonoBehaviour
{
    [SerializeField] private SphereCollider mCollider;

    public void Initialize(float createDelay, float radius)
    {
        mCollider.radius = radius;
        mCollider.isTrigger = true;
        StartCoroutine(knockbackRoutine(createDelay));
    }

    private IEnumerator knockbackRoutine(float createDelay)
    {
        yield return new WaitForSeconds(createDelay);
        mCollider.isTrigger = false;
        yield return null;
        Destroy(gameObject);
    }
}