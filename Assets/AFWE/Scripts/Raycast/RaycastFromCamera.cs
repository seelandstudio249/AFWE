using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameKit.Dependencies.Utilities;

public class RaycastFromCamera : MonoBehaviour
{
    public Camera cam; // Assign camera in Inspector
    public int rayCount = 30; // Number of rays to cast
    public float rayLength = 10f; // Length of the rays
    public string targetLayer = "Spatial Mesh";
    public Material myMat;
    public List<Vector3> meshDetectList = new List<Vector3>();
    public int rows, cols;
    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main; // Get main camera if not assigned
        }
        for (int i = 0; i < rayCount; i++)
        {
            meshDetectList.Add(Vector3.zero);
        }
        //StartCoroutine("CastRays");
    }

    void Update()
    {
      CastRays();
    }

    //IEnumerator CastRays()
    void CastRays()
    {
        float screenWidth = Screen.width; // Get actual screen width
        float screenHeight = Screen.height; // Get actual screen height

        cols = Mathf.CeilToInt(Mathf.Sqrt(rayCount)); // Columns based on square root
        rows = Mathf.CeilToInt((float)rayCount / cols); // Calculate rows

        int c = -1;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                c++;
                if (row * cols + col >= rayCount) // Ensure exactly 30 points
                    return;

                // Calculate screen position
                float x = (col + 0.5f) / cols * screenWidth;
                float y = (row + 0.5f) / rows * screenHeight;

                Ray ray = cam.ScreenPointToRay(new Vector3(x, y, 0));
                Debug.DrawRay(ray.origin, ray.direction * c, Color.green);
                int layerMask = 1 << LayerMask.NameToLayer(targetLayer);
                if (Physics.Raycast(ray, out RaycastHit hit, rayLength, layerMask))
                {
                    Debug.Log($"Hit {hit.collider.name} or {hit.transform.gameObject.name} at {hit.point}");
                    MeshRenderer renderer = hit.collider.GetComponent<MeshRenderer>();

                    renderer.material = myMat;
                    meshDetectList[c] = (hit.point);
                }
                else
                {
                    meshDetectList[c] = new Vector3(x, y, rayLength);
                   // myRayClassList[i] = new MyRayClass(Color.green, new Vector3(x, y, i));
                }

            }
        }
    }

	public static Vector3 CalculateCenter(List<Vector3> points) {
		if (points == null || points.Count == 0) {
			Debug.LogWarning("No points provided to calculate the center.");
			return Vector3.zero;
		}

		Vector3 sum = Vector3.zero;
		foreach (Vector3 point in points) {
			sum += point;
		}

		return sum / points.Count;
	}
}
