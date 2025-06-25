using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class computerpaddle : paddle
{
    public Rigidbody2D ball;

    private float sonic = 0.0f;
    private void FixedUpdate()
    {
 
        if (this.ball.velocity.x > sonic)
        {
            if(this.ball.position.y > this.transform.position.y)
            {
                rigid.AddForce(Vector2.up * this.speed);
                
            }
            else if (this.ball.position.y < this.transform.position.y)
            {
                rigid.AddForce(Vector2.down * this.speed);
                
            }
        }
        else
        {
            if(this.transform.position.y > sonic)
            {
                rigid.AddForce(Vector2.down * this.speed);
                
            }
            else if(this.transform.position.y< sonic)
            {
                rigid.AddForce(Vector2.up * this.speed);
                
            }
        }
        
    }
}
