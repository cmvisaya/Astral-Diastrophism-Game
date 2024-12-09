using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueToText : MonoBehaviour
{
    public Slider sliderUI;
    [SerializeField] private Text textSliderValue;

    void Awake()
    {
        if(sliderUI == null)
        {
            Debug.Log("SliderUI is null!");
        }
        textSliderValue = GetComponent<Text>();
    }

    public void ShowSliderValueHP()
    {
        if(sliderUI != null && textSliderValue != null)
        {
            string sliderMessage = sliderUI.value + " HP";
            textSliderValue.text = sliderMessage;
        }
    }

    public void ShowSliderValueMana()
    {
        if (sliderUI != null && textSliderValue != null)
        {
            string sliderMessage = sliderUI.value + " MP";
            textSliderValue.text = sliderMessage;
        }
    }
}
