using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Kamgam.UGUIWorldImage;

namespace Kamgam.UGUIWorldImage.Examples
{
    public partial class Rotate : MonoBehaviour
    {
        //Main camera
        public Camera mainCamera;

        //GridManager
        public GridManager gridManager;
        
        public Vector3 offsetRotation = Vector3.zero;
        
        public bool justRotate = false;

        // Start is called before the first frame update
        public void Start()
        {
            //Get the main camera
            mainCamera = Camera.main;

            //Get the GridManager
            gridManager = FindFirstObjectByType<GridManager>();
        }

        public void Update()
        {
            if (justRotate)
            {
                transform.Rotate(Vector3.up, Time.deltaTime * 90f, Space.Self);
            }
            
            if (gridManager == null)
            {
                transform.Rotate(Vector3.up, Time.deltaTime * 90f, Space.Self);
                
                //If the gridManager is not found, try to find it again
                gridManager = FindFirstObjectByType<GridManager>();
                return;
            }
            
            Vector3 forward = gridManager.gameObject.transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();
            
            float angle = Vector3.SignedAngle(cameraForward, forward, Vector3.up);
            
            //camera Y rotation 
            float cameraYRotation = mainCamera.transform.rotation.eulerAngles.y;
            
            //Rotate the object to face the angle
            transform.rotation = Quaternion.Euler(0 + offsetRotation.x, angle + offsetRotation.y, 0 + offsetRotation.z);
        }
    }
}