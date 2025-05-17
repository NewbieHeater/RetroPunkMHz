using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExplosionInteract
{
    // 읽기 전용 프로퍼티로 선언
    int RequiredAmpPts { get; }
    int RequiredPerPts { get; }
    int RequiredWavPts { get; }

    void OnExplosionInteract(Channel channel);
}
