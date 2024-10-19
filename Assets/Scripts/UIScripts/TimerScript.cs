using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    public Text timerText;
    float timeRemining = 600f;

    private void Update()
    {
        if(timeRemining > 0)
        {
            timeRemining -= Time.deltaTime;
            UpdateTimerText();
        }
        else
        {
            timeRemining = 0;

        }
    }

    void UpdateTimerText()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeRemining);

        timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
    }
}
