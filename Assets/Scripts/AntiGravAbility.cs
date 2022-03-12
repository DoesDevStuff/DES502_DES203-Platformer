using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AntiGravAbility : MonoBehaviour
{
    #region Variables
    #region attributes
    [Header("Misc")]
    public LayerMask groundMask;
    public GameObject bubblePrefab;

    [Header("Controls (these don't actually do anything)")]
    public KeyCode antiGravKey;
    [Space]
    public KeyCode upKey;
    public KeyCode leftKey;
    public KeyCode downKey;
    public KeyCode rightKey;

    [Header("Movement")]
    public float kickOffForce;
    [Space]
    public float pivotTime;
    [Space]
    public float maxPushableSpeed;
    public float pushForce;
    public float downwardsPushMultiplier;
    public float maxPushableDownwardsSpeed;

    [Header("Height Stress")]
    public float maxGravity;
    public float minGravity;
    [Space]
    public float maxHeight;
    public float minHeight;
    [Space]
    public float percentageHeightCutoffForUpForce;
    #endregion

    #region misc variables
    bool antiGravAbilityEnabled = true;
    bool antiGravActive = false;

    bool frameOneAntiGrav = true;

    Vector2 moveForce;

    PlayerController playerController;
    CharacterController2D characterController2D;

    Rigidbody2D rb2D;
    GameObject instantiatedBubble;

    Vector2 velocity = Vector2.zero;
    #endregion
    #endregion

    #region Execution
    void Start()
    {
        playerController = gameObject.GetComponent<PlayerController>();
        characterController2D = gameObject.GetComponent<CharacterController2D>();
        rb2D = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Keyboard.current.vKey.isPressed)
        {
            if (antiGravAbilityEnabled == true)
            {
                antiGravActive = true;

                moveForce = characterController2D._moveVelocity;

                if (frameOneAntiGrav == true)
                {
                    if (characterController2D.below == true)
                    {
                        //rb2D.AddForce(new Vector2(0, kickOffForce), ForceMode2D.Impulse);
                        moveForce.y = moveForce.y + kickOffForce;
                    }

                    instantiatedBubble = Instantiate(bubblePrefab, transform.position, transform.rotation, transform);

                    CharacterControllerInteraction();

                    frameOneAntiGrav = false;
                }


                //rb2D.gravityScale = CalculateBubbleGravity(HeightDetection());
                moveForce.y = moveForce.y - (CalculateBubbleGravity(HeightDetection()) * Time.deltaTime);
                AntiGravMovement();

                characterController2D.Move(moveForce);
                Debug.Log(moveForce + ", " + HeightDetection());
            }
        }
        if (Keyboard.current.vKey.wasReleasedThisFrame)
        {
            antiGravActive = false;

            frameOneAntiGrav = true;
            if (instantiatedBubble != null) Destroy(instantiatedBubble);
            CharacterControllerInteraction();
        }
    }
    #endregion

    #region Private Functions
    void CharacterControllerInteraction()
    {
        if (antiGravAbilityEnabled == true && antiGravActive == true)
        {
            playerController.isInAntiGrav = true;
            playerController.ClearAirAbilityFlags();
            playerController.ClearGroundAbilityFlags();

            characterController2D.antiGravActive = true;
        }
        else
        {
            playerController.isInAntiGrav = false;
            playerController.ClearAirAbilityFlags();
            playerController.ClearGroundAbilityFlags();

            characterController2D.antiGravActive = false;
        }
    }

    #region Calculations
    float HeightDetection()
    {
        RaycastHit2D groundCastResults = Physics2D.Raycast(transform.position, Vector2.down, 300, groundMask);

        float detectedHeight;

        if (groundCastResults.collider != null)
        {
            detectedHeight = transform.position.y - groundCastResults.point.y;
        }
        else
        {
            detectedHeight = maxHeight;
        }

        return detectedHeight;
    }

    float CalculateBubbleGravity(float height)
    {
        float clampedHeight = Mathf.Clamp(height, 0, maxHeight);

        float calculatedGravity;

        if (clampedHeight > minHeight)
        {
            float t = (clampedHeight - minHeight) / maxHeight;
            t = Mathf.Clamp(t, 0, 1);

            calculatedGravity = Mathf.Lerp(minGravity, maxGravity, t);
        }
        else
        {
            calculatedGravity = minGravity;
        }

        return calculatedGravity;
    }
    #endregion

    #region Anti-Grav Movement
    float UpwardsForceMultiplier()
    {
        // the higher you go the weaker the up key's effect is
        float t = CalculateBubbleGravity(HeightDetection()) / (maxGravity * percentageHeightCutoffForUpForce);
        return Mathf.Lerp(1, 0, t);
    }

    void AntiGravMovement()
    {
        // each of these apply force in the key's direction
        // for each direction, if the player is moving faster than their max velocity, in that direction, then the force wont be applied 
        // if a direction, opposite to the player's current speed, is inputed, then a smoothdamp equation is used to pivot the player

        if (Keyboard.current.wKey.isPressed)
        {
            if (characterController2D._moveVelocity.y < maxPushableSpeed)
            {
                moveForce = moveForce + new Vector2(0, pushForce) * UpwardsForceMultiplier();
                //rb2D.AddForce(new Vector2(0, pushForce) * UpwardsForceMultiplier(), ForceMode2D.Impulse);
            }

            if (characterController2D._moveVelocity.y < 0)
            {
                moveForce = Vector2.SmoothDamp(moveForce, new Vector2(moveForce.x, 0), ref velocity, pivotTime);
                //rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, new Vector2(rb2D.velocity.x, 0), ref velocity, pivotTime);
            }
        }
        if (Keyboard.current.aKey.isPressed)
        {
            if (characterController2D._moveVelocity.x > -maxPushableSpeed)
            {
                moveForce = moveForce + new Vector2(-pushForce, 0);
                //rb2D.AddForce(new Vector2(-pushForce, 0), ForceMode2D.Impulse);
            }

            if (characterController2D._moveVelocity.x > 0)
            {
                moveForce = Vector2.SmoothDamp(moveForce, new Vector2(0, moveForce.y), ref velocity, pivotTime);
                //rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, new Vector2(0, rb2D.velocity.y), ref velocity, pivotTime);
            }
        }
        if (Keyboard.current.sKey.isPressed)
        {
            if (characterController2D._moveVelocity.y > -maxPushableDownwardsSpeed)
            {
                moveForce = moveForce + new Vector2(0, -pushForce);
                //rb2D.AddForce(new Vector2(0, -pushForce) * downwardsPushMultiplier, ForceMode2D.Impulse);
            }

            if (characterController2D._moveVelocity.y < 0)
            {
                moveForce = Vector2.SmoothDamp(moveForce, new Vector2(moveForce.x, 0), ref velocity, pivotTime);
                //rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, new Vector2(rb2D.velocity.x, 0), ref velocity, pivotTime);
            }
        }
        if (Keyboard.current.dKey.isPressed)
        {
            if (characterController2D._moveVelocity.x < maxPushableSpeed)
            {
                moveForce = moveForce + new Vector2(pushForce, 0);
                //rb2D.AddForce(new Vector2(pushForce, 0), ForceMode2D.Impulse);
            }

            if (characterController2D._moveVelocity.x < 0)
            {
                moveForce = Vector2.SmoothDamp(moveForce, new Vector2(0, moveForce.y), ref velocity, pivotTime);
                //rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, new Vector2(0, rb2D.velocity.y), ref velocity, pivotTime);
            }
        }
    }
    #endregion
    #endregion

    #region Public Functions
    // these 2 functions are for turning the ability off and on, as in pressing the key for it will do nothing, if it is off
    public void DisableAntiGravAbility()
    {
        Debug.Log("Anti-Grav Disabled");
        antiGravAbilityEnabled = false;

        antiGravActive = false;
        frameOneAntiGrav = true;
        if (instantiatedBubble != null) Destroy(instantiatedBubble);
        CharacterControllerInteraction();
    }

    public void EnableAntiGravAbility()
    {
        Debug.Log("Anti-Grav Enabled");
        antiGravAbilityEnabled = true;

        CharacterControllerInteraction();
    }
    #endregion
}
