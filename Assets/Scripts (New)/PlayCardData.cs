using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayCardData : MonoBehaviour
{
    public int cardIndex;
    public GameObject cardObject;
    public int handIndex;
    [SerializeField] private RawImage childCard;
    [SerializeField] private Transform[] children;
    [SerializeField] private GameObject darkOverlay;
    
    public void UpdateCard()
    {
        childCard.texture = GetComponent<RawImage>().texture;
    }

    public void SetDarkOverlay(bool active, bool playAnimation)
    {
        darkOverlay.SetActive(true);
        if (playAnimation)
            darkOverlay.GetComponent<Animator>().Play("Card Black Overlay", -1, 0f);
        darkOverlay.GetComponent<RawImage>().enabled = active;
    }
    
    public void MakeChild()
    {
        GetComponent<RawImage>().enabled = false;
        childCard.gameObject.SetActive(true);
        foreach (var child in children)
        {
            child.SetParent(childCard.transform);
        }
    }

    public void MoveUp()
    {
        var tempCard = childCard.transform;
        tempCard.DOLocalMoveY(100f, .3f);
    }
    public void MoveDown()
    {
        var tempCard = childCard.transform;
        tempCard.DOLocalMoveY(0f, .3f);
    }
}
