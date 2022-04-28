using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsablePlatform : GroundEffector
{
    public float fallSpeed = 10f;
    public float delayTime = 0.5f;

    [Header("Wwise Events")]
    public AK.Wwise.Event JumpingOnFallingPlat;

    public Vector3 difference;


    private bool _platformCollapsing = false;
    private Rigidbody2D _rigidbody;
    private Vector3 _lastPosition;


    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //update happens before physical simulation (cache last position)
        _lastPosition = transform.position;

        if (_platformCollapsing)
        {
            _rigidbody.AddForce(Vector2.down * fallSpeed);

            if(_rigidbody.velocity.y == 0)
            {
                _platformCollapsing = false;
                _rigidbody.bodyType = RigidbodyType2D.Static;
            }
        }
    }



    private void LateUpdate()
    {
        difference = transform.position - _lastPosition;        
    }

    public void CollapsePlatform()
    {
        //so when we start this we're making the platform a physical object essentially
        StartCoroutine("CollapsePlatformCoroutine");

    }

    public IEnumerator CollapsePlatformCoroutine()
    {
        JumpingOnFallingPlat.Post(gameObject);
        yield return new WaitForSeconds(delayTime);
        _platformCollapsing = true;

        _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rigidbody.freezeRotation = true;
        _rigidbody.gravityScale = 1f;

        //// Why such a big mass ?
        ///player will be on top of the platform but we don't want the player's weight
        ///or the player's to be actually pushing the platform down unnecessarily
        ///so high value here won't affect gravity BUT stops player from accidentally pushing downwards too much

        _rigidbody.mass = 1000f;
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
    }
}
