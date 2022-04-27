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
    public float yPushMaxUsedHeight;
    public float yPushThresholdHeight;

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

    Vector2 moveForce;

    V2PlayerController playerController;
    V2CharacterController characterController2D;

    GameObject instantiatedBubble;
    #endregion
    #endregion

    #region Execution
    void Start()
    {
        playerController = gameObject.GetComponent<V2PlayerController>();
        characterController2D = gameObject.GetComponent<V2CharacterController>();

        if (yPushMaxUsedHeight > gravMaxUsedHeight) { maxRelevantHeight = yPushMaxUsedHeight; }
        else { maxRelevantHeight = gravMaxUsedHeight; }
    }

    void Update()
    {
        if (Keyboard.current.vKey.isPressed)
        {
            if (antiGravAbilityEnabled == true && (antiGravActive == true || antiGravUsed == false))
            {
                moveForce = characterController2D.actualVeclocity;

                antiGravActive = true;

                if (frameOneAntiGrav == true)
                {
                    CharacterControllerInteraction();

                    if (characterController2D.below == true)
                    {
                        moveForce.y = moveForce.y + kickOffForce;
                    }

                    instantiatedBubble = Instantiate(bubblePrefab, transform.position, transform.rotation, transform);

                    antiGravUsed = true;

                    frameOneAntiGrav = false;
                }

                PassiveMovement();
                AntiGravMovement();

                characterController2D.Move(moveForce);
            }
        }
        if (Keyboard.current.vKey.wasReleasedThisFrame)
        {
            antiGravActive = false;

            frameOneAntiGrav = true;
            if (instantiatedBubble != null) Destroy(instantiatedBubble);
            CharacterControllerInteraction();
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
        Gizmos.DrawRay(transform.position + pushXOffset1, Vector2.down * yPushMaxUsedHeight);
        Gizmos.DrawRay(transform.position + pushXOffset2, Vector2.down * yPushThresholdHeight);

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
            playerController._moveDirection = characterController2D.actualVeclocity;

            characterController2D.antiGravActive = false;
        }
    }

    #region Calculations
    void HeightDetection()
    {
        RaycastHit2D groundCastResults = Physics2D.BoxCast(transform.position, heightDetectorBoxSize, 0, Vector2.down, maxRelevantHeight, groundMask);

        if (groundCastResults.collider != null)
        {
            currentHeight = transform.position.y - groundCastResults.point.y;
        }
        else
        {
            currentHeight = maxRelevantHeight;
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

    float CalculateYDrag(float ySpeed)
    {
        float calculatedDrag;

        calculatedDrag = minYDrag;
        if (ySpeed > dragThresholdSpeed)
        {
            float t = (ySpeed - dragThresholdSpeed) / dragMaxUsedSpeed;

            calculatedDrag = Mathf.Lerp(minYDrag, maxYDrag, t);
        }

        return calculatedDrag;
    }

    float CalculateYPushForce(float height)
    {
        float calculatedPushForce;

        calculatedPushForce = maxYPushForce;
        if (height > yPushThresholdHeight)
        {
            float t = (height - yPushThresholdHeight) / yPushMaxUsedHeight;

            calculatedPushForce = Mathf.Lerp(maxYPushForce, minYPushForce, t);
        }

        return calculatedPushForce;
    }
    #endregion

    #region Anti-Grav Movement
    void PassiveMovement()
    {
        HeightDetection();

        moveForce.y -= CalculateBubbleGravity(currentHeight) * Time.deltaTime;

        float yDrag = CalculateYDrag(Mathf.Abs(characterController2D.actualVeclocity.y));
        if (characterController2D.actualVeclocity.y > 0)
        {
            moveForce.y -= yDrag * Time.deltaTime;
        }
        else
        {
            //moveForce.y += yDrag * Time.deltaTime;
        }
    }

    void AntiGravMovement()
    {
        // each of these apply force in the key's direction
        // for each direction, if the player is moving faster than their max velocity, in that direction, then the force wont be applied 
        // if a direction, opposite to the player's current speed, is inputed, then a smoothdamp equation is used to pivot the player

        if (Keyboard.current.wKey.isPressed)
        {
            if (Mathf.Abs(characterController2D.actualVeclocity.y) < maxPushableSpeed)
            {
                moveForce = moveForce + new Vector2(0, CalculateYPushForce(currentHeight));
            }

            if (characterController2D.actualVeclocity.y < 0)
            {
                moveForce = moveForce + (new Vector2(0, CalculateYPushForce(currentHeight)) * pivotStrength);
            }
        }
        if (Keyboard.current.aKey.isPressed)
        {
            if (Mathf.Abs(characterController2D.actualVeclocity.x) < maxPushableSpeed)
            {
                moveForce = moveForce + new Vector2(-pushForce, 0);
            }

            if (characterController2D.actualVeclocity.x > 0)
            {
                moveForce = moveForce + (new Vector2(-pushForce, 0) * pivotStrength);
            }
        }
        if (Keyboard.current.sKey.isPressed)
        {
            if (Mathf.Abs(characterController2D.actualVeclocity.y) < maxPushableDownwardsSpeed)
            {
                moveForce = moveForce + new Vector2(0, -pushForce);
            }

            if (characterController2D.actualVeclocity.y < 0)
            {
                moveForce = moveForce + (new Vector2(0, -pushForce) * pivotStrength);
            }
        }
        if (Keyboard.current.dKey.isPressed)
        {
            if (Mathf.Abs(characterController2D.actualVeclocity.x) < maxPushableSpeed)
            {
                moveForce = moveForce + new Vector2(pushForce, 0);
            }

            if (characterController2D.actualVeclocity.x < 0)
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
}
