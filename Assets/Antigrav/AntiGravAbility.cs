using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AntiGravAbility : MonoBehaviour
{
    // this script doesn't use a proper input system

    #region variables
    #region attributes
    [Header("Misc")]
    public LayerMask groundMask;
    public GameObject bubblePrefab;

    [Header("Controls")]
    [Tooltip("These don't work")] public KeyCode antiGravKey;
    [Space]
    [Tooltip("These don't work")] public KeyCode upKey;
    [Tooltip("These don't work")] public KeyCode leftKey;
    [Tooltip("These don't work")] public KeyCode downKey;
    [Tooltip("These don't work")] public KeyCode rightKey;

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
    public float heightCoyoteTime;
    public float strengthDwindleTime;
    public float minHeightDifferenceForCoyote;
    #endregion

    #region misc variables
    bool antiGravAbilityEnabled = true;
    bool antiGravActive = false;

    List<float> heights = new List<float>();

    bool coyoteActivated = false;
    float previousHeight;

    float heightCoyoteTimer;
    float strengthDwindleTimer;

    bool frameOneAntiGrav = true;

    Rigidbody2D rb2D;
    GameObject instantiatedBubble;

    Vector2 velocity = Vector2.zero;
    #endregion
    #endregion

    void Start()
    {
        rb2D = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Keyboard.current.vKey.isPressed)
        {
            if (antiGravAbilityEnabled == true)
            {
                antiGravActive = true;

                if (frameOneAntiGrav == true)
                {
                    instantiatedBubble = Instantiate(bubblePrefab, transform.position, transform.rotation, transform);
                    rb2D.AddForce(new Vector2(0, kickOffForce), ForceMode2D.Impulse);
                    CharacterControllerInteraction();

                    frameOneAntiGrav = false;
                }

                HeightsListUpdate();
                rb2D.gravityScale = CoyotedGravity();

                AntiGravMovement();
            }
        }
        if (Keyboard.current.vKey.wasReleasedThisFrame)
        {
            antiGravActive = false;

            coyoteActivated = false;
            frameOneAntiGrav = true;
            if (instantiatedBubble != null) Destroy(instantiatedBubble);
            CharacterControllerInteraction();
        }
    }

    void HeightsListUpdate()
    {
        // this list keeps track of the last frame's height

        heights.Add(HeightDetection());

        if (heights.Count > 2)
        {
            heights.RemoveAt(0);
        }
    }

//!! LOOK HERE
    void CharacterControllerInteraction()
    {
        // i tried disabling the update function of the CharacterController2D script but that didn't work
        
        // what needs to happen is for the other movement forms to be disabled and enabled here

        if(antiGravAbilityEnabled == true && antiGravActive == true)
        {
            // disable character movement
        }
        else
        {
            // enable character movement
        }
    }

    #region Calculations and Coyote
    float HeightDetection()
    {
        RaycastHit2D raycastResults = Physics2D.Raycast(transform.position, Vector2.down, 300, groundMask);

        float detectedHeight;

        if (raycastResults.collider != null)
        {
            detectedHeight = transform.position.y - raycastResults.collider.gameObject.transform.position.y;
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

        float calculatedGravity = minGravity;

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

    float CoyotedGravity()
    {
        // the way this works is by comparing the last frame's height to the current's
        // if the difference is larger than this variable: minHeightDifferenceForCoyote
        // then the previous gravity value, before the difference happened, is used instead of what the gravity should be, for a while
        // after that ends there's a dwindle time, lerping the current gravity to the gravity that it should be at, over time

        // the dwindle time might be a bit redundant and weird honestly, but i think its impact is low enough that it doesn't really matter
        // also there could be a better way of doing coyote time than this, using the height difference has some inate flaws

        // if the player is moving incredibly fast then it can trigger and it wont always detect a ledge
        // these aren't detrimental because if you're not detecting a ledge, through difference, then you're probably already at the max gravity/height, so it doesn't matter anyways
        // either that or the ledge is too small, which means it probably doesn't matter
        // and the other scenario is pretty unlikely

        float coyotedGravity = CalculateBubbleGravity(HeightDetection());

        // coyote calculations
        if (coyoteActivated == true)
        {
            heightCoyoteTimer = heightCoyoteTimer - Time.deltaTime;

            if (heightCoyoteTimer <= 0)
            {
                strengthDwindleTimer = strengthDwindleTimer - Time.deltaTime;

                float t = strengthDwindleTimer / strengthDwindleTime;
                t = Mathf.Clamp(t, 0, 1);

                float a = CalculateBubbleGravity(HeightDetection());
                float b = CalculateBubbleGravity(previousHeight);

                coyotedGravity = Mathf.Lerp(a, b, t);
            }
            else
            {
                coyotedGravity = CalculateBubbleGravity(previousHeight);
            }

            // if the gravity is better than the current gravity then go back to using normal gravity
            if (CalculateBubbleGravity(HeightDetection()) < coyotedGravity)
            {
                Debug.Log("End of coyote");

                coyotedGravity = CalculateBubbleGravity(HeightDetection());
                coyoteActivated = false;
            }
        }

        // coyote state decider
        if (heights.Count >= 2 && coyoteActivated == false)
        {
            if (heights[1] > heights[0] + minHeightDifferenceForCoyote)
            {
                coyoteActivated = true;
                heightCoyoteTimer = heightCoyoteTime;
                strengthDwindleTimer = strengthDwindleTime;
                previousHeight = heights[0];

                Debug.Log("COYOTE!");
            }
        }

        return coyotedGravity;
    }
    #endregion

    #region Anti-Grav Movement
    float UpwardsForceMultiplier()
    {
        // the higher you go the weaker the up key's effect is
        float t = CoyotedGravity() / maxGravity;
        return Mathf.Lerp(1, 0, t);
    }

    void AntiGravMovement()
    {
        // each of these apply force in the key's direction
        // for each direction, if the player is moving faster than their max velocity, in that direction, then the force wont be applied 
        // if a direction, opposite to the player's current speed, is inputed, then a smoothdamp equation is used to pivot the player

        if (Keyboard.current.wKey.isPressed)
        {
            if (rb2D.velocity.y < maxPushableSpeed)
            {
                rb2D.AddForce(new Vector2(0, pushForce) * UpwardsForceMultiplier(), ForceMode2D.Impulse);
            }

            if (rb2D.velocity.y < 0)
            {
                rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, new Vector2(rb2D.velocity.x, 0), ref velocity, pivotTime);
            }
        }
        if (Keyboard.current.aKey.isPressed)
        {
            if (rb2D.velocity.x > -maxPushableSpeed)
            {
                rb2D.AddForce(new Vector2(-pushForce, 0), ForceMode2D.Impulse);
            }

            if (rb2D.velocity.x > 0)
            {
                rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, new Vector2(0, rb2D.velocity.y), ref velocity, pivotTime);
            }
        }
        if (Keyboard.current.sKey.isPressed)
        {
            if (rb2D.velocity.y > -maxPushableDownwardsSpeed)
            {
                rb2D.AddForce(new Vector2(0, -pushForce) * downwardsPushMultiplier, ForceMode2D.Impulse);
            }

            if (rb2D.velocity.y < 0)
            {
                rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, new Vector2(rb2D.velocity.x, 0), ref velocity, pivotTime);
            }
        }
        if (Keyboard.current.dKey.isPressed)
        {
            if (rb2D.velocity.x < maxPushableSpeed)
            {
                rb2D.AddForce(new Vector2(pushForce, 0), ForceMode2D.Impulse);
            }

            if (rb2D.velocity.x < 0)
            {
                rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, new Vector2(0, rb2D.velocity.y), ref velocity, pivotTime);
            }
        }
    }
    #endregion

    #region Public Functions
    // these 2 functions are for turning the ability off and on, as in pressing the key for it will do nothing, if it is off
    public void DisableAntiGravAbility()
    {
        Debug.Log("Anti-Grav Disabled");
        antiGravAbilityEnabled = false;

        antiGravActive = false;
        frameOneAntiGrav = true;
        coyoteActivated = false;
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
