using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyCameraController : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera = null;
    [SerializeField] private List<GameObject> cameraPositions = new List<GameObject>();
    [SerializeField] private List<GameObject> cameraLookAts = new List<GameObject>();
    [SerializeField] private float smoothness = 0.5f;

    private Vector3 velocity = Vector3.zero;

    private int cameraFlag = 0;
    
    private void Update() 
    {
        if(cameraFlag == 0) { return; }
        MoveCamera(cameraFlag);
    }
    public void MoveToPosition(int num)
    {
        cameraFlag = num;
    }

    private void MoveCamera(int index)
    {
        
        if(index <= 0 || index >= cameraPositions.Count) { return; } 


        if(Vector3.Distance(mainCamera.transform.position, cameraPositions[index].transform.position) == 0) { cameraFlag = 0; }

        mainCamera.transform.position = Vector3.SmoothDamp(
                                                            mainCamera.transform.position, 
                                                            cameraPositions[index].transform.position,
                                                            ref velocity,
                                                            smoothness);
        mainCamera.transform.LookAt(cameraLookAts[index].transform.position);

    }
}
