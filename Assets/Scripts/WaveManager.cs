using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    // Water wave values
    public float Amplitude = 1f;
    public float length = 2f;
    public float speed = 1f;
    public float offest = 0f;

    private void Awake(){
        if(instance == null){
            instance = this;
        }
        else if(instance != this){
            Debug.Log("Instance already exists, destroying object!");
        }
    }

    // private void Update()
    // {
    //     offest += Time.deltaTime * speed;
    // }

    public float GetWaveHeight(float x){
        return Amplitude * Mathf.Sin(x / length + offest);
    }
}
