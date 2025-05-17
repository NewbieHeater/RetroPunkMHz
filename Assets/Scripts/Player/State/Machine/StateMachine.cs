
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    private T m_sender;

    //현재 상태를 담는 프로퍼티
    public IState<T> CurState { get; set; }


    //기본 상태를 생성시에 설정하게 생성자 선언
    public StateMachine(T sender, IState<T> state)
    {
        m_sender = sender;
        SetState(state);
    }

    public void SetState(IState<T> state)
    {
        // null에러출력
        if (m_sender == null)
        {
            Debug.LogError("m_sender ERROR");
            return;
        }

        if (CurState == state)
        {
            return;
        }

        if (CurState != null)
            CurState.OperateExit(m_sender);

        //상태 교체.
        CurState = state;

        //새 상태의 Enter를 호출한다.
        if (CurState != null)
            CurState.OperateEnter(m_sender);

        //Debug.Log("SetNextState : " + state);
    }

    public void DoOperateUpdate()
    {
        if (m_sender == null)
        {
            Debug.LogError("invalid m_sener");
            return;
        }
        CurState.OperateUpdate(m_sender);
    }
    public void DoOperateFixedUpdate()
    {
        if (m_sender == null)
        {
            Debug.LogError("invalid m_sener");
            return;
        }
        CurState.OperateFixedUpdate(m_sender);
    }
}
