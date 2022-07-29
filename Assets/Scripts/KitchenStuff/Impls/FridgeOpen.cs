using System;
using System.Collections;
using Databases;
using UnityEngine;
using UnityEngine.UI;

namespace KitchenStuff.Impls
{
    public class FridgeOpen : MonoBehaviour, IInteractableStuff
    {
        [SerializeField] private Animator _fridgeAnimator;
        [SerializeField] private Animator _cameraAnimator;
        [SerializeField] private Text _timeText;
        [SerializeField] private ProductsDatabase _productsDatabase;
        private bool _isFridgeOpened = false;
        private bool _isAnimationPassed = true;

        private void Start()
        {
            SetClockText();
        }

        private IEnumerator ClockTimer()
        {
            yield return new WaitForSeconds(60 - DateTime.Now.Second);
            SetClockText();
        }

        private void SetClockText()
        {
            string timeText = DateTime.Now.Hour + ":";
            timeText += DateTime.Now.Minute < 10 ? "0" + DateTime.Now.Minute : DateTime.Now.Minute.ToString();
            _timeText.text = timeText;
            StartCoroutine(ClockTimer());
        }

        void Update()
        {
            if (!_isAnimationPassed)
                return;
        
            if (Input.GetMouseButtonDown(0))
            {
                Ray rayToMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(rayToMouse, out hit);
            
                if (_isFridgeOpened)
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject.CompareTag("Sliceable"))
                        {
                            Instantiate(_productsDatabase.GetProguctByName(hit.collider.gameObject.name).product, new Vector3(0, 3, 0), Quaternion.identity);
                        }
                    }
                    ChangeStuffState();
                }
                else
                {
                    if (hit.collider != null)
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        if (hit.collider.gameObject.CompareTag("Fridge"))
                        {
                            ChangeStuffState();
                        }
                    }
                }
            }
        }

        public IEnumerator AnimationGoing()
        {
            yield return new WaitForSeconds(1f);
            _isAnimationPassed = true;
        }

        public void ChangeStuffState()
        {
            _isAnimationPassed = false;
            StartCoroutine(AnimationGoing());
            _isFridgeOpened = !_isFridgeOpened;
            _fridgeAnimator.SetBool("FridgeAnimationOpen", _isFridgeOpened);
            _cameraAnimator.SetBool("FridgeAnimationOpen", _isFridgeOpened);
        }
    }
}