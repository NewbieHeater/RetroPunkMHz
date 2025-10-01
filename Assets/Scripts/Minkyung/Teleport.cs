using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{

    public GameObject targetObj;

    public GameObject toObj;



    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            targetObj = collision.gameObject;
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.CompareTag("Player")&&Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartCoroutine(TeleportRoutine());
        }
    }


    IEnumerator TeleportRoutine()
    {
        yield return null;
        targetObj.transform.position = toObj.transform.position;
    }
}
