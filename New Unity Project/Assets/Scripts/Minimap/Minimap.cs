using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    [SerializeField] private float mapScale = 30f;
    [SerializeField] private float offset = -6f;

    private Transform playerCameraTransform;

    private void Update() 
    {
        if(playerCameraTransform != null) { return; }

        if(NetworkClient.connection.identity == null){ return; }

        playerCameraTransform = NetworkClient.connection.identity.GetComponent<GamePlayer>().GetCameraTransform();
    }
    
    private void MoveCamera()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            mousePosition,
            null,
            out Vector2 localPoint)) // is point in rectangle
        {
            return;
        }

        Vector2 lerp = new Vector2(
            (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
            (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        Vector3 newCameraPosition = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerp.x),
            playerCameraTransform.position.y,
            Mathf.Lerp(-mapScale, mapScale, lerp.y ));

        //Camera Move

        playerCameraTransform.position = newCameraPosition + new Vector3(0f, 0f, offset);

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }
}
