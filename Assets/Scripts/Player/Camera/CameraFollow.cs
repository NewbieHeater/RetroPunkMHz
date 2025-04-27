using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject Player;

    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    // Start is called before the first frame update
    void Start()
    {
        minX = -50f;
        maxX = 500f;
        minY = -50f;
        maxY = 50f;

        transform.position = Player.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Player != null)
        {
            var position = Player.transform.position;
            float posX = Mathf.Clamp(position.x, minX, maxX);
            float posY = Mathf.Clamp(position.y, minY, maxY);
            transform.position = new Vector3(posX, posY, -15f);
        }
    }
}
