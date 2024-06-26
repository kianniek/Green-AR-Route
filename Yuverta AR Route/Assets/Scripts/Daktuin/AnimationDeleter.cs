using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDeleter : MonoBehaviour
{
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(WaitOnEnd());
    }
    
    private IEnumerator WaitOnEnd()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Destroy(transform.parent.gameObject);
    }
}
