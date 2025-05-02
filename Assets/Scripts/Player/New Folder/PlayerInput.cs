using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float Horizontal { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool PrimaryAttack { get; private set; }
    public bool SecondaryAttackHeld { get; private set; }
    public float ChargeTimer { get; private set; }

    public void HandleInput()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            JumpPressed = true;

        if (Input.GetMouseButtonDown(0))
            PrimaryAttack = true;

        if (Input.GetMouseButtonDown(1))
        {
            SecondaryAttackHeld = true;
            ChargeTimer = 0f;
        }
        if (Input.GetMouseButton(1) && SecondaryAttackHeld)
            ChargeTimer += Time.deltaTime;
        if (Input.GetMouseButtonUp(1))
            SecondaryAttackHeld = false;
    }

    public void ConsumeJump() => JumpPressed = false;
}
