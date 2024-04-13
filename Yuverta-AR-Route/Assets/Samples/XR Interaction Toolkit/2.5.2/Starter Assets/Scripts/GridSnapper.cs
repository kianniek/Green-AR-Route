namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class GridSnapper : MonoBehaviour
    {
        [SerializeField] private GameObject previewObject;  // Holds the preview duplicate of the object
        [SerializeField] protected bool m_smoothPosition = true;
        protected Vector3 m_targetPosition;
        protected Vector3 m_originalPosition;
        //Varibles for override grid size
        private float m_overridegridSize = 1.0f;
        private bool m_HasOverrideGridSize = false;

        private bool flaggedForRemoval = false;
        private void Start()
        {
            CreatePreviewObject();
        }

        // Update the position to snap to grid when moved
        void Update()
        {
            foreach (var position in GridManager.Instance.OccupiedPositions)
            {
                print(position.Key);
            }
            if (transform.hasChanged)
            {
                UpdatePreviewObject();

                m_originalPosition = transform.position;
                m_targetPosition = SnapToGrid(m_originalPosition);
                if (!GridManager.Instance.IsPositionOccupied(m_targetPosition))
                {
                    StopAllCoroutines();
                    StartCoroutine(SmoothPosition(m_targetPosition));
                }
                else
                {
                    // Move back to the original position if the target is occupied
                    StopAllCoroutines();
                    StartCoroutine(SmoothPosition(m_originalPosition));
                }
                transform.hasChanged = false;
            }
        }

        IEnumerator SmoothPosition(Vector3 position)
        {
            if (m_smoothPosition)
            {
                while (Vector3.Distance(transform.position, position) > 0.001f)
                {
                    transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10);
                    yield return null;
                }
            }
            else
            {
                transform.position = position;
            }
            // Ensure the object is snapped to the grid
            RemovePreviewObject();
            transform.position = position;
            GridManager.Instance.SetPositionOccupied(position, true);
        }

        // Function to snap a position to the nearest grid point using the grid size from GridManager
        private Vector3 SnapToGrid(Vector3 position)
        {
            float gridSize = m_HasOverrideGridSize ? m_overridegridSize : GridManager.Instance.GridSize;
            float x = Mathf.Round(position.x / gridSize) * gridSize;
            float y = Mathf.Round(position.y / gridSize) * gridSize;
            float z = Mathf.Round(position.z / gridSize) * gridSize;
            return new Vector3(x, y, z);
        }

        // Creates a duplicate of the object as a visual preview
        private void CreatePreviewObject()
        {
            if (previewObject != null) return;  // Ensure there isn't already a preview object
            print("Creating preview object");
            //make a new gameobject with the same mesh as the object
            previewObject = new($"PreviewObject ({name})");
            previewObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
            previewObject.AddComponent<MeshFilter>().sharedMesh = GetComponentInChildren<MeshFilter>().sharedMesh;
            previewObject.AddComponent<MeshRenderer>().sharedMaterials = GetComponentInChildren<MeshRenderer>().sharedMaterials;
            previewObject.transform.localScale = transform.lossyScale * 0.5f;

            // Optionally, disable any non-visual components that should not be active in the preview
            DisableComponents(previewObject);
        }

        // Updates the position of the preview object to the nearest grid snap
        private void UpdatePreviewObject()
        {
            // Remove the preview object if flagged for removal
            if (flaggedForRemoval)
            {
                Destroy(previewObject);
                flaggedForRemoval = false;
            }

            // Create the preview object if it doesn't exist
            if (previewObject == null)
            {
                CreatePreviewObject();
            }

            Vector3 snapPosition = SnapToGrid(transform.position);
            previewObject.transform.position = snapPosition;
        }

        // Call this method to remove the preview object, for example when object placement is confirmed
        public void RemovePreviewObject()
        {
            if (previewObject != null)
            {
                flaggedForRemoval = true;
            }
        }

        // Example method to disable certain components in the preview object
        private void DisableComponents(GameObject obj)
        {
            foreach (Collider comp in obj.GetComponentsInChildren<Collider>())
            {
                comp.enabled = false;
            }

            foreach (GridSnapper comp in obj.GetComponentsInChildren<GridSnapper>())
            {
                comp.enabled = false;
            }

            // Disable other components like Rigidbody, scripts etc. as needed
        }

        public void ObjectSmoothing(bool smoothing)
        {
            m_smoothPosition = smoothing;
        }

        /// <summary>
        /// Override the grid size with a new size for this object only (does not affect the GridManager).
        /// </summary>
        /// <param name="newSize"></param>
        public void OverrideFromGrid(float newSize)
        {
            m_HasOverrideGridSize = true;
            m_overridegridSize = newSize;
        }
        /// <summary>
        /// Reset the grid size to the GridManager size for this object (does not affect the GridManager).
        /// </summary>
        public void ResetToGrid()
        {
            m_HasOverrideGridSize = false;
            m_overridegridSize = 1.0f;
        }
    }
}
