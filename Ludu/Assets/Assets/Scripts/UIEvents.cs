using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEvents : MonoBehaviour
{
    [SerializeField] private Toggle zoomToggle;
    [SerializeField] private ToggleGroup zoomToggleGroup;
    [SerializeField] private GameObject zoomPanelUI;

    public void ToggleZoom(bool tog)
    {

        if (tog)
        {
            zoomPanelUI.SetActive(true);
            zoomPanelUI.GetComponent<Animator>().Play("zoomEnter");
        }
        else
        {
            zoomPanelUI.GetComponent<Animator>().Play("zoomExit");
            StartCoroutine(RemoveZoomPanelUi());
        }
    }

    IEnumerator RemoveZoomPanelUi() {
        yield return new WaitForSeconds(0.5f);
        zoomPanelUI.SetActive(false);
    }

    public void ResetGame()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadLevel(int lvl)
    {
        SceneManager.LoadScene(lvl);
    }

    public void deactivateUI(GameObject ui)
    {
        ui.SetActive(false);
    }

    public void SwapUi(List<GameObject> tow)
    {
        tow[0].SetActive(false);
        tow[1].SetActive(true);
    }

    public void EnableZoomUI(GameObject zoomUI)
    {
        zoomUI.SetActive(true);
    }

    //public void enabled(Boolean enabled)
    //{
    //    print(enabled);
    //}
}
