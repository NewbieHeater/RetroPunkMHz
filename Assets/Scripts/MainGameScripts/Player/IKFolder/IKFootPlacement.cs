using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKFootPlacement : MonoBehaviour
{
    [Header("Grounding")]
    public LayerMask groundLayers;
    [Range(0f, 2f)] public float rayExtraHeight = 0.8f;
    [Range(0.05f, 0.5f)] public float sphereRadius = 0.15f;
    [Range(0f, 0.3f)] public float snapDistance = 0.08f; // 이 거리 이내면 지면에 붙임
    [Range(0f, 1f)] public float footHeightOffset = 0.05f; // 지면 위로 띄우기

    [Header("Weights & Smoothing")]
    [Range(0f, 1f)] public float plantedWeight = 0.95f;
    [Range(0f, 1f)] public float swingWeight = 0.15f;
    [Range(0.01f, 0.3f)] public float posSmoothTime = 0.06f;  // 발 위치 스무딩
    [Range(0.01f, 0.3f)] public float rotLerp = 0.12f;        // 발 회전 보간 계수

    [Header("Pelvis")]
    [Range(0f, 0.3f)] public float pelvisDropMax = 0.12f; // cliffDrop의 상한
    [Range(0.01f, 0.5f)] public float pelvisSmoothTime = 0.18f;

    [Header("Swing/Plant Heuristics")]
    [Tooltip("발이 이 높이(본의 로컬 Y)보다 낮고, 수직 속도가 작으면 플랜트로 간주")]
    public float plantLocalYThreshold = -0.02f;
    public float verticalSpeedThreshold = 0.02f;

    private Animator anim;
    private Quaternion defLeftToeLocal, defRightToeLocal;

    private float basePelvisY;
    private float pelvisY, pelvisVel;

    struct FootState
    {
        public AvatarIKGoal goal;
        public HumanBodyBones footBone;
        public HumanBodyBones toeBone;

        public Vector3 lastPlantedPos;
        public Vector3 posVel;
        public Vector3 lastNormal;
        public bool planted;
        public float currentWeight;
        public float lastFootLocalY;
    }

    private FootState left, right;

    void Start()
    {
        anim = GetComponent<Animator>();

        defLeftToeLocal = anim.GetBoneTransform(HumanBodyBones.LeftToes).localRotation;
        defRightToeLocal = anim.GetBoneTransform(HumanBodyBones.RightToes).localRotation;

        basePelvisY = transform.localPosition.y;
        pelvisY = basePelvisY;

        left = NewFoot(AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes);
        right = NewFoot(AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot, HumanBodyBones.RightToes);
    }

    FootState NewFoot(AvatarIKGoal goal, HumanBodyBones foot, HumanBodyBones toe)
    {
        return new FootState
        {
            goal = goal,
            footBone = foot,
            toeBone = toe,
            lastPlantedPos = anim.GetIKPosition(goal),
            lastNormal = Vector3.up,
            planted = false,
            currentWeight = 0f,
            lastFootLocalY = 0f
        };
    }

    void Update()
    {
        // 골반 스무딩 이동
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            Mathf.SmoothDamp(transform.localPosition.y, pelvisY, ref pelvisVel, pelvisSmoothTime),
            transform.localPosition.z
        );
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!anim) return;

        // 각 발 업데이트
        UpdateFoot(ref left);
        UpdateFoot(ref right);

        // 골반 드롭(둘 다 닿으면 높이 차, 한쪽만 닿으면 제한된 드롭)
        float lY = left.lastPlantedPos.y;
        float rY = right.lastPlantedPos.y;

        bool lValid = left.planted;
        bool rValid = right.planted;

        float drop;
        if (lValid && rValid) drop = Mathf.Min(Mathf.Abs(lY - rY) * 0.5f, pelvisDropMax);
        else if (lValid ^ rValid) drop = pelvisDropMax * 0.7f; // 한쪽만 닿으면 적당히
        else drop = 0f;

        pelvisY = basePelvisY - drop;
    }

    void UpdateFoot(ref FootState f)
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            if (!GameManager.Instance.player.IsGrounded)
                return;
        }
        // 현재 본 로컬 Y로 “스윙 vs 플랜트” 판정 보조
        Transform footT = anim.GetBoneTransform(f.footBone);
        float footLocalY = footT.localPosition.y;
        float vSpeed = (footLocalY - f.lastFootLocalY) / Mathf.Max(Time.deltaTime, 1e-4f);
        f.lastFootLocalY = footLocalY;

        // 캐스트 시작점: 현재 IK 목표 + 위로
        Vector3 ikPos = anim.GetIKPosition(f.goal);
        Vector3 origin = ikPos + Vector3.up * rayExtraHeight;
        //Debug.Log(ikPos);
        // 스피어캐스트로 부드럽게
        RaycastHit hit;
        bool hitOk = Physics.SphereCast(origin, sphereRadius, Vector3.down, out hit,
                                        rayExtraHeight + 1.0f, groundLayers, QueryTriggerInteraction.Ignore);
        //Debug.Log(hitOk);
        // 지면까지 거리
        float groundDist = hitOk ? (ikPos.y - hit.point.y) : float.MaxValue;

        // 플랜트 판단:
        //  - 지면 감지 & 지면까지 거리가 snapDistance 이내
        //  - 발의 로컬Y가 낮고(=지면에 가까움), 수직속도가 작음(멈춤)
        bool wantPlant = hitOk
                         && (groundDist <= snapDistance)
                         && (footLocalY <= plantLocalYThreshold)
                         && (Mathf.Abs(vSpeed) <= verticalSpeedThreshold);

        // 플레이어가 공중이면 강제 스윙
        

        if (wantPlant)
        {
            f.planted = true;
            // 목표 위치/노멀
            Vector3 targetP = hit.point + Vector3.up * footHeightOffset;
            f.lastNormal = Vector3.Lerp(f.lastNormal, hit.normal, 0.25f);
            // 부드럽게 고정점으로
            f.lastPlantedPos = Vector3.SmoothDamp(f.lastPlantedPos, targetP, ref f.posVel, posSmoothTime);
            // 가중치 올리기
            f.currentWeight = Mathf.MoveTowards(f.currentWeight, plantedWeight, Time.deltaTime * 6f);
        }
        else
        {
            // 스윙: 최근 고정점은 살짝 풀고, 가중치 내리기
            f.planted = false;
            f.currentWeight = Mathf.MoveTowards(f.currentWeight, swingWeight, Time.deltaTime * 6f);
            // 스윙 동안엔 위치를 애니메이션에 더 맡기므로 lastPlantedPos를 현재 IK 근처로 천천히 복귀
            f.lastPlantedPos = Vector3.SmoothDamp(f.lastPlantedPos, ikPos, ref f.posVel, posSmoothTime * 1.2f);
            f.lastNormal = Vector3.Lerp(f.lastNormal, Vector3.up, 0.1f);
        }

        // IK 적용
        anim.SetIKPositionWeight(f.goal, f.currentWeight);
        anim.SetIKRotationWeight(f.goal, f.currentWeight);

        // 위치
        anim.SetIKPosition(f.goal, f.lastPlantedPos);

        // 회전: 지면 법선 정렬(너무 급한 회전은 rotLerp로 보간)
        Vector3 fwd = footT.forward;
        Vector3 slopeFwd = Vector3.ProjectOnPlane(fwd, f.lastNormal).normalized;
        if (slopeFwd.sqrMagnitude < 1e-4f) slopeFwd = Vector3.ProjectOnPlane(Vector3.forward, f.lastNormal).normalized;
        Quaternion targetRot = Quaternion.LookRotation(slopeFwd, f.lastNormal);
        Quaternion blended = Quaternion.Slerp(footT.rotation, targetRot, rotLerp);
        anim.SetIKRotation(f.goal, blended);

        // 발가락은 기본 로컬 회전으로 리셋 (오른발/왼발 각각 올바르게)
        if (f.toeBone == HumanBodyBones.LeftToes)
            anim.SetBoneLocalRotation(HumanBodyBones.LeftToes, defLeftToeLocal);
        else
            anim.SetBoneLocalRotation(HumanBodyBones.RightToes, defRightToeLocal);
    }
}
