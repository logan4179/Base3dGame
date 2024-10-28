using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrDataDisplay : MonoBehaviour
{
    public static scrDataDisplay _me_singleton = null;

    [Header("REFERENCE")]
    public Enemy_Bug[] _scrBasicEnemies;


    void Awake()
    {
        if (_me_singleton == null)
        {
            _me_singleton = gameObject.GetComponent<scrDataDisplay>();
        }
        else if (_me_singleton.gameObject != gameObject)
        {
            print($"Found duplicate data display, destroying now...");
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
