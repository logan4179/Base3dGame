using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PV_InstanceDebugCanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt;

    void Start()
    {
        txt.text = string.Empty;
    }

    void Update()
    {
        
    }

    public void RefreshText( string value_passed )
    {
        txt.text = string.Empty;
        txt.text = value_passed;
    }
}
