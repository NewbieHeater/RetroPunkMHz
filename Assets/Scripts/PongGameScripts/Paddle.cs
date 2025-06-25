using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paddle : MonoBehaviour
{
    protected Rigidbody2D rigid;
    public float speed = 10.0f;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void ResetPosition()
    {
        rigid.position = new Vector2(rigid.position.x, 0.0f);
        rigid.velocity = Vector2.zero;
    }
}
