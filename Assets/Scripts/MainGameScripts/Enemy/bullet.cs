using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [SerializeField] private float speed = 0f;
    void OnEnable()
    {
        Destroy(gameObject, 5f);
    }


    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
