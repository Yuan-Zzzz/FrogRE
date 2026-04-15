using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PlayBGM("BGM_Background");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
