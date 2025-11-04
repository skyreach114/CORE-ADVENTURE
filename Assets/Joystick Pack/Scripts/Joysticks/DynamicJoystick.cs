using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicJoystick : Joystick
{
    private PlayerController playerControllerRef;
    public float quickSwipeSpeedThreshold = 1200f; // px/sec
    private Vector2 lastPointerPos;
    private float lastPointerTime;

    public float MoveThreshold { get { return moveThreshold; } set { moveThreshold = Mathf.Abs(value); } }

    [SerializeField] private float moveThreshold = 1;

    protected override void Start()
    {
        MoveThreshold = moveThreshold;
        base.Start();
        background.gameObject.SetActive(false);
        playerControllerRef = GetComponent<PlayerController>();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true);
        lastPointerPos = eventData.position;
        lastPointerTime = Time.unscaledTime;
        base.OnPointerDown(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        float now = Time.unscaledTime;
        float dt = now - lastPointerTime;
        if (dt > 0)
        {
            float speed = Mathf.Abs((eventData.position.x - lastPointerPos.x) / dt);
            if (speed >= quickSwipeSpeedThreshold)
            {
                // PlayerController を直接参照するのが簡単（InspectorでViewにセット）
                if (playerControllerRef != null)
                {
                    //playerControllerRef.RegisterMobileDashBurst();
                }
            }
        }
        lastPointerPos = eventData.position;
        lastPointerTime = now;

        base.OnDrag(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        background.gameObject.SetActive(false);
        base.OnPointerUp(eventData);
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > moveThreshold)
        {
            Vector2 difference = normalised * (magnitude - moveThreshold) * radius;
            background.anchoredPosition += difference;
        }
        base.HandleInput(magnitude, normalised, radius, cam);
    }
}