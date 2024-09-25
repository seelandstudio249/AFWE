using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QRCodeScannerIndicator : MonoBehaviour
{
    public static QRCodeScannerIndicator instance;

    public GameObject spriteIndicator;
    public TMP_Text loadingStatusText;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    public void ObjectActivation(GameObject obj, bool isActive) {
        obj.SetActive(isActive);
    }
}
