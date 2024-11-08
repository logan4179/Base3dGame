using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scrTest : MonoBehaviour
{
    public Transform t;
    public Transform _t_other;
    public Vector3 v_itp, v_itd, v_itv;

    public float lineLength = 10f;

    public Text _txtITP;

    public static Vector3 v_tgt;

    private void Awake()
    {

    }

    void Start()
    {
        
    }

    void Update()
    {
        //t.LookAt(v_tgt);

        //v_itp = t.InverseTransformPoint(_t_other.position);
        //v_itd = t.InverseTransformDirection(_t_other.position);
        //v_itv = t.InverseTransformVector(_t_other.position);

        //Debug.DrawLine(t.position, t.position + (t.forward * lineLength));

        //_txtITP.text = $"itp: {v_itp}\nitd: {v_itd}\nitv: {v_itv}";
    }

    private void OnTriggerEnter(Collider other)
    {
        print($"Triggered with {other.name}, my name: {name}");

    }
}
