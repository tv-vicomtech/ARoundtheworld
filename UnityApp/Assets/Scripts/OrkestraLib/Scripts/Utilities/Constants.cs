using System;

namespace OrkestraLib
{
    class Constants
    {
        public static string GLOBAL_VARIABLE = "__global__";
        public static string META_VARIABLE = "__meta__";
        public static string META_SUB_VARIABLE = "__metasub__";
        public static string DATA_VARIABLE = "__val__";

        public static string GetGlobalVariable(string key)
        {
            return GLOBAL_VARIABLE + key;
        }

        public static string GetMetaVariable(string key)
        {
            return META_VARIABLE + key;
        }

        public static string GetMetaSubVariable(string key)
        {
            return META_SUB_VARIABLE + key;
        }

        public static string GetVariable(string key)
        {
            return DATA_VARIABLE + key;
        }
    }
}
