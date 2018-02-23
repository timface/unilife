using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameTime {

    public int day;
    public int hour;
    public int minute;

    public float tickTime = 2f;
    float timeSinceLastTick = 0f;

    public Action<GameTime> onTimeChanged;

    public GameTime()
    {
        day = 1;
        hour = 6;
        minute = 0;
        if (onTimeChanged != null)
            onTimeChanged(this);
    }

    public GameTime(int day, int hour, int minute)
    {
        this.day = day;
        this.hour = hour;
        this.minute = minute;
        if (onTimeChanged != null)
            onTimeChanged(this);
    }

    // Update is called once per frame
    public void Update (float deltaTime) {

        timeSinceLastTick += deltaTime;
        if(timeSinceLastTick >= tickTime)
        {
            minute ++;
            if(minute == 6)
            {
                minute = 0;
                hour++;
                if(hour == 24)
                {
                    hour = 0;
                    day++;
                }
            }

            timeSinceLastTick = 0f;
            if (onTimeChanged != null)
                onTimeChanged(this);
        }
		
	}

    public string GetTimeString()
    {
        string timeString = "Day " + day + " " + hour + ":" + minute + "0";
        return timeString;
    }

    public void RegisterOnTimeChanged(Action<GameTime> timeChangedCB)
    {
        onTimeChanged += timeChangedCB;
    }
}
