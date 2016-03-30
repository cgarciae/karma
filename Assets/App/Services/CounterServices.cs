using UnityEngine;
using System.Collections;
using Karma;
using Karma.Metadata;

[Service]
public class CounterServices
{
    public int maxValue
    {
        get { return PlayerPrefs.GetInt(C.counterMaxValue, 0); }
        set
        {
            PlayerPrefs.SetInt(C.counterMaxValue, value);
        }
    }
}
