using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FPController : MonoBehaviour
{
    //other modules
    [Header("other mods")]
    [SerializeField] Movement movement;
    [SerializeField] Dash dash;
    [SerializeField] Climbing climbing;
    [SerializeField] Wallrunning wallrunning;
    [SerializeField] Sliding slide;



    // --- movement settings ---
    // walk speed - duh
    // sprint speed - duh
    // acceleration - the player's acceleration (duh)
    [Header("speed")]
    [SerializeField] float WalkSpeed = 5f;
    [SerializeField] public float SprintSpeed = 12f;
    [SerializeField] float Acceleration = 20f;

    float MaxSpeed => SprintInput ? SprintSpeed : WalkSpeed;

    // --- physics settings and stuff ---
    // jump height - how high you can jump
    // gravity scale - gravity's impact
    // fall multiplier - falling/gravity compounder
    // horizontal velocity - how fast we are moving forward
    // vertical velocity - how fast we are moving up or down
    // current speed - the current speed
    // coyotetimer - think like looney toons wowowowoow, hang over the edge time for smoother jumping
    // jump buffer time - buffer time for the jump
    [Header("physics / gravity")]
    [SerializeField] public float JumpHeight = 2f;
    [SerializeField] public float GravityScale = 3f;
    [SerializeField] public float FallMultiplier = .4f;
    public Vector3 horizontalVelocity;
    public float VerticalVelocity;
    public float CurrentSpeed { get; private set; }


    // --- dash stuff ---
    // dash force - the oomph from dashing
    // dash duration - how long you get that oomph for
    // dash cooldown - duh
    // dash curve - the movement curve for the dash
    // dashing - true or false, whether we are in the state of dashing rn
    // dashdirXZ - horizontal dash direction
    [Header("dash")]
    [SerializeField] public float DashForce = 20f;
    [SerializeField] public float DashDuration = 0.2f;
    [SerializeField] public float DashCooldownDuration = 1f;
    [SerializeField] public AnimationCurve DashCurve;
    public bool dashing;
    public float dashTimer;
    public float dashCooldownTimer;
    public Vector3 dashDirXZ;

    // --- slide stuff ---
    // slide duration - duh
    // slideinitialboost - boost you get from sliding to incentivize doing so
    // slide friction - how much you slow down during slide
    // minslidespeed - minimum speed needed to slide
    // slideturninfluence - clamp on slide turning
    // sliding - true or false, are we sliding rn or nah?
    // slidedirXZ - horizontal direction of slide 
    [Header("slide")]
    [SerializeField] public float SlideDuration = 1.0f;
    [SerializeField] public float SlideInitialBoost = 6f;
    [SerializeField] public float SlideFriction = 1.0f;
    [SerializeField] public float MinSlideSpeed = 4f;
    [SerializeField] public float SlideTurnInfluence = 0.35f;
    public bool sliding;
    public float slideTimer;
    public Vector3 slideDirXZ;
    [Header("Slide (Advanced)")]
    [SerializeField] public float SlopeAcceleration = 15f;
    [SerializeField] public float MaxSlideSpeed = 20f;
    [SerializeField] public float SlopeEndThreshold = 8f;
    [SerializeField] public float SlopeDetectionDistance = 1.5f;
    [SerializeField] public float SlopeBoostMultiplier = 1.2f;


    // --- wall climbing stuff ---
    // wall check distance - how far away you can detect a wall
    // max climb height - what it sounds like you dummy
    // climb speed - how fast you climb
    // climb duration - how long you can climb for
    // climb cooldown - how often you can attempt to climb
    // wall mask - what you can climb
    // vault boost - the boost you get from vaulting
    // climb start - mark the y where you start
    // is climbing - true or false, are we climbing rn or nah
    // can climb - true or false, for enabling/disabling climbing
    // climb timer - keeps track of how long we have been climbing
    // wall normal - vector that is normal to the wall
    [Header("wall climb")]
    [SerializeField] public float WallCheckDistance = 0.7f;
    [SerializeField] public float MaxClimbHeight = 2.5f;
    [SerializeField] public float ClimbSpeed = 3.5f;
    [SerializeField] public float ClimbDuration = 1.5f;
    [SerializeField] public float ClimbCooldown = 0.5f;
    [SerializeField] public LayerMask WallMask;
    [SerializeField] public float vaultBoost = 10f;
    [SerializeField] public float WallClimbJumpUpwardBoost = 1.5f;
    [SerializeField] public float WallClimbJumpOutwardForce = 2.0f;
    [SerializeField] public float WallClimbJumpForwardCarry = 2.5f;
    public float climbStartY;
    public bool isClimbing;
    public bool canClimb = true;
    public float climbTimer;
    public Vector3 wallNormal;


    // -- wall climb jump stuffies ---
    // wall climb jump curve - determines the arc of the wall jump
    // wall climb jump duration - how long you are jumping for
    // wall climb jump height - height of the jump
    [Header("Wall Climb Jump Settings")]
    [SerializeField]
    public AnimationCurve wallClimbJumpCurve = new AnimationCurve(
    new Keyframe(0f, 1f),
    new Keyframe(0.3f, 0.8f),
    new Keyframe(0.6f, 0.4f),
    new Keyframe(1f, 0f)
);
    [SerializeField] public float wallClimbJumpDuration = 0.6f;
    [SerializeField] public float wallClimbJumpHeight = 10f;

    // --- wall run stuff ---
    // wallrun gravity - the pull down while wall running
    // wallrun speed - the speed of wallrunning at base level
    // wallrun duration - how long you can wallrun before falling
    // wallrun cooldown - how often you can attempt to wallrun
    // wallruncheckdistance - how far from wall you can be to check for running
    // wallstickforce - how much you stick to the wall while running on it
    // wallrunjumpforce - how much you jump off the wall
    // wall run mask - what you can wall run on
    // is wall running - true or false
    // can wall run - true or false, for enabling/disabling
    [Header("wall run")]
    [SerializeField] public float WallRunGravity = 1.5f;
    [SerializeField] public float WallRunSpeed = 8f;
    [SerializeField] public float WallRunDuration = 2f;
    [SerializeField] public float WallRunCooldown = 0.3f;
    [SerializeField] public float WallRunCheckDistance = 0.8f;
    [SerializeField] public float WallStickForce = 2f;
    [SerializeField] public float WallRunJumpForce = 8f;
    [SerializeField] public LayerMask WallRunMask;
    public float wallDetachTimer = 0f;

    public bool isWallRunning;
    public float wallRunTimer;
    public bool canWallRun = true;
    public Vector3 wallRunNormal;

    // --- looking/camera stuff ---
    // looksensitivity - duh
    // cinemachinecam - the fp camera
    // camera normal fov - normal fov
    // sprint fov - fov while sprinting
    // camerafovsmoothing - for changing between normal and sprint
    // current pitch - pitch of where looking
    // pitch limit - how much you can look at an angle
    [Header("look/cam")]
    public Vector2 LookSensitivity = new Vector2(0.1f, 0.1f);
    [SerializeField] public CinemachineCamera fpCamera;
    [SerializeField] float CameraNormalFOV = 60f;
    [SerializeField] float CameraSprintFOV = 80f;
    [SerializeField] float CameraFOVSmoothing = 5f;
    float currentPitch = 0f;
    public float PitchLimit = 85f;

    // --- wallrun camera tilt ---
    float tiltAngle = 0f;
    float currentTilt = 0f;

    // other stuff
    // character controller
    // event for landed
    [Header("other")]
    [SerializeField] public CharacterController characterController;
    [SerializeField] UnityEvent Landed;
    [SerializeField] public LaunchableCharacter launchableCharacter;

    // inputs here
    public bool SprintInput;
    public bool JumpInputHeld;
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public Vector3 wish;

    // stuff that happens on validate
    void OnValidate()
    {
        // if no character controller, get this objects character controller, same for all the rest below
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (launchableCharacter == null)
        {
            launchableCharacter = GetComponent<LaunchableCharacter>();
        }

        if (movement == null)
        {
            movement = GetComponent<Movement>();
        }

        if (dash == null)
        {
            dash = GetComponent<Dash>();
        }

        if (climbing == null)
        {
            climbing = GetComponent<Climbing>();
        }

        if (wallrunning == null)
        {
            wallrunning = GetComponent<Wallrunning>();
        }

        if (slide == null)
        {
            slide = GetComponent<Sliding>();
        }
    }

    // update
    void Update()
    {
        // locked for cutscenes
        if (locked) return;
        //update sliding first
        slide.Update();
        //update wallrunning second
        wallrunning.Update();

        // update movement
        MoveUpdate();
        // update physics
        movement.Update();

        dash.Update();

        //update climbing after wallrunning
        climbing.Update();


        LookUpdate();
        // update camera
        CameraUpdate();



    }

    public void SetJumpHeld(bool held) => JumpInputHeld = held;


    // movement update, happens every frame and checks EVERYthing holy lord
    void MoveUpdate()
    {

        //return if doing something else
        if (isWallRunning)
        {
            return;
        }

        if (isClimbing)
        {
            return;
        }

        // wish is which way youre going based on movement inputs
        wish = (transform.forward * MoveInput.y + transform.right * MoveInput.x);
        wish.y = 0f;

        // gets direction by normalizing it to a scale of 1, keeping direction
        if (wish.sqrMagnitude > 0.0001f)
        {
            wish.Normalize();
        }
        if (!sliding)        {
            // reguloar moving stuffies
            if (wish.sqrMagnitude > 0.0001f)
            {
                // find the speed based onw hether sprimting or not
                float targetSpeed = SprintInput ? SprintSpeed : WalkSpeed;
                // desired speed towards direction
                Vector3 desired = wish * targetSpeed;
                // current velocity
                Vector3 currentXZ = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
                // difference
                Vector3 delta = (desired - currentXZ);
                // the accel needed to cover the difference
                Vector3 accel = delta.normalized * Acceleration * Time.deltaTime;
                // add accel to horizontal velocity till target velocity is reached
                horizontalVelocity += accel;

                // magnitude is speed
                float currentSpeedXZ = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z).magnitude;
                if (currentSpeedXZ < MaxSpeed)
                {
                    // if less than maxspeed, clamp the magnitude and allow max speed
                    Vector3 currentXZMovement = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
                    Vector3 desiredMovement = wish * MaxSpeed;

                    // Smoothly accelerate towards desired velocity
                    horizontalVelocity = Vector3.MoveTowards(currentXZMovement, desiredMovement, Acceleration * Time.deltaTime);

                    // After movement, reapply the vertical axis (keep y untouched)
                    horizontalVelocity.y = 0f;
                }
            }

            // when not actively moving, slow down to this extent
            if (wish.sqrMagnitude < 0.001f)
            {
                //increased friction when no magnitude
                float stopFriction = characterController.isGrounded ? 15f : 2f;
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, stopFriction * Time.deltaTime);
            }
            else
            {
                // some drag even when moving, so you slowly move down if no fancy moves are happening
                float moveDrag = characterController.isGrounded ? 2f : 0.2f;
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, moveDrag * Time.deltaTime);
            }

        }

        bool strafing = Mathf.Abs(MoveInput.x) > 0.1f;

        if (!strafing && horizontalVelocity.magnitude > 0.1f)
        {
            Vector3 flatVel = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z);
            Vector3 lookForward = transform.forward;

            float turnRate = 6f;

            Vector3 rotated = Vector3.RotateTowards(
                flatVel,
                lookForward * flatVel.magnitude,
                turnRate * Time.deltaTime,
                999f
            );

            horizontalVelocity = new Vector3(rotated.x, horizontalVelocity.y, rotated.z);
        }
        // full velocity is all the shit together
        Vector3 fullVelocity = new Vector3(horizontalVelocity.x, VerticalVelocity, horizontalVelocity.z);
        // if hitting shit do hitting shit stuff
        CollisionFlags flags = characterController.Move(fullVelocity * Time.deltaTime);
        // speed is velocity's magnitude
        CurrentSpeed = new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z).magnitude;
    }

    // camera stuff right here
    void LookUpdate()
    {
        // use sense for looking
        Vector2 inL = new Vector2(LookInput.x * LookSensitivity.x, LookInput.y * LookSensitivity.y);
        //current look, keep between pitch limits
        currentPitch = Mathf.Clamp(currentPitch - inL.y, -PitchLimit, PitchLimit);
        // if theres a camera, rotate accordingly
        if (fpCamera != null)
        {
            fpCamera.transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }
        // rotate
        transform.Rotate(Vector3.up * inL.x);
    }

    void CameraUpdate()
    {
        // return if no cam
        if (fpCamera == null)
        {
            return;
        }
        // transition to target fov nicely
        float targetFOV = Mathf.Lerp(CameraNormalFOV, CameraSprintFOV, Mathf.Clamp01(CurrentSpeed / SprintSpeed));
        fpCamera.Lens.FieldOfView = Mathf.Lerp(fpCamera.Lens.FieldOfView, targetFOV, CameraFOVSmoothing * Time.deltaTime);



        if (isWallRunning)
        {
            // Determine side using wall normal
            float sideSign = Mathf.Sign(Vector3.Dot(transform.right, wallRunNormal));
            tiltAngle = -sideSign * 10f; // left = -15°, right = +15°
        } else
        {
            tiltAngle = 0f;
        }

            currentTilt = Mathf.Lerp(currentTilt, tiltAngle, Time.deltaTime * 6f);
        fpCamera.transform.localRotation = Quaternion.Euler(currentPitch, 0f, currentTilt);
    }



    private bool locked = false;

    public void LockInput(bool state)
    {
        locked = state;
        MoveInput = Vector2.zero;
        SprintInput = false;
        JumpInputHeld = false;
    }
}
