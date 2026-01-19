using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turntable : MonoBehaviour
{
    [SerializeField] private PlayerHp _playerhp;
    private float _healtime = 5f;

    private void Update()
    {
        /*if ( _playerhp.hp < 100)
        {
            _playerhp.hp = _healtime * Time.deltaTime;
        }*/
    }
        
    
}
