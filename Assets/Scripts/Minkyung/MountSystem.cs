using UnityEngine;

public class MountSystem : MonoBehaviour
{
    [Header("필수 참조")]
    public Transform player;           // 플레이어 Transform
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    // 플레이어 이동 스크립트 이름을 여기서 지정
    // Rigidbody 기반 등 본인 캐릭터 이동 스크립트를 넣으세요.
    public MonoBehaviour playerMoveScript;

    private bool isMounted = false;
    private Transform currentMount;

    void Update()
    {
        if (!isMounted)
        {
            // E키를 누르면 주변 Mount 태그 오브젝트 찾기
            if (Input.GetKeyDown(interactKey))
            {
                Collider[] hits = Physics.OverlapSphere(player.position, interactDistance);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Mount"))
                    {
                        Mount(hit.transform);
                        break;
                    }
                }
            }
        }
        else
        {
            // 하차
            if (Input.GetKeyDown(interactKey))
            {
                Dismount();
            }
        }
    }

    void Mount(Transform mount)
    {
        currentMount = mount;


        player.SetParent(mount);
        player.localPosition = new Vector3(0, 1f, 0);
        player.localRotation = Quaternion.identity;


        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        if (playerMoveScript != null) playerMoveScript.enabled = false;


        var cubeController = mount.GetComponent<MountCubeController>();
        if (cubeController) cubeController.canControl = true;

        isMounted = true;
    }

    void Dismount()
    {
        // 부모 해제
        player.SetParent(null);


        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;


        if (playerMoveScript != null) playerMoveScript.enabled = true;


        if (currentMount != null)
        {
            var cubeController = currentMount.GetComponent<MountCubeController>();
            if (cubeController) cubeController.canControl = false;
        }

        isMounted = false;
    }
}
