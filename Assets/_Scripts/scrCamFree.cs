using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrCamFree : MonoBehaviour
{
    [Header("REFERENCE")]
    public Transform trans, _transCam = null;

    [Header("STATS")]
    public float moveSpeed = 10f;
    public float rotSpeed = 50f;
    public int mouseSmooth = 10;


    [Header("TRUTH")]

    [Header("OTHER")]
    public float mouseX, mouseY, mouseLookX, mouseLookY;
    public float minLookY = -70f;
    public float maxLookY = 70f;


    void Awake()
    {
        if (trans == null) trans = GetComponent<Transform>();
        if (_transCam == null) _transCam = trans.Find("camFree").gameObject.GetComponent<Transform>();
    }

    void Start()
    {
        
    }

    void Update()
    {

        if (Input.GetMouseButton(1))
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            mouseLookX += mouseX * rotSpeed * Time.deltaTime;
            mouseLookY = Mathf.Clamp(mouseLookY + (rotSpeed * mouseY * Time.deltaTime), minLookY, maxLookY);

            //trans.localRotation = Quaternion.Lerp(trans.localRotation, Quaternion.AngleAxis(mouseLookX, trans.up), 0.2f);

            //_transCam.localRotation = Quaternion.Lerp(trans.localRotation, Quaternion.AngleAxis(-mouseLookY, Vector3.right), 0.2f);

            trans.localRotation = Quaternion.AngleAxis(mouseLookX, trans.up);

            _transCam.localRotation = Quaternion.AngleAxis(-mouseLookY, Vector3.right);



            /*if (Input.GetMouseButton(1)) //Right-click hold
            {
                mouseX = Input.GetAxis("Mouse X");
                mouseY = Input.GetAxis("Mouse Y");

                if ((Mathf.Abs(mouseX) > 0 || Mathf.Abs(mouseY) > 0))
                {
                    float rSpd = rotSpeed * Time.deltaTime;

                    trans.Rotate(trans.up, mouseX*Time.deltaTime*rotSpeed, Space.World);
                    trans.Rotate(trans.right, mouseY*rotSpeed*Time.deltaTime, Space.World );
                }
            }*/

            if (Input.GetKey(KeyCode.W))
                trans.Translate(_transCam.forward * moveSpeed * Time.deltaTime, Space.World);
            else if (Input.GetKey(KeyCode.S))
                trans.Translate(_transCam.forward * -moveSpeed * Time.deltaTime, Space.World);

            if (Input.GetKey(KeyCode.A))
                trans.Translate(_transCam.right * -moveSpeed * Time.deltaTime, Space.World);
            else if (Input.GetKey(KeyCode.D))
                trans.Translate(_transCam.right * moveSpeed * Time.deltaTime, Space.World);
        }

    }
}

