using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScrip : MonoBehaviour
{
    public GameObject button;
    bool change = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Change()
    {
        button.transform.localScale *=  change ? 0.25f : 4f;
        change = !change;
    }
}
