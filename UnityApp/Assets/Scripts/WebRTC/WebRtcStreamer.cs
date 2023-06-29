using UnityEngine;
using Unity.RenderStreaming;
using System.Threading;
using Unity.RenderStreaming.Signaling;
using Unity.WebRTC;
using static OrkestraLib.Orkestra;
using OrkestraLib.Message;

class WebRtcStreamer : MonoBehaviour
{
    public string SignalingAddress = "localhost";
    public float SignalingInterval = 5;
    public bool SignalingSecured = false;
    public bool EnableHWCodec = false;
    public Camera remoteCamera;
    public GameObject StopButton;
    public GameObject StartButton;

    private RenderStreaming renderStreaming;
    private ScreenStreamSender videoStreamSender;
    private StreamingChannels streams;
    private WebRtcSettings settings;

    [SerializeField, Tooltip("Array to set your own STUN/TURN servers.")]
    private RTCIceServer[] iceServers = new RTCIceServer[]
    {
            new RTCIceServer() {urls = new string[] {
                "stun:stun.l.google.com:19302",
                "stun1.l.google.com:19302",
                "stun2.l.google.com:19302"
            }}
    };

    public ISignaling Signaling(MainApp app)
    {
        var schema = SignalingSecured ? "wss" : "ws";
        // var schema = SignalingSecured ? "https" : "http";
        WebSocketSignaling ws = new WebSocketSignaling($"{schema}://{SignalingAddress}", SignalingInterval, SynchronizationContext.Current);
        ws.OnStart += (ISignaling signaling) =>
        {
            // app.Dispatch("Remote servers can connect now")
            Debug.Log("Connected to " + signaling.Url + " and waiting for remote clients");
            if (app)
                app.Dispatch(Channel.Application,
                    new StatusWebRTC(app.GetUsername(),
                        app.GetUsername() + " is ready to stream the video"));
        };
        ws.OnOffer += (ISignaling signaling, DescData e) =>
        {
            // app.Dispatch("Got a remote request for video")
            Debug.Log("Remote view request " + e.connectionId);
            if (app)
                app.Dispatch(Channel.Application,
                    new StatusWebRTC(app.GetUsername(),
                        app.GetUsername() + " received your video request"));
        };
        ws.OnAnswer += (ISignaling signaling, DescData e) =>
        {
            // app.Dispatch("Replying to remote viewer")
            Debug.Log("Sending data to " + e.connectionId);
            if (app)
                app.Dispatch(Channel.Application,
                    new StatusWebRTC(app.GetUsername(),
                        app.GetUsername() + " is live streaming"));
        };
        ws.OnDestroyConnection += (ISignaling signaling, string connectionId) =>
        {
            // app.Dispatch("Disconnect from remote viewer")
            Debug.Log("Remote viewer disconnected from this app " + connectionId);
            if (app)
                app.Dispatch(Channel.Application,
                    new StatusWebRTC(app.GetUsername(), "Live streaming stopped"));
        };

        return ws;
    }

    // Start is called before the first frame update
    public void StartConnection(MainApp app)
    {
        Debug.Log("------ Starts publishing WebRTC stream!");

        StopConnection();
        gameObject.SetActive(false);
        streams = gameObject.AddComponent<StreamingChannels>();
        renderStreaming = gameObject.AddComponent<RenderStreaming>();
        settings = gameObject.GetComponentInChildren<WebRtcSettings>();
        renderStreaming.runOnAwake = false;

        videoStreamSender = remoteCamera.gameObject.AddComponent<ScreenStreamSender>();
        videoStreamSender.OnStartedStream += (string connectionId) =>
        {
            Debug.Log("Stream started " + connectionId);
        };
        settings.SetUserPreferences(videoStreamSender);

        streams.streams.Add(videoStreamSender);
        gameObject.SetActive(true);

        RTCConfiguration conf = new RTCConfiguration { iceServers = iceServers };
        renderStreaming.Run(
            conf: conf,
            signaling: Signaling(app),
            handlers: gameObject.GetComponents<StreamingChannels>());

        StopButton?.SetActive(true);
        StartButton?.SetActive(false);
    }

    public void StartVideo()
    {
        StartConnection(null);
    }

    public void StopConnection()
    {
        StopButton?.SetActive(false);
        StartButton?.SetActive(true);
        if (renderStreaming)
        {
            Debug.Log("------Killing WebRTC connection");
            renderStreaming.Stop();
            DestroyImmediate(renderStreaming);
        }

        settings?.SetUserPreferences(null);
        if (streams) DestroyImmediate(streams);
        if (videoStreamSender) DestroyImmediate(videoStreamSender);
    }
}
