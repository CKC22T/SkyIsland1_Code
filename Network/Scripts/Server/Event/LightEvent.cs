using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEvent : BaseLocationEventTrigger
{
    public GameObject light;
    public Transform lightDirTo;

    public override void TriggeredEvent(BaseEntityData other)
    {
        StartCoroutine(boxMove());

        IEnumerator boxMove()
        {
            float time = 0;
            var from = light.transform.rotation;
            while (time <= 1.0f)
            {
                time += Time.deltaTime;

                light.transform.rotation = Quaternion.Slerp(from, lightDirTo.rotation, time);

                yield return new WaitForEndOfFrame();
            }
        }

        GetComponentInParent<BoxCollider>().enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
