using System;
using UnityEngine;

public class RotateAndPoint : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }
    
    public enum AxisDirection
    {
        Positive,
        Negative
    }

    public AxisDirection rotationDirection = AxisDirection.Positive; // Direction to rotate around the axis
    public Axis rotationAxis = Axis.Y; // Axis to rotate around (default is Y-axis)
    public Axis forwardAxis = Axis.Z; // Defined forward axis (default is Z-axis)

    void Update()
    {
        // Get the main camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Calculate the direction to the main camera
            Vector3 direction = mainCamera.transform.position - transform.position;

            

            // If direction is not zero, rotate
            if (direction.magnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                // Adjust rotation based on the defined forward axis
                Vector3 forward = Vector3.zero;
                switch (forwardAxis)
                {
                    case Axis.X:
                        switch (rotationDirection)
                        {
                            case AxisDirection.Positive:
                                forward = Vector3.right;
                                break;
                            case AxisDirection.Negative:
                                forward = Vector3.left;
                                break;
                        }
                        break;
                    case Axis.Y:
                        switch (rotationDirection)
                        {
                            case AxisDirection.Positive:
                                forward = Vector3.up;
                                break;
                            case AxisDirection.Negative:
                                forward = Vector3.down;
                                break;
                        }
                        break;
                    case Axis.Z:
                        switch (rotationDirection)
                        {
                            case AxisDirection.Positive:
                                forward = Vector3.forward;
                                break;
                            case AxisDirection.Negative:
                                forward = Vector3.back;
                                break;
                        }
                        break;
                }
                
                var dir = Quaternion.LookRotation(direction, forward);
                // Set the rotation with the specified forward direction
                
                // Remove unwanted axes based on the selected rotation axis
                switch (rotationAxis)
                {
                    case Axis.X:
                        transform.rotation = Quaternion.Euler(dir.eulerAngles.x, 0, 0);
                        break;
                    case Axis.Y:
                        transform.rotation = Quaternion.Euler(0, dir.eulerAngles.y, 0);
                        break;
                    case Axis.Z:
                        transform.rotation = Quaternion.Euler(0,0, dir.eulerAngles.z);
                        break;
                }
            }
        }
    }
}
