using OrkestraLib.Network;
using System;
using UnityEngine;

namespace Tests
{
    public class OrkestraMock : IOrkestra
    {
        public Action<string, string, Action<IWebSocketAdapter>, Action<bool>> OpenSocket { get; set; }

        public Action<IWebSocketAdapter> CloseSocket { get; set; }

        public string GetURL() { return "dummy"; }

        public GameObject GetGameObject() { throw new NotImplementedException(); }
    }
}