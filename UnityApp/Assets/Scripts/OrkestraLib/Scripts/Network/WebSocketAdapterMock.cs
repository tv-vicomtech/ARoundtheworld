using System;
using OrkestraLib.Message;
using UnityEngine;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib.Network
{
    public class WebSocketAdapterMock : IWebSocketAdapter
    {
        private string url;
        public int requests;

        public WebSocketAdapterMock()
        {
            requests = 0;
        }

        public void Connect(string url)
        {
            this.url = url;
        }

        public void Connect(string url, Action<IWebSocketAdapter> config, Action<bool> onConnect)
        {
            this.url = url;
        }

        public void Disconnect()
        {
            requests = 0;
        }

        public void Emit(string evtKey, string data)
        {
            requests++;
        }

        public string GetURL()
        {
            return url;
        }

        public void On(string evtKey, Action<string> callback)
        {
        }

        public bool IsConnected()
        {
            return true;
        }

        public void Emit(string evtKey, IPacket data)
        {
            Emit(evtKey, data.ToJSON());
        }

    }
}
