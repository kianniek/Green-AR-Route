using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaktuinManager : BaseManager
{
    public static DaktuinManager Instance;
    
    [NonSerialized] public QuizManager quizManager;

    [NonSerialized] public QRCodeManager QrCodeManager;
    [NonSerialized] public LeafCollectionScript leafScript;
    
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
        quizManager = FindObjectOfType<QuizManager>();
        leafScript = FindObjectOfType<LeafCollectionScript>();
        QrCodeManager = FindObjectOfType<QRCodeManager>();
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
