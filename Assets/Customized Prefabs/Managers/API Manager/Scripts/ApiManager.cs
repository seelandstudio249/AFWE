using UnityEngine;
using System.Collections;
using TMPro;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using UnityEngine.Networking;
using MixedReality.Toolkit.UX;

public class ApiManager : ManagerBaseScript {
    public static string token = "36|uzg9O82lpY2mQB1u9B9S1WNMUCRTA3noVdJqQZfD";
    [SerializeField] string baseUrl = "https://api.lyrics.ovh/";

    public static WWWForm form;

    [SerializeField] MRTKTMPInputField artistName, songName;
    [SerializeField] TMP_Text lyrics;
    [SerializeField] MRButtonClass getLyricsButton;


    protected override void Awake() {
        base.Awake();
        getLyricsButton.button.OnClicked.AddListener(delegate {
            StartCoroutine(GetDataFromAPI());
        });
    }

    IEnumerator GetDataFromAPI() {
        UnityWebRequest request = UnityWebRequest.Get(baseUrl + "v1/" + artistName.text + "/" + songName.text);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError(request.error);
        } else {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse);
            lyrics.text = jsonResponse;
            // Parse the JSON and use the data as needed in your Unity project
        }
    }

    public static IEnumerator POSTMethod(Action<ResponseData> callBack = null) {
        string uP = "";
        //uP = "https://jsonplaceholder.typicode.com/posts/1";
        uP = "https://api.7digital.com/artists/search?q=pogues";

        using (UnityWebRequest www = UnityWebRequest.Post(uP, form)) {
            www.SetRequestHeader("Authorization", "Bearer " + token);

            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();


            int errorcode = JsonHelperClasses.FromJsonErrorBase(www.downloadHandler.text).error; //json obj error info

            string msg = JsonHelperClasses.FromJsonErrorBase(www.downloadHandler.text).msg;//json obj error info
            if (errorcode != 200 && errorcode != 201) {
                Debug.Log("Error : ErrorCode : " + errorcode + " : " + msg);
            } else {
                ResponseData data = new ResponseData();
                data = JsonHelperClasses.FromJsonResponseDataBase(www.downloadHandler.text); //json obj game data
                Debug.Log("Msg:: " + msg);
                if (callBack != null) {
                    callBack.Invoke(data);
                }
            }
        }
    }

    WWWForm GenerateForm() {
        WWWForm form = new WWWForm();

        //form.AddField("TableFormName", Value);
        return form;

    }
}
