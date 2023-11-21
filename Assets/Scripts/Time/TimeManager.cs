using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{

    public static TimeManager Instance { get; private set; }

    [Header("Internal Clock")]
    [SerializeField]
    GameTimeStamp timestamp;
    public float timeScale = 1.0f;

    [Header("Day and Night Cycle")]
    // the transform of the directional light (sun)
    public Transform sunTransform;
    Vector3 sunAngle;

    // list of object to inform of changes to time
    List<ITimeTracker> listeners = new List<ITimeTracker>();


    private void Awake(){
        // If there is more than one instance, distroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else{
            // Set the static instance to this instance
            Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        // initialise the time stamp
        timestamp = new GameTimeStamp(1, 6, 0);
        StartCoroutine(TimeUpdate());
    }

    IEnumerator TimeUpdate()
    {
        while (true)
        {
            Tick();

            yield return new WaitForSeconds(1 / timeScale);
        }
    }

    void Tick()
    {
        timestamp.UpdateClock();
        UpdateSunMovement();

        foreach(ITimeTracker listner in listeners)
        {
            listner.ClockUpdate(timestamp);
        }
    }

    // Day and Night Cycle
    void UpdateSunMovement()
    {
        // convert current time to minutes
        int timeInMinutes = GameTimeStamp.HoursToMinutes(timestamp.hour) + timestamp.minute;

        // during daytime
        // sun moves 0.2 degrees per minute
        // during nighttime
        // sun moves 0.6 degrees in a minute
        float sunAngle = 0;
        if(timeInMinutes <= 15 * 60)
        {
            sunAngle = 0.2f * timeInMinutes;
        }
        else if(timeInMinutes > 15* 60)
        {
            sunAngle = 180f + 0.6f * (timeInMinutes - (15 * 60));
        }

        // Apply angle to directional light
        // sunTransform.eulerAngles = new Vector3(sunAngle, 0, 0);
        this.sunAngle = new Vector3(sunAngle, 0, 0);

    }
    

    // Update is called once per frame
    void Update()
    {
        sunTransform.rotation = Quaternion.Slerp(sunTransform.rotation, Quaternion.Euler(sunAngle), 1f * Time.deltaTime);
    }


    // Debug function to skip time
    // public void SkipTime(GameTimeStamp timeToSkipTp){
    //     // convert to minutes;
    //     int timeToSkipInMinutes = GameTimeStamp.TimeStampInMinutes(timeToSkipTp);
    //     Debug.Log("Skip time by: " + timeToSkipInMinutes);
    //     int timeNowInMinutes = GameTimeStamp.TimeStampInMinutes(timestamp);
    //     Debug.Log("Time now: " + timeNowInMinutes);

        
    // }


    // Add on object tob listners list
    public void RegisterTracker(ITimeTracker listener)
    {
        listeners.Add(listener);
    }

    // Remove object from listeners list
    public void UnregisterTracker(ITimeTracker listener)
    {
        listeners.Remove(listener);
    }
}
