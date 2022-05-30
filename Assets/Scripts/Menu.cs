using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
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
}
