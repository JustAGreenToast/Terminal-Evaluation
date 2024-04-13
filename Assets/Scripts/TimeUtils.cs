using System;
using UnityEngine;

public static class TimeUtils
{
    public static string SecondsToTimerString(float _t)
    {
        TimeSpan t = TimeSpan.FromSeconds(Mathf.Clamp(_t, 0, 5999.999f));
        return $"{((int)t.TotalMinutes).ToString().PadLeft(2, '0')}:{t.Seconds.ToString().PadLeft(2, '0')}.{t.Milliseconds.ToString().PadLeft(3, '0')}";
    }
}