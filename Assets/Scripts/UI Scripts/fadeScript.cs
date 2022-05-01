using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fadeScript : MonoBehaviour
{

    [SerializeField] private CanvasGroup theUIGroup;
    [SerializeField] private bool fadingIn = false;
    [SerializeField] private bool fadingOut = false;


    /// 
    /// function for revealing UI
    /// 

    public void ShowUI()
    {
        fadingIn = true;
    }

    /// 
    /// function for hiding UI
    ///

    public void HideUI()
    {
        fadingOut = true;
    }

    private void Update()
    {
        if (fadingIn)
        {
            // transparency of canvas handled by alpha value
            if (theUIGroup.alpha < 1)
            {
                theUIGroup.alpha += Time.deltaTime;  // transparency changes over time
                if(theUIGroup.alpha >= 1)
                {
                    fadingIn = false;
                }
            }

        }

        if (fadingOut)
        {
            
            if (theUIGroup.alpha >= 0)
            {
                theUIGroup.alpha += Time.deltaTime;  // transparency changes over time
                if (theUIGroup.alpha == 0)
                {
                    fadingOut = false;
                }
            }

        }

    }
}
