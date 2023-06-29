using System;

namespace OrkestraLib
{
    public class InstrumentMap
    {
        private readonly Func<object, string> Init;
        private readonly Func<object, string> On;
        private readonly Func<object, string> Off;

        public string Value { get; set; }

        public InstrumentMap(string value)
        {
            Init = (arg) => null;
            On = (arg) => null;
            Off = (arg) => null;
            Value = value;
        }

        public InstrumentMap(string value, 
                             Func<object, string> init, 
                             Func<object, string> on,
                             Func<object, string> off)
        {
            Init = init;
            On = on;
            Off = off;
            Value = value;
        }

        public void InitEvent(object value)
        {
            Init?.Invoke(value);
        }

        public void OnEvent(object value)
        {
            On?.Invoke(value);
        }

        public void OffEvent(object value)
        {
            Off?.Invoke(value);
        }
    }
}