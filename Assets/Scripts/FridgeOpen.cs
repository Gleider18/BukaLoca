using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FridgeOpen : MonoBehaviour
{
    [SerializeField] private Animator _fridgeAnimator;
    [SerializeField] private Animator _cameraAnimator;
    [SerializeField] private Text _timeText;
    private bool _isFridgeOpened = false;
    private bool _isAnimationPassed = true;
    private static readonly int FridgeAnimationOpen = Animator.StringToHash("FridgeAnimationOpen");

    private void Start()
    {
        _timeText.text = DateTime.Now.Hour + ":" + DateTime.Now.Minute;
    }

    void Update()
    {
        if (!_isAnimationPassed)
            return;
        
        if (Input.GetMouseButtonDown(0))
        {
            _isAnimationPassed = false;
            StartCoroutine(AnimationGoing());
            if (_isFridgeOpened)
            {
                ChangeFridgeState();
            }
            else
            {
                Ray rayToMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(rayToMouse, out hit);

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.CompareTag("Fridge"))
                    {
                        ChangeFridgeState();
                    }
                }
            }
        }
    }

    private IEnumerator AnimationGoing()
    {
        yield return new WaitForSeconds(1f);
        _isAnimationPassed = true;
    }

    private void ChangeFridgeState()
    {
        _isFridgeOpened = !_isFridgeOpened;
        _fridgeAnimator.SetBool("FridgeAnimationOpen", _isFridgeOpened);
        _cameraAnimator.SetBool("FridgeAnimationOpen", _isFridgeOpened);
    }
}