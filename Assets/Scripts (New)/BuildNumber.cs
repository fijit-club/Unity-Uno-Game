using TMPro;
using UnityEngine;

public class BuildNumber : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TMP_Text>().text = Application.version;
    }
}
