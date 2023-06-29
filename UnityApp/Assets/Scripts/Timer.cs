using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    public float quizTime = 30;
    public TextMeshProUGUI textUI;
    public float time;
    private bool timerRunning = false;
    protected Action callback;
    private bool stopped = false;
    public float helpTime = 5;

    void Start()
    {
        this.time = this.quizTime;
        this.textUI.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.timerRunning)
        {
            this.time -= Time.deltaTime;
            this.textUI.text = "" + this.time.ToString("f0");
            if (0 < this.time && this.time <= 5)
            {
                this.textUI.color = new Color(255, 0, 0, 255);
            }
            if (this.time <= 0)
            {
                // Timer ended...
                this.timerRunning = false;
                this.time = this.quizTime;
                this.textUI.gameObject.SetActive(false);
                if (this.callback != null)
                {
                    this.callback.Invoke();
                    this.callback = null;
                }
            }
        }
    }

    public void startTimer(Action pCallback = null)
    {
        if (pCallback != null)
        {
            this.callback = pCallback;
        }
        this.textUI.color = new Color(255, 255, 255, 255);
        this.textUI.gameObject.SetActive(true);
        this.timerRunning = true;
    }

    public void stopTimer()
    {
        if (!stopped)
        {
            this.timerRunning = false;
            this.stopped = true;
        }
    }

    public void resumeTimer()
    {
        if (stopped)
        {
            this.timerRunning = true;
            this.stopped = false;
        }
    }

    public void resetTimer()
    {
        this.timerRunning = false;
        this.stopped = false;
        this.time = this.quizTime;
        this.textUI.gameObject.SetActive(false);
        this.callback = null;
    }

    public void addHelpTime()
    {
        this.time += helpTime;
        if (this.time > this.quizTime)
        {
            this.time = this.quizTime;
        }
    }
}
