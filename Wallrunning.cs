using System.Collections;
using UnityEngine;

public class Wallrunning : MonoBehaviour
{
    private FPController player;
    private CharacterController characterController;
    private void OnValidate()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (player == null)
        {
            player = GetComponent<FPController>();
        }
    }

    public void Update()
    {
        // if not climbing, check wall running and then wall climbing
        if (!player.isClimbing)
        {
            // check wall run first, if not check if can, if so keep wall running
            if (!player.isWallRunning)
                CheckForWallRun();
            else
            {
                HandleWallRun();
                return;
            }
        }
    }

    // check for wall to run on here
    void CheckForWallRun()
    {
        // return if wall running conditions not met; cant wall run, am climing, am grounded
        if (!player.canWallRun || player.isClimbing || characterController.isGrounded)
        {
            return;
        }


        if (!player.JumpInputHeld) return;


        if (!player.canWallRun)
            Debug.LogWarning("WALLRUN DISABLED — stuck?");
        // origin is right here
        Vector3 origin = transform.position + Vector3.up * 1.0f;

        // left checks for a left wall, right checks for a right wall
        bool left = Physics.Raycast(origin, -transform.right, out RaycastHit leftHit, player.WallRunCheckDistance, player.WallRunMask);
        bool right = Physics.Raycast(origin, transform.right, out RaycastHit rightHit, player.WallRunCheckDistance, player.WallRunMask);

        // if both walls are there, kinda corner logic here, choose your favorite!
        if (left && right)
        {
            // use dots to figure out whats better to ride on based on whats forward
            float leftDot = Mathf.Abs(Vector3.Dot(transform.forward, -leftHit.normal));
            float rightDot = Mathf.Abs(Vector3.Dot(transform.forward, -rightHit.normal));
            if (leftDot > rightDot)
            {
                StartWallRun(leftHit.normal, true);
            }
            else
            {
                StartWallRun(rightHit.normal, false);
            }
        }
        //otherwise left wall maybe:
        else if (left)
        {
            StartWallRun(leftHit.normal, true);
        }
        // def right then right
        else if (right)
        {
            StartWallRun(rightHit.normal, false);
        }
    }

    void StartWallRun(Vector3 normal, bool leftSide)
    {
        // prevent rapid sideswitching
        if (player.isWallRunning && Vector3.Dot(normal, player.wallRunNormal) < 0.3f)
        {
            return;
        }
        // wall running true, cant run, wall normal is the wall, timer is duration and vertical velocity is 0
        player.isWallRunning = true;
        player.canWallRun = false;
        player.wallRunNormal = normal;
        player.wallRunTimer = player.WallRunDuration;
        player.VerticalVelocity = 0f;

    }

    // handling wall run
    void HandleWallRun()
    {
        Debug.Log("wall running...");
        // if not holding jump, stop the run
        if (!player.JumpInputHeld)
        {
            Debug.Log("no jump held");
            StopWallRun();
            if (!player.isWallRunning) return;
        }
        Debug.Log("jump held");

        // if out of time or on the ground, stop the run
        player.wallRunTimer -= Time.deltaTime;
        if (player.wallRunTimer <= 0f || characterController.isGrounded)
        {
            StopWallRun();
            if (!player.isWallRunning) return;
        }

        // this is direction on the wall, using walls direction here
        Vector3 alongWall = Vector3.Cross(player.wallRunNormal, Vector3.up);
        // switch if other way
        if (Vector3.Dot(alongWall, transform.forward) < 0f)
        {
            alongWall = -alongWall;
        }

        // use velocity in this forward direction
        Vector3 moveDir = Vector3.ProjectOnPlane(player.horizontalVelocity, player.wallRunNormal);


        // counter gravity here a little bit using a lifting force
        player.VerticalVelocity = Mathf.MoveTowards(player.VerticalVelocity, 0f, 10f * Time.deltaTime);
        player.VerticalVelocity += Physics.gravity.y * player.WallRunGravity * Time.deltaTime;

        // combine it all here
        Vector3 velocity = moveDir + Vector3.up * player.VerticalVelocity;

        // apply the velocity and move with it
        characterController.Move(velocity * Time.deltaTime);

        // check for ground again cause this can be buggy
        if (characterController.isGrounded)
        {
            StopWallRun();
            if (!player.isWallRunning) return;
        }

        // stick to walk a lil bit
        characterController.Move(-player.wallRunNormal * (player.WallStickForce * 0.005f) * Time.deltaTime);


        // if not looking right or moved too far, stop the run
        if (!Physics.Raycast(transform.position + Vector3.up * 1f, -player.wallRunNormal, player.WallRunCheckDistance, player.WallRunMask))
        {
            StopWallRun();
            if (!player.isWallRunning) return;
        }

        // if you move backwards in the run, cancel <--- feel free to remove this
        if (player.MoveInput.y < 0f)
            StopWallRun();
    }

    //stopping wall run
    void StopWallRun(bool preserveMomentum = false)
    {
        //if already not watchu doing here bruh
        if (!player.isWallRunning)
        {
            return;
        }

        //set states to false
        player.isWallRunning = false;
        player.canWallRun = false;

        if (!preserveMomentum)
        {
            // a little push from wall so player doesnt get stuck in it
            Vector3 pushOff = player.wallRunNormal * 3f; // < ------- tweak this if it feels weird
            player.horizontalVelocity += pushOff;

            // little vertical oomph too for fun
            if (player.VerticalVelocity < 0f)
                player.VerticalVelocity = 0.5f;

        }

        // wallruncooldown call
        Invoke(nameof(ResetWallRun), player.WallRunCooldown);
    }


    //cooldown ends we go here
    public void ResetWallRun() => player.canWallRun = true;


    // wall run jump logic
    public void WallRunJump()
    {
        if (!player.isWallRunning)
            return;

        // stop wallrun WITHOUT adding pushOff
        StopWallRun(preserveMomentum: true);

        // clean outward jump values
        float outwardForce = player.WallRunJumpForce * 2f;
        float upwardForce = player.WallRunJumpForce * 2f;
        float forwardForce = Mathf.Clamp(player.horizontalVelocity.magnitude, 4f, 12f) * 0.6f;

        Vector3 outward = player.wallRunNormal * outwardForce;
        Vector3 upward = Vector3.up * upwardForce;
        Vector3 forward = transform.forward * forwardForce;

        Vector3 jumpVel = outward + upward + forward;

        // apply velocities
        player.horizontalVelocity = new Vector3(jumpVel.x, 0f, jumpVel.z);
        player.VerticalVelocity = jumpVel.y;

        // light along-wall assist (not required but this feels good)
        Vector3 alongWall = Vector3.Cross(player.wallRunNormal, Vector3.up);
        if (Vector3.Dot(alongWall, transform.forward) < 0)
            alongWall = -alongWall;

        player.horizontalVelocity += alongWall * (player.WallRunSpeed * 0.1f);

        // reset dash
        player.dashCooldownTimer = 0f;
    }

}
