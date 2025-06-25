using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerpaddel : paddle
{
    private Vector2 _direction;
    public PongGameManger GameManger;
    private void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            _direction = Vector2.up;
        }
        else if(Input.GetKey(KeyCode.S))
        {
            _direction = Vector2.down;
        }
        else if(Input.GetKeyDown(KeyCode.R))
        {
            this.GameManger.Reset();
        }
        else
        {
            _direction = Vector2.zero;
        }


    }

    private void FixedUpdate()
    {
        if(_direction.sqrMagnitude != 0)
        {
            rigid.AddForce(_direction * this.speed);
        }


    }
}
