using UnityEngine;

namespace UnityEngine.Animations
{
    public class AnimatorSetBool : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] private string boolName;

        void Start()
        {
            if (!animator)
                animator = GetComponent<Animator>();
        }

        public void SetBool(bool value)
        {
            if (!animator)
                animator = GetComponent<Animator>();

            animator.SetBool(boolName, value);
        }
    }
}