using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracking : NetworkBehaviour
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
    public Transform myCameraTransform;
    public readonly SyncVar<string> myDeviceName = new();

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            myCameraTransform = Camera.main.transform;
            myDeviceName.Value = SystemInfo.deviceName;
        }
    }

    private void LateUpdate()
    {
        if (myCameraTransform != null)
        {
            gameObject.transform.SetPositionAndRotation(myCameraTransform.position, myCameraTransform.rotation);
        }
    }

}
