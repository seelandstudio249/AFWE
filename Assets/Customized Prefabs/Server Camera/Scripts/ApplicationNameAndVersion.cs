using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplicationNameAndVersion : MonoBehaviour
{
    public Text applicationNameVersionText;
    private void Awake()
    {
        applicationNameVersionText.text = Application.productName +" [ "+Application.version +" ] ";
        //Screen.fullScreen = false;
        //Screen.SetResolution(960, 540, FullScreenMode.Windowed);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    public void FullScreen()
    {
        
       
        if (Screen.fullScreen == true)
        {
            Screen.fullScreen = false;
            Screen.SetResolution(960, 540,FullScreenMode.Windowed);
            
        }
        else
        {
            Screen.fullScreen = true;
            Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        }
    }
}
