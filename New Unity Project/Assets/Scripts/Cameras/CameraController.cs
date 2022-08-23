using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 30f;
    [SerializeField] private float screenBorderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    private Vector2 previousInput;
    private Controls controls;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);

        controls = new Controls();

        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;

        controls.Enable();
    }
    [ClientCallback]
    private void Start() 
    {
        GameStartMenu.OnGameStartSetting += SetDefaultCameraPosition;
    }

    [ClientCallback]
    private void Update()
    {
        if(!hasAuthority || !Application.isFocused) { return; }

        UpdateCameraPosition();

    }
    [ClientCallback]
    private void OnDestroy() 
    {
        GameStartMenu.OnGameStartSetting -= SetDefaultCameraPosition;
    }

    private void SetDefaultCameraPosition()
    {   
        if(playerCameraTransform == null) { return; }
        Vector3 pos = playerCameraTransform.position;
        UnitBase[] unitBases = GameObject.FindObjectsOfType<UnitBase>();
        foreach(UnitBase unitBase in unitBases){

            if(!unitBase.hasAuthority) { continue; }
            
            pos.x = unitBase.gameObject.transform.position.x;
            pos.z = -pos.y * (3f/4f) + unitBase.gameObject.transform.position.z; // 60도 삼각함수
            break;
        }

        playerCameraTransform.position = pos;
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        if(previousInput == Vector2.zero) // Mouse Not Keyboard Input
        {
            Vector3 cursor = Vector3.zero;

            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            if(cursorPosition.y >= Screen.height - screenBorderThickness)
            {
                cursor.z += 1;
            }
            else if(cursorPosition.y <= screenBorderThickness)
            {
                cursor.z -= 1;
            }
            if(cursorPosition.x >= Screen.width - screenBorderThickness)
            {
                cursor.x += 1;
            }
            else if(cursorPosition.x <= screenBorderThickness)
            {
                cursor.x -= 1;
            }

            pos += cursor.normalized * speed * Time.deltaTime;
        }
        else // Keyboard Input
        {
            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

        playerCameraTransform.position = pos;
    }

    private void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previousInput = ctx.ReadValue<Vector2>();
    }
}
