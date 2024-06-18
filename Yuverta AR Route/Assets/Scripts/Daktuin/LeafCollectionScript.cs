using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LeafCollectionScript : MonoBehaviour
{
    [Serializable]
    public struct Leaf
    {
        public Image image;
        public Sprite sprite;
        public string animationName;
        public bool collected;
    }

    public UnityEvent AllLeavesCollected;
    public List<Leaf> leaves;
    private int collectedLeafCount = 0;
    private Animator animator;
    
    private void Start()
    {
        animator = FindObjectOfType<Animator>();
    }

    public void SetQRCodeEvents()
    {
        for (int i = 0; i < DaktuinManager.Instance.QrCodeManager.qrCodes.Count; i++)
        {
            // ReSharper disable once AccessToModifiedClosure
            DaktuinManager.Instance.QrCodeManager.qrCodes[i].action.AddListener(OnLeafCollected);
        }
    }

    private void OnLeafCollected(int index)
    {
        Leaf leaf = leaves[index];
        leaf.collected = true;
        leaf.image.sprite = leaf.sprite;
        animator.Play(leaf.animationName);
        collectedLeafCount++;
        if (collectedLeafCount == leaves.Count)
        {
            AllLeavesCollected.Invoke();
        }
    }
}
