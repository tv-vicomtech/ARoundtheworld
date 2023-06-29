using UnityEngine;
using UnityEngine.UI;

public class Statistics : MonoBehaviour
{
    public float updateInterval = 0.5f;
    public int MessagesReceived = 0;
    public int MessagesSent = 0; 
    public int Stack = 0;
    public Text fpsText;
    public Font font;
    float accum = 0.0f;
    int frames = 0;
    float timeleft;
    float fps;

    void Start()
    {
        // Make the game run as fast as possible. It will go down to Screen.currentResolution refresh rate
        Application.targetFrameRate = 300;

        fpsText = this.gameObject.AddComponent<Text>();
        fpsText.font = font;
        fpsText.alignment = TextAnchor.UpperCenter;
    }

    // Update is called once per frame
    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            fps = (accum / frames);
            fpsText.text = fps.ToString("F2") + "FPS " + (1 / fps).ToString("F4") + "ms " + MessagesReceived + "/" + MessagesSent + "msgs, stack " + Stack;
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
            MessagesReceived = 0;
            MessagesSent = 0;
            Stack = 0;
        }
    }
}
