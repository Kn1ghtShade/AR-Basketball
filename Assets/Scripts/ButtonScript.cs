using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    bool pressed = false;

    public void press()
    {
        pressed = true;
    }

    public bool isPressed()
    {
        return pressed;
    }

}
