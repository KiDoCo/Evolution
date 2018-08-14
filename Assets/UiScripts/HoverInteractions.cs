using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverInteractions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler//, IPointerClickHandler
{

    [SerializeField]
    Text ButtonText;

    [SerializeField]
    ParticleSystem Right;

    [SerializeField]
    ParticleSystem Left;

    [SerializeField]
    AudioSource Bubbles;

    [SerializeField]
    int FontSize;

    //[SerializeField]
    int HoverFontSize;

    private Text TC;

    [SerializeField]
    string HoverColorHex;

    Color hoverColor = new Color();

    public void Start()
    {
        HoverFontSize = FontSize + 3;
        ColorUtility.TryParseHtmlString(HoverColorHex, out hoverColor);

        if (GetComponent<Text>() != null)
        {
            TC = GetComponent<Text>();

            RectTransform RT = GetComponent<RectTransform>();

            RT.sizeDelta = new Vector2(Mathf.Round(TC.preferredWidth),  Mathf.Round(TC.preferredHeight));
        }
    }

    public void Update()
    {
        if(gameObject.transform.Find("BubbleHolder"))
        {
            float textWidth = TC.preferredWidth;

            GetComponentInChildren<HorizontalLayoutGroup>().spacing = Mathf.Round(textWidth);
        }
    }

    public void OnEnable()
    {
        ButtonText.color = Color.white;
        ButtonText.fontSize = FontSize;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ButtonText.color = hoverColor;

        ButtonText.fontSize = HoverFontSize;

        if(Bubbles)
        {
            Bubbles.Play();
        }

        if (Right != null || Left != null)
        {
            Right.Play();
            Left.Play();

        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ButtonText.color = Color.white;
        ButtonText.fontSize = FontSize;
    }

	/* public void OnPointerClick(PointerEventData eventData)
    {
     
    } */
}
