using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRUIManager : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private GameObject mainMenu;
    [SerializeField] TPPCameraSwitcher tPPCameraSwitcher;


    //1. open main menu and close main menu
    //2. tab functionality in main menu
    //display task ui

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenCloseMainMenu()
    {
        //swtich on and off the main menu
        if(mainMenu.activeSelf)
        {
            mainMenu.SetActive(false);
            tPPCameraSwitcher.SetTPPView();

        }
        else
        {
            mainMenu.SetActive(true);
            tPPCameraSwitcher.SetTPPView();
        }
        //if main menu is on, lock camera
        //if main menu is off, unlock camera
    }
}
