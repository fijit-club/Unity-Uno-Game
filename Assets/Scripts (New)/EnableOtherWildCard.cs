using UnityEngine;

public class EnableOtherWildCard : MonoBehaviour
{
    [SerializeField] private GameObject plus4;
    [SerializeField] private GameObject wild;
    [SerializeField] private GameObject number;
    
    public void SetPlus4(bool enable)
    {
        if (enable)
        {
            plus4.SetActive(true);
            wild.SetActive(false);
            number.SetActive(true);
        }
        else
        {
            plus4.SetActive(false);
            wild.SetActive(true);
            number.SetActive(false);
        }
    }
}
