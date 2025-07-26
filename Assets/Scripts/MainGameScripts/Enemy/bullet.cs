using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [SerializeField] private float speed = 0f;
    [SerializeField] private float lifeDuration = 2f;

    private float disableTime;

    private void OnEnable()
    {
        disableTime = Time.time + lifeDuration;
    }


    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        if (Time.time > disableTime)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ObjectPooler.ReturnToPool(this.gameObject);
    }
}
