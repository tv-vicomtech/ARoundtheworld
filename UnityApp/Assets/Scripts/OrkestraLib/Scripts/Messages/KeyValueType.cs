using System;

namespace OrkestraLib
{
    namespace Message
    {

        [Serializable]
        public class KeyTypeValue : KeyValue, IKeyValue
        {
            public string type;

            public KeyTypeValue(string json) : base(json) 
            {
            }

            public KeyTypeValue(string key, string type, string value = "[]") : base(key, value)
            {
                this.type = type;
            }

            public bool IsMetaKey()
            {
                return key.IndexOf(Constants.META_VARIABLE) > -1;
            }

            public string GetMetaKey() 
            {
                return key.Substring(8);
            }

            public bool IsMetaSubKey()
            {
                return key.IndexOf(Constants.META_VARIABLE) > -1;
            }

            public bool IsVarKey()
            {
                return key.IndexOf(Constants.DATA_VARIABLE) > -1;
            }

            public string GetMetaSubKey()
            {
                return key.Substring(11);
            }

            public bool IsGlobalKey()
            {
                return key.IndexOf(Constants.GLOBAL_VARIABLE) > -1;
            }

            public string GetGlobalKey()
            {
                return key.Substring(10);
            }

            public override string FriendlyName()
            {
                return typeof(KeyTypeValue).Name;
            }
        }
    }
}

