using UnityEngine;
using UnityEngine.Serialization;

public class KeepInView : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject targetObject;
    public float distanceFromCamera = 10f;
    public float padding = 1f;
    public Position presetPosition;
    [Range(0,1)]
    public float positionLerpSpeed = 0.618f;
    public bool rotateWithCamera = false;
    [Range(0,1)]

    public float rotationLerpSpeed = 0.618f;


    public enum Position
    {
        BottomLeft,
        BottomMiddleRight,
        BottomMiddle,
        BottomRight,
        Left,
        Middle,
        Right,
        TopLeft,
        TopMiddle,
        TopRight
    }

    void LateUpdate()
    {
        KeepObjectInView(presetPosition);
    }

    void KeepObjectInView(Position position)
    {
        Vector3 screenPosition = GetScreenPosition(position);
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(screenPosition);

        Vector3 objectOffset = CalculateOffset(worldPosition);
        worldPosition += objectOffset;

        targetObject.transform.position =
            Vector3.Lerp(targetObject.transform.position, worldPosition, positionLerpSpeed); 

        if (rotateWithCamera)
        {
            targetObject.transform.rotation = Quaternion.Lerp(targetObject.transform.rotation,  mainCamera.transform.rotation, rotationLerpSpeed);
        }
    }

    Vector3 GetScreenPosition(Position position)
    {
        switch (position)
        {
            case Position.BottomLeft: return new Vector3(0, 0, distanceFromCamera);
            case Position.BottomMiddle: return new Vector3(0.5f, 0, distanceFromCamera);
            case Position.BottomRight: return new Vector3(1, 0, distanceFromCamera);
            case Position.BottomMiddleRight: return new Vector3(0.75f, 0, distanceFromCamera);
            case Position.Left: return new Vector3(0, 0.5f, distanceFromCamera);
            case Position.Middle: return new Vector3(0.5f, 0.5f, distanceFromCamera);
            case Position.Right: return new Vector3(1, 0.5f, distanceFromCamera);
            case Position.TopLeft: return new Vector3(0, 1, distanceFromCamera);
            case Position.TopMiddle: return new Vector3(0.5f, 1, distanceFromCamera);
            case Position.TopRight: return new Vector3(1, 1, distanceFromCamera);
            default: return Vector3.zero;
        }
    }

    Vector3 CalculateOffset(Vector3 worldPosition)
    {
        Vector3 centerWorldPosition = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distanceFromCamera));
        Vector3 objectOffset = centerWorldPosition - worldPosition;
        return objectOffset.normalized * padding;
    }

    void OnDrawGizmos()
    {
        Vector3 screenPosition = GetScreenPosition(presetPosition);
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(screenPosition);
        Vector3 objectOffset = CalculateOffset(worldPosition);
        worldPosition += objectOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(worldPosition, padding);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetObject.transform.position, 0.1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(worldPosition, objectOffset);
    }
}
