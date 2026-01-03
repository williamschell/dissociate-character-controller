using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FPController))]
public class FPPlayer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] FPController Controller;
    [SerializeField] Movement movement;
    [SerializeField] Dash dash;
    [SerializeField] Sliding slide;

    void OnMove(InputValue value)
    {
        Controller.MoveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        Controller.LookInput = value.Get<Vector2>();
    }

    void OnSprint(InputValue value)
    {
        Controller.SprintInput = value.isPressed;
    }

    void OnJump(InputValue value)
    {
        bool pressed = value.isPressed;

        // tell controller whether jump is held
        Controller.SetJumpHeld(pressed);

        // only queue a jump when it *starts* being pressed
        if (pressed)
            movement.QueueJump();
    }

    void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            slide.StartSlide();
        }
        else
        {
            slide.StopSlide();
        }
    }

    void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            dash.TryDash();
        }
    }

    void OnValidate()
    {
        if (Controller == null)
        {
            Controller = GetComponent<FPController>();
        }

        if (movement == null)
        {
            movement = GetComponent<Movement>();
        }

        if (dash == null)
        {
            dash = GetComponent<Dash>();
        }

        if (slide == null)
        {
            slide = GetComponent<Sliding>();
        }
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
