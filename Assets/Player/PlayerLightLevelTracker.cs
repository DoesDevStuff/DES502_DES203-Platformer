using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLightLevelTracker : MonoBehaviour
{
    [System.Serializable]
    public class LightObject
    {
        public GameObject source;
        [Space]
        public AnimationCurve lightFallOffCurve;
        public float lightMaximumRange;

        public LightObject(GameObject Source, AnimationCurve LightFallOffCurve, float LightMaximumRange)
        {
            source = Source;
            lightFallOffCurve = LightFallOffCurve;
            lightMaximumRange = LightMaximumRange;
        }
    }

    [Tooltip("if true then ignore player object reference field")] public bool attachedToPlayer;
    public GameObject playerObjectReference;
    public LayerMask playerMask;
    [Space]
    [Tooltip("if true then casted shadows will matter")] public bool doLineOfSightChecks;
    public LayerMask lightObstructing;
    [Space]
    public List<LightObject> lightObjects = new List<LightObject>();

    [HideInInspector] public float currentPlayerLightLevel;
    List<float> foundPlayerLightLevels = new List<float>();

    void Start()
    {
        if (attachedToPlayer == true) { playerObjectReference = gameObject; }
    }

    // Update is called once per frame
    void Update()
    {
        foundPlayerLightLevels.Clear();
        currentPlayerLightLevel = 0;

        foreach (LightObject lightObject in lightObjects)
        {
            StartCoroutine(LightCast(lightObject));
        }

        foreach (float playerLightLevel in foundPlayerLightLevels)
        {
            if (playerLightLevel > currentPlayerLightLevel)
            {
                currentPlayerLightLevel = playerLightLevel;
            }
        }
    }

    IEnumerator LightCast(LightObject _lightObject)
    {
        RaycastHit2D playerLightCast = Physics2D.CircleCast(_lightObject.source.transform.position, _lightObject.lightMaximumRange, Vector2.zero, 0, playerMask);

        if (playerLightCast.collider != null)
        {
            float distance = Vector2.Distance(_lightObject.source.transform.position, playerObjectReference.transform.position);
            distance = Mathf.Clamp(distance, 0.001f, _lightObject.lightMaximumRange);

            bool lineOfSightObstructed = false;
            if (doLineOfSightChecks == true)
            {
                Vector3 direction = (playerObjectReference.transform.position - _lightObject.source.transform.position).normalized;
                RaycastHit2D lineOfSightCheck = Physics2D.Raycast(_lightObject.source.transform.position, direction, distance, lightObstructing);
                if (lineOfSightCheck.collider != null) { lineOfSightObstructed = true; }
            }

            if (lineOfSightObstructed == false)
            {
                foundPlayerLightLevels.Add(_lightObject.lightFallOffCurve.Evaluate(distance / _lightObject.lightMaximumRange));
            }
            else
            {
                foundPlayerLightLevels.Add(0);
            }
        }
        else
        {
            foundPlayerLightLevels.Add(0);
        }

        yield break;
    }
}
