using UnityEngine;


// handles jumping and gravity logic
public class Movement : MonoBehaviour
{
    private FPController player;


    float coyoteTimer;
    float jumpBufferTimer;

    private void OnValidate()
    {
        player = GetComponent<FPController>();
    }
    public void Update()
    {
        ApplyTimers();
        HandleJumpLogic();
        ApplyGravity();
    }


    // all physics timers
    void ApplyTimers()
    {
        if (player.characterController.isGrounded)
        {
            coyoteTimer = 0.15f;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }


        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;
    }

    //queue jump using buffer
    public void QueueJump() => jumpBufferTimer = 0.15f;

    // jump stuffies
    void HandleJumpLogic()
    {
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            jumpBufferTimer = 0;
            coyoteTimer = 0;
            ExecuteJump();
        }
    }

    // jumps!
    void ExecuteJump()
    {
        // jump logic, just jump lol
       player.VerticalVelocity = Mathf.Sqrt(player.JumpHeight * -2f * Physics.gravity.y * player.GravityScale);
    }

    public void ApplyGravity()
    {
        if (player.characterController.isGrounded && player.VerticalVelocity < 0)
        {
            player.VerticalVelocity = -2f;
            return;
        }

        float gravity = 9.81f * player.GravityScale;

        if (player.VerticalVelocity > 0)
        {
            gravity *= player.FallMultiplier;
        }


        gravity *= player.launchableCharacter.GetGravityMultiplier();

        player.VerticalVelocity -= gravity * Time.deltaTime;
    }
}
