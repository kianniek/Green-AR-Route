using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptTextController : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;

    [SerializeField] private Animator promptTextAnimator;
    
    [SerializeField] private bool showOnStart = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!promptTextAnimator)
            promptTextAnimator = promptText.GetComponent<Animator>();
        
        if (showOnStart)
            ToMiddel();
            ToTopAfterDelay(5f);
    }

    public void ToMiddel()
    {
        promptTextAnimator.SetBool("Hidden", false);
        promptTextAnimator.SetBool("Middle", true);
        promptTextAnimator.SetBool("Top", false);
    }
    
    public void ToTop()
    {
        promptTextAnimator.SetBool("Hidden", false);
        promptTextAnimator.SetBool("Middle", false);
        promptTextAnimator.SetBool("Top", true);
    }
    
    public void Hide()
    {
        promptTextAnimator.SetBool("Hidden", true);
        promptTextAnimator.SetBool("Middle", false);
        promptTextAnimator.SetBool("Top", false);
    }
    
    public void HideAfterDelay(float delay)
    {
        StartCoroutine(HideAfterDelayCoroutine(delay));
    }
    
    public void ToTopAfterDelay(float delay)
    {
        StartCoroutine(ToTopAfterDelayCoroutine(delay));
    }
    
    private IEnumerator HideAfterDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
        yield return null;

    }
    
    private IEnumerator ToTopAfterDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        ToTop();
        
        //stop this coroutine
        yield return null;
    }
    
    public void SetText(string text)
    {
        promptText.text = text;
    }
    
    public void ForceTop()
    {
        promptTextAnimator.SetBool("Hidden", false);
        promptTextAnimator.SetBool("Middle", false);
        promptTextAnimator.SetBool("Top", false);
        
        promptTextAnimator.SetTrigger("ForceTop");
        promptTextAnimator.ResetTrigger("ForceMiddle");
        promptTextAnimator.ResetTrigger("ForceHide");
    }
    
    public void ForceMiddle()
    {
        promptTextAnimator.SetBool("Hidden", false);
        promptTextAnimator.SetBool("Middle", false);
        promptTextAnimator.SetBool("Top", false);
        
        promptTextAnimator.SetTrigger("ForceMiddle");
        promptTextAnimator.ResetTrigger("ForceTop");
        promptTextAnimator.ResetTrigger("ForceHide");
    }
    
    public void ForceHide()
    {
        promptTextAnimator.SetBool("Hidden", false);
        promptTextAnimator.SetBool("Middle", false);
        promptTextAnimator.SetBool("Top", false);
        
        promptTextAnimator.SetTrigger("ForceHide");
        promptTextAnimator.ResetTrigger("ForceTop");
        promptTextAnimator.ResetTrigger("ForceMiddle");
    }
}