using UnityEngine;
using UnityEngine.UI;
using OrkestraLib;
using OrkestraLib.Message;
using TMPro;
using System.Collections.Generic;

public class HelpManager : Orkestra
{
    MainApp mainAppScript;

    public GameObject continentHelpPanel;
    public TMP_Text africaText;
    public TMP_Text antarcticaText;
    public TMP_Text asiaText;
    public TMP_Text oceaniaText;
    public TMP_Text europeText;
    public TMP_Text northAmericaText;
    public TMP_Text southAmericaText;
    public TMP_Text continentUIText;

    public GameObject thumbHelpPanel;
    public TMP_Text thumbsUpText;
    public TMP_Text thumbsDownText;
    public TMP_Text thumbsUIText;

    internal bool helpSent = false;
    internal bool help2Sent = false;

    private Image continentPanelImage;
    private Image thumbPanelImage;
    private List<RawImage> continentButtonsImageList = new List<RawImage>();
    private List<RawImage> thumbButtonsImageList = new List<RawImage>();

    // Start is called before the first frame update
    void Start()
    {
        this.mainAppScript = FindObjectOfType<MainApp>();

        // Orkestra SDK
        this.mainAppScript.ApplicationEvents += AppEventSubscriber;

        // Hide help panels
        this.resetContinentHelpUI();
        this.resetThumbHelpUI();
        this.continentHelpPanel.SetActive(false);
        this.thumbHelpPanel.SetActive(false);

        this.continentPanelImage = this.continentHelpPanel.GetComponent<Image>();
        this.thumbPanelImage = this.thumbHelpPanel.GetComponent<Image>();
        for (int i = 0; i < this.continentHelpPanel.transform.childCount; i++)
        {
            GameObject child = this.continentHelpPanel.transform.GetChild(i).gameObject;
            if (child.name.Contains("Btn"))
            {
                this.continentButtonsImageList.Add(child.GetComponent<RawImage>());
            }
        }
        for (int i = 0; i < this.thumbHelpPanel.transform.childCount; i++)
        {
            GameObject child = this.thumbHelpPanel.transform.GetChild(i).gameObject;
            if (child.name.Contains("Btn"))
            {
                this.thumbButtonsImageList.Add(child.GetComponent<RawImage>());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        base.LateUpdate();
    }

    /// <summary>Process app events</summary>
    /// <param name="evt">Application event received</param>
    /// <param name="sender"></param>
    void AppEventSubscriber(object sender, ApplicationEvent evt)
    {
        // Help Messages management
        if (evt.IsEvent(typeof(HelpMessage)))
        {
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("------ HelpMessage message received: " + evt.value);
            HelpMessage helpMessage = new HelpMessage(evt.value);
            Events.Add(() =>
            {
                if (helpMessage.helpType == "continent")
                {
                    registerContinentHelpMessage(helpMessage.content);
                }
                else if (helpMessage.helpType == "thumb")
                {
                    registerThumbHelpMessage(helpMessage.content);
                }
            });
        }
    }

    public void continentHelpClickHandler(string buttonId)
    {
        if (!mainAppScript.isPlaying && !this.helpSent)
        {
            sendHelp("continent", buttonId);
            // Change state variable to disable continent buttons
            disableHelpPanelColor("continent");
            toggleHelpUIText("continent", false);
            this.helpSent = true;
        }
    }

    public void thumbHelpClickHandler(string buttonId)
    {
        if (!mainAppScript.isPlaying && !this.help2Sent)
        {
            sendHelp("thumb", buttonId);
            // Change state variable to disable thumb buttons
            disableHelpPanelColor("thumb");
            toggleHelpUIText("thumb", false);
            this.help2Sent = true;
        }
    }

    void disableHelpPanelColor(string panel)
    {
        Image panelImage = panel == "continent" ? this.continentPanelImage : this.thumbPanelImage;
        Color col = panelImage.color;
        panelImage.color = new Color(col.r, col.g, col.b, 0.5f);
        List<RawImage> buttonList = (panel == "continent" ? this.continentButtonsImageList : this.thumbButtonsImageList);
        for (int i = 0; i < buttonList.Count; i++)
        {
            Color buttonColor = buttonList[i].color;
            buttonList[i].color = new Color(buttonColor.r, buttonColor.g, buttonColor.b, 0.5f);
        }
    }

    public void toggleHelpUIText(string helpType, bool active)
    {
        Events.Add(() =>
        {
            if (helpType == "continent")
            {
                continentUIText.gameObject.SetActive(active);
            }
            else
            {
                thumbsUIText.gameObject.SetActive(active);
            }
        });
    }

    public void enableHelpPanelColor(string panel)
    {
        Image panelImage = panel == "continent" ? this.continentPanelImage : this.thumbPanelImage;
        if (panelImage)
        {
            Color col = panelImage.color;
            panelImage.color = new Color(col.r, col.g, col.b, 1f);
            List<RawImage> buttonList = (panel == "continent" ? this.continentButtonsImageList : this.thumbButtonsImageList);
            for (int i = 0; i < buttonList.Count; i++)
            {
                Color buttonColor = buttonList[i].color;
                buttonList[i].color = new Color(buttonColor.r, buttonColor.g, buttonColor.b, 1f);
            }
        }
    }

    void sendHelp(string type, string content)
    {
        HelpMessage msg = new HelpMessage(mainAppScript.GetUsername(), content, type);
        Events.Add(() =>
        {
            if (type == "continent")
            {
                registerContinentHelpMessage(content);
            }
            else
            {
                registerThumbHelpMessage(content);
            }
            mainAppScript.sendHelpStatement(type, content);
        });
        mainAppScript.dispatchHelp(msg);
    }

    bool IgnoreMessage(string value)
    {
        Message msg = new Message(value);
        if (msg.sender.Equals(this.mainAppScript.GetUsername())) return true;
        return false;
    }

    void registerContinentHelpMessage(string value)
    {
        if (value == "africa")
        {
            this.africaText.text = (int.Parse(this.africaText.text) + 1).ToString();
            return;
        }
        if (value == "antarctica")
        {
            this.antarcticaText.text = (int.Parse(this.antarcticaText.text) + 1).ToString();
            return;
        }
        if (value == "asia")
        {
            this.asiaText.text = (int.Parse(this.asiaText.text) + 1).ToString();
            return;
        }
        if (value == "oceania")
        {
            this.oceaniaText.text = (int.Parse(this.oceaniaText.text) + 1).ToString();
            return;
        }
        if (value == "europe")
        {
            this.europeText.text = (int.Parse(this.europeText.text) + 1).ToString();
            return;
        }
        if (value == "north-america")
        {
            this.northAmericaText.text = (int.Parse(this.northAmericaText.text) + 1).ToString();
            return;
        }
        if (value == "south-america")
        {
            this.southAmericaText.text = (int.Parse(this.southAmericaText.text) + 1).ToString();
            return;
        }
    }

    void registerThumbHelpMessage(string value)
    {
        if (value == "up")
        {
            this.thumbsUpText.text = (int.Parse(this.thumbsUpText.text) + 1).ToString();
            return;
        }
        if (value == "down")
        {
            this.thumbsDownText.text = (int.Parse(this.thumbsDownText.text) + 1).ToString();
            return;
        }
    }

    public void resetContinentHelpUI()
    {
        this.africaText.text = "0";
        this.antarcticaText.text = "0";
        this.asiaText.text = "0";
        this.oceaniaText.text = "0";
        this.europeText.text = "0";
        this.northAmericaText.text = "0";
        this.southAmericaText.text = "0";
        this.enableHelpPanelColor("continent");
    }

    public void resetThumbHelpUI()
    {
        this.thumbsUpText.text = "0";
        this.thumbsDownText.text = "0";
        this.enableHelpPanelColor("thumb");
    }
}
