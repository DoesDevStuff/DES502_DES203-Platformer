using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class FlareGunScript : MonoBehaviour
{
    //!! I think we will want to seperate the sprites of the legs and upper body for the sake of not having aiming variants of many animations
    //!! it would also be very complicated to have to have to sync up a running animation while somehow having the character be able to aim in all directions, on the same sprite

    //!! on the otherhand we could make the flaregun completely detached from the character
    //!! if we do that i think we should change the flare gun to be something magical, a floating gun probably wouldn't match the game
    //!! i'd imagine a floating latern that shoots fire balls could as an example, but i do like the flare gun as is
    //!! it just depends if we want the player to be able to shoot while in air, or don't want to have to seperate the animations for legs and arms

    // the flare gun needs a empty game object attached to player for it's script and a gameobject attached to that gameobject, representing the firing point
    // the firing point must be called "FlareSpawnPoint"

    #region Attributes
    public GameObject flarePrefab;
    public int maxSpawnedFlares;

    [Header("Sticky")]
    public bool shootStickyFlares;
    [Space]
    public KeyCode switchStickyAndBouncy;
    // im not 100% sure on this being allowed or not
    // sticky flares are much better for lighting something up, but bouncy flares are better for puzzles and interactions
    // having this be on it's own key is a bit bloated though, i feel ppl will forget about it
    // a weapon selector would help that but im not sure if that is right for the game
    // im up for trying it though, depends on how the group feels

    [Header("Firing")]
    public float fireRate;
    public float aimAgainWindow;
    public float gunImpulsePower;

    [Header("Wwise Events")]
    public AK.Wwise.Event FlareShot;
    public AK.Wwise.Event Powering_Up;

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

    CharacterController2D characterController2D;
    GameObject flareSpawnPoint;
    SpriteRenderer debugSprite;

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
        characterController2D = gameObject.GetComponentInParent<CharacterController2D>();

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
                Powering_Up.Post(gameObject);
                
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
                FlareShot.Post(gameObject);

                if (fireRateTimer >= fireRate)
                {
                    // fire gun
                    // animation event and variable will be needed so the script knows when firing animation is over

                    GameObject instantiatedFlare = Instantiate(flarePrefab, flareSpawnPoint.transform.position, gameObject.transform.rotation);

                    Rigidbody2D flareRB2D = instantiatedFlare.GetComponent<Rigidbody2D>();
                    FlareScript flareScript = instantiatedFlare.GetComponent<FlareScript>();
                    flareScript.flareGunReference = this;

                    if (shootStickyFlares == true)
                    {
                        flareScript.stickyFlare = true;
                    }

                    spawnedFlares.Add(instantiatedFlare);
                    if (spawnedFlares.Count > maxSpawnedFlares)
                    {
                        Destroy(spawnedFlares[0]);
                        spawnedFlares.RemoveAt(0);
                    }

                    float rotationInRadians = gameObject.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
                    float vectorX = Mathf.Cos(rotationInRadians);
                    float vectorY = Mathf.Sin(rotationInRadians);

                    Debug.Log(rotationInRadians);

                    flareRB2D.AddForce(new Vector2(vectorX, vectorY) * gunImpulsePower, ForceMode2D.Impulse);

                    fireRateTimer = 0;
                    gunReadyCuePlayed = false;
                }
            }
        }
        else
        {
            aiming = false;
        }
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
