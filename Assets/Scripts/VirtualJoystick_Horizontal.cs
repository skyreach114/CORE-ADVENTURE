using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick_Horizontal : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform bg;
    public RectTransform knob;
    public float radius = 80f; // px
    [Range(0f, 1f)] public float dashThreshold = 0.8f; // 横方向の正規化閾値
    public float swipeSpeedThreshold = 1200f; // optional: px/sec for quick swipe -> dash

    public float InputX { get; private set; }  // -1 .. 1
    public bool IsDashing { get; private set; }

    Vector2 lastPos;
    float lastTime;

    // Canvas参照（座標変換用）
    private Canvas parentCanvas;

    void Start()
    {
        if (bg == null || knob == null) Debug.LogError("Assign bg and knob.");

        // 親のCanvasを取得
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("VirtualJoystick: Canvas が見つかりません！UI要素として配置してください。");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
        lastPos = eventData.position;
        lastTime = Time.unscaledTime;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bg,
            eventData.position,
            parentCanvas.worldCamera,  // Overlay なら null、Camera なら指定
            out localPoint
        );

        // 水平方向のみで制限
        float clampedX = Mathf.Clamp(localPoint.x, -radius, radius);

        // ノブの位置を更新（Yは0固定）
        knob.anchoredPosition = new Vector2(clampedX, 0f);

        // 正規化した入力値（-1 ～ 1）
        InputX = clampedX / radius;

        // --- ノブの見た目変化 ---
        if (Mathf.Abs(InputX) > 0.05f)
        {
            // 左右反転
            Vector3 scale = knob.localScale;
            scale.x = InputX < 0 ? -0.9f : 0.9f;
            knob.localScale = scale;

            // 傾き演出（オプション）
            knob.localRotation = Quaternion.Euler(0, 0, InputX * 10f);
        }
        else
        {
            knob.localRotation = Quaternion.identity;
        }

        // --- ダッシュ判定 ---
        // 1. スティックを大きく倒したらダッシュ
        IsDashing = Mathf.Abs(InputX) >= dashThreshold;

        // 2. 素早くスワイプした場合もダッシュ
        float now = Time.unscaledTime;
        float dt = now - lastTime;
        if (dt > 0)
        {
            float speed = Mathf.Abs((eventData.position.x - lastPos.x) / dt);
            if (speed >= swipeSpeedThreshold) IsDashing = true;
        }
        lastPos = eventData.position;
        lastTime = now;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // リセット
        InputX = 0f;
        IsDashing = false;
        knob.anchoredPosition = Vector2.zero;
        knob.localScale = Vector3.one;
        knob.localRotation = Quaternion.identity;
    }
}
