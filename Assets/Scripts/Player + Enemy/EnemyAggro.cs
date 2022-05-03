using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggro : MonoBehaviour
{
    #region Animation stuff
    /// <summary>
    /// In case we decide to do a wake up animation on just the face 
    /// 
    /// We have a GameObject face ensure this is inside the enemy object on top.
    /// This is where we have animation for enemy.
    /// </summary>

    [SerializeField] GameObject rig; //reference to the animated rig

    Animator hellhound; // reference to the animator as well

    #endregion

    #region Enemy accessible stuff
    [SerializeField]
    Transform player;

    [SerializeField]
    float aggroRange; // range to see player

    [SerializeField]
    float moveSpeed;

    [SerializeField]
    Transform raycastStartPoint;
    #endregion

    Rigidbody2D rb2d;

    //bool isFacingLeft;

    //private bool _isAggro = false;
    //private bool _isSearching = false;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
       hellhound = rig.GetComponent<Animator>(); // we get the animator component from the rig gameobject
    }

    // Update is called once per frame
    void Update()
    {
       /* 
        if (enemyLineOfSight(aggroRange))
        {
            //aggro
            _isAggro = true;
            
        }
        else
        {
            if (_isAggro)
            {
                _isSearching = true;
                if (!_isSearching)
                {
                    _isSearching = true;
                    // adding a delay to keep searching for player
                    Invoke("StopChasingPlayer", 5);
                }
                
            }
        }

        if (_isAggro)
        {
            ChasePlayer();
        }
        */
        
        // OLD HANDLER FOR DISTANCE TO ENEMY CHECK
        // check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.Log("distanceToPlayer : " + distanceToPlayer);

        if(distanceToPlayer < aggroRange)
        {
            //chase player
            ChasePlayer();
        }
        else
        {
            //stop chasing player
            StopChasingPlayer();
        }
        
        
    }
    /*
    // setting up line of sight
    bool enemyLineOfSight(float distance)
    {
        bool val = false;
        var castDist = distance; // function specific

        if (isFacingLeft == true)
        {
            castDist = -distance; // flips
        }

        Vector2 endPos = raycastStartPoint.position + Vector3.left * castDist;

        RaycastHit2D hit = Physics2D.Linecast(raycastStartPoint.position, endPos, 1 << LayerMask.NameToLayer("Player"));

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                val = true;
            }
            else
            {
                val = false;
            }

            Debug.DrawLine(raycastStartPoint.position, hit.point, Color.red);
        }
        else
        {
            Debug.DrawLine(raycastStartPoint.position, endPos, Color.green);
        }

        return val;
    }
    */
    private void ChasePlayer()
    {
        if (transform.position.x < player.position.x)
        {
            // enemy on left  of player i.e move right
            rb2d.velocity = new Vector2(moveSpeed, 0);
            //transform.localScale = new Vector2(-1, 1); // turn left
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            //isFacingLeft = true;
        }
        else if (transform.position.x > player.position.x) // counter for if we're on top
        {
            //enemy on right of player move left
            rb2d.velocity = new Vector2(-moveSpeed, 0);
            //transform.localScale = new Vector2(1, 1); // turn right
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            //isFacingLeft = false;
        }

        hellhound.Play("Hellhound_Run");
    }

    private void StopChasingPlayer()
    {
        //_isAggro = false;
        //_isSearching = false;
        rb2d.velocity = Vector2.zero;
        hellhound.Play("Hellhound_Idle");
    }
}
