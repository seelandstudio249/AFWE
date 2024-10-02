using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListChunker {
    public static List<List<T>> SplitList<T>(List<T> list, int chunkSize) {
        Debug.LogError(list.Count);
        List<List<T>> chunks = new List<List<T>>();
        int totalChunks = (list.Count + chunkSize - 1) / chunkSize; // calculate number of chunks

        for (int i = 0; i < totalChunks; i++) {
            List<T> chunk = list.GetRange(i * chunkSize, Mathf.Min(chunkSize, list.Count - i * chunkSize));
            chunks.Add(chunk);
        }

        return chunks;
    }
}

public class UtilitiesFunctions : MonoBehaviour
{
    public static UtilitiesFunctions Instance;

    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
