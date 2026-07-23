using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class RunningGraph : MonoBehaviour
{
    [Header("Display Text")]
    public TextMeshProUGUI maxText;
    public TextMeshProUGUI minText;
    public TextMeshProUGUI measurementText;
    [Header("Graph Settings")]
    public RectTransform graphContainer; // Assign the UI Panel
    public GameObject pointPrefab; // Assign a UI Image (dot/circle)

    [Header("Graph Behavior")]
    public float updateInterval = 0.5f; // Time between updates
    public int maxVisiblePoints = 10; // Maximum points before shifting

    private List<GameObject> points = new List<GameObject>();
    private int currentIndex = 0;
    private float graphWidth;
    private float graphHeight;
    private float xSpacing;
    private float minYValue;
    private float maxYValue;

    public List<int> DataArray = new List<int>(); // Using List to allow dynamic updates

    void Start()
    {
    }
    public void SetUpGraphData(string MeasurementText, List<int> dataArray)
    {
        measurementText.text = MeasurementText;
        DataArray = dataArray;
        UpdateGraphSize();
    }

    public void AddNewValue(int newValue)
    {
        DataArray.Add(newValue);
        UpdateGraphSize(); // Update scaling dynamically
    }

    public void StartRunningGraph()
    {
        if (DataArray == null || DataArray.Count == 0)
        {
            Debug.LogWarning("DataArray is empty. Cannot start graph.");
            return;
        }

        currentIndex = 0;
        UpdateGraphSize();
        ClearGraph(); // Clear previous data points

        InvokeRepeating(nameof(UpdateGraph), 0, updateInterval);
    }

    public void StopRunningGraph()
    {
        CancelInvoke(nameof(UpdateGraph));
        ClearGraph();
    }

     void UpdateGraphSize()
    {
        if (DataArray == null || DataArray.Count == 0) return;

        graphWidth = graphContainer.rect.width;
        graphHeight = graphContainer.rect.height;
        xSpacing = graphWidth / (float)maxVisiblePoints; // Adjust for visible points

        // Find dynamic min/max values
        minYValue = Mathf.Min(DataArray.ToArray());
        maxYValue = Mathf.Max(DataArray.ToArray());

        if (minYValue == maxYValue) maxYValue += 1; // Prevent division by zero
        minText.text = minYValue.ToString();
        maxText.text = maxYValue.ToString();
        // Adjust all points when min/max changes
        RescaleGraph();
    }

    void UpdateGraph()
    {
        if (DataArray == null || DataArray.Count == 0) return;

        if (currentIndex >= DataArray.Count)
        {
            currentIndex = 0; // Restart when all data is shown
        }

        float newY = DataArray[currentIndex]; // Get current data point
        currentIndex++;

        // Update dynamic min/max values
        if (newY < minYValue || newY > maxYValue)
        {
            UpdateGraphSize(); // Recalculate scaling when new extreme values appear
        }

        // Normalize Y within the graph bounds
        float normalizedY = (newY - minYValue) / (maxYValue - minYValue);
        float newYPosition = normalizedY * graphHeight;

        // Ensure the point starts from the leftmost of the graph
        float newX = points.Count * xSpacing;

        if (newX <= graphWidth)
        {
            GameObject point = Instantiate(pointPrefab, graphContainer);
            RectTransform rt = point.GetComponent<RectTransform>();

            rt.anchoredPosition = new Vector2(newX, newYPosition);
            points.Add(point);
        }

        // Shift graph when max points are reached
        if (points.Count > maxVisiblePoints)
        {
            Destroy(points[0]);
            points.RemoveAt(0);

            foreach (GameObject p in points)
            {
                RectTransform prt = p.GetComponent<RectTransform>();
                prt.anchoredPosition -= new Vector2(xSpacing, 0);
            }
        }
    }

    void RescaleGraph()
    {
        foreach (GameObject point in points)
        {
            RectTransform rt = point.GetComponent<RectTransform>();
            float pointValue = DataArray[points.IndexOf(point)];

            // Recalculate Y position based on new min/max
            float normalizedY = (pointValue - minYValue) / (maxYValue - minYValue);
            float newYPosition = normalizedY * graphHeight;

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, newYPosition);
        }
    }

     void ClearGraph()
    {
        foreach (GameObject point in points)
        {
            Destroy(point);
        }
        points.Clear();
    }
}
