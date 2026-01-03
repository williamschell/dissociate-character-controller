using UnityEngine;

public class Sliding : MonoBehaviour
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
    //TODO: this isn't working as intended but isn't highest priority, return and fix later
    
    // update is called once per frame
    public void Update()
    {
        if (player.sliding) {
            player.slideTimer -= Time.deltaTime;

            // calculate slope
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit slopeHit, player.SlopeDetectionDistance))
            {
                Vector3 slopeNormal = slopeHit.normal;
                float slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);

                // project slide direction onto slope
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, slopeNormal).normalized;

                float currentSpeed = new Vector3(player.horizontalVelocity.x, 0f, player.horizontalVelocity.z).magnitude;

                // accelerate downhill if slope steep enough
                if (slopeAngle > player.SlopeEndThreshold)
                {
                    // add downhill acceleration
                    player.horizontalVelocity += slopeDirection * (player.SlopeAcceleration * Time.deltaTime);

                    // cap speed
                    player.horizontalVelocity = Vector3.ClampMagnitude(player.horizontalVelocity, player.MaxSlideSpeed);

                    // slightly boost control on steep downhills
                    player.horizontalVelocity += player.slideDirXZ * (player.SlopeBoostMultiplier * Time.deltaTime);
                }
                else
                {
                    // if flat or uphill: apply friction
                    currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, player.SlideFriction * Time.deltaTime);
                    player.horizontalVelocity = player.slideDirXZ * currentSpeed;
                }

                // smoothly turn based on player input
                Vector3 desiredDir = (player.MoveInput.sqrMagnitude > 0.001f) ?
                    (transform.forward * player.MoveInput.y + transform.right * player.MoveInput.x).normalized : player.slideDirXZ;

                player.slideDirXZ = Vector3.Slerp(player.slideDirXZ, desiredDir, player.SlideTurnInfluence * Time.deltaTime);
            }

            bool flatOrUphill = false;
            if (Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, out RaycastHit checkHit, player.SlopeDetectionDistance))
            {
                flatOrUphill = Vector3.Angle(checkHit.normal, Vector3.up) < player.SlopeEndThreshold;
            }

            if ((!characterController.isGrounded && flatOrUphill) ||
                (player.slideTimer <= 0f && flatOrUphill && player.CurrentSpeed < player.MinSlideSpeed))
            {
                StopSlide();
            }
        }
    }


    // execute slide is handled in moveupdate
    public void StartSlide()
    {
        // if grounding or already sliding, return
        if (!characterController.isGrounded || player.sliding)
        {
            return;
        }
        // if no sprint input, or slow ass speed, return
        if (!player.SprintInput || player.CurrentSpeed < player.MinSlideSpeed)
        {
            return;
        }
        // sliding to true, start timer, get direction and add slide boost to velocity
        player.sliding = true;
        player.slideTimer = player.SlideDuration;
        player.slideDirXZ = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        player.horizontalVelocity += player.slideDirXZ * (player.SlideInitialBoost + player.CurrentSpeed * 0.25f);
    }

    // stopslide sets sliding to false
    public void StopSlide() => player.sliding = false;
}
