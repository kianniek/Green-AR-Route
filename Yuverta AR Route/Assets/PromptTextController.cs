using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptTextController : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;

    [SerializeField] private Animator promptTextAnimator;

    // Start is called before the first frame update
    void Start()
    {
        if (!promptTextAnimator)
            promptTextAnimator = promptText.GetComponent<Animator>();
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
    
    private IEnumerator HideAfterDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }
    
    public void SetText(string text)
    {
        promptText.text = text;
    }
}