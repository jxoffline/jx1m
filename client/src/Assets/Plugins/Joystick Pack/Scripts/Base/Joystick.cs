using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Axis option Enumerator
/// </summary>
public enum AxisOptions
{
    /// <summary>
    /// Both horizontal and vertical
    /// </summary>
    Both,
    /// <summary>
    /// Horizontal only
    /// </summary>
    Horizontal,
    /// <summary>
    /// Vertical only
    /// </summary>
    Vertical
}

/// <summary>
/// UI JoyStick
/// </summary>
public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    #region Define
    /// <summary>
    /// Handle range
    /// </summary>
    [SerializeField]
    private float handleRange = 1;
    /// <summary>
    /// Dead zone
    /// </summary>
    [SerializeField]
    private float deadZone = 0;
    /// <summary>
    /// Axis options
    /// </summary>
    [SerializeField]
    private AxisOptions axisOptions = AxisOptions.Both;
    /// <summary>
    /// Snap X
    /// </summary>
    [SerializeField]
    private bool snapX = false;
    /// <summary>
    /// Snap Y
    /// </summary>
    [SerializeField]
    private bool snapY = false;
    /// <summary>
    /// Background
    /// </summary>
    [SerializeField]
    protected RectTransform background = null;
    /// <summary>
    /// Handle
    /// </summary>
    [SerializeField]
    private RectTransform handle = null;
    #endregion

    #region Private fields
    /// <summary>
    /// RectTransform contains this JoyStick
    /// </summary>
    private RectTransform baseRect = null;
    /// <summary>
    /// Canvas contains this JoyStick
    /// </summary>
    private Canvas canvas;
    /// <summary>
    /// Camera refers this JoyStick
    /// </summary>
    private Camera cam;
    /// <summary>
    /// Input of this JoyStick
    /// </summary>
    private Vector2 input = Vector2.zero;
    #endregion

    #region Properties
    /// <summary>
    /// Horizontal value of JoyStick
    /// </summary>
    public float Horizontal
    {
        get
        {
            return (this.snapX) ? this.SnapFloat(this.input.x, AxisOptions.Horizontal) : this.input.x;
        }
    }

    /// <summary>
    /// Vertical value of JoyStick
    /// </summary>
    public float Vertical
    {
        get
        {
            return (this.snapY) ? this.SnapFloat(this.input.y, AxisOptions.Vertical) : this.input.y;
        }
    }

    /// <summary>
    /// Current direction Vector of JoyStick
    /// </summary>
    public Vector2 Direction
    {
        get
        {
            return new Vector2(this.Horizontal, this.Vertical);
        }
    }

    /// <summary>
    /// Handle range of JoyStick
    /// </summary>
    public float HandleRange
    {
        get
        {
            return this.handleRange;
        }
        set
        {
            this.handleRange = Mathf.Abs(value);
        }
    }

    /// <summary>
    /// Dead zone of JoyStick
    /// </summary>
    public float DeadZone
    {
        get
        {
            return this.deadZone;
        }
        set
        {
            this.deadZone = Mathf.Abs(value);
        }
    }

    /// <summary>
    /// Axis options of JoyStick
    /// </summary>
    public AxisOptions AxisOptions
    {
        get
        {
            return this.axisOptions;
        }
        set
        {
            this.axisOptions = value;
        }
    }
    #endregion

    #region Core MonoBehaviour
    /// <summary>
    /// This function is called at the first frame
    /// </summary>
    protected virtual void Start()
    {
        this.HandleRange = this.handleRange;
        this.DeadZone = this.deadZone;
        this.baseRect = this.GetComponent<RectTransform>();
        this.canvas = this.GetComponentInParent<Canvas>();

        if (this.canvas == null)
        {
            Debug.LogError("The Joystick is not placed inside a canvas");
        }

        Vector2 center = new Vector2(0.5f, 0.5f);
        this.background.pivot = center;
        this.handle.anchorMin = center;
        this.handle.anchorMax = center;
        this.handle.pivot = center;
        this.handle.anchoredPosition = Vector2.zero;
    }
    #endregion

    #region Impements IPointer
    /// <summary>
    /// This function is called when pointer down
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        this.OnDrag(eventData);
    }

    /// <summary>
    /// This function is called during the drag frames
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        this.cam = null;
        if (this.canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            this.cam = this.canvas.worldCamera;
        }

        Vector2 position = RectTransformUtility.WorldToScreenPoint(this.cam, this.background.position);
        Vector2 radius = this.background.sizeDelta / 2;
        this.input = (eventData.position - position) / (radius * this.canvas.scaleFactor);
        this.FormatInput();
        this.HandleInput(this.input.magnitude, this.input.normalized, radius, this.cam);
        this.handle.anchoredPosition = this.input * radius * this.handleRange;
    }

    /// <summary>
    /// This function is called when pointer up
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        this.input = Vector2.zero;
        this.handle.anchoredPosition = Vector2.zero;
    }
    #endregion

    #region Private methods
    /// <summary>
    /// Handle input value
    /// </summary>
    /// <param name="magnitude"></param>
    /// <param name="normalised"></param>
    /// <param name="radius"></param>
    /// <param name="cam"></param>
    protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > this.deadZone)
        {
            if (magnitude > 1)
            {
                this.input = normalised;
            }
        }
        else
        {
            this.input = Vector2.zero;
        }
    }

    /// <summary>
    /// Format input
    /// </summary>
    private void FormatInput()
    {
        if (this.axisOptions == AxisOptions.Horizontal)
        {
            this.input = new Vector2(this.input.x, 0f);
        }
        else if (this.axisOptions == AxisOptions.Vertical)
        {
            this.input = new Vector2(0f, this.input.y);
        }
    }

    /// <summary>
    /// Snap the value corresponding to axis
    /// </summary>
    /// <param name="value"></param>
    /// <param name="snapAxis"></param>
    /// <returns></returns>
    private float SnapFloat(float value, AxisOptions snapAxis)
    {
        if (value == 0)
        {
            return value;
        }

        if (this.axisOptions == AxisOptions.Both)
        {
            float angle = Vector2.Angle(this.input, Vector2.up);
            if (snapAxis == AxisOptions.Horizontal)
            {
                if (angle < 22.5f || angle > 157.5f)
                {
                    return 0;
                }
                else
                {
                    return (value > 0) ? 1 : -1;
                }
            }
            else if (snapAxis == AxisOptions.Vertical)
            {
                if (angle > 67.5f && angle < 112.5f)
                {
                    return 0;
                }
                else
                {
                    return (value > 0) ? 1 : -1;
                }
            }
            return value;
        }
        else
        {
            if (value > 0)
            {
                return 1;
            }
            if (value < 0)
            {
                return -1;
            }
        }
        return 0;
    }

    /// <summary>
    /// Convert screen pos to anchor pos
    /// </summary>
    /// <param name="screenPosition"></param>
    /// <returns></returns>
    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.baseRect, screenPosition, this.cam, out Vector2 localPoint))
        {
            Vector2 pivotOffset = this.baseRect.pivot * this.baseRect.sizeDelta;
            return localPoint - (this.background.anchorMax * this.baseRect.sizeDelta) + pivotOffset;
        }
        return Vector2.zero;
    }
    #endregion
}