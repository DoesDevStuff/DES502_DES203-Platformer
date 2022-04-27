using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class V2FlareGun : MonoBehaviour
{
    #region Attributes
    public GameObject flarePrefab;
    public int maxSpawnedFlares;
    [Space]
    public float creepingAndSlidingYOffset;

    [Header("Sticky")]
    [Tooltip("Sticky flares break on objects un-uniformly scaled")] public bool shootStickyFlares;
    [Space]
    public KeyCode switchStickyAndBouncy;

    [Header("Firing")]
    public float fireRate;
    public float aimAgainWindow;
    public float gunImpulsePower;

    [Header("Aiming")]
    public int noAimIncrements;
    [Space]
    public bool useDownwardsBlindAngle = true;
    public float blindAngle = 10;

    [Header("Debug")]
    [Tooltip("Turning this on during play will cause errors")] public bool debugMode;
    public Color stickyFlareGunColor;
    public Color bouncyFlareGunColor;
    #endregion

    #region misc variables
    [HideInInspector] public List<GameObject> spawnedFlares = new List<GameObject>();

    V2PlayerController playerController;
    V2CharacterController characterController2D;
    GameObject flareSpawnPoint;
    SpriteRenderer debugSprite;

    Vector3 originalPosition;
    Vector3 crouchingSlidingPosition;

    float rightSideEdgeAngle = 0;
    float leftSideEdgeAngle = 0;

    float fireRateTimer;
    float aimAgainWindowTimer;
    bool aiming = false;
    bool gunReadyCuePlayed = false;
    #endregion

    #region Execution
    // Start is called before the first frame update
    void Start()
    {
        characterController2D = gameObject.GetComponentInParent<V2CharacterController>();
        playerController = gameObject.GetComponentInParent<V2PlayerController>();

        originalPosition = transform.localPosition;
        crouchingSlidingPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + creepingAndSlidingYOffset);

        if (debugMode == true)
        {
            debugSprite = gameObject.GetComponentInChildren<SpriteRenderer>();
            debugSprite.color = new Color(debugSprite.color.r, debugSprite.color.g, debugSprite.color.b, 0);
        }

        if (useDownwardsBlindAngle == true)
        {
            CalculateEdgeAngles();
        }

        flareSpawnPoint = transform.Find("FlareSpawnPoint").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.isSliding == true || playerController.isCreeping == true)
        {
            transform.localPosition = crouchingSlidingPosition;
        }
        else
        {
            transform.localPosition = originalPosition;
        }

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            shootStickyFlares = !shootStickyFlares;
        }

        GunPointAtCursor();

        AimAndFireGun();

        if (debugMode == true)
        {
            DebugStuff();
        }
    }
    #endregion

    void GunPointAtCursor()
    {
        // this incremented aiming was added for the sake of animation
        // it is editable through the variables so you could just crank up the numbers if you want it to be less teleporty
        Vector3 aimDirection = (GetMouseWorldPosition() - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        float angleIncrement = 360 / noAimIncrements;
        float roundedAngle = Mathf.Round(angle / angleIncrement) * angleIncrement;

        if (useDownwardsBlindAngle == true)
        {
            float negativeAngleOnRight = -90 + blindAngle;
            float negativeAngleOnLeft = -90 - blindAngle;
            float positiveAngleOnRight = 270 + blindAngle;
            float positiveAngleOnLeft = 270 - blindAngle;

            if (roundedAngle > 0)
            {
                if (roundedAngle > positiveAngleOnLeft && roundedAngle < positiveAngleOnRight)
                {
                    if (angle > 270)
                    {
                        roundedAngle = rightSideEdgeAngle;
                    }
                    else
                    {
                        roundedAngle = leftSideEdgeAngle;
                    }
                }
            }
            else
            {
                if (roundedAngle > negativeAngleOnLeft && roundedAngle < negativeAngleOnRight)
                {
                    if (angle > -90)
                    {
                        roundedAngle = rightSideEdgeAngle;
                    }
                    else
                    {
                        roundedAngle = leftSideEdgeAngle;
                    }
                }
            }
        }

        // eventually, this will have to be fed into an animation system rather than just rotating 
        transform.eulerAngles = new Vector3(0, 0, roundedAngle);
    }

    void AimAndFireGun()
    {
        fireRateTimer = fireRateTimer + Time.deltaTime;

        if (fireRateTimer >= fireRate && gunReadyCuePlayed == false)
        {
            gunReadyCuePlayed = true;
            // add visual or audio cue 
        }

        if (characterController2D.below == true)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                // aim weapon
                // the weapon isn't held unless this is happening
                // if we want a gun readying animation then an animation even will be needed 

                aimAgainWindowTimer = aimAgainWindow;
                aiming = true;
            }
            else
            {
                aimAgainWindowTimer = aimAgainWindowTimer - Time.deltaTime;

                gunReadyCuePlayed = false;

                // this is so the player can fire again while not having to holster their gun
                if (aimAgainWindowTimer <= 0)
                {
                    aiming = false;
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (fireRateTimer >= fireRate)
                {
                    // fire gun
                    // animation event and variable will be needed so the script knows when firing animation is over
                    FireGun();
                }
            }
        }
        else
        {
            aiming = false;
        }
    }

    void FireGun()
    {
        GameObject instantiatedFlare = Instantiate(flarePrefab, flareSpawnPoint.transform.position, gameObject.transform.rotation);

        Rigidbody2D flareRB2D = instantiatedFlare.GetComponent<Rigidbody2D>();
        V2Flare flareScript = instantiatedFlare.GetComponent<V2Flare>();
        flareScript.flareGunReference = this;

        flareScript.stickyFlare = shootStickyFlares;

        spawnedFlares.Add(instantiatedFlare);
        if (spawnedFlares.Count > maxSpawnedFlares)
        {
            Destroy(spawnedFlares[0]);
        }


        float rotationInRadians = gameObject.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        float vectorX = Mathf.Cos(rotationInRadians);
        float vectorY = Mathf.Sin(rotationInRadians);

        flareRB2D.AddForce(new Vector2(vectorX, vectorY) * gunImpulsePower, ForceMode2D.Impulse);

        fireRateTimer = 0;
        gunReadyCuePlayed = false;
    }

    void CalculateEdgeAngles()
    {
        // this function calculates the 2 rounded angles on the edge of the downwards exclusion cone 

        float negativeAngleOnRight = -90 + blindAngle;
        float positiveAngleOnLeft = 270 - blindAngle;

        float angleIncrement = 360 / noAimIncrements;


        float angleLoop = Mathf.Round(90 / angleIncrement) * angleIncrement;

        bool edgeAngleOnRightAcquired = false;
        float lastValidRightAngle = 0;
        while (edgeAngleOnRightAcquired == false)
        {
            angleLoop = angleLoop - angleIncrement;

            if (angleLoop < negativeAngleOnRight)
            {
                edgeAngleOnRightAcquired = true;
                rightSideEdgeAngle = lastValidRightAngle;
            }
            else
            {
                lastValidRightAngle = angleLoop;
            }
        }

        bool edgeAngleOnLeftAcquired = false;
        float lastValidLeftAngle = 0;
        angleLoop = Mathf.Round(90 / angleIncrement) * angleIncrement;
        while (edgeAngleOnLeftAcquired == false)
        {
            angleLoop = angleLoop + angleIncrement;

            if (angleLoop > positiveAngleOnLeft)
            {
                edgeAngleOnLeftAcquired = true;
                leftSideEdgeAngle = lastValidLeftAngle;
            }
            else
            {
                lastValidLeftAngle = angleLoop;
            }
        }
    }

    void DebugStuff()
    {
        float alpha = 0;
        Color color = bouncyFlareGunColor;

        if (aiming == true)
        {
            alpha = 1;
        }

        if (shootStickyFlares == true)
        {
            color = stickyFlareGunColor;
        }

        debugSprite.color = new Color(color.r, color.g, color.b, alpha);
    }

    // this could be added to a utilities class
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0;
        return mousePosition;
    }
}
