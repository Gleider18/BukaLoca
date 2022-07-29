using System.Collections;
using Databases;
using UnityEngine;
using UnityEngine.UI;

namespace KitchenStuff.Impls
{
    public class StoveOpen : MonoBehaviour, IInteractableStuff
    {
        [SerializeField] private Animator _stoveAnimator;
        [SerializeField] private Animator _cameraAnimator;
        private bool _isStoveOpened = false;
        private bool _isAnimationPassed = true;
        
        void Update()
        {
            if (!_isAnimationPassed)
                return;
        
            if (Input.GetMouseButtonDown(0))
            {
                if (_isStoveOpened)
                {
                    ChangeStuffState();
                }
                else
                {
                    Ray rayToMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    Physics.Raycast(rayToMouse, out hit);
                    if (hit.collider != null)
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        if (hit.collider.gameObject.CompareTag("Stove"))
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
            _isStoveOpened = !_isStoveOpened;
            //_stoveAnimator.SetBool("StoveAnimationOpen", _isStoveOpened);
            _cameraAnimator.SetBool("StoveAnimationOpen", _isStoveOpened);
        }
    }
}