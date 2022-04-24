using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareScript : MonoBehaviour
{
    // the way bouncy flares work is with a circle cast that uses the reflection equation to bounce them on detection
    // while in this state it also rotates to match its velocity
    // after the flare has bounced more than the max bounces the raycast is disabled and a collider is attached to the flare

    // sticky flares just parent themselves on hit
    // they dont work properly if the object is scaled

    #region attributes
    [Header("Stats")]
    public float lifetimeOnSettle;
    public bool stickyFlare = false;

    [Header("Physics")]
    public Vector2 boxColliderSize;
    public float bounceColliderRadius;
    public LayerMask bounceMask;
    [Space]
    public int maxBounces;
    public float baseBounciness;
    public float bounceChangeFactor;
    #endregion

    #region misc variables
    [HideInInspector] public FlareGunScript flareGunReference;
    Rigidbody2D rb2D;

    int bounceCounter = 0;
    float bounciness;

    bool bounceEntrance = false;

    float lifetimer;

    bool stuck;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb2D = gameObject.GetComponent<Rigidbody2D>();

        bounciness = baseBounciness;

        lifetimer = lifetimeOnSettle;
    }

    // Update is called once per frame
    void Update()
    {
        if (stickyFlare == false)
        {
            if (bounceCounter < maxBounces)
            {
                RotateToTrajectory();
            }
            else
            {
                lifetimer = lifetimer - Time.deltaTime;

                if (lifetimer <= 0)
                {
                    flareGunReference.spawnedFlares.Remove(gameObject);
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            if (stuck == true)
            {
                lifetimer = lifetimer - Time.deltaTime;

                if (lifetimer <= 0)
                {
                    flareGunReference.spawnedFlares.Remove(gameObject);
                    Destroy(gameObject);
                }
            }
            else
            {
                RotateToTrajectory();
                Stick();
            }
        }
    }

    void FixedUpdate()
    {
        if (bounceCounter < maxBounces && stickyFlare == false)
        {
            Bounce();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, bounceColliderRadius);
        Gizmos.DrawWireCube(transform.position, boxColliderSize * transform.localScale);
    }

    void Bounce()
    {
        RaycastHit2D bounceCollider = Physics2D.CircleCast(transform.position, bounceColliderRadius, new Vector2(rb2D.velocity.x, rb2D.velocity.y), 0, bounceMask);

        if(bounceCollider.collider != null && bounceEntrance == false)
        {
            bounceEntrance = true;

            Vector2 d = new Vector2(rb2D.velocity.x, rb2D.velocity.y);
            Vector2 n = bounceCollider.normal;

            Vector2 r = d - 2 * Vector2.Dot(d, n) * n;

            rb2D.velocity = new Vector2(0, 0);
            rb2D.AddForce(r * bounciness, ForceMode2D.Impulse);

            bounceCounter++;
            bounciness = bounciness * bounceChangeFactor;

            if(bounceCounter >= maxBounces)
            {
                BoxCollider2D boxColl2D = gameObject.AddComponent<BoxCollider2D>();
                boxColl2D.size = boxColliderSize;
            }
        }
        else
        {
            bounceEntrance = false;
        }
    }

    void Stick()
    {
        RaycastHit2D stickCollider = Physics2D.CircleCast(transform.position, bounceColliderRadius, new Vector2(rb2D.velocity.x, rb2D.velocity.y), 0, bounceMask);

        if(stickCollider.collider != null)
        {
            gameObject.transform.SetParent(stickCollider.collider.gameObject.transform);

            rb2D.velocity = new Vector2(0, 0);
            rb2D.bodyType = RigidbodyType2D.Static;

            stuck = true;
        }
    }

    void RotateToTrajectory()
    {
        float angle = Mathf.Atan2(rb2D.velocity.y, rb2D.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
