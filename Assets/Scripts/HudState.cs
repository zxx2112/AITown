using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudState : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text iconDescription;
    
    public void SetIcon(Sprite sprite,string description = "")
    {
        icon.sprite = sprite;
        iconDescription.text = description;
        
        if (sprite == null)
        {
            icon.gameObject.SetActive(false);
        }
        else
        {
            icon.gameObject.SetActive(true);
        }
    }
}
