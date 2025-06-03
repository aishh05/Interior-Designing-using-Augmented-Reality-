using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class UIManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject aboutPanel;
    public GameObject arSessionOrigin;
    public ARSession arSession;
    public GameObject ARUIPanel;
    public GameObject categoryDropdownObject;
    public GameObject variationDropdownObject;


    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        aboutPanel.SetActive(false);
        arSessionOrigin.SetActive(false);
        ARUIPanel.SetActive(false);
    }

    public void ShowAbout()
    {
        mainMenuPanel.SetActive(false);
        aboutPanel.SetActive(true);
    }

    public void StartARSession()
    {
        mainMenuPanel.SetActive(false);
        arSessionOrigin.SetActive(true);
        ARUIPanel.SetActive(true);
        arSession.Reset();
    }

    public void ExitApp()
    {
        Application.Quit();
    }

    public void ToggleDropdowns()
    {
        bool isActive = categoryDropdownObject.activeSelf;
        categoryDropdownObject.SetActive(!isActive);
        variationDropdownObject.SetActive(!isActive);
    }

}
