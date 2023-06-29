using System;
using UnityEngine;
using OrkestraLib.Message;
using OrkestraLib.Plugins;
using static OrkestraLib.Utilities.ParserExtensions;
using OrkestraLib.Exceptions;
using System.Collections;
using System.Text;

namespace OrkestraLib
{
    public class ExampleWithHSocketIO : Orkestra
    {
        static readonly System.Random RandomGenerator = new System.Random();
        private readonly Queue LogQueue = new Queue();
        private string key = "Delete specific key";
        private string roomUI = "ORKESTRA ROOM";
        private string agentIDUI = "ORKESTRA AGENTID";
        private string URLUI = "https://cloud.flexcontrol.net";
        private bool connected;
        private string myLog;
        private Vector2 scrollPosition;

        public static string RandomString(int length)
        {
            StringBuilder strbuilder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                double myFloat = RandomGenerator.NextDouble();
                var myChar = Convert.ToChar(Convert.ToInt32(Math.Floor(25 * myFloat) + 65));
                strbuilder.Append(myChar);
            }
            return strbuilder.ToString().ToLower();
        }

        void OnGUI()
        {
            if (connected)
            {
                GUILayout.BeginArea(new Rect(Screen.width / 2 - 500, Screen.height / 2 - 500, 1000, 1000));
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Debug Messages:");

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(1000), GUILayout.Height(100));
                GUILayout.Label(myLog);
                GUILayout.EndScrollView();
                GUILayout.Label("Connection count " + Users.Count);
                if (GUILayout.Button("Emit Something"))
                {
                    try
                    {
                        Dispatch(Channel.Application, new MyNotification(agentId, "prueba"));
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                if (GUILayout.Button("Emit Something user"))
                {
                    try
                    {
                        Dispatch(Channel.User, new MyNotification(agentId, "HolaSoy" + agentId), agentId);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                if (GUILayout.Button("Emit other user"))
                {
                    try
                    {
                        foreach (string agentid in AppContext.RemoteAgents.Keys)
                        {
                            //Debug.Log("REMOTE USER " + agentid);
                            if (agentid.Equals(agentId))
                            {
                                continue;
                            }
                            //Debug.Log("TO USER " + agentid);
                            Dispatch(Channel.User, new MyNotification(agentId, "HolaSoy" + agentId), agentid);

                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                if (GUILayout.Button("Delete All Keys"))
                {
                    try
                    {
                        RemoveAllKeys();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                key = GUILayout.TextField(key);
                if (GUILayout.Button("Delete Key"))
                {
                    try
                    {
                        if (key.Length != 0)
                            RemoveKey(key);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
                if (GUILayout.Button("Disconnect"))
                {
                    try
                    {
                        Disconnect();
#if UNITY_EDITOR
                        // Application.Quit() does not work in the editor so
                        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndArea();
            }
            else
            {
                GUILayout.BeginArea(new Rect(Screen.width / 2 - 500, Screen.height / 2 - 500, 1000, 1000));
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                roomUI = GUILayout.TextField(roomUI);
                agentIDUI = GUILayout.TextField(agentIDUI);
                URLUI = GUILayout.TextField(URLUI);
                if (GUILayout.Button("Connect"))
                {
                    room = roomUI;
                    url = URLUI;
                    agentId = agentIDUI;
                    connected = true;
                    ConnectOrkestra();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndArea();

            }
        }
        void ConnectOrkestra()
        {
            UserEvents += UserEventSubscriber;
            ApplicationEvents += AppEventSubscriber;

#if UNITY_EDITOR && ORKESTRALIB
                UnityDataViewer.ShowWindow();
#endif
            RegisterEvents(new Type[]{
                typeof(MyNotification),
            });

            // Use Orkestra SDK with SocketIOClient
            OrkestraWithHSIO.Install(this, (a,b) => { });

            try
            {
                // Start Orkestra
                Connect(() =>
                {
                    //Debug.Log("All stuff is ready");
                });
            }
            catch (ServiceException e)
            {
                Debug.LogError(e.Message);
            }

        }
        void Start()
        {
            key = RandomString(5);
            roomUI = RandomString(5);
            agentIDUI = RandomString(5);
        }

        /// <summary>
        /// User subscriber event
        /// </summary>
        void UserEventSubscriber(object sender, UserEvent evt)
        {
            //Debug.Log("UserEventSubscriber(" + evt.ToJSON() + ")");
            HandleLog("UserEventSubscriber(" + evt.ToJSON() + ")", "", LogType.Log);
        }

        /// <summary>
        /// App subscriber event
        /// </summary>
        void AppEventSubscriber(object sender, ApplicationEvent evt)
        {
            //Debug.Log("AppEventSubscriber(" + evt.ToJSON() + ")");
            HandleLog("AppEventSubscriber(" + evt.ToJSON() + ")", "", LogType.Log);
        }

        // Called when there is an exception
        void HandleException(string condition, string stackTrace, LogType type)
        {
            Debug.LogError(string.Format("{0}\n{1}", condition, stackTrace));
            HandleLog(condition, stackTrace, type);
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleException;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleException;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            myLog = logString;
            string newString = "\n [" + type + "] : " + myLog;
            LogQueue.Enqueue(newString);
            if (type == LogType.Exception)
            {
                newString = "\n" + stackTrace;
                LogQueue.Enqueue(newString);
            }
            myLog = string.Empty;
            foreach (string mylog in LogQueue)
            {
                myLog += mylog;
            }
        }
    }
}
