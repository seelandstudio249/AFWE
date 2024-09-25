using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class ServerCameraView : NetworkBehaviour
{
    #region Singleton Declaration
    private static ServerCameraView _Instance;
    public static ServerCameraView Instance
    {
        get
        {
            if (!_Instance)
                _Instance = FindObjectOfType<ServerCameraView>();

            return _Instance;
        }
    }

    #endregion

    private Vector3 defaultCameraPos;
    private Quaternion defaultCameraRot;
    private Vector3 defaultContainerPos;
    private Quaternion defaultContainerRot;

    [Tooltip("Leave it empty by default and do not change the name of 'MRTKInputSimulator' leave the name as default as well")] public GameObject MRTKInput;
    public GameObject uiPanel;
    public GameObject btmPanel;
    [HideInInspector] public Transform customCameraTransform;
    public GameObject playerCamBtnPrefab;
    public GameObject playerCamListParent;
    public GameObject camListParent;
    public GameObject areaCamBtnPrefab;
    [HideInInspector] public List<GameObject> buttonList = new List<GameObject>();
    public GameObject isOnSpectatorModeBar;

    public CanvasGroup userListPanel;
    public CanvasGroup camListPanel;
    public CanvasGroup freeCamViewPanel;

    public int CurViewID;

    public GameObject areaCam_Pref;
    [HideInInspector] public List<Camera> areaCamList = new List<Camera>();
    private GameObject myCurrentAreaCam;

    public TextMeshProUGUI CurrentAngleText;
    public bool isAreaCamMode = false;

    private void Awake()
    {
        uiPanel.SetActive(false);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        btmPanel.SetActive(true);
        CheckAreaCam();
        CurrentAngleText.text = "Free View";
        freeCamViewPanel.alpha = 1;

#endif


    defaultCameraPos = Camera.main.transform.position;
        defaultCameraRot = Camera.main.transform.rotation;

    }

    private void CheckAreaCam()
    {
        if (myCurrentAreaCam == null)
        {
            GameObject go = GameObject.Find("AreaCam");

            if (go != null)
            {
                myCurrentAreaCam = go;
                areaCamList = myCurrentAreaCam.GetComponentsInChildren<Camera>().ToList();
            }
            else
            {
                Transform container = GameObject.Find("Container").transform;
                myCurrentAreaCam = GameObject.Instantiate(areaCam_Pref, container);
                areaCamList = myCurrentAreaCam.GetComponentsInChildren<Camera>().ToList();
            }
        }
        else
        {
            areaCamList = myCurrentAreaCam.GetComponentsInChildren<Camera>().ToList();
        }
        GenerateAreaCamBtn();
    }




    public override void OnStartServer()
    {
        base.OnStartServer();
        base.NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
        MRTKInput = GameObject.Find("MRTKInputSimulator");
        uiPanel.SetActive(true);
    }
    private void ServerManager_OnRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
    {
        if (arg2.ConnectionState == RemoteConnectionState.Stopped)
        {
            OnPlayerDCRefreshCamList(arg2.ConnectionId);
        }
    }
    public void ResetCameraViewToDefault()
    {
        customCameraTransform = null;
        Camera.main.transform.position = defaultCameraPos;
        Camera.main.transform.rotation = defaultCameraRot;
        CurrentAngleText.text = "Free View";
        freeCamViewPanel.alpha = 1;
        isAreaCamMode = false;
        //StartCameraView();
        TriggleBar(false);
    }
    private void TriggleBar(bool isOnSpectatorMode)
    {
        if (MRTKInput != null && base.IsServer )
            MRTKInput.SetActive(!isOnSpectatorMode);
        //if(!isOnSpectatorMode)
        //{
            camListPanel.alpha = 0f;
            camListPanel.blocksRaycasts = false;
            camListPanel.interactable = false;

            userListPanel.alpha = 0f;
            userListPanel.blocksRaycasts = false;
            userListPanel.interactable = false;
       // }
    
    }
    public void SetCameraView(Transform transform)
    {
        customCameraTransform = transform;
        TriggleBar(true);
    }

    public Transform GetCurrentCameraViewTransform()
    {
        return customCameraTransform;
    }

    private void Update()
    {
        if (customCameraTransform != null)
        {
            Camera.main.transform.SetPositionAndRotation(customCameraTransform.position, customCameraTransform.rotation);
           // Camera.main.transform.SetLocalPositionAndRotation(customCameraTransform.localPosition, customCameraTransform.localRotation);
        }

        if (isAreaCamMode)
        {

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                ManualAdjustAreaCam(0.01f, 0f);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                ManualAdjustAreaCam(-0.01f, 0f);
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                ManualAdjustAreaCam(0f, 0.01f);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                ManualAdjustAreaCam(0f, -0.01f);
            }
        }
    }
    public void StartCameraView()
    {
        OnOffCanvasGroup(userListPanel, camListPanel);

        if (userListPanel.interactable)
        {
            buttonList.ForEach(p => GameObject.Destroy(p));
            buttonList.Clear();
            FishNetPlayer[] playersCam = FindObjectsOfType<FishNetPlayer>(); // find other player object
            for (int i = 0; i < playersCam.Length; i++)
            {
                if (!playersCam[i].IsOwner && playersCam[i].isPlayerReady.Value)  // Ignore Server's Player
                {
                    Transform transform = playersCam[i].transform;
                    GameObject plyerBtn = Instantiate(playerCamBtnPrefab, playerCamListParent.transform);
                    buttonList.Add(plyerBtn);
                    plyerBtn.GetComponentInChildren<Text>().text = playersCam[i].GetName();
                    int id = playersCam[i].playerConnectionID.Value;
                    plyerBtn.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        SetCameraView(transform);
                        CurViewID = id;
                        CurrentAngleText.text = plyerBtn.GetComponentInChildren<Text>().text;
                        freeCamViewPanel.alpha = 0;
                        isAreaCamMode = false;
                    });
                }
            } 

        }
        else
        {
            buttonList.ForEach(p => GameObject.Destroy(p));
            buttonList.Clear();
        }
    }

    private void OnPlayerDCRefreshCamList(int dcPId)
    {
        buttonList.ForEach(p => GameObject.Destroy(p));
        buttonList.Clear();
        if (dcPId == CurViewID)
        {
            Debug.Log("Spectator Player is DC");
            ResetCameraViewToDefault();
        }
    }
    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Area Camera & Cam list
    /// </summary>

    public float areaCamOffSetValue = 0f;
    public float areaCamOffSetValueY = 0.5f;
    public void OnResizeAreaCam(List<Vector3> casualtyPosList)
    {
        float minX = 0;
        float minY = 0;
        float maxX = 0;
        float maxY = 0;
        float minZ = 0;
        float maxZ = 0;

        foreach (var item in casualtyPosList)
        {
            //get min 
            minX = GetMin(minX, item.x);
            minY = GetMin(minY, item.y);
            minZ = GetMin(minZ, item.z);

            //get max
            maxX = GetMax(maxX, item.x);
            maxY = GetMax(maxY, item.y);
            maxZ = GetMax(maxZ, item.z);
        }
        float aY = (minY + maxY) / 2;
        areaCamList[0].gameObject.transform.localPosition = new Vector3(minX - areaCamOffSetValue, aY + areaCamOffSetValueY, minZ - areaCamOffSetValue);
        areaCamList[1].gameObject.transform.localPosition = new Vector3(minX - areaCamOffSetValue, aY + areaCamOffSetValueY, maxZ + areaCamOffSetValue);
        areaCamList[2].gameObject.transform.localPosition = new Vector3(maxX + areaCamOffSetValue, aY + areaCamOffSetValueY, maxZ + areaCamOffSetValue);
        areaCamList[3].gameObject.transform.localPosition = new Vector3(maxX + areaCamOffSetValue, aY + areaCamOffSetValueY, minZ - areaCamOffSetValue);

        //GenerateAreaCamBtn();
    }
    private void ManualAdjustAreaCam(float leftRight, float UpDown)
    {
        Vector3 pos1 = areaCamList[0].gameObject.transform.localPosition;
        Vector3 pos2 = areaCamList[1].gameObject.transform.localPosition;
        Vector3 pos3 = areaCamList[2].gameObject.transform.localPosition;
        Vector3 pos4 = areaCamList[3].gameObject.transform.localPosition;
        areaCamList[0].gameObject.transform.localPosition = new Vector3(pos1.x - leftRight, pos1.y + UpDown, pos1.z - leftRight);
        areaCamList[1].gameObject.transform.localPosition = new Vector3(pos2.x - leftRight, pos2.y + UpDown, pos2.z + leftRight);
        areaCamList[2].gameObject.transform.localPosition = new Vector3(pos3.x + leftRight, pos3.y + UpDown, pos3.z + leftRight);
        areaCamList[3].gameObject.transform.localPosition = new Vector3(pos4.x + leftRight, pos4.y + UpDown, pos4.z - leftRight);
    }

    //public List<GameObject> casualtyList = new List<GameObject>();
    //public List<Transform> casualtyTransformList = new List<Transform>();
    //public void GenerateCasualtyBtn()
    //{
    //    casualtyList = GameObject.FindGameObjectsWithTag("Casualty").ToList();
      
    //    foreach (var item in casualtyList)
    //    {
    //        casualtyTransformList.Add(item.GetNamedChild("CA").transform);
    //    }

    //    for (int i = 0; i < casualtyTransformList.Count; i++)
    //    {
    //        int  old = i;
    //        int temp = i;
    //        i = old;
    //        GameObject areaCamBtn = Instantiate(areaCamBtnPrefab, camListParent.transform);
    //        areaCamBtn.GetComponentInChildren<Text>().text = casualtyList[i].GetComponent<ObjectIdentifier>().objectId;
    //        areaCamBtn.GetComponent<Button>().onClick.AddListener(() => {
    //            SetCameraView(casualtyTransformList[temp]);
    //            CurViewID = -1;
    //            CurrentAngleText.text = areaCamBtn.GetComponentInChildren<Text>().text;
    //            freeCamViewPanel.alpha = 0;
    //            isAreaCamMode = false;
    //        });
    //    }

    //}

    private float GetMin(float old, float cur)
    {
        if (old == 0)
        {
            old = cur;
        }
        else
        {
            if (cur < old) old = cur;
        }
        return old;
    }

    private float GetMax(float old, float cur)
    {
        if (old == 0)
        {
            old = cur;
        }
        else
        {
            if (cur > old) old = cur;
        }
        return old;
    }

    private void GenerateAreaCamBtn()
    {
        List<Button> bl = new List<Button>();
        for(int i = 0;i<areaCamList.Count;i++)
        {
            GameObject areaCamBtn = Instantiate(areaCamBtnPrefab, camListParent.transform);
            areaCamBtn.GetComponentInChildren<Text>().text = areaCamList[i].name;
            bl.Add(areaCamBtn.GetComponent<Button>());
        }
        OnPreSetCamBtnListener(bl, 0);
        OnPreSetCamBtnListener(bl, 1);
        OnPreSetCamBtnListener(bl, 2);
        OnPreSetCamBtnListener(bl, 3);
    }
    private void OnPreSetCamBtnListener(List<Button> bl,int i)
    {
        bl[i].onClick.AddListener(() => {
            SetCameraView(areaCamList[i].gameObject.transform);
            CurViewID = -1;
            CurrentAngleText.text = bl[i].GetComponentInChildren<Text>().text;
            freeCamViewPanel.alpha = 0;
            isAreaCamMode = true;
        });
    }
    private void OnOffCanvasGroup(CanvasGroup cg , CanvasGroup cg2)
    {
        if(cg != null)
        {
            if(!cg.interactable)
            {
                cg.alpha = 1.0f;
                cg.blocksRaycasts = true;
                cg.interactable = true;

                cg2.alpha = 0f;
                cg2.blocksRaycasts = false;
                cg2.interactable = false;
            }
            else
            {
                cg.alpha = 0f;
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
        }
    }
    public void StartAreaCameraViewPanel()
    {
        OnOffCanvasGroup(camListPanel, userListPanel);

    }

}
