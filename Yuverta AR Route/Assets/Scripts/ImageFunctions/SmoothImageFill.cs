using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ImageFunctions
{
    [RequireComponent(typeof(Image))]
    public class SmoothImageFill : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float fillSpeed = 0.1f;
        
        [SerializeField] private UnityEvent onFillComplete;
        
        private Image _image;
        private float _targetFillAmount;
        private float _currentFillAmount;
        private float _timer;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }
        
        // Coroutine version
        private IEnumerator FillImage()
        {
            _timer = 0.0f;
            while (_timer < fillSpeed)
            {
                _timer += Time.deltaTime;
                var t = _timer / fillSpeed;
                
                _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, t);
                _image.fillAmount = _currentFillAmount;
                yield return null;
            }
            _image.fillAmount = _targetFillAmount;
            
            if (Math.Abs(_targetFillAmount - 1) < 0.01f)
            {
                onFillComplete.Invoke();
            }
        }

        public void SetFillAmount(float fillAmount)
        {
            //Stop any running coroutines
            StopCoroutine(FillImage());
            
            //Set the target fill amount and start the coroutine
            _targetFillAmount = fillAmount;
            StartCoroutine(FillImage());
        }
    }
}