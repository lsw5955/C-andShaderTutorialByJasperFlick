using UnityEngine;
using System;

public class Clock : MonoBehaviour {

    public Transform hoursArm, minutesArm, secondsArm;
    public bool continuous;

    const float degreesPerHour = 30f, degreesPerMinutes = 6f, degreesPerSecond = 6f;

    private void Update() {
        if (continuous) {
            UpdateContinuous();
        }
        else {
            UpdateDiscrete();
        }
    }

    private void UpdateContinuous() {
        //DateTime time = DateTime.Now;
        TimeSpan time = DateTime.Now.TimeOfDay;

        //Debug.Log(DateTime.Now);
        hoursArm.localRotation = Quaternion.Euler(0f, (float)time.TotalHours * degreesPerHour, 0f);
        minutesArm.localRotation = Quaternion.Euler(0f, (float)time.TotalMinutes * degreesPerMinutes, 0f);
        secondsArm.localRotation = Quaternion.Euler(0f, (float)time.TotalSeconds * degreesPerSecond, 0f);
    }

    private void UpdateDiscrete()
    {
        DateTime time = DateTime.Now;

        //Debug.Log(DateTime.Now);
        hoursArm.localRotation = Quaternion.Euler(0f, time.Hour * degreesPerHour, 0f);
        minutesArm.localRotation = Quaternion.Euler(0f, time.Minute * degreesPerMinutes, 0f);
        secondsArm.localRotation = Quaternion.Euler(0f, time.Second * degreesPerSecond, 0f);
    }
}