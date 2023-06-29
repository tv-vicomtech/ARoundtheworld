using System.Collections.Generic;
using System;

namespace OrkestraLib
{
    public class ContextItem
    {
        public List<Action<string>> EventActions;
        private InstrumentMap Map;
        public string CurrentValue;

        public ContextItem(InstrumentMap instrumentMap)
        {
            CurrentValue = instrumentMap.Value;
            EventActions = new List<Action<string>>();
            Map = instrumentMap;
        }

        public void OnEvent(object value)
        {
            Map?.OnEvent(value);
        }
    }
}
