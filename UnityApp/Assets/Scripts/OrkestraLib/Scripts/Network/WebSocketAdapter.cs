using OrkestraLib.Message;
using System;

namespace OrkestraLib.Network
{
    public interface IWebSocketAdapter
    {
        public string GetURL();

        /// <exception cref="ServiceException">Thrown when there is a connection problem</exception>
        public void Connect(string url);

        public void Connect(string serverUrl,
                                      Action<IWebSocketAdapter> config,
                                      Action<bool> onConnect);

        public bool IsConnected();

        public void Disconnect();

        public void On(string eventName, Action<string> callback);

        public void Emit(string evtKey, string data);

        public void Emit(string evtKey, IPacket data);
    }
}
