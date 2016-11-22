using UnityEngine;
using System.Collections;

public class buttonPress : MonoBehaviour {

    void OnPress(bool isPressed)
    {
        Debug.Log("OnPress+" + isPressed);
    }
}
