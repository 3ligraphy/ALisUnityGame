using UnityEngine;

/// <summary>
/// Shared raycast cache so only ONE physics query runs per tap instead of N (one per clickable object).
/// Fixes lag when facing many objects in museum/gemstones scenes.
/// </summary>
public static class InteractionHitCache
{
    private static int cachedFrame = -1;
    private static Transform cachedHitTransform = null;
    private static Vector3 cachedScreenPoint;
    private static Camera cachedCamera;
    private const float DefaultRadius = 2.5f;

    /// <summary>
    /// Get the transform hit by a tap this frame. Performs at most one SphereCast + one Raycast per frame.
    /// </summary>
    public static Transform GetHitTransformForTap(Camera cam, float sphereRadius = DefaultRadius)
    {
        if (cam == null) return null;

        Vector3 screenPoint = Input.mousePosition;
        if (Input.touchCount > 0)
            screenPoint = Input.GetTouch(0).position;

        // Reuse result if same frame and same tap position
        if (cachedFrame == Time.frameCount && cachedCamera == cam &&
            Vector3.Distance(cachedScreenPoint, screenPoint) < 1f)
        {
            return cachedHitTransform;
        }

        cachedFrame = Time.frameCount;
        cachedCamera = cam;
        cachedScreenPoint = screenPoint;

        Ray ray = cam.ScreenPointToRay(screenPoint);
        RaycastHit hit;

        if (Physics.SphereCast(ray, sphereRadius, out hit, Mathf.Infinity))
        {
            cachedHitTransform = hit.transform;
            return cachedHitTransform;
        }

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            cachedHitTransform = hit.transform;
            return cachedHitTransform;
        }

        cachedHitTransform = null;
        return null;
    }
}
