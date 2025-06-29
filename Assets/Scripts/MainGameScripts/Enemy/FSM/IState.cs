using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void OperateEnter();
    void OperateUpdate();
    void OperateFixedUpdate();
    void OperateExit();

}