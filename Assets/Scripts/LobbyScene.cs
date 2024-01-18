using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class LobbyScene : MonoSingleton<LobbyScene>
{
    [SerializeField] private Button joinButton;
    [SerializeField] private InputField nickname;

    private void Start()
    {
        joinButton.onClick.AddListener(OnClickJoin);
    }

    private void OnClickJoin()
    {
        if (string.IsNullOrEmpty(nickname.text) || string.IsNullOrWhiteSpace(nickname.text))
            return;

        PhotonNetwork.NickName = nickname.text;
        PhotonNetwork.JoinRandomRoom();
    }
}
