using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.RenderStreaming;

public class WebRtcSettings : MonoBehaviour
{
    [SerializeField] private Dropdown bandwidthSelector;
    [SerializeField] private Dropdown scaleResolutionDownSelector;
    [SerializeField] private Dropdown framerateSelector;
    [SerializeField] private Dropdown receiverVideoCodecSelector;
    [SerializeField] private Dropdown senderVideoCodecSelector;
    [SerializeField] private Dropdown streamSizeSelector;

    private Dictionary<string, uint> bandwidthOptions =
        new Dictionary<string, uint>()
        {
                        { "10000", 10000 },
                        { "2000", 2000 },
                        { "1000", 1000 },
                        { "500",  500 },
                        { "250",  250 },
                        { "125",  125 },
        };

    private Dictionary<string, float> scaleResolutionDownOptions =
        new Dictionary<string, float>()
        {
                { "Not scaling", 1.0f },
                { "Down scale by 2.0", 2.0f },
                { "Down scale by 4.0", 4.0f },
                { "Down scale by 8.0", 8.0f },
                { "Down scale by 16.0", 16.0f }
        };

    private Dictionary<string, float> framerateOptions =
        new Dictionary<string, float>
        {
                { "90", 90f },
                { "60", 60f },
                { "30", 30f },
                { "20", 20f },
                { "10", 10f },
                { "5", 5f },
        };

    [SerializeField]
    private List<Vector2Int> streamSizeList = new List<Vector2Int>
        {
            new Vector2Int(640, 360),
            new Vector2Int(1280, 720),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
            new Vector2Int(3840, 2160),
            new Vector2Int(360, 640),
            new Vector2Int(720, 1280),
            new Vector2Int(1080, 1920),
            new Vector2Int(1440, 2560),
            new Vector2Int(2160, 3840),
        };

    private static Vector2Int s_streamSize = new Vector2Int(Screen.width, Screen.height);
    private static int s_selectVideoCodecIndex = -1;

    // todo(kazuki): This is be only a temporary measure.
    private static int s_selectSenderVideoCodecIndex = -1;
    private static int s_framerate = -1;
    private static uint s_bitrate = 0;
    private static float s_scaleResolutionDown = 1.0f;
    private VideoStreamSender stream;

    public static Vector2Int StreamSize
    {
        get { return s_streamSize; }
        set { s_streamSize = value; }
    }

    public static int SelectVideoCodecIndex
    {
        get { return s_selectVideoCodecIndex; }
        set { s_selectVideoCodecIndex = value; }
    }

    public static int SelectSenderVideoCodecIndex
    {
        get { return s_selectSenderVideoCodecIndex; }
        set { s_selectSenderVideoCodecIndex = value; }
    }

    public static int Framerate
    {
        get { return s_framerate; }
        set { s_framerate = value; }
    }

    public static uint Bitrate
    {
        get { return s_bitrate; }
        set { s_bitrate = value; }
    }

    public static float ScaleResolutionDown
    {
        get { return s_scaleResolutionDown; }
        set { s_scaleResolutionDown = value; }
    }

    private void Start()
    {
        bandwidthSelector.options = bandwidthOptions
            .Select(pair => new Dropdown.OptionData { text = pair.Key })
            .ToList();
        framerateSelector.SetValueWithoutNotify(2);
        bandwidthSelector.onValueChanged.AddListener(ChangeBandwidth);

        scaleResolutionDownSelector.options = scaleResolutionDownOptions
            .Select(pair => new Dropdown.OptionData { text = pair.Key })
            .ToList();
        scaleResolutionDownSelector.onValueChanged.AddListener(ChangeScaleResolutionDown);

        framerateSelector.options = framerateOptions
            .Select(pair => new Dropdown.OptionData { text = pair.Key })
            .ToList();
        framerateSelector.SetValueWithoutNotify(2);
        framerateSelector.onValueChanged.AddListener(ChangeFramerate);

        var optionList = streamSizeList.Select(size => new Dropdown.OptionData($" {size.x} x {size.y} ")).ToList();
        optionList.Add(new Dropdown.OptionData(" Custom "));
        streamSizeSelector.options = optionList;

        var existInList = streamSizeList.Contains(StreamSize);
        if (existInList)
        {
            streamSizeSelector.value = streamSizeList.IndexOf(StreamSize);
        }
        else
        {
            streamSizeSelector.value = optionList.Count - 1;
        }
        streamSizeSelector.onValueChanged.AddListener(OnChangeStreamSizeSelect);

        var videoCodecList = AvailableCodecsUtils.GetAvailableVideoCodecsName()
                .Select(pair => new Dropdown.OptionData(pair.Value)).ToList();
        if (videoCodecList.Count > 0) SelectVideoCodecIndex = 0;

        receiverVideoCodecSelector.options.AddRange(videoCodecList);
        senderVideoCodecSelector.options.AddRange(videoCodecList);

        if (SelectVideoCodecIndex >= 0 &&
            SelectVideoCodecIndex < videoCodecList.Count)
        {
            receiverVideoCodecSelector.value = SelectVideoCodecIndex + 1;
        }

        if (SelectSenderVideoCodecIndex >= 0 &&
            SelectSenderVideoCodecIndex < videoCodecList.Count)
        {
            senderVideoCodecSelector.value = SelectSenderVideoCodecIndex + 1;
        }

        receiverVideoCodecSelector.onValueChanged.AddListener(OnChangeReceiverVideoCodecSelect);
        senderVideoCodecSelector.onValueChanged.AddListener(OnChangeSenderVideoCodecSelect);
    }

    private void OnChangeSenderVideoCodecSelect(int index)
    {
        SelectSenderVideoCodecIndex = index - 1;
    }

    private void OnChangeStreamSizeSelect(int index)
    {
        Debug.Log($"Changing stream resolution to {StreamSize.x} x {StreamSize.y} ");
        var isCustom = index >= streamSizeList.Count;
        if (isCustom)
        {
            return;
        }
        StreamSize = streamSizeList[index];
        if (stream) stream.streamingSize = StreamSize;
    }

    private void OnChangeReceiverVideoCodecSelect(int index)
    {
        SelectVideoCodecIndex = index - 1;
        var name = senderVideoCodecSelector.options.ElementAt(index).text;
        Debug.Log($"Changing codecs to {name}");
        stream?.FilterVideoCodecs(SelectVideoCodecIndex);
    }

    private void ChangeBandwidth(int index)
    {
        Bitrate = bandwidthOptions.Values.ElementAt(index);
        Debug.Log($"Changing bandwidth to {Bitrate} ");
        stream?.SetBitrate(Bitrate, Bitrate);
    }

    private void ChangeScaleResolutionDown(int index)
    {
        ScaleResolutionDown = scaleResolutionDownOptions.Values.ElementAt(index);
        Debug.Log($"Changing resolution scale to {ScaleResolutionDown} ");
        stream?.SetScaleResolutionDown(ScaleResolutionDown);
    }

    private void ChangeFramerate(int index)
    {
        Framerate = (int)framerateOptions.Values.ElementAt(index);
        Debug.Log($"Changing framerate to {Framerate} ");
        stream?.SetFrameRate(Framerate);
    }

    public void SetUserPreferences(VideoStreamSender stream)
    {
        // Debug.Log("Applying user settings");
        this.stream = stream;
        if (stream == null)
        {
            gameObject.SetActive(true);
            return;
        }
        else
        {
            gameObject.SetActive(false);
        }

        if (stream.streamingSize != StreamSize) stream.streamingSize = StreamSize;
        if (stream.scaleResolutionDown != ScaleResolutionDown) stream.SetScaleResolutionDown(ScaleResolutionDown);
        if (stream.frameRate != Framerate && Framerate != -1) stream.SetFrameRate(Framerate);
        if (stream.minBitrate != Bitrate && Bitrate != 0) stream.SetBitrate(Bitrate, Bitrate);
    }
}
