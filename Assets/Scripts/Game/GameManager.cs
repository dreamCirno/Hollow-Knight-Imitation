using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    bool enableInput = true;
    bool waiting = false;
    public void Stop(float duration, float timeScale)
    {
        if (waiting)
            return;
        Time.timeScale = timeScale;
        StartCoroutine(Wait(duration));
    }
    public void Stop(float duration)
    {
        Stop(duration, 0.0f);
    }
    IEnumerator Wait(float duration)
    {
        waiting = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
        waiting = false;
    }
    public bool IsEnableInput()
    {
        return enableInput;
    }
    public void SetEnableInput(bool enabled)
    {
        enableInput = enabled;
    }
}
