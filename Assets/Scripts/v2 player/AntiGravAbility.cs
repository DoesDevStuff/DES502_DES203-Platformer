using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AntiGravAbility : MonoBehaviour
{
    #region Variables
    #region attributes
    public float wallCastRange;
    public float yOffset;
    public float xOffset;
    public LayerMask walls;

    [Header("Misc")]
    public bool active;
    public LayerMask groundMask;
    public GameObject bubblePrefab;
    public Vector2 heightDetectorBoxSize;

    [Header("Controls have to be manually set inside the script")]

    [Header("Movement")]
    public float kickOffForce;
    [Space]
    public float pivotStrength;
    [Space]
    public float maxPushableSpeed;
    public float pushForce;
    public float downwardsPushMultiplier;
    public float maxPushableDownwardsSpeed;

    [Header("Positive Y Movement")]
    public float maxYPushForce;
    public float minYPushForce;
    [Space]
    public float yPushMaxUsedSpeed;
    public float yPushMinSpeed;

    [Header("Y Drag")]
    public float maxYDrag;
    public float minYDrag;
    [Space]
    public float dragMaxUsedSpeed;
    public float dragThresholdSpeed;

    [Header("Gravity")]
    public float minGravity;
    public float maxGravity;
    [Space]
    public float gravMaxUsedHeight;
    public float gravThresholdHeight;


    #endregion

    #region misc variables
    bool antiGravAbilityEnabled = true;
    bool antiGravActive = false;

    bool antiGravUsed = false;

    bool frameOneAntiGrav = true;

    float currentHeight;
    float maxRelevantHeight;

    [HideInInspector] public Vector2 moveForce;

    V2PlayerController playerController;
    V2CharacterController characterController2D;

    GameObject instantiatedBubble;

    bool vIsPressed = false;
    bool vIsReleased = false;

    bool wIsPressed = false;
    bool aIsPressed = false;
    bool sIsPressed = false;
    bool dIsPressed = false;
    #endregion
    #endregion

    #region Execution
    void Start()
    {
        playerController = gameObject.GetComponent<V2PlayerController>();
        characterController2D = gameObject.GetComponent<V2CharacterController>();
    }

    void Update()
    {
        if (active == true)
        {
            if (Keyboard.current.vKey.isPressed)
            {
                vIsPressed = true;
            }
            else
            {
                vIsPressed = false;
            }
            if (Keyboard.current.vKey.wasReleasedThisFrame)
            {
                vIsReleased = true;
            }

            if (Keyboard.current.wKey.isPressed)
            {
                wIsPressed = true;
            }
            else
            {
                wIsPressed = false;
            }
            if (Keyboard.current.aKey.isPressed)
            {
                aIsPressed = true;
            }
            else
            {
                aIsPressed = false;
            }
            if (Keyboard.current.sKey.isPressed)
            {
                sIsPressed = true;
            }
            else
            {
                sIsPressed = false;
            }
            if (Keyboard.current.dKey.isPressed)
            {
                dIsPressed = true;
            }
            else
            {
                dIsPressed = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (vIsPressed)
        {
            if (antiGravAbilityEnabled == true && (antiGravActive == true || antiGravUsed == false))
            {
                antiGravActive = true;
                if (frameOneAntiGrav == true)
                {
                    moveForce = playerController._moveDirection;

                    CharacterControllerInteraction();

                    if (characterController2D.below == true)
                    {
                        moveForce.y = moveForce.y + kickOffForce;
                    }

                    instantiatedBubble = Instantiate(bubblePrefab, transform.position, transform.rotation, transform);

                    antiGravUsed = true;

                    frameOneAntiGrav = false;
                }

                yDrag();

                AntiGravMovement();

                HeightDetection();
                moveForce.y = moveForce.y - CalculateBubbleGravity(currentHeight);

                WallCheck();


                characterController2D.Move(moveForce);
            }
        }
        if (vIsReleased == true)
        {
            antiGravActive = false;

            frameOneAntiGrav = true;
            if (instantiatedBubble != null) Destroy(instantiatedBubble);
            CharacterControllerInteraction();
            vIsReleased = false;
        }

        if (characterController2D.below == true && antiGravActive == false)
        {
            antiGravUsed = false;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 gravXOffset1 = Vector2.right * 0.15f;
        Vector3 gravXOffset2 = Vector2.right * 0.3f;
        Gizmos.DrawRay(transform.position + gravXOffset1, Vector2.down * gravMaxUsedHeight);
        Gizmos.DrawRay(transform.position + gravXOffset2, Vector2.down * gravThresholdHeight);

        Vector3 pushXOffset1 = Vector2.left * 0.15f;
        Vector3 pushXOffset2 = Vector2.left * 0.3f;

        Vector3 boxPosition = new Vector3(transform.position.x, transform.position.y - gravMaxUsedHeight);
        Gizmos.DrawWireCube(boxPosition, heightDetectorBoxSize);
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
            playerController._moveDirection = characterController2D._moveVelocity;
            characterController2D.antiGravActive = false;
        }
    }

    #region Calculations
    void HeightDetection()
    {
        RaycastHit2D groundCastBox = Physics2D.BoxCast(transform.position, heightDetectorBoxSize, 0, Vector2.down, float.MaxValue, groundMask);

        if (groundCastBox.collider != null)
        {
            if (groundCastBox.normal == Vector2.left || groundCastBox.normal == Vector2.right)
            {
                RaycastHit2D groundCastRay = Physics2D.Raycast(transform.position, Vector2.down, float.MaxValue, groundMask);
                if (groundCastRay.collider == null)
                {
                    currentHeight = float.MaxValue;
                }
                else
                {
                    currentHeight = Mathf.Abs(transform.position.y - groundCastRay.point.y);
                }
            }
            else
            {
                currentHeight = Mathf.Abs(transform.position.y - groundCastBox.point.y);
            }

        }
        else
        {
            currentHeight = float.MaxValue;
        }
    }

    float CalculateBubbleGravity(float height)
    {
        float calculatedGravity;

        calculatedGravity = minGravity;
        if (height > gravThresholdHeight)
        {
            float t = (height - gravThresholdHeight) / gravMaxUsedHeight;

            calculatedGravity = Mathf.Lerp(minGravity, maxGravity, t);
        }

        return calculatedGravity;
    }

    float CalculateYPushForce()
    {
        float calculatedPushForce;

        calculatedPushForce = maxYPushForce;
        if (moveForce.y > yPushMinSpeed)
        {
            float t = (moveForce.y - yPushMinSpeed) / yPushMaxUsedSpeed;

            calculatedPushForce = Mathf.Lerp(maxYPushForce, minYPushForce, t);
        }

        Debug.Log(calculatedPushForce);
        return calculatedPushForce;
    }
    #endregion

    #region Anti-Grav Movement

    void yDrag()
    {
        if(moveForce.y > dragThresholdSpeed)
        {
            float t = (moveForce.y - dragThresholdSpeed) / dragMaxUsedSpeed;
            float drag = Mathf.Lerp(minYDrag, maxYDrag, t);
            moveForce.y = moveForce.y - drag;
        }
    }

    void AntiGravMovement()
    {
        // each of these apply force in the key's direction
        // for each direction, if the player is moving faster than their max velocity, in that direction, then the force wont be applied 
        // if a direction, opposite to the player's current speed, is inputed, then a smoothdamp equation is used to pivot the player

        if (wIsPressed)
        {
            if (moveForce.y < maxPushableSpeed)
            {
                moveForce = moveForce + new Vector2(0, CalculateYPushForce());
            }
        }
        if (aIsPressed)
        {
            if (moveForce.x > -maxPushableSpeed)
            {
                moveForce = moveForce + new Vector2(-pushForce, 0);
            }

            if (moveForce.x > 0)
            {
                moveForce = moveForce + (new Vector2(-pushForce, 0) * pivotStrength);
            }
        }
        if (sIsPressed)
        {
            if (moveForce.y > -maxPushableDownwardsSpeed)
            {
                moveForce = moveForce + new Vector2(0, -pushForce);
            }

            if (moveForce.y < 0)
            {
                moveForce = moveForce + (new Vector2(0, -pushForce) * pivotStrength);
            }
        }
        if (dIsPressed)
        {
            if (moveForce.x < maxPushableSpeed)
            {
                moveForce = moveForce + new Vector2(pushForce, 0);
            }

            if (moveForce.x < 0)
            {
                moveForce = moveForce + (new Vector2(pushForce, 0) * pivotStrength);
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

    void WallCheck()
    {
        Vector3 upCheckPosition = transform.position + new Vector3(0, yOffset);
        Vector3 leftCheckPosition = transform.position + new Vector3(-xOffset, 0);
        Vector3 downCheckPosition = transform.position + new Vector3(0, -yOffset);
        Vector3 rightCheckPosition = transform.position + new Vector3(xOffset, 0);

        RaycastHit2D upCheck = Physics2D.Raycast(upCheckPosition, Vector2.up, wallCastRange, walls);
        RaycastHit2D leftCheck = Physics2D.Raycast(leftCheckPosition, Vector2.left, wallCastRange, walls);
        RaycastHit2D downCheck = Physics2D.Raycast(downCheckPosition, Vector2.down, wallCastRange, walls);
        RaycastHit2D rightCheck = Physics2D.Raycast(rightCheckPosition, Vector2.right, wallCastRange, walls);

        if (upCheck.collider != null)
        {
            if(upCheck.normal == Vector2.down && moveForce.y > 0)
            {
                moveForce.y = 0;
            }
        }
        if (leftCheck.collider != null)
        {
            if (leftCheck.normal == Vector2.right && moveForce.x < 0)
            {
                moveForce.x = 0;
            }
        }
        if (downCheck.collider != null)
        {
            if (downCheck.normal == Vector2.up && moveForce.y < 0)
            {
                moveForce.y = 0;
            }
        }
        if (rightCheck.collider != null)
        {
            if (rightCheck.normal == Vector2.down && moveForce.x > 0)
            {
                moveForce.x = 0;
            }
        }
    }
}
