using FijitAddons;
using UnityEngine;
using UnityEngine.UI;

public class TimerHandler : MonoBehaviour
{
    public float maxTime = 20f;
    
    [SerializeField] private Control control;
    [SerializeField] private bool thisPlayer;
    [SerializeField] private GameObject[] wildCardsScreen;
    
    [SerializeField] private float _time;
    private Image _turnTimerImage;
    
    private void OnEnable()
    {
        Bridge.GetInstance().VibrateBridge(Bridge.Haptics.notificationError);
        
        _turnTimerImage = GetComponent<Image>();
        _time = maxTime;
        
        CancelInvoke(nameof(ReduceTime));
        CancelInvoke(nameof(ReduceTimeOther));
        
        if (thisPlayer)
            InvokeRepeating(nameof(ReduceTime), 1f, 1f);
        else
            InvokeRepeating(nameof(ReduceTimeOther), 1f, 1f);
    }

    private void ReduceTimeOther()
    {
        _time -= 1;
    }
    
    private void ReduceTime()
    {
        _time -= 1;
        if (_time == 0)
        {
            StopTimer(true);
            control.players[0].NextPlayersTurn(control.gameNetworkHandler, control);
        }
    }

    private void OnDisable()
    {
        StopTimer(false);
    }

    public void StopTimer(bool disableWild)
    {
        if (disableWild)
        {
            foreach (var wild in wildCardsScreen)
            {
                wild.SetActive(false);
            }

            control.players[0].playedWild = false;
        }
        _time = maxTime;
        CancelInvoke(nameof(ReduceTime));
        CancelInvoke(nameof(ReduceTimeOther));
    }

    private void Update()
    {
        _turnTimerImage.fillAmount = Mathf.Lerp(_turnTimerImage.fillAmount, _time / maxTime, 10f * Time.deltaTime);
    }
}
