using FijitAddons;
using UnityEngine;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject muteButton;
    [SerializeField] private GameObject unmuteButton;
    [SerializeField] private AudioSource[] audioSources;
    [SerializeField] private GameObject setHostButton;
    
    public void EnableHost()
    {
        Bridge.GetInstance().thisPlayerInfo.data.multiplayer.isHost = true;
    }
    
    private void Start()
    {
        if (Bridge.GetInstance().testing && setHostButton != null)
            setHostButton.SetActive(true);

        if (!Bridge.GetInstance().thisPlayerInfo.sound)
        {
            unmuteButton.SetActive(true);
            muteButton.SetActive(false);
        }
    }

    public void ExitGame()
    {
        Bridge.GetInstance().SendScore(0);
    }

    public void Mute(bool mute)
    {
        if (mute)
        {
            Bridge.GetInstance().Silence("true");
            Bridge.GetInstance().thisPlayerInfo.sound = false;
        }
        else
        {
            Bridge.GetInstance().Silence("false");
            Bridge.GetInstance().thisPlayerInfo.sound = true;
        }
    }
}
