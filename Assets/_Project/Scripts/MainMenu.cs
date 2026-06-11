using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject mapSelectionPanel;

    private void Start()
    {
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(false);
        }
    }

    public void ShowMapSelection()
    {
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(true);
        }
    }

    public void HideMapSelection()
    {
        if (mapSelectionPanel != null)
        {
            mapSelectionPanel.SetActive(false);
        }
    }

    public void LoadMap(string mapName)
    {
        SceneManager.LoadScene(mapName);
    }
}
