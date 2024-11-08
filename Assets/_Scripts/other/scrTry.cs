using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class scrTry : MonoBehaviour
{
    public float movementX, movementY;

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    void OnSayHo(InputValue movementValue)
    {
        print("ho");
        //movementValue.Get<>
    }

    void OnButtons(InputValue movementValue)
    {
        print("OnButtons");
        //movementValue.Get<>
    }

    private void OnMove(InputValue movementValue)
    {
        print("OnMove()");
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }
}
