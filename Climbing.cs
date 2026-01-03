using System.Collections;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    private FPController player;
    private CharacterController characterController;
    private Wallrunning running;

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
    }

    public void Update()
    {
        if (player.climbTimer > 0f)
        {
            player.climbTimer -= Time.deltaTime;
        }

        if (!player.isClimbing)
        {
            if (!player.isWallRunning && player.canClimb)
            {
                CheckForWallClimb();
            }
        }

        // if climbing (checked above, will be set to true by now if can)
        if (player.isClimbing)
        {
            // send to handle, then return before any other movement updates
            HandleWallClimb();
            return;
        }
    }

    // --- WALL CLIMB HANDLING ---
    public void CheckForWallClimb()
    {
        // if not holding jump, dont climb
        if (!player.JumpInputHeld)
        {
            return;
        }

        // if wall in front, using a raycast to check
        if (Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, out RaycastHit hit, player.WallCheckDistance, player.WallMask))
        {
            // climb if facing wall
            float facingDot = Vector3.Dot(transform.forward, -hit.normal);
            if (facingDot > 0.85f) // within 30 of perpendicular
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                if (angle > 75f && angle < 100f)
                {
                    StartWallClimb(hit.normal);
                }
            }
        }
    }


    public void StartWallClimb(Vector3 normal)
    {
        // mark starting y, set climing true, lock in wall, set climber timer, and reset vertical velocity
        player.climbStartY = transform.position.y;
        player.isClimbing = true;
        player.wallNormal = normal;
        player.climbTimer = player.ClimbDuration;
        player.VerticalVelocity = 0f;
    }



    public void HandleWallClimb()
    {
        // if wall climb is out of time or not pressed end it
        if (!player.JumpInputHeld || player.climbTimer <= 0f)
        {
            StopWallClimb();
            return;
        }

        // if climbed more than you can, stop wall climb
        if (transform.position.y - player.climbStartY >= player.MaxClimbHeight)
        {
            StopWallClimb();
        }

        // climb up the wall, using climb speed and up vector
        Vector3 climbVel = Vector3.up * player.ClimbSpeed;
        // move up with this velocity
        characterController.Move(climbVel * Time.deltaTime);

        // apply small force toward the wall to maintain contact
        characterController.Move(-player.wallNormal * 0.05f);

        // stop if top reached
        // check if wall in front is gone
        if (!Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, player.WallCheckDistance, player.WallMask))
        {
            // confirm there's ground just ahead and slightly below 
            Vector3 ledgeCheckOrigin = transform.position + transform.forward * 0.6f + Vector3.up * 1.5f;
            bool notHasLedge = !Physics.Raycast(ledgeCheckOrigin, Vector3.down, out _, 1.5f, player.WallMask);

            // if no ledge drop here, if ledge vault over it
            if (notHasLedge)
            {
                StopWallClimb(false);
                return;
            }
            else
            {
                StopWallClimb(true);
                return;
            }
        }

    }

    // stopping the wall climb
    void StopWallClimb(bool reachedTop = false)
    {
        // if not climbing, end here!
        if (!player.isClimbing)
        {
            return;
        }
        // set climbing and canclimb to false
        player.isClimbing = false;
        player.canClimb = false;

        // climbcooldown starts
        Invoke(nameof(ResetClimbCooldown), player.ClimbCooldown);

        // if stuff from stopping earlier is true, then vault the ledge
        if (reachedTop)
        {
            StartCoroutine(LedgeVault());
        }
    }


    // ledge vault stuffies
    IEnumerator LedgeVault()
    {
        // start is this position right here
        Vector3 start = transform.position;
        // target is up and forward a little bit
        Vector3 targetPos = start + Vector3.up * 1.0f + transform.forward * 1.2f;

        // how long it takes to get there
        // the shorter the time the snappier the climb
        float duration = 0.18f;
        float elapsed = 0f;

        // interpolate position smoothy over the duration
        while (elapsed < duration)
        {
            // add time to elapsed
            elapsed += Time.deltaTime;
            // t is percent of time left
            float t = Mathf.Clamp01(elapsed / duration);
            // step over the period of time left
            float eased = Mathf.SmoothStep(0f, 1f, t);

            // update player position during the vault
            transform.position = Vector3.Lerp(start, targetPos, eased);
            // and then end this here i suppose
            yield return null;
        }

        // apply addition boost if sprint is held
        float vaultBoostSpeed;
        if (player.SprintInput)
        {
            // if sprinting go quickly here, adjustable using vaultboost setting
            vaultBoostSpeed = Mathf.Lerp(7f, 12f, Mathf.Clamp01(player.CurrentSpeed / player.SprintSpeed)) * player.vaultBoost;
        }
        else
        {
            // apply reduced boost if not sprinting
            vaultBoostSpeed = player.vaultBoost * .25f;
        }

        // caluclate velocity using this boost from above
        player.horizontalVelocity += (transform.forward * vaultBoostSpeed);
        player.VerticalVelocity += 3.5f + vaultBoostSpeed * 0.1f;


        // cooldown before climbing again
        player.canClimb = false;
        Invoke(nameof(ResetClimbCooldown), 0.35f);
    }


    // cooldown to set climb to true again
    void ResetClimbCooldown() => player.canClimb = true;

    public void WallClimbJump()
    {
        if (!player.isClimbing)
            return;

        StopWallClimb();

        // setup the direction
        Vector3 backFromWall = player.wallNormal * player.WallClimbJumpOutwardForce;
        Vector3 upward = Vector3.up * player.WallClimbJumpUpwardBoost;
        Vector3 jumpDir = (backFromWall + upward).normalized;

        // apply horizontal push
        player.horizontalVelocity = jumpDir * (player.WallRunJumpForce * 3.0f);
        player.horizontalVelocity += transform.forward * player.WallClimbJumpForwardCarry;

        // start curved vertical arc
        StartCoroutine(WallClimbJumpArc());

        // cooldowns
        player.canClimb = false;
        player.canWallRun = false;
        Invoke(nameof(ResetClimbCooldown), 0.5f);
        running.ResetWallRun();

        // 180 degree spin, so player can jump back and forth
        StartCoroutine(RotateCameraBack());
    }


    IEnumerator RotateCameraBack()
    {
        //how long this rotation takes
        float duration = 0.35f;
        float elapsed = 0f;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, transform.eulerAngles.y + 180f, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        transform.rotation = targetRot;
    }


    IEnumerator WallClimbJumpArc()
    {
        float elapsed = 0f;

        while (elapsed < player.wallClimbJumpDuration)
        {
            elapsed += Time.deltaTime;
            Debug.Log("wall climb arc");
            // normalized time 0 -> 1
            float t = Mathf.Clamp01(elapsed / player.wallClimbJumpDuration);

            // evaluate curve to get current lift fraction
            float liftFactor = player.wallClimbJumpCurve.Evaluate(t);

            // apply to vertical velocity
            player.VerticalVelocity = (player.wallClimbJumpHeight * liftFactor);

            yield return null;
        }

        // once the arc finishes let normal gravity take over
    }


}

