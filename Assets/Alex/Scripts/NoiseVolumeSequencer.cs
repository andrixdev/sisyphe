using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseVolumeSequencer : MonoBehaviour
{
    public List<NoiseVolume> volumes;

    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i<volumes.Count; i++)
        {
            volumes[i].enabled = false;
            volumes[i].enabled = true;
        }   
    }
}
