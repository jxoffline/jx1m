using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
    private Vector2 OriginPos;

    protected override void Start()
    {
        base.Start();
        this.OriginPos = this.background.anchoredPosition;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        this.background.anchoredPosition = this.ScreenPointToAnchoredPosition(eventData.position);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        this.background.anchoredPosition = this.OriginPos;
        base.OnPointerUp(eventData);
    }
}