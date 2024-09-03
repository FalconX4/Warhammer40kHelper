using UnityEngine;

public class PinchZoom : MonoBehaviour
{
    public RectTransform Canvas;
    public RectTransform Container;
    public float maxScale = 2f;
    public float zoomSpeed = 0.1f;

    private float lastTouchDistance;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                Zoom(touch0.position, touch1.position);
            else
                lastTouchDistance = 0;
        }
    }

    void Zoom(Vector2 currentTouch0, Vector2 currentTouch1)
    {

        float currentTouchDistance = Vector2.Distance(currentTouch0, currentTouch1);

        if (lastTouchDistance != 0)
        {
            float deltaDistance = currentTouchDistance - lastTouchDistance;
            float deltaScale = deltaDistance * zoomSpeed * Time.deltaTime;
            float newScale = Mathf.Clamp(Container.localScale.x + deltaScale, 1f, maxScale);
            Container.localScale = new Vector3(newScale, newScale, 1f);

            Vector2 midPoint = (currentTouch0 + currentTouch1) / 2f;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas, midPoint, null, out var localMidPoint);
            var possiblePosition = (Container.anchoredPosition - new Vector2(localMidPoint.x, localMidPoint.y)) / newScale;
            var max = new Vector2((Canvas.sizeDelta.x / 2f) * (newScale - 1f), (Canvas.sizeDelta.y / 2f) * (newScale - 1f));
            Container.anchoredPosition = new Vector2(Mathf.Clamp(possiblePosition.x, -max.x, max.x), Mathf.Clamp(possiblePosition.y, -max.y, max.y));
        }

        lastTouchDistance = currentTouchDistance;
    }
}