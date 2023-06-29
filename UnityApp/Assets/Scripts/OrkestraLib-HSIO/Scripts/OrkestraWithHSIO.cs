using H.Socket.IO;
using OrkestraLib.Message;
using OrkestraLib.Network;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using H.Socket.IO.Utilities;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Plugins
    {
        public class OrkestraWithHSIO : IWebSocketAdapter
        {
            // Pool with all websocket connections created
            public static readonly List<OrkestraWithHSIO> Connections = new List<OrkestraWithHSIO>();

            private readonly ConcurrentDictionary<string, Action<string>> HandledEvents;

            public string ServiceUrl { get; private set; }

            public static SocketIoClient Socket { get; set; }

            public Action<string, string> DebugOn;
            public bool isConnected;

            //List with messages to send
            private List<string> MessagesToSend = new List<string>();

            //Flag to end thread in disconnect
            private bool StopSendMessages = false;

            /**
             * [isError, message]
             */
            public Action<bool, string> onNetworkEvents;

            public OrkestraWithHSIO()
            {
                HandledEvents = new ConcurrentDictionary<string, Action<string>>();
                DebugOn = null;
                isConnected = false;

                Task.Run(async () =>
                {
                    int attempts = 0;
                    while (!StopSendMessages)
                    {
                        while (MessagesToSend.Count > 0)
                        {
                            // Ensure we send all messages in list                                
                            try
                            {
                                if (Socket.EngineIoClient.IsOpened)
                                {
                                    string msg = MessagesToSend.First();
                                    if (msg != null && attempts++ < 3)
                                    {
                                        await Socket.SendEventAsync(msg.RemoveNonUnicodeLetters()).ConfigureAwait(true);
                                    }

                                    if (MessagesToSend.Count > 0)
                                    {
                                        MessagesToSend.RemoveAt(0);
                                        attempts = 0;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e);
                            }
                        }
                    }
                });
            }

            public async void Connect(string serverUrl,
                                      Action<IWebSocketAdapter> config,
                                      Action<bool> onConnect)
            {
                isConnected = false;
                Debug.Log("Connecting to " + serverUrl);
                if (string.IsNullOrEmpty(serverUrl)) throw new Exception("serverUrl missing");
                ServiceUrl = serverUrl;

                SocketIoClient createSocket(OrkestraWithHSIO that)
                {
                    SocketIoClient Socket = new SocketIoClient();
                    Socket.Connected += (sender, args) => { };//Debug.LogWarning($"Connected: {args.Namespace} Value: {args.Value}");
                    Socket.Disconnected += (sender, args) =>
                    {
                        onNetworkEvents?.Invoke(true, args.Reason);
                    };

                    Socket.EventReceived += (sender, args) => { };// Debug.LogWarning($"EventReceived: Namespace: {args.Namespace}, Value: {args.Value}, IsHandled: {args.IsHandled}");
                    Socket.HandledEventReceived += (sender, args) => { };// Debug.LogWarning($"HandledEventReceived: Namespace: {args.Namespace}, Value: {args.Value}");
                    Socket.UnhandledEventReceived += (sender, args) =>
                    {
                        try
                        {
                            var values = args.Value.GetJsonArrayValues();
                            var name = values.ElementAt(0);
                            var text = values.ElementAtOrDefault(1);
                            if (HandledEvents.ContainsKey(name))
                            {
                                if (name.Equals("joined")) text = "[" + text + "]";
                                DebugOn?.Invoke(name, text);
                                HandledEvents[name](text);
                            }
                            else if (name.Equals("connected"))
                            {
                                if (!isConnected)
                                {
                                    Debug.Log("Connected to " + serverUrl);
                                    onNetworkEvents?.Invoke(false, "Connected! Waiting configuration...");
                                    isConnected = true;
                                    onConnect?.Invoke(isConnected);
                                }
                            }
                            else Debug.LogWarning("Unhandled (" + name + ") ");
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message == "no connection" ? "No Internet Connection?" : e.Message;
                            onNetworkEvents?.Invoke(false, msg);
                            Debug.LogError(e);
                        }
                    };

                    Socket.ErrorReceived += (sender, args) => Debug.LogWarning($"ErrorReceived: Namespace: {args.Namespace}, Value: {args.Value}");
                    Socket.ExceptionOccurred += (sender, args) => Debug.LogWarning($"ExceptionOccurred: {args.Value}");
                    Socket.On("error", (e) =>
                    {
                        Debug.LogError("Connection error " + serverUrl + ":" + e);
                        isConnected = false;
                        onConnect?.Invoke(isConnected);
                    });
                    return Socket;
                }

                try
                {
                    // Remove trailing /
                    if (serverUrl.EndsWith("/")) serverUrl = serverUrl.Substring(0, serverUrl.LastIndexOf("/"));

                    // Only supports tld http(s)://XXXX/
                    if (serverUrl.LastIndexOf("/") > 8)
                    {
                        Socket = createSocket(this);
                        config?.Invoke(this);
                        string url = serverUrl.Substring(0, serverUrl.LastIndexOf("/"));
                        string nmspace = serverUrl.Substring(serverUrl.LastIndexOf("/") + 1);
                        var uri = new Uri(uriString: url);
                        Socket.DefaultNamespace = nmspace;
                        await Socket.ConnectAsync(uri, namespaces: new string[] { nmspace });
                    }
                    else
                    {
                        Socket = createSocket(this);
                        config?.Invoke(this);

                        var uri = new Uri(uriString: serverUrl);
                        await Socket.ConnectAsync(uri);
                    }
                }
                catch (WebSocketException e1)
                {
                    onNetworkEvents?.Invoke(true, e1.Message);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            public async void Disconnect()
            {
                await Socket.DisconnectAsync();
                isConnected = false;
            }

            public void Emit(string evtKey, IPacket data)
            {
                if (Socket == null) throw new Exception("Socket.Connect() required");

                try
                {
                    string value = data.ToJSON();
                    Emit(evtKey, value);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            public void Emit(string evtKey, string data)
            {
                if (Socket == null) throw new Exception("Socket.Connect() required");

                string msg = $"[\"{evtKey}\", {data}]";

                //add the message to the list
                MessagesToSend.Add(msg);
            }

            public void Emit(string evtKey, object data)
            {
                if (Socket == null) throw new Exception("Socket.Connect() required");
                try
                {
                    //Debug.Log("Sending object " + data);
                    _ = Socket.Emit(evtKey, data);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            public void On(string eventName, Action<string> callback)
            {
                if (!HandledEvents.ContainsKey(eventName)) HandledEvents.TryAdd(eventName, callback);
            }

            public string GetURL()
            {
                return ServiceUrl;
            }

            public bool IsConnected()
            {
                return isConnected;
            }

            public void Connect(string url)
            {
                Connect(url, null, null);
            }

            /// <summary>
            /// This function instantiates orkestra's OpenSocket and CloseSocket
            /// </summary>
            public static void Install(IOrkestra orkestra, Action<bool, string> onNetworkEvents)
            {
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    //Debug.Log("Setting up certificates");
                    if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None) return true;
                    else Debug.LogError(sslPolicyErrors);
                    return false;
                };

                orkestra.OpenSocket = delegate (string id, string url, Action<IWebSocketAdapter> config, Action<bool> onConnect)
                {
                    var conn = new OrkestraWithHSIO();
                    conn.onNetworkEvents = onNetworkEvents;
                    conn.Connect(url, config, onConnect);
                    Connections.Add(conn);
                };

                orkestra.CloseSocket = delegate (IWebSocketAdapter socket)
                {
                    //Debug.Log("Socket disconnect requested");
                    if (socket == null) return;
                    OrkestraWithHSIO s = socket as OrkestraWithHSIO;
                    try
                    {
                        Connections.Remove(s);
                        socket.Disconnect();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                };
            }
        }
    }
}
