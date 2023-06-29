using OrkestraLib.Network;
using System;
using UnityEngine;

/// <summary>
/// Interface for the OrkestraLib main entry point
/// </summary>
public interface IOrkestra
{
    /// <summary>
    /// User-defined function to open a websocket with arbitrary libraries
    /// </summary>
    Action<string, string, Action<IWebSocketAdapter>, Action<bool>> OpenSocket { get; set; }

    /// <summary>
    /// User-defined function to close a websocket.
    /// This function is implemented by the application main controller
    /// </summary>
    Action<IWebSocketAdapter> CloseSocket { get; set; }

    string GetURL();

    GameObject GetGameObject();
}
