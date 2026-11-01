
using UnityEngine;

public class ARPinchToZoom : MonoBehaviour
{
    public float zoomSpeed = 1f;
    public float minScale = 0.5f;
    public float maxScale = 2f;

    private float startDistance;
    private Vector3 startScale;

    void Update()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            float currentDistance = Vector2.Distance(
                touch1.position,
                touch2.position
            );

            if (touch2.phase == TouchPhase.Began)
            {
                startDistance = currentDistance;
                startScale = transform.localScale;
            }
            else if (startDistance > 0)
            {
                float scaleFactor =
                    currentDistance / startDistance;

                float newScale = Mathf.Clamp(
                    scaleFactor * zoomSpeed,
                    minScale,
                    maxScale
                );

                transform.localScale =
                    startScale * newScale;
            }
        }
        else
        {
            startDistance = 0;
        }
    }
}
