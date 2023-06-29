using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using OrkestraLib;
using OrkestraLib.Message;
using OrkestraLib.Plugins;
using Transform = UnityEngine.Transform;
using static OrkestraLib.Utilities.ParserExtensions;

// bug share disables on new user

public class MainApp : Orkestra
{
    StatementSender statementSender;

    public float MessageDispatchRate = 0.2f;
    public float Speed = 0.01f;
    public float smoothTime = 0.3f;

    public GameObject Prefab;
    public TextMeshProUGUI InfoHeader;
    public TextMeshProUGUI InfoUsers;
    public TextMeshProUGUI chat;
    public string userType = "Teacher";
    public TextMeshProUGUI usernameUI;
    private string username = "";
    public TextMeshProUGUI roomname;
    public TextMeshProUGUI loginMessage;
    public SwitchManager isPickMode;

    public TextMeshProUGUI WaitScreenText;
    public TextMeshProUGUI WaitScreenText2;
    public CustomDropdown chatUserList;
    public NotificationManager Notification;
    public ModalWindowManager Modal;
    public ProgressBar Points;
    public WindowManager Windows;
    public CustomDropdown UserList;

    public ModalWindowManager CorrectAnswerForm;
    public ModalWindowManager WaitScoreModal;
    public ModalWindowManager SendAnswer;

    public List<PushPin> pins;
    public Sprite defaultIcon;
    public GameObject RegainControl;
    public GameObject NewSessionBtn;
    public GameObject ChatBtn;

    public GameObject ShareBtnHome;
    public GameObject ShareBtnWait;

    public GameObject PlaceSceneHome;
    public GameObject PlaceSceneWait;

    public SwitchManager SharedHome;
    public SwitchManager SharedWait;

    public TextMeshProUGUI ScoreUI;
    public TextMeshProUGUI ScoreUI2;
    public GameObject Welcome;

    private string studentId = "";
    private string activeUserId = "-";
    internal bool isPlaying = false;
    private Question receivedQuestion;
    private string activeQuestionId;
    private string activeQuestionText;
    private string activeQuestionCorrectAnswer;
    private Multiplatform platform;

    public Action OnLateUpdate;
    public Action OnPlaceScene;
    public Action<CameraTransform, bool> ApplyObjectPose;
    private bool IsSceneInPlace = false;
    private string PREFIX_UNITY_STUDENT = "#u_s_";

    public Planet defaultSelectionObject;

    public Transform RemoteRotation;
    public Transform RemoteReferenceSystem;
    public GameObject ReferenceSystem;
    private GameObject pushPin;
    private Answer lastAnswer;
    private bool isShared = false;
    internal bool confirmAnswerWindow;
    public Statistics stats;
    WebRtcStreamer webrtc;
    public Timer timer;
    public Action OnTurnTimeout;
    public HelpManager helpManager;
    /**
     * Variables for desktop behaviour
     **/
    internal bool isMouseDragging = false;
    private readonly float touchSpeed = 6f;
    private Vector3 lastMousePosition;
    /**
     * Variables for desktop behaviour end
     **/

    // [SerializeField] private UnityEvent onSendAnswer;

    /// <summary>
    /// Dispatchs camera messages at constant rate (fps): <code>60 / <see cref="MessageDispatchRate"/></code>
    /// </summary>
    public IEnumerator UpdateRemoteCameras()
    {
        for (; ; )
        {
            if (isPlaying)
            {
                string userId = GetUsername();
                if (platform != null && platform.SceneCamera != null && isShared)
                {
                    GameObject cam = platform.SceneCamera.gameObject;
                    CameraTransform pose = new CameraTransform(userId, cam, ReferenceSystem);
                    Dispatch(Channel.Application, pose);
                    if (stats) stats.MessagesSent++;
                    // if (platform.HasTarget())
                    // {
                    //     if (stats) stats.MessagesSent++;
                    //     Dispatch(Channel.Application, new ObjectTransform(userId, platform.Target().gameObject));
                    // }
                }
            }
            yield return new WaitForSeconds(MessageDispatchRate);
        }
    }

    /// <summary>
    /// Updates UI with new camera sharing settings 
    /// </summary>
    public void SetShared(bool shared)
    {
        isShared = shared;
        // if (IsHome())
        // {
        //     if (SharedHome.isOn != shared) SharedHome.AnimateSwitch();
        // }
        // else if (SharedWait.isOn != shared) SharedWait.AnimateSwitch();
    }

    /// <summary>
    /// Is camera sharing enabled 
    /// </summary>
    public bool IsShared()
    {
        return isShared;
    }

    /// <summary>Get logged-in user type</summary>
    public string GetUserType() { return userType; }

    /// <summary>Get username </summary>
    public string GetUsername() { return username.RemoveNonUnicodeLetters(); }

    /// <summary>Get username from UI</summary>
    public string GetUsernameUI() { return usernameUI.text.RemoveNonUnicodeLetters(); }

    /// <summary>Get roomname from login interface</summary>
    public string GetRoomname() { return roomname.text.RemoveNonUnicodeLetters(); }

    /// <summary>Checks if it is a student</summary>
    public bool IsStudent() { return GetUserType().Equals("Student"); }

    /// <summary> Check if the user is playing </summary>    
    public bool IsPlaying() { return isPlaying; }

    public bool IsHome()
    {
        if (Windows.currentWindowIndex >= 0)
            return Windows.windows[Windows.currentWindowIndex].windowName == "Home";
        return false;
    }

    void Start()
    {
        this.statementSender = new StatementSender();
        var streaming = GameObject.Find("RenderStreaming");
        webrtc = streaming != null ? streaming.GetComponent<WebRtcStreamer>() : null;
        GameObject mp = GameObject.Find("Multiplatform");
        if (mp != null)
        {
            platform = mp.GetComponent<Multiplatform>();
        }
        else
        {
            platform = gameObject.AddComponent<Multiplatform>();
            platform.SceneCamera = gameObject;
            Debug.LogError("You cannot play this scene. Instead open Desktop or Android");
            Quit();
        }

        SelectObject(defaultSelectionObject);

#if UNITY_ANDROID || UNITY_IOS
        PlaceSceneHome.SetActive(true);
        PlaceSceneWait.SetActive(true);
#endif

        // Disable message debugger
        EnableParserDebugger = false;

        // Orkestra SDK
        UserEvents += UserEventSubscriber;
        ApplicationEvents += AppEventSubscriber;

        // Register custom events to be used in the app
        RegisterEvents(new Type[]{
            typeof(ActiveUser),
            typeof(Question),
            typeof(PendingAnswer),
            typeof(Answer),
            typeof(Result),
            typeof(CameraTransform),
            typeof(ObjectTransform),
            typeof(SharedView),
            typeof(Notification),
            typeof(SelectObject),
            typeof(ActiveCamera),
            typeof(MobileDevice),
            typeof(StatusWebRTC),
            typeof(TimeoutTurn),
            typeof(HelpMessage),
            typeof(ScoreMessage),
            typeof(QuizFinishMessage)
        });

        // List of active pins
        pins = new List<PushPin>();

        // Start helper coroutine
        StartCoroutine("UpdateRemoteCameras");

        // Pre-requirement to Connect()
        OrkestraWithHSIO.Install(this, (graceful, message) =>
        {
            Events.Add(() =>
            {
                if (!graceful)
                {
                    loginMessage.text = message;
                }
                else
                {
                    ShowNotification(new Notification("Disconnected", "Disconnected from server"));
                }
            });
        });

        // THIS IS THE DESKTOP BEHAVIOUR FOR DEVELOPING PURPOSES, MOBILE DEVICE BEHAVIOUR IS IN MainAppAndroid.cs
        OnLateUpdate = () =>
        {
            if (UserInteractionTurn())
            {
                // Select object
                if (Input.GetMouseButtonDown(0)) // Left click down
                {
                    this.isMouseDragging = true;
                }

                if (Input.GetMouseButtonUp(0)) // Left click up
                {
                    this.isMouseDragging = false;
                }

                if (this.isMouseDragging)
                {
                    Vector3 mousePos = Input.mousePosition;
                    Vector3 offset = this.lastMousePosition - mousePos;
                    float rotX = offset.x * touchSpeed * Mathf.Deg2Rad;
                    float rotY = offset.y * touchSpeed * Mathf.Deg2Rad;
                    if (IsShared() && platform.HasTarget())
                    {
                        platform.Target().transform.Rotate(Vector3.up, rotX);
                        ObjectTransform transform = new ObjectTransform(GetUsername(), platform.Target().gameObject);
                        Dispatch(Channel.Application, transform);
                    }
                    this.lastMousePosition = mousePos;
                }

                // Place/validate object
                if (Input.GetMouseButtonDown(1) && isPlaying)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        OnUserAnswerClick(hit);
                    }
                }
            }
        };

        // Default to desktop behaviour. That is, we should do nothing
        OnPlaceScene = () => { };

        // Remote camera transform logic
        ApplyObjectPose = (cv, isSharedView) =>
        {
            if (platform && platform.SceneCamera != null && cv.name != null && platform.HasTarget())
            {
                // Android
                if (Camera.main.name.Equals("ARCamera"))
                {
                    Transform from = platform.RemoteSceneCamera.transform;
                    Transform user = platform.RemoteUser.transform;
                    Transform target = platform.Target().transform;
                    Transform to = Camera.main.transform;

                    // Update remote scene camera
                    cv.Update(from, RemoteReferenceSystem);
                    cv.Update(user);

                    // Only others can see it
                    user.gameObject.SetActive(!isSharedView && !UserInteractionTurn());

                    // Set relative to this reference system
                    from.localPosition += ReferenceSystem.transform.localPosition;
                    from.localPosition -= RemoteReferenceSystem.localPosition;
                    user.localPosition += ReferenceSystem.transform.localPosition;
                    user.localPosition -= RemoteReferenceSystem.localPosition;

                    // Update Target rotation
                    target.rotation = RemoteRotation.rotation;
                    if (isSharedView)
                    {
                        target.LookAt(to.position);
                        Quaternion rot = Quaternion.Inverse(from.transform.rotation);
                        rot = new Quaternion(rot.x, -rot.y, rot.z, -rot.w);
                        target.rotation *= rot * Quaternion.Euler(0, 180f, 0);

                        // Add rotation offset 
                        target.transform.Rotate(RemoteRotation.rotation.eulerAngles);
                    }
                    else
                    {
                        // Rotate user position by planet rotation offset
                        user.Rotate(RemoteRotation.rotation.eulerAngles);
                        //platform.RemoteUser.transform.rotation *= Quaternion.Euler(0, 180f, 0);
                    }
                }
                // Desktop
                else
                {
                    Transform cam = platform.SceneCamera.transform;
                    Transform remoteCam = platform.RemoteSceneCamera.transform;

                    // if (isSharedView)
                    //     cv.Update(cam, ReferenceSystem.transform);

                    cv.Update(remoteCam, ReferenceSystem.transform);

                    // Shift by user-rotation
                    if (platform.HasTarget())
                    {
                        platform.Target().gameObject.transform.rotation = RemoteRotation.rotation;
                    }
                }
            }
        };

        OnTurnTimeout = () =>
        {
            // Stop the playmode
            isPlaying = false;
            // HideShareButton();
            Events.Add(WaitTurn);
            // Send the answer send statement
            sendStatement(Utils.stripPrefix(GetUsername()), "Ran Out", activeQuestionId);
            // Send an orkestra event of type timeout (sender, username, question index)
            Dispatch(Channel.Application, new TimeoutTurn(GetUsername(), "teacher", activeQuestionId));
            activeQuestionId = null;
            // Stop sending video when submitting the answer
            // webrtc.StopConnection();
            DisableSharedView(); // Stop the sharing mode
            // Reset help values
            this.helpManager.resetContinentHelpUI();
            this.helpManager.resetThumbHelpUI();
            this.helpManager.continentHelpPanel.SetActive(false);
            this.helpManager.thumbHelpPanel.SetActive(false);
            // Show modal informing about the timeout
            Notification msg = new Notification("{\"sender\": \"Notification\", \"timestamp\": " + DateTimeOffset.Now.ToUnixTimeMilliseconds() + ", \"content\":\"You ran out of time! The question was sent to the next student!\"}");
            Events.Add(() => ShowNotification(msg));
        };

        SendAnswer.onConfirm.AddListener(() =>
        {
            // Stop the playmode
            isPlaying = false;
            Events.Add(WaitTurn);

            // Clear game info
            InfoHeader.text = "";
            InfoHeader.color = Color.white;
            InfoUsers.text = "";

            // Stop WebRTC transmission
            // webrtc.StopConnection();
            DisableSharedView();  // Stop the sharing mode
            // Reset timer
            this.timer.resetTimer();
            // Clear the pin
            if (pushPin)
            {
                Destroy(pushPin);
            }
            this.pins.Clear();
        });

        SendAnswer.onCancel.AddListener(() =>
        {
            // Resume the timer but add 5 extra seconds
            this.timer.addHelpTime();
            this.timer.resumeTimer();
            // Reset UI
            MovePickSwitch(false);
            isPickMode.AnimateSwitch();
            // Clear the pin
            if (pushPin)
            {
                Destroy(pushPin);
            }
            this.pins.Clear();
            // Update UI
            Events.Add(() =>
            {
                // Hide the thumb help panel
                this.helpManager.thumbHelpPanel.SetActive(false);
                this.helpManager.resetThumbHelpUI();
                // Show continent help panel but hide the text
                this.helpManager.continentHelpPanel.SetActive(true);
                this.helpManager.continentUIText.gameObject.SetActive(false);
            });

            // Send a reject message to the spectators
            Notification msg = new Notification(GetUsername(), Utils.stripPrefix(GetUsername()) + " rejected the answer");
            Events.Add(() => Dispatch(Channel.Application, msg));
        });
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
        OnLateUpdate?.Invoke();
    }

    void OnApplicationQuit()
    {
        Debug.LogWarning("Application ending after " + Time.time + " seconds");
    }

    public void Quit()
    {
        // Send the logout statement
        sendStatement(Utils.stripPrefix(GetUsername()), "Logged Out", GetRoomname());
        Logout();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Validate and remove local pin</summary>
    public void RemovePin(PushPin pp)
    {
        if (pp)
        {
            studentId = pp.answer.sender;
            CorrectAnswerForm.titleText = "Validate answer";
            CorrectAnswerForm.descriptionText = "Is this answer correct ?";
            CorrectAnswerForm.UpdateUI();
            CorrectAnswerForm.OpenWindow();
            // Clear the pin
            if (pp.gameObject)
            {
                Destroy(pp.gameObject);
            }
            this.pins.Clear();
        }
    }

    void Logout()
    {
        // If the teacher leaves, the application should stop the session.
        if (!IsStudent()) NewSession();
    }

    /// <summary>Connect to Orkestralib after login</summary>
    public void Login()
    {
        string name = this.PREFIX_UNITY_STUDENT + GetUsernameUI(); // Add the user prefix
        string roomId = GetRoomname();
        if (GetUsernameUI().Length > 2 && roomId.Length > 2)
        {
            // Use username as agentid
            agentId = name;

            // Update the username
            this.username = name;

            // Use login room as orkestra room
            room = roomId;

            // Connect
            try
            {
                loginMessage.text = "Authenticating...";
                Connect(null, (isError, message) =>
                {
                    Events.Add(() =>
                    {
                        if (!isError)
                        {
                            loginMessage.text = message;
                        }
                        else
                        {
                            InfoHeader.text = message;
                            InfoHeader.color = Color.white;
                        }
                    });
                });
            }
            catch (Exception e)
            {
                loginMessage.text = e.Message;
            }
        }
        else
        {
            loginMessage.text = "Please insert a valid name";
        }
    }

    public void StartAsTeacher()
    {
        userType = "Teacher";
        Login();
    }

    public void StartAsStudent()
    {
        userType = "Student";
        Login();
    }

    /// <summary>Updates list of users</summary>
    public void UpdateUserList()
    {
        chatUserList.dropdownItems.Clear();
        List<string> users = new List<string>(AppContext.GetRemoveUserIds());
        foreach (string id in users)
        {
            // Excludes this user from the selectable list
            if (!id.Equals(GetUsername()))
            {
                chatUserList.SetItemTitle(id);
                chatUserList.SetItemIcon(defaultIcon);
                chatUserList.CreateNewItem();
            }
        }
    }

    bool IgnoreMessage(string value)
    {
        Message msg = new Message(value);
        if (msg.sender.Equals(GetUsername())) return true;
        return false;
    }

    /// <summary> Process user events </summary>
    /// <param name="orkestraLib">Reference to OrkestraLib instance</param>
    /// <param name="evt">User event received from the server</param>
    /// 
    /// @BUG: join event does not happen in some cases 
    void UserEventSubscriber(object orkestraLib, UserEvent evt)
    {
        if (stats) stats.MessagesReceived++;
        if (evt.IsPresenceEvent())
        {
            // Start session only when user is logged in
            if (evt.IsUser(GetUsername()) && evt.IsJoinEvent())
            {
                Debug.Log("----- Logged in as '" + evt.agentid + "' ");
                // Send the login statement
                sendStatement(Utils.stripPrefix(GetUsername()), "Logged In", GetRoomname());
                Events.Add(StartSession);
            }
            Events.Add(UpdateUserList);
        }
        else if (false && evt.evt.Equals("agent_event")) // Initial settings ?
        {
            KeyValue kv = evt.data;
            if (!kv.value.Equals("undefined"))
            {
                Debug.Log("UserEventSubscriber(" + kv.ToJSON() + ") ");
            }
        }
        // else if (evt.IsEvent(typeof(Question)))
        // {
        //     Question msg = new Question(evt.data.value);
        //     Debug.Log("----- Question received " + msg.ToJSON());
        //     receivedQuestion = msg;
        //     Events.Add(() => AskQuestion(msg));
        // }
        else if (evt.IsEvent(typeof(Result)))
        {
            Result result = new Result(evt.data.value);
            Debug.Log("----- Result received " + result.ToJSON());
            Events.Add(() =>
            {
                int p = int.Parse(result.value);
                if (p <= 100 && p >= 0)
                {
                    Points.currentPercent += p;
                    Points.textPercent.text = "" + Points.currentPercent;
                }
                ShowNotification(new Notification("Results", result.sender + ", you got " + p + " points"));
            });
        }

        if (stats) stats.Stack = Events.Count;
    }

    /// <summary>Process app events</summary>
    /// <param name="evt">Application event received</param>
    /// <param name="sender"></param>
    void AppEventSubscriber(object sender, ApplicationEvent evt)
    {
        if (stats) stats.MessagesReceived++;
        // Active (playing) user interface
        if (evt.IsEvent(typeof(ActiveUser)))
        {
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("------ ActiveUser message received: " + evt.value);

            ActiveUser user = new ActiveUser(evt.value);
            activeUserId = user.username;
            if (activeUserId == "teacher")
            {
                Events.Add(() =>
                {
                    WaitScreenText.text = "Waiting for the next question...";
                    WaitScreenText.color = Color.white;
                });
            }
            else
            {
                Events.Add(() =>
                {
                    WaitScreenText.text = Utils.stripPrefix(activeUserId) + " is playing";
                    WaitScreenText.color = Color.white;
                    WaitScreenText2.text = "Question: " + activeQuestionText;
                });
            }

            // Set the active player logic
            if (activeUserId.Equals(GetUsername())) // This player is playing
            {
                isPlaying = true;
                Events.Add(PlayTurn);
            }
            else // Another player is playing
            {
                isPlaying = false;
                Events.Add(() =>
                {
                    WaitTurn();
                });
            }
        }
        // Question received
        else if (evt.IsEvent(typeof(Question)))
        {
            Question question = new Question(evt.value);
            if (IgnoreMessage(evt.value)) return;
            Events.Add(() =>
            {
                if (question.username == GetUsername())
                {
                    Debug.Log("----- Question received " + question.ToJSON());
                    receivedQuestion = question;
                    Events.Add(() => AskQuestion(question));
                }
                activeQuestionText = question.content;
                activeQuestionCorrectAnswer = question.correctAnswer;
            });
        }
        // Draw/Validate answers
        else if (evt.IsEvent(typeof(Answer)))
        {
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("------ Answer message received: " + evt.value);
            Answer answer = new Answer(evt.value);
            answer.time_left = this.timer.time;

            if (GetUsername().Equals(answer.sender))
            {
                Events.Add(() =>
                {
                    // Update UI texts
                    InfoHeader.text = "Waiting to the teacher...";
                    InfoHeader.color = Color.white;
                    InfoUsers.text = "";
                    WaitScreenText.text = "Waiting to the teacher...";
                    WaitScreenText.color = Color.white;
                    WaitScreenText2.text = "";
                    SubmitAnswer(answer);
                });

            }
            else
            {
                Events.Add(() =>
                {
                    // Update UI texts
                    InfoHeader.text = "Waiting to the teacher...";
                    InfoHeader.color = Color.white;
                    InfoUsers.text = "";
                    WaitScreenText.text = "Waiting to the teacher...";
                    WaitScreenText.color = Color.white;
                    WaitScreenText2.text = "";
                    // Hide and reset help panels
                    helpManager.continentHelpPanel.SetActive(false);
                    helpManager.thumbHelpPanel.SetActive(false);
                    helpManager.resetContinentHelpUI();
                    helpManager.resetThumbHelpUI();
                    InfoHeader.text = "";
                    InfoUsers.text = "";
                    ValidateAnswer(answer);
                });
            }
        }
        // Active scene
        else if (evt.IsEvent(typeof(SelectObject)))
        {
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("------ SelectObject message received: " + evt.value);
            Events.Add(() =>
            {
                SelectObject scene = new SelectObject(evt.value);
                GameObject targetPlanet = GameObject.Find(scene.name);
                if (targetPlanet)
                {
                    SelectObject(targetPlanet.GetComponent<Planet>());
                }
            });
        }
        // Update shared view
        else if (evt.IsEvent(typeof(SharedView)))
        {
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("------ SharedView message received: " + evt.value);
            Events.Add(() =>
            {
                SharedView sv = new SharedView(evt.value);
                if (sv.active) ActivateSharedView();
                else DisableSharedView();
            });
        }
        // Camera updates
        else if (evt.IsEvent(typeof(CameraTransform)))
        {
            if (IgnoreMessage(evt.value)) return;
            Events.Add(() =>
            {
                ApplyObjectPose?.Invoke(new CameraTransform(evt.value), IsShared());
            });
        }
        // Object updates
        else if (evt.IsEvent(typeof(ObjectTransform)))
        {
            if (IgnoreMessage(evt.value)) return;
            Events.Add(() =>
            {
                ObjectTransform cv = new ObjectTransform(evt.value);
                if (cv.name.Equals("Earth")) cv.Update(RemoteRotation);
            });
        }
        // Notifications
        else if (evt.IsEvent(typeof(Notification)))
        {
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("------ Notification message received: " + evt.value);
            Notification msg = new Notification(evt.value);

            // if the notification contains the string "rejected the answer" -> set the thumb help values to zero
            if (msg.content.Contains("rejected the answer"))
            {
                Events.Add(() =>
                {
                    ResetAnswers();
                    this.helpManager.help2Sent = false;
                    this.helpManager.thumbHelpPanel.SetActive(false);
                    // Hide the continent help panel
                    this.helpManager.continentHelpPanel.SetActive(true);
                    this.helpManager.continentUIText.gameObject.SetActive(!this.helpManager.helpSent);
                    ShowNotification(msg);
                });
            }
            else if (msg.content.Contains("accepted the new question"))
            {
                Events.Add(() =>
                {
                    // Delete all possible pins
                    this.deleteScenePins();
                    // Update UI texts
                    WaitScreenText.text = Utils.stripPrefix(msg.sender) + " is playing";
                    WaitScreenText2.text = "Question: " + activeQuestionText;
                    // Display the continent help panel with the text
                    this.helpManager.helpSent = false;
                    this.helpManager.help2Sent = false;
                    this.helpManager.resetContinentHelpUI();
                    this.helpManager.resetThumbHelpUI();
                    this.helpManager.continentHelpPanel.SetActive(true);
                    this.helpManager.continentUIText.gameObject.SetActive(true);
                    ShowNotification(msg);
                });
            }
        }
        // Active WEBRTC share camera 
        else if (evt.IsEvent(typeof(ActiveCamera)))
        {
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("------ WEBRTC share camera message received: " + evt.value);
            ActiveCamera ac = new ActiveCamera(evt.value);
            if (GetUsername().Equals(ac.user))
            {
                Events.Add(() =>
                {
                    if (ac.user.Equals(this.agentId) && ac.value)
                    {
#if UNITY_ANDROID
                        Dispatch(Channel.Application, new MobileDevice(agentId, true));
#endif
#if UNITY_IOS
                        Dispatch(Channel.Application, new MobileDevice(agentId, false));
#endif
                        // webrtc.StartConnection(this);
                    }
                });
            }
        }
        else if (evt.IsEvent(typeof(TimeoutTurn)))
        {
            TimeoutTurn timeoutTurnMsg = new TimeoutTurn(evt.value);
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("----- TimeoutTurn event received " + timeoutTurnMsg.ToJSON());
            Events.Add(() =>
            {
                // Update UI texts
                InfoHeader.text = "Waiting to the teacher...";
                InfoHeader.color = Color.white;
                InfoUsers.text = "";
                WaitScreenText.text = "Waiting to the teacher...";
                WaitScreenText.color = Color.white;
                WaitScreenText2.text = "";

                helpManager.continentHelpPanel.SetActive(false);
                helpManager.thumbHelpPanel.SetActive(false);
                helpManager.resetContinentHelpUI();
                helpManager.resetThumbHelpUI();
            });
        }
        else if (evt.IsEvent(typeof(PendingAnswer)))
        {
            PendingAnswer pendingAnswerMsg = new PendingAnswer(evt.value);
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("----- PendingAnswer event received " + pendingAnswerMsg.ToJSON());
            Events.Add(() =>
            {
                // Display UI for sending help
                DrawRemoteAnswer(new Answer(pendingAnswerMsg));

                helpManager.help2Sent = false;
                helpManager.resetThumbHelpUI();
                helpManager.enableHelpPanelColor("thumb");
                helpManager.thumbHelpPanel.SetActive(true);
                helpManager.thumbsUIText.gameObject.SetActive(!isPlaying);
            });
        }
        else if (evt.IsEvent((typeof(ScoreMessage))))
        {
            ScoreMessage scoreMessage = new ScoreMessage(evt.value);
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("----- ScoreMessage event received " + scoreMessage.ToJSON());
            if (scoreMessage.name == GetUsername())
            {
                Events.Add(() =>
                {
                    this.updatePointsUI(scoreMessage.score);
                });
            }
        }
        else if (evt.IsEvent((typeof(QuizFinishMessage))))
        {
            QuizFinishMessage quizFinishMessage = new QuizFinishMessage(evt.value);
            if (IgnoreMessage(evt.value)) return;
            Debug.Log("----- QuizFinishMessage event received " + quizFinishMessage.ToJSON());
            Events.Add(() =>
            {
                // Delete all possible pins
                this.deleteScenePins();
                // Reset points to 0
                this.updatePointsUI("0");
                // Hide help UI
                helpManager.continentHelpPanel.SetActive(false);
                helpManager.thumbHelpPanel.SetActive(false);
                helpManager.resetContinentHelpUI();
                helpManager.resetThumbHelpUI();
                // Update UI texts
                InfoHeader.text = "Quiz finished! Now, wait for the teacher";
                InfoHeader.color = Color.green;
                InfoUsers.text = "";
                WaitScreenText.text = "Quiz finished! Now, wait for the teacher";
                WaitScreenText.color = Color.green;
                WaitScreenText2.text = "";
                // Display modal informing that the quiz finished
                ShowNotification(new Notification("Quiz Finished", "Congratulations! You finished the quiz " + quizFinishMessage.quiz + "!"));
            });
        }

        if (stats) stats.Stack = Events.Count;
    }

    void updatePointsUI(string pScore)
    {
        // Leave only 2 decimals in pScore
        pScore = String.Format("{0:0.00}", pScore);
        this.ScoreUI.text = pScore;
        this.ScoreUI2.text = pScore;
    }

    public void SelectObject(Planet target, bool webSync = true)
    {
        if (platform && target)
        {
            if (InfoHeader.color != Color.red)
                InfoHeader.text = string.Format("Viewing: {0}", target.name);

            platform.SetTarget(target);
            if (webSync && UserInteractionTurn())
            {
                string sender = GetUsername();
                Dispatch(Channel.Application, new SelectObject(sender, target.name));
            }
        }
    }

    /// <summary>Called on chat submit message</summary>
    public void SendChatMessage()
    {
        Debug.Log("------ SendChatMessage");
        try
        {
            CloseChat();
            ResetAnswers();
            string user = chatUserList.selectedText.text;
            string message = chat.text;
            if (string.IsNullOrEmpty(message))
            {
                ShowNotification(new Notification("Nothing sent!",
                                                  "Your message was empty so nothing was sent."));
            }
            else if (string.IsNullOrEmpty(user) || user.Equals("None"))
            {
                ShowNotification(new Notification("Nothing sent!",
                                                  "You did not set the user that will receive the message."));
            }
            else if (!IsStudent())
            {
                string sender = GetUsername();

                // Remove previous answer from app state
                DispatchUnset(Channel.Application, typeof(Answer));

                // Set new state
                // Dispatch(Channel.User, new Question(sender, message, activeQuestionId), user);
                // SetShared(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void OpenChat()
    {
        platform.DisableControls();
        UpdateUserList();
        Windows.OpenPanel("Chat");
    }

    public void CloseChat()
    {
        platform.EnableControls();
        Windows.OpenPanel("Home");
        isPlaying = true;
    }

    public void ResetRemoteScene()
    {
        RemoteRotation.localPosition = Vector3.zero;
        RemoteRotation.localRotation = Quaternion.identity;
        RemoteReferenceSystem.localPosition = Vector3.zero;
        RemoteReferenceSystem.localRotation = Quaternion.identity;
        if (platform.HasTarget())
        {
            Transform p = platform.Target().transform;
            p.localPosition = Vector3.zero;
            p.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void NewSession()
    {
        ResetAnswers();
        DispatchUnset(Channel.Application, typeof(Answer));
        DispatchUnset(Channel.Application, typeof(Question));
        DispatchUnset(Channel.Application, typeof(ActiveUser));
        DispatchUnset(Channel.Application, typeof(CameraTransform));
        DispatchUnset(Channel.Application, typeof(Notification));
        DispatchUnset(Channel.Application, typeof(Result));
        DispatchUnset(Channel.Application, typeof(SelectObject));
        DispatchUnset(Channel.Application, typeof(TimeoutTurn));
        DispatchUnset(Channel.Application, typeof(HelpMessage));
        DispatchUnset(Channel.Application, typeof(ScoreMessage));
        DispatchUnset(Channel.Application, typeof(QuizFinishMessage));
    }

    public void StartSession()
    {
        Welcome.SetActive(false);
        WaitScreenText.text = "Please wait for your teacher";
        WaitScreenText.color = Color.white;

#if UNITY_ANDROID || UNITY_IOS
        ResetPlacement();
#endif

        // Enable professor interface, if it applies
        // Otherwise shows a wait notification (app ctx event)
        if (!IsStudent())
        {
            string sender = GetUsername();
            Dispatch(Channel.Application, new ActiveUser(sender, "teacher"));
            PlayTurn(); // First message doest not get processed
        }
        else
        {
            WaitTurn(); // First message does not get processed
        }
    }

    /// <summary>
    ///  When student presses "End turn" button
    /// </summary>
    public void EndTurn()
    {
        string sender = GetUsername();
        Dispatch(Channel.Application, new ActiveUser(sender, "teacher"));
    }

    /// <summary>
    /// Send event to switch to this user
    /// </summary>
    public void StartTurn()
    {
        if (!IsStudent())
        {
            string sender = GetUsername();
            Dispatch(Channel.Application, new ActiveUser(sender, "teacher"));
            // ShowShareButton();
        }
    }

    public void PlayTurn()
    {
        Debug.Log("-----PlayTurn");
#if UNITY_ANDROID || UNITY_IOS

#else
        platform.EnableControls();
#endif

        Windows.OpenPanel("Home");

        bool isTeacher = !IsStudent();
        ChatBtn.SetActive(isTeacher);
        NewSessionBtn.SetActive(isTeacher);
        RegainControl.SetActive(isTeacher);

        // ScoreUI2.gameObject.SetActive(true);
        // ShowShareButton();
        ResetRemoteScene();
        DisableSharedView();
        // webrtc.StartConnection(this);
        ActivateSharedView();
        this.timer.startTimer(OnTurnTimeout);
        // Display the continent help panel
        this.helpManager.helpSent = false;
        this.helpManager.help2Sent = false;
        this.helpManager.resetContinentHelpUI();
        this.helpManager.resetThumbHelpUI();
        this.helpManager.continentHelpPanel.SetActive(true);
        this.helpManager.continentUIText.gameObject.SetActive(false);

        // Delete all possible pins
        Events.Add(() => this.deleteScenePins());
    }

    public void deleteScenePins()
    {
        foreach (GameObject pin in GameObject.FindGameObjectsWithTag("Pin"))
        {
            if (pin.name.Contains("PushPin"))
            {
                Destroy(pin);
            }
        }
    }

    public void WaitTurn()
    {
#if UNITY_ANDROID || UNITY_IOS

#else
        platform.DisableControls();
#endif
        Windows.OpenPanel("WaitTurn");
        ResetRemoteScene();
    }

    public void ValidateAnswer(Answer answer)
    {
        ShowNotification(new Notification("User Answer",
                                          "Answer received from " + Utils.stripPrefix(answer.sender)));
        Dispatch(Channel.Application, new ActiveUser(GetUsername(), "teacher"));
        DrawAnswer(answer);
    }

    public void OnUserAnswerClick(RaycastHit hit)
    {
        // Stop the timer
        this.timer.stopTimer();
        // Create the pin and display the modal for answer cofirmation
        string sender = GetUsername();
        this.pushPin = PushPin.CreateAnswer(Prefab, hit.collider.gameObject);
        lastAnswer = PushPin.getAnswer(sender, this.pushPin, hit.collider.gameObject, hit.point);

        SendAnswer.titleText = "Confirm Answer";
        SendAnswer.descriptionText = "Is this your final answer? If you are not sure you can wait to see what your colleagues think.";
        confirmAnswerWindow = true;

        // Send an orkestra event with the pending answer
        Events.Add(() => Dispatch(Channel.Application, lastAnswer.toPendingAnswer()));

        // Show the thumb help panel
        this.helpManager.thumbHelpPanel.SetActive(true);
        this.helpManager.thumbsUIText.gameObject.SetActive(false);

        SendAnswer.UpdateUI();
        SendAnswer.OpenWindow();
    }

    public void DispatchAnswer(Answer answer)
    {
        Camera camera = Camera.main;
        camera.Render();

        this.timer.resetTimer();
        Events.Add(() => Dispatch(Channel.Application, answer));
    }

    private Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    public void SubmitAnswer(Answer answer)
    {
        WaitScoreModal.titleText = "Answer submited";
        WaitScoreModal.descriptionText = "";
        WaitScoreModal.UpdateUI();
        WaitScoreModal.OpenWindow();

        // Hide all help panels
        this.helpManager.resetContinentHelpUI();
        this.helpManager.resetThumbHelpUI();
        this.helpManager.continentHelpPanel.SetActive(false);
        this.helpManager.thumbHelpPanel.SetActive(false);

        // Clear the pin
        if (this.pushPin)
        {
            Destroy(this.pushPin);
        }
        this.pins.Clear();
    }

    private void DrawAnswer(Answer answer)
    {
        if (answer == null || answer.name == null) return;
        GameObject hitObject = GameObject.Find(answer.name);
        if (hitObject)
        {
            GameObject pushpin = Instantiate(Prefab, answer.OnPlanet(), Quaternion.identity);
            PushPin pin = pushpin.GetComponent<PushPin>();

            pin.answer = answer;
            answer.Apply(hitObject, pin);
            pins.Add(pin);
        }
        else
        {
            ShowNotification(new Notification("System", "Could not find object: " + answer.name));
        }
    }

    private void DrawRemoteAnswer(Answer answer)
    {
        if (answer == null || answer.name == null) return;
        GameObject hitObject = GameObject.Find(answer.name);
        GameObject pushpin = Instantiate(Prefab, answer.OnPlanet(), Quaternion.identity);
        PushPin pin = pushpin.GetComponent<PushPin>();

        pin.answer = answer;
        answer.Apply(hitObject, pin);
        pins.Add(pin);
    }

    public void CreateAnswer(bool accept)
    {
        if (accept)
        {
            if (isPickMode.isOn == true) isPickMode.AnimateSwitch();
            Answer a = new Answer(lastAnswer.ToJSON());
            a.time_left = this.timer.time;
            SubmitAnswer(lastAnswer);

            DispatchAnswer(a);

            // webrtc.StopConnection(); // Stop sending video when submitting the answer
            DisableSharedView(); // Stop the sharing mode
        }
        confirmAnswerWindow = false;
    }

    public void IsAnswerCorrect(bool correct)
    {
        // Send score to user
        if (correct) Dispatch(Channel.User, new Result(GetUsername(), "10"), studentId);
        else Dispatch(Channel.User, new Result(GetUsername(), "0"), studentId);

        // Remove answer from server
        DispatchUnset(Channel.Application, typeof(Answer));
    }

    public void ResetAnswers()
    {
        foreach (var p in pins)
        {
            if (p && p.gameObject)
            {
                Destroy(p.gameObject);
            }
        }
        pins.Clear();
    }

    /// <summary>Ask question received from teacher</summary>
    public void AskQuestion(Question msg)
    {
        Modal.titleText = msg.content;
        Modal.descriptionText = "Assignment from " + Utils.stripPrefix(msg.sender);
        Modal.UpdateUI();
        Modal.OpenWindow();
        ResetAnswers();
    }

    public void AcceptQuestion(bool accept)
    {
        string sender = GetUsername();

        // Remove question from server
        DispatchUnset(Channel.User, typeof(Question), sender);
        if (accept)
        {
            // Notify server that the question has been accepted
            Dispatch(Channel.Application, new Notification(sender, Utils.stripPrefix(sender) + " accepted the new question"));

            // SetShared(true);
            // Dispatch(Channel.Application, new SharedView(sender, true));

            // Send the question accepted statement
            sendStatement(Utils.stripPrefix(GetUsername()), "Accepted", receivedQuestion.id);

            isPlaying = true;
            activeQuestionId = receivedQuestion.id;
            receivedQuestion = null;
            activeUserId = sender;
            InfoHeader.text = "Now playing: You";
            InfoUsers.text = "Question: " + activeQuestionText;
            Events.Add(PlayTurn);

            // Notify the server that the player is playing
            Dispatch(Channel.Application, new ActiveUser(sender, sender));
        }
        else
        {
            // Send the question rejected statement
            sendStatement(Utils.stripPrefix(GetUsername()), "Rejected", receivedQuestion.id);

            // Notify server that the question has been rejected
            Dispatch(Channel.Application, new Notification(sender, Utils.stripPrefix(sender) + " rejected the new question"));
            isPlaying = false;
        }
    }

    /// <summary>
    /// Show a new message popup
    /// </summary>
    public void ShowNotification(Notification msg)
    {
        Notification.title = Utils.stripPrefix(msg.sender);
        Notification.description = Utils.stripPrefix(msg.content);
        Notification.UpdateUI();
        Notification.OpenNotification();
    }

    /// <summary>
    /// Dispatches a help message prepared in the HelpManager script
    /// </summary>
    public void dispatchHelp(HelpMessage msg)
    {
        Dispatch(Channel.Application, msg);
    }

    /// <summary>
    /// Sends a request to generate a xAPI statement with the suggestion information
    /// </summary>
    public void sendHelpStatement(string type, string content)
    {
        string statementData = "";
        if (type == "continent" && activeQuestionCorrectAnswer != null)
        {
            Debug.Log("--- sending continent type help and the correct answer is:");
            Debug.Log(activeQuestionCorrectAnswer);
            string isCorrect = content == activeQuestionCorrectAnswer ? "OK" : "NOK";
            statementData = content + "|" + isCorrect;
            Debug.Log(statementData);
        }
        // Send the answer send statement
        sendStatement(Utils.stripPrefix(GetUsername()), "Suggested", statementData);
    }

    /// <summary>
    /// Asks the user to place the 3D scene. Will call ResetPlacement()
    /// </summary>
    public void InvokePlaceScene()
    {
        Dispatch(Channel.Application, new MobileDevice(agentId, false));
        Events.Add(() =>
        {
            OnPlaceScene?.Invoke();
        });
    }

    /// <summary>
    /// Introduces a delay to avoid imediate scene placement
    /// </summary>
    public void PlaceScene()
    {
        Invoke("InvokePlaceScene", 0.05f);
    }

    public void ActivateSharedView()
    {
        SetShared(true);
        if (UserInteractionTurn())
        {
            Dispatch(Channel.Application, new SharedView(GetUsername(), true));
        }
    }

    public void DisableSharedView()
    {
        SetShared(false);
        if (UserInteractionTurn())
        {
            Dispatch(Channel.Application, new SharedView(GetUsername(), false));
        }
    }

    /// <summary>
    /// Pre-condition: the user is allowed to interact. 
    /// Returns true if the action is to place a marker or false if it is to validate a object
    /// </summary>
    public bool PlaceMarker()
    {
        return IsStudent();
    }

    /// <summary>
    /// The user is allowed to interact if it is her/his turn and is viewing the 3D content
    /// </summary>
    public bool UserInteractionTurn()
    {
        if (!IsStudent() && activeUserId.Equals("teacher")) return true;
        return activeUserId.Equals(GetUsername());
        //return IsPlaying() && IsHome();
    }

    public bool AwaitsPlacement()
    {
        return !IsSceneInPlace;
    }

    public void OnPlacement()
    {
        IsSceneInPlace = true;
        Dispatch(Channel.Application, new Notification(GetUsername(), GetUsername() + "'s AR configuration is ready!"));
        // Send the placed statement
        sendStatement(Utils.stripPrefix(GetUsername()), "Placed", "Earth");
        ShowNotification(new Notification("AR Configuration",
                                          "Your AR space is now properly configured!"));
    }

    public void ResetPlacement()
    {
        IsSceneInPlace = false;
        ShowNotification(new Notification("AR Configuration",
                                          "Tap in a detected surface to place your AR scene!!!"));
    }

    /// <summary>
    /// Hide replace scene button in desktop
    /// </summary>
    public void ShowShareButton()
    {
        ShareBtnHome.SetActive(true);
        ShareBtnWait.SetActive(true);

    }

    /// <summary>
    /// Hide replace scene button in desktop
    /// </summary>
    public void HideShareButton()
    {
        ShareBtnHome.SetActive(false);
        ShareBtnWait.SetActive(false);
    }

    public bool IsPickActive()
    {
        return isPickMode.isOn;
    }

    public void MovePickSwitch(bool active)
    {
        ShowNotification(new Notification(
           !active ? "Rotation Mode" : "Picking Mode",
            !active ? "Use touch events to rotate the planet" :
                     "Use touch events to pick a location"));
    }

    public void sendStatement(string actor, string verb, string definition)
    {
        statementSender._actor = actor;
        statementSender._definition = definition;
        statementSender._verb = verb;
        statementSender.SendStatement();
    }
}
