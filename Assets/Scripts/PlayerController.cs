using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Input input;
    Rigidbody rb;

    public float moveSpeed;
    public float jumpForce;
    public float grav = 0.5f;
    public Vector2 moveInput;

    public WorldController controller;

    void Awake(){
        input = new Input();

        input.Gameplay.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Gameplay.Movement.canceled += ctx => moveInput = Vector2.zero;

        input.Gameplay.Jump.performed += ctx => Jump();
        input.Gameplay.Shoot.performed += ctx => Shoot();

        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate(){
        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y + grav);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    void Shoot()
    {
        controller.UpdateChunk(new Vector2(transform.position.x, transform.position.y));
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }
}
