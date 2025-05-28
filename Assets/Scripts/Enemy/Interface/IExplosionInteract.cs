using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExplosionInteract
{
    // �б� ���� ������Ƽ�� ����
    int RequiredAmpPts { get; }
    int RequiredPerPts { get; }
    int RequiredWavPts { get; }

    void OnExplosionInteract(Channel channel);
}
