using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class SpalshScene : MonoSingleton<SpalshScene>
{
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject loading;

    private void Start()
    {
        startButton.onClick.AddListener(OnClickStart);
        startButton.gameObject.SetActive(false);
    }
    private void OnClickStart()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnConnectedToMaster()
    {
        loading.SetActive(false);
        startButton.gameObject.SetActive(true);
    }
}
