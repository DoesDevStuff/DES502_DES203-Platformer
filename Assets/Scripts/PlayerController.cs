using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GlobalTypes;
using System;

public class PlayerController : MonoBehaviour
{
    #region variables/properties
    #region public properties
    public GameObject spriteObject;
    SpriteRenderer spriteRenderer;

    [Header("Player Properties")]
    [Header("Input")]
    public float deadzoneValue = 0.15f;
    public float coyoteTime; // to be added
    public float jumpBuffer; // to be added

    [Header("Drag")] // to be added
    public float minWalkingDrag; 
    public float maxWalkingDrag;
    public float maxUsedWalkSpeed;
    [Space]
    public float minRunningDrag; 
    public float maxRunningDrag;
    public float maxUsedRunSpeed;
    [Space]
    public float slidingDrag; 

    [Header("xMovement")]
    public float walkAccelleration = 10f;
    public float maxWalkSpeed;
    public float runAccelleration;
    public float maxRunSpeed;
    public float pivotTime;
    public float creepSpeed = 5f;

    [Header("Jumping")]
    public float jumpSpeed = 15f;
    public float minJumpTime;
    public float doubleJumpSpeed = 10f;

    [Header("Wall Jumping")]
    public float xWallJumpSpeed = 15f;
    public float yWallJumpSpeed = 15f;
    public float wallSlideAmount = 0.1f;

    [Header("Sliding")]
    public bool applyDeccelInAir;
    public float minSpeedUsedSlideDrag;
    public float maxSpeedUsedSlideDrag;
    public float minSlideDrag;
    public float maxSlideDrag;
    public float slideHopPower;
    public float slideHopXPower;
    public float requiredMinimumVelocity;

    [Header("Physics")]
    public float gravity = 20f;
    public float peakGravity;

    [Header("Other")]
    public float wallRunAmount = 8f;
    public float glideTime = 2f;
    public float glideDescentAmount = 2f;
    public float powerJumpSpeed = 40f;
    public float powerJumpWaitTime = 1.5f;
    public float dashSpeed = 20f;
    public float dashTime = 0.2f;
    public float dashCooldownTime = 1f;
    public float groundSlamSpeed = 60f;

    [Space]
    [Space]
    [Space]
    [Space]
    [Header("Player Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun;
    public bool canMultipleWallRun;
    public bool canWallSlide;
    public bool canGlide;
    public bool canGlideAfterWallContact;
    public bool canPowerJump;
    public bool canGroundDash;
    public bool canAirDash;
    public bool canGroundSlam;
    public bool canSlide;

    [Space]
    [Space]
    [Space]
    [Space]
    [Header("Player State")]
    public bool isRunning;
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isWallJumping;
    public bool isWallRunning;
    public bool isWallSliding;
    public bool isDucking;
    public bool isCreeping;
    public bool isGliding;
    public bool isPowerJumping;
    public bool isDashing;
    public bool isGroundSlamming;
    public bool isInAntiGrav;
    public bool isSliding;
    public bool isSlideJumping;
    #endregion

    #region private properties
    bool slideFrameOne = false;
    bool slideExit = false;

    //input flags
    private bool _startJump;
    private bool _releaseJump;

    private Vector2 _input;
    private CharacterController2D _characterController;

    private bool _ableToWallRun = true;

    private CapsuleCollider2D _capsuleCollider;
    private Vector2 _originalColliderSize;
    //TODO: remove later when not needed
    private SpriteRenderer _spriteRenderer;

    private float _currentGlideTime;
    private bool _startGlide = true;

    private float _powerJumpTimer;

    private bool _facingRight;
    private float _dashTimer;

    Vector2 velocity = Vector2.zero;
    #endregion

    [HideInInspector] public Vector2 _moveDirection;
    #endregion

    #region Execution
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _originalColliderSize = _capsuleCollider.size;

        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isInAntiGrav == false)
        {
            if (_dashTimer > 0)
                _dashTimer -= Time.deltaTime;

            ApplyDeadzones();

            Sliding();

            if (isSliding == false)
            {
                ProcessHorizontalMovement();
            }

            if (_characterController.below) //On the ground
            {
                OnGround();
            }
            else //In the air
            {
                InAir();
            }

            _characterController.Move(_moveDirection); // Changed (it use to multiply the move direction by time.deltatime)
            // the reason it can't do that is because if we have velocity based movements multiplying it by time.deltatime will also then multiply the current velocity by that
            // what this means is everything (that's supposed to be timescaled) has to be multiplied by time.deltatime individually
            // this has already been implimented in this script
        }
    }

    void FixedUpdate()
    {
        if (isSliding == true) { SlideMovement(); }
    }
    #endregion

    private void ApplyDeadzones()
    {
        if (_input.x > -deadzoneValue && _input.x < deadzoneValue)
            _input.x = 0f;

        if (_input.y > -deadzoneValue && _input.y < deadzoneValue)
            _input.y = 0f;
    }

    void Drag()
    {
        if(isRunning == false)
        {

        }
        if(isRunning == true)
        {

        }
        if(isSliding == true)
        {

        }

        // move velocity = itself minus some drag;
    }

    void Sliding()
    {
        SlideInputAndState();

        if (isSliding == true)
        {
            if (slideFrameOne == false)
            {
                ChangeSlidingSprite(true);
                _moveDirection = _characterController.actualVeclocity;
                slideFrameOne = true;
            }

            SlideHop();
            SlideGravity();
        }
        SpriteRotater();

        // slide movement is handled in fixed update
    }
    #region Sliding
    void SlideInputAndState()
    {
        // if on ground and press key then slide, you can slide while airborne but you can't start sliding while airbone
        if (Keyboard.current.sKey.isPressed && _characterController.below == true)
        {
            isSliding = true;
        }
        if (isSliding && Keyboard.current.sKey.isPressed)
        {
            isSliding = true;
        }
        else
        {
            isSliding = false;

            if(slideFrameOne == true)
            {
                ChangeSlidingSprite(false);
            }
            slideFrameOne = false;
        }

        // if speed is slow then stop sliding (and stand up)
        if(Mathf.Abs(_characterController.actualVeclocity.x) < requiredMinimumVelocity)
        {
            if (isSliding == true)
            {
                slideExit = true;
            }

            isSliding = false;

            if (slideFrameOne == true)
            {
                ChangeSlidingSprite(false);
            }
            slideFrameOne = false;
        }

        // if the player's slide was cancelled by lack of speed then they have to let go of the key to slide again
        if(slideExit == true)
        {
            isSliding = false;
            if (Keyboard.current.sKey.wasReleasedThisFrame)
            {
                slideExit = false;
            }
        }
        

        // if on a too steep slope then slide
        if (_characterController._slopeAngle > _characterController.slopeAngleLimit && _characterController.below == true)
        {
            isSliding = true;
        }


        // transfer state to character controller 
        if (isSliding == true) _characterController.isSliding = true;
        else _characterController.isSliding = false;

        // if ceiling hit then jumping no 
        if (_characterController.above == true)
        {
            isSlideJumping = false;
        }
    }

    void SlideMovement()
    {
        if (isSliding == true)
        {
            float speedUsed = Mathf.Abs(_characterController.actualVeclocity.x);
            if (speedUsed < minSpeedUsedSlideDrag)
            {
                speedUsed = minSpeedUsedSlideDrag;
            }
            speedUsed = Mathf.Clamp(speedUsed, 0.001f, maxSpeedUsedSlideDrag);
            float t = speedUsed / maxSpeedUsedSlideDrag;
            float lerpDrag = Mathf.Lerp(minSlideDrag, maxSlideDrag, t);

            if (_characterController.below == true || applyDeccelInAir == true)
            {
                _moveDirection.x = _characterController.actualVeclocity.x * lerpDrag;
            }
        }
    }

    void SlideHop()
    {
        if (_startJump == true && _characterController.below == true && isSlideJumping == false)
        {
            //normal jump
            _moveDirection = _moveDirection + _characterController._slopeNormal * slideHopPower;

            //x push part of jump
            if(_facingRight == true)
            {
                _moveDirection.x = _moveDirection.x + slideHopXPower;
            }
            else
            {
                _moveDirection.x = _moveDirection.x - slideHopXPower;
            }

            isSlideJumping = true;
            _startJump = false;

            _characterController.DisableGroundCheck();
            _characterController.ClearMovingPlatform();
        }
    }

    void SlideGravity()
    {
        if (isSlideJumping == false && _characterController.below == true)
        {
            _moveDirection.y = _characterController.actualVeclocity.y;
        }
        _moveDirection.y = _moveDirection.y - (gravity * Time.deltaTime);
    }

    void ChangeSlidingSprite(bool sliding)
    {
        // change sprite to sliding sprite
        // changing the sprite could handled by animation system instead
        // that is probably a better way of doing this
        // if we do that it's also better to have animation variables assigned in a seperate script that interfaces with this one
        // it makes it simpler to visualise

        // if we do that then remove the sprite stuff here but keep the capsule collider stuff

        if (sliding == true)
        {
            spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp_crouching");
            _capsuleCollider.enabled = false;
        }
        else
        {
            spriteRenderer.sprite = Resources.Load<Sprite>("Player_normal");
            _capsuleCollider.enabled = true;
        }
    }

    void SpriteRotater()
    {
        // i want to add somekind of drag to this so it doesn't 'snap'

        if (isSliding == true)
        {
            if (_characterController.below == true)
            {
                spriteObject.transform.eulerAngles = new Vector3(0, 0, -_characterController._slopeAngle);
            }
            else
            {
                spriteObject.transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
        else
        {
            spriteObject.transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
    #endregion


    void OnGround()
    {
        ClearAirAbilityFlags();

        if (isSliding == false)
        {
            _moveDirection.y = 0;

            Jump();

            DuckingAndCreeping();
        }
    }
    #region On Ground
    public void ClearAirAbilityFlags() // CHANGED to public (this is so the antigrav can interact with it)
    {
        //clear flags for in air abilities
        isSlideJumping = false;
        isJumping = false;
        isDoubleJumping = false;
        isTripleJumping = false;
        isWallJumping = false;
        _currentGlideTime = glideTime;
        isGroundSlamming = false;
        _startGlide = true;
    }

    private void ProcessHorizontalMovement()
    {

        if (!isWallJumping)
        {
            //_moveDirection.x = _input.x; //- Removed

            if (_input.x < 0) // changed
            {
                //transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                spriteRenderer.flipX = true;
                _facingRight = false;
            }
            else if (_input.x > 0) // changed
            {
                //transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                spriteRenderer.flipX = false;
                _facingRight = true;
            }

            if (isDashing)
            {
                if (_facingRight)
                {
                    _moveDirection.x = dashSpeed;
                }
                else
                {
                    _moveDirection.x = -dashSpeed;
                }
                _moveDirection.y = 0;
            }
            else if (isCreeping)
            {
                //_moveDirection.x = _input.x * creepSpeed; // changed
            }
            else
            {
                //_moveDirection.x *= walkAccelleration; //- Removed

                //new from here
                _moveDirection.x = _characterController._moveVelocity.x;

                // pivot
                if (_input.x > 0 && _characterController._moveVelocity.x < 0)
                {
                    _moveDirection.x = Vector2.SmoothDamp(_characterController._moveVelocity, new Vector2(0, _characterController._moveVelocity.y), ref velocity, pivotTime).x;
                }
                if (_input.x < 0 && _characterController._moveVelocity.x > 0)
                {
                    _moveDirection.x = Vector2.SmoothDamp(_characterController._moveVelocity, new Vector2(0, _characterController._moveVelocity.y), ref velocity, pivotTime).x;
                }
                if (_input.x == 0 && _characterController.below == true)
                {
                    _moveDirection.x = Vector2.SmoothDamp(_characterController._moveVelocity, new Vector2(0, _characterController._moveVelocity.y), ref velocity, pivotTime).x;
                }

                // movement
                float timeScaledWalkAccelleration = walkAccelleration * Time.deltaTime;
                if (Mathf.Abs(_characterController._moveVelocity.x) < maxWalkSpeed)
                {
                    _moveDirection.x = _moveDirection.x + (timeScaledWalkAccelleration * _input.x);
                }
                // to here
            }

        }
    }

    private void DuckingAndCreeping()
    {
        /*
        //ducking and creeping
        if (_input.y < 0f)
        {
            if (!isDucking && !isCreeping)
            {
                _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, _capsuleCollider.size.y / 2);
                transform.position = new Vector2(transform.position.x, transform.position.y - (_originalColliderSize.y / 4));
                isDucking = true;
                _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp_crouching");
            }

            _powerJumpTimer += Time.deltaTime;
        }
        else
        {
            if (isDucking || isCreeping)
            {
                RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center,
                    transform.localScale, CapsuleDirection2D.Vertical, 0f, Vector2.up,
                    _originalColliderSize.y / 2, _characterController.layerMask);

                if (!hitCeiling.collider)
                {
                    _capsuleCollider.size = _originalColliderSize;
                    transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y / 4));
                    _spriteRenderer.sprite = Resources.Load<Sprite>("Player_normal");
                    isDucking = false;
                    isCreeping = false;
                }
            }

            _powerJumpTimer = 0f;
        }

        if (isDucking && _moveDirection.x != 0)
        {
            isCreeping = true;
            _powerJumpTimer = 0f;
        }
        else
        {
            isCreeping = false;
        }
        */
    }

    private void Jump()
    {
        //jumping
        if (_startJump)
        {
            _startJump = false;

            if (canPowerJump && isDucking &&
                _characterController.groundType != GroundType.OneWayPlatform && (_powerJumpTimer > powerJumpWaitTime))
            {
                _moveDirection.y = powerJumpSpeed;
                StartCoroutine("PowerJumpWaiter");
            }
            //check to see if we are on a one way platform
            else if (isDucking && _characterController.groundType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(true));
            }
            else
            {
                _moveDirection.y = jumpSpeed;
            }

            isJumping = true;
            _characterController.DisableGroundCheck();
            _characterController.ClearMovingPlatform();
            _ableToWallRun = true;
        }
    }
    #endregion

    void InAir()
    {
        ClearGroundAbilityFlags();

        if (isSliding == false)
        {
            AirJump();

            WallRunning();

            GravityCalculations();
        }
    }
    #region In Air
    public void ClearGroundAbilityFlags() // CHANGED to public (this is so the antigrav can interact with it)
    {
        if ((isDucking || isCreeping) && _moveDirection.y > 0)
        {
            StartCoroutine("ClearDuckingState");
        }
        //clear powerJumpTimer
        _powerJumpTimer = 0f;
    }

    private void AirJump()
    {
        if (_releaseJump)
        {
            Debug.Log("JumpRelease");

            _releaseJump = false;

            if (_moveDirection.y > 0)
            {
                Debug.Log("glimtooka");

                _moveDirection.y *= 0.5f;
            }

        }

        //pressed jump button in air
        if (_startJump)
        {
            Debug.Log("AirJump");

            #region TripleJump
            if (canTripleJump && (!_characterController.left && !_characterController.right))
            {
                if (isDoubleJumping && !isTripleJumping)
                {
                    _moveDirection.y = doubleJumpSpeed;
                    isTripleJumping = true;
                }
            }
            #endregion

            #region Double Jump
            if (canDoubleJump && (!_characterController.left && !_characterController.right))
            {
                if (!isDoubleJumping)
                {
                    _moveDirection.y = doubleJumpSpeed;
                    isDoubleJumping = true;
                }
            }
            #endregion

            #region Wall Jump
            if (canWallJump && (_characterController.left || _characterController.right))
            {
                if (_moveDirection.x <= 0 && _characterController.left)
                {

                    _moveDirection.x = xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;
                    //transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    spriteRenderer.flipX = false;
                }
                else if (_moveDirection.x >= 0 && _characterController.right)
                {

                    _moveDirection.x = -xWallJumpSpeed;
                    _moveDirection.y = yWallJumpSpeed;
                    // transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    spriteRenderer.flipX = true;
                }

                //isWallJumping = true;

                StartCoroutine("WallJumpWaiter");

                if (canJumpAfterWallJump)
                {
                    isDoubleJumping = false;
                    isTripleJumping = false;
                }
            }
            #endregion

            _startJump = false;
        }
    }

    private void WallRunning()
    {
        //wall running
        if (canWallRun && (_characterController.left || _characterController.right))
        {
            if (_input.y > 0 && _ableToWallRun)
            {
                _moveDirection.y = wallRunAmount * Time.deltaTime;

                if (_characterController.left)
                {
                    transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else if (_characterController.right)
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }

                StartCoroutine("WallRunWaiter");
            }
        }
        else
        {
            if (canMultipleWallRun)
            {
                StopCoroutine("WallRunWaiter");
                _ableToWallRun = true;
                isWallRunning = false;
            }
        }

        //canGlideAfterWallContact
        if ((_characterController.left || _characterController.right) && canWallRun)
        {
            if (canGlideAfterWallContact)
            {
                _currentGlideTime = glideTime;
            }
            else
            {
                _currentGlideTime = 0;
            }
        }
    }

    void GravityCalculations()
    {
        //detects if something above player
        if (_moveDirection.y > 0f && _characterController.above)
        {
            if (_characterController.ceilingType == GroundType.OneWayPlatform)
            {
                StartCoroutine(DisableOneWayPlatform(false));
            }
            else
            {
                _moveDirection.y = 0f;
            }

        }

        //apply wall slide adjustment
        if (canWallSlide && isWallJumping == false && (_characterController.left || _characterController.right)) // NEW (is wall jumping clause added)
        {
            if (_characterController.hitWallThisFrame) 
            {
                _moveDirection.y = 0;
            }
            

            if (_moveDirection.y <= 0)
            { 
                _moveDirection.y -= (gravity * wallSlideAmount) * Time.deltaTime;
            }
            else
            {
                _moveDirection.y -= gravity * Time.deltaTime;
            }
            
        }
        else if (canGlide && _input.y > 0f && _moveDirection.y < 0.2f) // glide adjustment
        {
            if (_currentGlideTime > 0f)
            {
                isGliding = true;

                if (_startGlide)
                {
                    _moveDirection.y = 0;
                    _startGlide = false;
                }

                _moveDirection.y -= glideDescentAmount * Time.deltaTime;
                _currentGlideTime -= Time.deltaTime;
            }
            else
            {
                isGliding = false;
                _moveDirection.y -= gravity * Time.deltaTime;
            } 

        }
        //else if (canGroundSlam  && !isPowerJumping && _input.y < 0f && _moveDirection.y < 0f) // ground slam
        else if (isGroundSlamming && !isPowerJumping && _moveDirection.y <0f)
        {
            _moveDirection.y = -groundSlamSpeed * Time.deltaTime;
        }
        else if (!isDashing) //regular gravity
        {
            _moveDirection.y -= gravity * Time.deltaTime;

            if(_characterController.actualVeclocity.y < 0)
            {
                _moveDirection.y -= peakGravity * Time.deltaTime;
            }
        }

        
    }
    #endregion


    #region Input Methods
    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJump (InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _startJump = true;
            _releaseJump = false;
        }
        else if (context.canceled)
        {
            _releaseJump = true;
            _startJump = false;
        }
    }

    public void OnDash (InputAction.CallbackContext context)
    {
        if (context.started && _dashTimer <= 0)
        {
            if ((canAirDash && !_characterController.below)
                || (canGroundDash && _characterController.below))
            {
                StartCoroutine("Dash");
            }
        }
    }
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && _input.y < 0f)
        {
            if (canGroundSlam)
            {
                isGroundSlamming = true;
            }
        }
    }
    #endregion

    #region Coroutines
    
    IEnumerator WallJumpWaiter()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(0.4f);
        isWallJumping = false;
    }

    IEnumerator WallRunWaiter()
    {
        isWallRunning = true;
        yield return new WaitForSeconds(0.5f);
        isWallRunning = false;
        if (!isWallJumping)
        {
            _ableToWallRun = false;
        }
       
    }

    IEnumerator ClearDuckingState()
    {
        yield return new WaitForSeconds(0.05f);

        RaycastHit2D hitCeiling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale,
            CapsuleDirection2D.Vertical, 0f, Vector2.up, _originalColliderSize.y / 2, _characterController.layerMask);
        
        if (!hitCeiling.collider) { 
            _capsuleCollider.size = _originalColliderSize;
            //transform.position = new Vector2(transform.position.x, transform.position.y + (_originalColliderSize.y / 4));
            _spriteRenderer.sprite = Resources.Load<Sprite>("Player_normal");
            isDucking = false;
            isCreeping = false;
        }
    }

    IEnumerator PowerJumpWaiter()
    {
        isPowerJumping = true;
        yield return new WaitForSeconds(0.8f);
        isPowerJumping = false;
    }

    IEnumerator Dash()
    {
        isDashing = true;
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        _dashTimer = dashCooldownTime;
    }

    IEnumerator DisableOneWayPlatform(bool checkBelow)
    {
        bool originalCanGroundSlam = canGroundSlam;
        GameObject tempOneWayPlatform = null;

        if (checkBelow)
        {
            Vector2 raycastBelow = transform.position - new Vector3(0, _capsuleCollider.size.y * 0.5f, 0);
            RaycastHit2D hit = Physics2D.Raycast(raycastBelow, Vector2.down,
                _characterController.raycastDistance, _characterController.layerMask);
            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }

        }
        else
        {
            Vector2 raycastAbove = transform.position + new Vector3(0, _capsuleCollider.size.y * 0.5f, 0);
            RaycastHit2D hit = Physics2D.Raycast(raycastAbove, Vector2.up,
                _characterController.raycastDistance, _characterController.layerMask);
            if (hit.collider)
            {
                tempOneWayPlatform = hit.collider.gameObject;
            }
        }

        if (tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = false;
            canGroundSlam = false;
        }

        yield return new WaitForSeconds(0.25f);

        if (tempOneWayPlatform)
        {
            tempOneWayPlatform.GetComponent<EdgeCollider2D>().enabled = true;
            canGroundSlam = originalCanGroundSlam;
        }

    }

    #endregion
}
