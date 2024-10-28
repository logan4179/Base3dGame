using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testProfiler : MonoBehaviour
{
    public int rate1 = 100;

    public int assignmentInt;
    public int myInt1, myInt2, myInt3, myInt4, myInt5, myInt6, myInt7, myInt8, myInt9, myInt10;
    
    public int myInt1_exposed
    {
        get
        {
            return myInt1;
        }
        set
        {
            myInt1 = value;
        }
    }
    public int myInt2_exposed
    {
        get
        {
            return myInt2;
        }
        set
        {
            myInt2 = value;
        }
    }
    public int myInt3_exposed
    {
        get
        {
            return myInt3;
        }
        set
        {
            myInt3 = value;
        }
    }
    public int myInt4_exposed
    {
        get
        {
            return myInt4;
        }
        set
        {
            myInt4 = value;
        }
    }
    public int myInt5_exposed
    {
        get
        {
            return myInt5;
        }
        set
        {
            myInt5 = value;
        }
    }
    public int myInt6_exposed
    {
        get
        {
            return myInt6;
        }
        set
        {
            myInt6 = value;
        }
    }
    public int myInt7_exposed
    {
        get
        {
            return myInt7;
        }
        set
        {
            myInt7 = value;
        }
    }
    public int myInt8_exposed
    {
        get
        {
            return myInt8;
        }
        set
        {
            myInt8 = value;
        }
    }
    public int myInt9_exposed
    {
        get
        {
            return myInt9;
        }
        set
        {
            myInt9 = value;
        }
    }
    public int myInt10_exposed
    {
        get
        {
            return myInt10;
        }
        set
        {
            myInt10 = value;
        }
    }


    public bool checkingValue = true;
    public bool changingNumber = false;

    void Start()
    {
        
    }

    void Update()
    {
        /*
        // Case 1: Checking
        for( int i = 0; i < rate1; i++ )
        {
            if (myInt1 == 0)
                myInt1 = myInt2;

            //The following are cpu profiler times, roughly averaged, based on different 'assignmentInt' values (10, 50, 100, 1000, 5000, etc)
            //10: 0.06
            //50: 0.06
            //100: 0.06
            //1000: 0.07
            //5000; 0.08
            //10000: 0.1
            //20000: 0.15
        }
        */

        /*
        // Case 2: Checking and force-assigning every frame
        for (int i = 0; i < rate1; i++)
        {
            if (myInt1 == 0)
                myInt1 = myInt2;

            myInt1 = 0;

            //10:
            //50: 
            //100: 0.05/0.06
            //1000: 0.06
            //5000; 0.08
            //10000: 
            //20000: 
        }
        */

        
        // Case 3: assigning
        for (int i = 0; i < rate1; i++)
        {
            myInt1 = assignmentInt;

            //10:
            //50: 
            //100: 0.05/0.06
            //1000: 0.06
            //5000; 0.07
            //10000: 0.08
            //20000: 0.12
        }
        

        /*
        // Case 4: Assigning with changing variable
        for (int i = 0; i < rate1; i++)
        {
            myInt1 = myInt2;
            myInt2++;

            //10:
            //50: 
            //100: 
            //1000: 0.06
            //5000; 0.07
            //10000: 0.09
            //20000: 0.12
        }
        */

        /*
        // Case 5: assigning a lot of variables
        for (int i = 0; i < rate1; i++)
        {
            myInt1 = assignmentInt;
            myInt2 = assignmentInt;
            myInt3 = assignmentInt;
            myInt4 = assignmentInt;
            myInt5 = assignmentInt;
            myInt6 = assignmentInt;
            myInt7 = assignmentInt;
            myInt8 = assignmentInt;
            myInt9 = assignmentInt;
            myInt10 = assignmentInt;


            //10:
            //50: 
            //100: 
            //1000: 0.07
            //5000; 0.1
            //10000: 
            //20000: 
        }
        */

        /*
        // Case 6: assigning a lot of variables with a forced assigning every frame
        for (int i = 0; i < rate1; i++)
        {
            myInt1 = assignmentInt;
            myInt2 = assignmentInt;
            myInt3 = assignmentInt;
            myInt4 = assignmentInt;
            myInt5 = assignmentInt;
            myInt6 = assignmentInt;
            myInt7 = assignmentInt;
            myInt8 = assignmentInt;
            myInt9 = assignmentInt;
            myInt10 = assignmentInt;

            myInt1 = 0;
            myInt2 = 0;
            myInt3 = 0;
            myInt4 = 0;
            myInt5 = 0;
            myInt6 = 0;
            myInt7 = 0;
            myInt8 = 0;
            myInt9 = 0;
            myInt10 = 0;

            //10:
            //50: 
            //100: 
            //1000:
            //5000; 
            //10000: 
            //20000: 
        }
        */

        /*
        // Case 5: assigning one property
        for (int i = 0; i < rate1; i++)
        {
            myInt1_exposed = assignmentInt;

            //10:
            //50: 
            //100: 
            //1000: 0.07
            //5000; 0.1
            //10000: 
            //20000:                     
        }
        */

        /*
        // Case 5: assigning one property and changing value
        for (int i = 0; i < rate1; i++)
        {
            myInt1_exposed = assignmentInt;
            assignmentInt++;

            //10:
            //50: 
            //100: 
            //1000: 
            //5000; 
            //10000: 
            //20000:                     
        }
        */

        /*
        // Case 5: assigning a lot of properties
        for (int i = 0; i < rate1; i++)
        {
            myInt1_exposed = assignmentInt;
            myInt2_exposed = assignmentInt;
            myInt3_exposed = assignmentInt;
            myInt4_exposed = assignmentInt;
            myInt5_exposed = assignmentInt;
            myInt6_exposed = assignmentInt;
            myInt7_exposed = assignmentInt;
            myInt8_exposed = assignmentInt;
            myInt9_exposed = assignmentInt;
            myInt10_exposed = assignmentInt;

            //10:
            //50: 
            //100: 
            //1000: 
            //5000; 
            //10000: 
            //20000:                     
        }
        */

        /*
        // Case 6: assigning a lot of properties with a forced assigning every frame
        for (int i = 0; i < rate1; i++)
        {
            myInt1 = assignmentInt;
            myInt2 = assignmentInt;
            myInt3 = assignmentInt;
            myInt4 = assignmentInt;
            myInt5 = assignmentInt;
            myInt6 = assignmentInt;
            myInt7 = assignmentInt;
            myInt8 = assignmentInt;
            myInt9 = assignmentInt;
            myInt10 = assignmentInt;

            myInt1 = 0;
            myInt2 = 0;
            myInt3 = 0;
            myInt4 = 0;
            myInt5 = 0;
            myInt6 = 0;
            myInt7 = 0;
            myInt8 = 0;
            myInt9 = 0;
            myInt10 = 0;

            //10:
            //50: 
            //100: 
            //1000:
            //5000; 
            //10000: 
            //20000: 
        }
        */

    }


}
