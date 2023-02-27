using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisDisappear : MonoBehaviour
{
    private float StayDelay = 5;
    private float DisappearDelay = 20;

    private float delay = 0;
    private Vector3 initialLocalSacle = Vector3.one;

    public void Start()
    {
        initialLocalSacle = transform.localScale;
        StartCoroutine(disappear());
    }

    private IEnumerator disappear()
    {
        delay = DisappearDelay;

        yield return new WaitForSeconds(StayDelay);

        while (delay > 0)
        {
            delay -= Time.deltaTime;

            float scale = delay / DisappearDelay;
            transform.localScale = initialLocalSacle * scale;
            yield return null;
        }

        Destroy(gameObject);
    }
}
