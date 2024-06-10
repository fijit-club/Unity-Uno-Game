using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingScript : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    string originalText = "";
    // Start is called before the first frame update
    void Start()
    {
        loadingText = gameObject.GetComponent<TextMeshProUGUI>();
        originalText = loadingText.text;
        StartCoroutine(LoadingText());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator LoadingText()
    {
        int i = 0;
        while (true)
        {
            if (i == 4)
            {
                i = 0;
                loadingText.text = originalText;
            }
            else
            {
                i++;
                loadingText.text += ".";
            }

            yield return new WaitForSeconds(0.25f);
        }
    }
}
