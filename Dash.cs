using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Dash : MonoBehaviour
{
    private FPController player;
    private CharacterController characterController;
    private Wallrunning running;
    private Climbing climb;

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

        if (running == null)
        {
            running = GetComponent<Wallrunning>();
        }

        if (climb == null)
        {
            climb = GetComponent<Climbing>();
        }
    }

    public void Update()
    {
        // cooldown
        if (player.dashCooldownTimer > 0f)
        {
            player.dashCooldownTimer -= Time.deltaTime;
        }
        // timer
        if (player.dashing)
        {
            player.dashTimer -= Time.deltaTime;
        }

        // dont dash and slide together, else keep dashing
        if (player.dashing)
        {
            if (player.sliding)
            {
                StopDash();
                return;
            }

            ApplyDash();
        }
    }

    // try dashing
    public void TryDash()
    {
        // if wall running, dash is a wall jump
        if (player.isWallRunning)
        {
            running.WallRunJump();
            return;
        }

        if (player.isClimbing)
        {
            climb.WallClimbJump();
            return;
        }

        if (player.dashCooldownTimer > 0 || player.dashing)
        {
            return;
        }


        // start dash
        player.dashing = true;
        player.dashTimer = player.DashDuration;
        player.dashCooldownTimer = player.DashCooldownDuration;

        Vector3 forward = player.fpCamera != null ? player.fpCamera.transform.forward : player.transform.forward;
        player.dashDirXZ = Vector3.ProjectOnPlane(forward.normalized, Vector3.up);

        player.horizontalVelocity += player.dashDirXZ * player.DashForce;

        if (player.VerticalVelocity < 0f)
        {
            player.VerticalVelocity = forward.y * player.DashForce;
        } else
        {
            player.VerticalVelocity += forward.y * player.DashForce;
        }
    }

    private void ApplyDash()
    {
        // clamp t to the percent of time left in dash
        float t = Mathf.Clamp01(1f - (player.dashTimer / player.DashDuration));
        // dash curve - use the point in the curve based on the t percentage to find the strength of this dash at that percent
        float curve = player.DashCurve != null ? player.DashCurve.Evaluate(t) : 1f;
        // strength of dash in this particular frame using the curve
        float strengthPerFrame = player.DashForce * curve * Time.deltaTime;
        // horizontal movement multiplier is based on dash this frame
        player.horizontalVelocity += player.dashDirXZ * strengthPerFrame;

        // if magnitude is big do this
        if (player.wish.sqrMagnitude > 0.001f)
        {
            // set horizontal velocity to a point between the current velocity and the velocity at the given angle, based on the time
            player.horizontalVelocity = Vector3.Lerp(player.horizontalVelocity, player.wish * player.horizontalVelocity.magnitude, 0.35f * Time.deltaTime);
        }

        // if dashtimer is over, end dash
        if (player.dashTimer <= 0f)
        {
            player.dashing = false;
        }
    }

    private void StopDash()
    {
        player.dashing = false;
    }
}
