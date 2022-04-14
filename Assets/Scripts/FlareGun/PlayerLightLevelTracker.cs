using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLightLevelTracker : MonoBehaviour
{
    public class LightObject
    {
        public GameObject source;
        [Space]
        public AnimationCurve lightFallOffCurve;
        public float lightMaximumRange;
    }

    public GameObject playerObjectReference;
    public LayerMask playerMask;
    public List<LightObject> lightObjects = new List<LightObject>();
  

    [HideInInspector] public float currentPlayerLightLevel;
    List<float> foundPlayerLightLevels = new List<float>();

    // Update is called once per frame
    void Update()
    {
        foundPlayerLightLevels.Clear();
        currentPlayerLightLevel = 0;

        foreach (LightObject lightObject in lightObjects)
        {
            StartCoroutine(LightCast(lightObject));
        }

        foreach(float playerLightLevel in foundPlayerLightLevels)
        {
            if(playerLightLevel > currentPlayerLightLevel)
            {
                currentPlayerLightLevel = playerLightLevel;
            }
        }
    }

    IEnumerator LightCast(LightObject _lightObject)
    {
        RaycastHit2D playerLightCast = Physics2D.CircleCast(_lightObject.source.transform.position, _lightObject.lightMaximumRange, Vector2.zero, 0, playerMask);

        if(playerLightCast.collider != null)
        {
            float distance = Vector2.Distance(_lightObject.source.transform.position, playerObjectReference.transform.position);
            distance = Mathf.Clamp(distance, 0.001f, _lightObject.lightMaximumRange);

            foundPlayerLightLevels.Add(_lightObject.lightFallOffCurve.Evaluate(distance / _lightObject.lightMaximumRange));
        }
        else
        {
            foundPlayerLightLevels.Add(0);
        }

        yield break;
    }

    public LightObject BuildLightObject(GameObject source, AnimationCurve lightFallOffCurve, float lightMaximumRange)
    {
        LightObject builtLightObject = null;
        builtLightObject.source = source;
        builtLightObject.lightFallOffCurve = lightFallOffCurve;
        builtLightObject.lightMaximumRange = lightMaximumRange;
        return builtLightObject;
    }
}
