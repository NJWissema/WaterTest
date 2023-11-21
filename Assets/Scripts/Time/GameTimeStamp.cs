using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores minutes, days, hours
[System.Serializable]
public class GameTimeStamp
{
    public int day;
    public int hour;
    public int minute;

    // Constructor
    public GameTimeStamp(int day, int hour, int minute)
    {
        this.day = day;
        this.hour = hour;
        this.minute = minute;
    }

    // Create new timestamp from existing
    public GameTimeStamp( GameTimeStamp timeStamp)
    {
        this.day = timeStamp.day;
        this.hour = timeStamp.hour;
        this.minute = timeStamp.minute;
    }
    
    // Increment increase in time
    public void UpdateClock()
    {
        minute++;

        // 60 minutes to hour
        if(minute >=60){
            minute = 0;
            hour++;
        }
        // 20 hours to day
        if(hour >= 20){
            hour = 0;
            day++;
        }
    }

    // convert hours to minutes
    public static int HoursToMinutes(int hour)
    {
        return hour * 60;
    }

    // returns current time stamp in minutes
    public static int TimeStampInMinutes(GameTimeStamp timeStamp)
    {
        return (HoursToMinutes(timeStamp.day*20) + HoursToMinutes(timeStamp.hour) + timeStamp.minute);
    }
}
