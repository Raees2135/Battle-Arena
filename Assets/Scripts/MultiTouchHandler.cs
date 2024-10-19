using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class MultiTouchHandler : MonoBehaviour
{
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                // Process touch began
                Vector2 touchPosition = touch.screenPosition;
                Debug.Log($"Touch {touch.touchId} began at {touchPosition}");
            }

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                // Process touch moved
                Vector2 touchPosition = touch.screenPosition;
                Debug.Log($"Touch {touch.touchId} moved to {touchPosition}");
            }

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                // Process touch ended
                Vector2 touchPosition = touch.screenPosition;
                Debug.Log($"Touch {touch.touchId} ended at {touchPosition}");
            }
        }
    }
}
