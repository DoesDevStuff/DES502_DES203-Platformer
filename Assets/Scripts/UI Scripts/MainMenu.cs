using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string firstLevel;
    public GameObject optionsScreen;
    public GameObject lucy;
    public GameObject oldMan;
    public GameObject kingArthur;
    public GameObject credits;
    public GameObject controlsPage;
    

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(firstLevel);
    }

    /// <summary>
    /// Opens Compendium canvas
    /// </summary>
    public void OpenOptions()//opens compendium
    {
        optionsScreen.SetActive(true);
    }

    public void CloseOptions()//closes and sets canvas back to invisible
    {
        optionsScreen.SetActive(false);
    }

    /// <summary>
    /// Opens the Canvas for lucy
    /// </summary>

    public void OpenLucy()
    {
        
        lucy.SetActive(true);
    }

    public void CloseLucy()
    {
        lucy.SetActive(false);
        
    }

    /// <summary>
    /// Opens canvas for old man
    /// </summary>
    public void OpenOldMan()
    {

        oldMan.SetActive(true);
    }

    public void CloseOldMan()
    {
        oldMan.SetActive(false);

    }

    /// <summary>
    /// Opens canvas for King Arthur
    /// </summary>
    public void OpenKingArthur()
    {

        kingArthur.SetActive(true);
    }

    public void CloseKingArthur()
    {
        kingArthur.SetActive(false);

    }

    /// <summary>
    /// Opens canvas for Credits
    /// </summary>
    public void OpenCredits()
    {

        credits.SetActive(true);
    }

    public void CloseCredits()
    {
        credits.SetActive(false);

    }

    /// <summary>
    /// Opens canvas for Controls Menu
    /// </summary>
    public void OpenControls()
    {

        controlsPage.SetActive(true);
    }

    public void CloseControls()
    {
        controlsPage.SetActive(false);

    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }
}
