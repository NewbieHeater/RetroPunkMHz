using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float range = 2f;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out var hit, range))
            {
                var reverb = hit.collider.GetComponentInParent<ReverbObject>();
                if (reverb != null) reverb.Interact();
            }
        }
    }
}
