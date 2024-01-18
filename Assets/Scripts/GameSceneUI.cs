using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUI : MonoSingleton<GameSceneUI>
{
    [SerializeField] private Button quitButton;
    [SerializeField] private Text warningText;

    private void Start()
    {
        quitButton.onClick.AddListener(OnClickQuit);
    }

    private void OnDestroy()
    {
        quitButton.onClick.RemoveAllListeners();
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    public void PlayerKilled(string killed, string atttacker)
    {
        StartCoroutine(ShowWarningText($"{killed} killed by {atttacker}"));
    }

    private IEnumerator ShowWarningText(string warning)
    {
        warningText.text = warning;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(5);

        warningText.gameObject.SetActive(false);
    }
}
