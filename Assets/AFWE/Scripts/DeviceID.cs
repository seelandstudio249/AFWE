using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceID : MonoBehaviour
{
	public static string GetDeviceId() {
#if ENABLE_WINMD_SUPPORT
        var deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
        return deviceInfo.Id.ToString();
#else
		return "Device ID unavailable in the editor or on non-UWP platforms.";
#endif
	}

	private void Start() {
		//hintText.text = (SystemInfo.deviceUniqueIdentifier.ToString());
		//hintText.text = GetDeviceId();
	}
}
