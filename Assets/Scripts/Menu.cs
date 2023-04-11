using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private SliceWorker sliceWorker;
    [SerializeField] private Text stateText;

    public void OnResetClick()
    {
        SceneManager.LoadScene(0);
    }
    
    public void OnClearClick()
    {
        foreach (var item in GameObject.FindGameObjectsWithTag("Sliceable"))
        {
            if (item.GetComponent<Sliceable>() != null)
            {
                Destroy(item);
            }
        }
    }
    
    public void OnStateChangeClick()
    {
        switch (sliceWorker.gameState)
        {
            case EGameState.Afk:
                sliceWorker.gameState = EGameState.Slice;
                break;
            case EGameState.Slice:
                sliceWorker.gameState = EGameState.Throw;
                break;
            case EGameState.Throw:
                sliceWorker.gameState = EGameState.Afk;
                break;
        }

        stateText.text = sliceWorker.gameState.ToString();
    }
}
