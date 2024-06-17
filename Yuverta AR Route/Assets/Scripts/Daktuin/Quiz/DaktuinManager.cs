using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaktuinManager : BaseManager
{
    public static DaktuinManager Instance;
    
    [NonSerialized] public QuizManager quizManager;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    
    // Start is called before the first frame update
    void Start()
    {
        SwipeDetection.Instance.currentManager = this;
        SwipeDetection.Instance.tagToCheck = "WorldUI";
        quizManager = FindObjectOfType<QuizManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SelectedObject(GameObject obj)
    {
        obj.GetComponent<QuizButton>().OnClick();
    }
    
    public override void UpdateObject()
    {
        
    }
}
