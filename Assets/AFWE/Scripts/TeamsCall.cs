using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamsCall : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        OpenMicrosoftTeams();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenMicrosoftTeams() {
        // The URL for Microsoft Teams Web app
        string teamsUrl = "https://teams.microsoft.com/";
        Application.OpenURL(teamsUrl);
    }
}
