using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    #region boss state
    public enum BossState { Dormant, Patrol, Investigative, Attack, Shadow }
    public BossState bossState = BossState.Dormant;

    // patrol
    // patrol between sets of points that are updated when the player gets further
    // watch certain points 

    // investigate
    // watch new points of intrest
    // move near the points of intrest

    // attack 
    // attack move quickly towards the spotted player
    // aim and fire 

    // shadow 
    // fire at point based on player's last know trajectory
    // move towards the area they were last scene
    // mark the area as a point of intrest to look around in
    #endregion

    [System.Serializable]
    public class PatrolRoute
    {
        public List<GameObject> PointsList = new List<GameObject>();
        public List<float> WaitTimesList = new List<float>();
        public List<WatchPoint> watchPoints = new List<WatchPoint>();
    }
    public List<PatrolRoute> patrolRoutes = new List<PatrolRoute>();

    [System.Serializable]
    public class WatchPoint
    {
        public bool isSweep = false;
        public GameObject watchPoint;

        public List<GameObject> sweepPoints = new List<GameObject>();
        public float sweepSpeed;

        public float waitTime;
        public int order;
    }
    // make something that allows eye lights to be enabled and disabled

    float attackMoveSpeed;
    float patrolMoveSpeed;
    float shadowMoveSpeed;
    float moveTransitionRate;

    float attackRate;
    float minimumTrackingAccuracy;
    float maximumTrackingAccuracy;
    float accuracyIncreaseTime;

    float shadowTime;



    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
