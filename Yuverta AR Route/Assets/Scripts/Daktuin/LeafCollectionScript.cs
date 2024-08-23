using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Events.GameEvents;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class LeafCollectionScript : MonoBehaviour
{
    public enum FlowerPart
    {
        middel,
        top,
        bottom,
        left,
        right
    }

    [Serializable]
    public class Leaf
    {
        public Image image;
        [FormerlySerializedAs("sprite")] public GameObject spriteGameobjectUI;
        public GameObject animation;
        public bool collected;
        public FlowerPart flowerPart;
        public GameObject spawnParticles;

        public Leaf(Image image, GameObject spriteGameobjectUI, GameObject animation, bool collected)
        {
            this.image = image;
            this.spriteGameobjectUI = spriteGameobjectUI;
            this.animation = animation;
            this.collected = collected;
        }

        public void SetCollected(bool collected)
        {
            this.collected = collected;
        }
    }

    public List<Leaf> leaves;

    public RectTransform[] leafPositions = new RectTransform[5];

    public UnityEvent allLeavesCollected;
    public UnityEvent<int> onLeafCollected = new();
    public string promptTextTemplate = "Scan nog {collectedMarkers} markers <br> om de quiz te kunnen beginnen";
    public PromptTextController promptTextController;
    private int collectedLeafCount;

    public GameObject leaveUIParent;
    public GameObject leaveUIPrefab;

    private ARRaycastManager arRaycastManager;

    [SerializeField] private bool deleteAnimationIfFinished;

    void Awake()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    public void OnLeafCollected(int index)
    {
        Debug.Log("Leaf collected");
        Debug.Log(index);

        var leaf = leaves[index];

        if (leaf.collected)
            return;

        leaf.collected = true;

        var leafObj = Instantiate(leaf.spriteGameobjectUI, leaveUIParent.transform);
        // set the position of the leaf object to the middle of the screen
        leafObj.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        switch (leaf.flowerPart)
        {
            case FlowerPart.middel:
                StartCoroutine(LeafCollectedAnimation(leafPositions[0], leafObj));
                leafObj.transform.SetSiblingIndex(0);
                break;
            case FlowerPart.top:
                StartCoroutine(LeafCollectedAnimation(leafPositions[1], leafObj));
                leafObj.transform.SetSiblingIndex(1);

                break;
            case FlowerPart.bottom:
                StartCoroutine(LeafCollectedAnimation(leafPositions[2], leafObj));
                leafObj.transform.SetSiblingIndex(2);

                break;
            case FlowerPart.left:
                StartCoroutine(LeafCollectedAnimation(leafPositions[3], leafObj));
                leafObj.transform.SetSiblingIndex(3);

                break;
            case FlowerPart.right:
                StartCoroutine(LeafCollectedAnimation(leafPositions[4], leafObj));
                leafObj.transform.SetSiblingIndex(4);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        PerformRaycast(leaf.animation, leaf.spawnParticles);
        collectedLeafCount++;
        onLeafCollected.Invoke(collectedLeafCount);
        if (collectedLeafCount == leaves.Count)
        {
            allLeavesCollected.Invoke();
        }
    }

    private void SpawnNewAnimation(GameObject animationPrefab, GameObject spawnParticles, Vector3 position)
    {
        var animation = Instantiate(animationPrefab, position, Quaternion.identity, this.transform);

        var particles = Instantiate(spawnParticles, position, Quaternion.identity, this.transform);

        if (deleteAnimationIfFinished)
        {
            animation.transform.GetChild(1).gameObject.AddComponent<AnimationDeleter>();
        }
    }

    public IEnumerator LeafCollectedAnimation(RectTransform targetTransform, GameObject leafUIPrefab)
    {
        // Adjust this speed value as needed
        float speed = 5.0f;

        while (Vector3.Distance(leafUIPrefab.transform.position, targetTransform.transform.position) > 0.1f)
        {
            leafUIPrefab.transform.position = Vector3.Lerp(leafUIPrefab.transform.position,
                targetTransform.transform.position, Time.deltaTime * speed);
            yield return null;
        }

        // Ensure the leaf reaches the exact target position
        leafUIPrefab.transform.position = targetTransform.transform.position;
    }


    private void PerformRaycast(GameObject animationPrefab, GameObject spawnParticles)
    {
        //get camera
        Camera camera = Camera.main;

        //get camera forward direction on a horizontal plane
        Vector3 cameraForward = camera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        //get camera position
        Vector3 cameraPosition = camera.transform.position;

        //remove the camera from the y position
        cameraPosition.y = 0;

        //get the postion of the camera + 1 meter in the forward direction
        Vector3 position = cameraPosition + cameraForward * 3.0f;


        SpawnNewAnimation(animationPrefab, spawnParticles, position);
    }

    public void PromptTextHandler(int amountCollected)
    {
        if (promptTextController)
        {
            var collectedMarkers = leaves.Count - amountCollected;
            var text = promptTextTemplate.Replace("{collectedMarkers}", collectedMarkers.ToString());
            promptTextController.SetText(text);
        }
    }

    private void OnDrawGizmos()
    {
        //get camera
        Camera camera = Camera.main;

        //get camera forward direction on a horizontal plane
        Vector3 cameraForward = camera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        //get camera position
        Vector3 cameraPosition = camera.transform.position;

        //remove the camera from the y position
        cameraPosition.y = 0;

        //get the postion of the camera + 1 meter in the forward direction
        Vector3 position = cameraPosition + cameraForward * 3.0f;
        

#if UNITY_EDITOR
        Gizmos.color = Color.red;
        Handles.DrawAAPolyLine();
#endif
    }
}