using System;
using UnityEngine;
using System.Diagnostics;
using OrkestraLib.Message;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace OrkestraLib
{
    namespace Utilities
    {
        public static class ParserExtensions
        {
            public static string InitializationMessage = "undefined";
            public static bool EnableParserDebugger = false;
            static readonly Regex nonUnicodeRx = new Regex("[^\\p{L}\\p{N}\\p{P}\\p{S} ]");

            public static string[] SplitByIndexes(this string text, int[] indexes)
            {
                text = text ?? throw new ArgumentNullException(nameof(text));
                indexes = indexes ?? throw new ArgumentNullException(nameof(indexes));

                var values = new List<string>();
                var lastIndex = 0;
                foreach (var index in indexes)
                {
                    values.Add(text.Substring(lastIndex, index - lastIndex));

                    lastIndex = index + 1;
                }

                values.Add(text.Substring(lastIndex));

                return values.ToArray();
            }

            public static string[] GetArrayValues(this string text)
            {
                text = text ?? throw new ArgumentNullException(nameof(text));
                if (!text.StartsWith("[", StringComparison.OrdinalIgnoreCase) || !text.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("text must begin with '[' and end with ']'");
                }

                text = text.Substring(1, text.Length - 2);

                var indexes = new List<int>();
                var countOfObjects = 0;
                var countOfArrays = 0;
                for (var i = 0; i < text.Length; i++)
                {
                    switch (text[i])
                    {
                        case '{':
                            countOfObjects++;
                            break;

                        case '[':
                            countOfArrays++;
                            break;

                        case '}':
                            countOfObjects--;
                            break;

                        case ']':
                            countOfArrays--;
                            break;

                        case ',' when countOfObjects == 0 && countOfArrays == 0:
                            indexes.Add(i);
                            break;
                    }
                }

                return text
                    .SplitByIndexes(indexes.ToArray())
                    .Select(value => value.Trim('\"'))
                    .ToArray();
            }

            public static string GetArrayValue(this string text)
            {
                if (text.StartsWith("[")) return text.GetArrayValues()[0].FixJSON();
                return text.FixJSON();
            }

            public static string RemoveNonUnicodeLetters(this string input)
            {
                return nonUnicodeRx.Replace(input, "");
            }

            public static bool IsSystemInitMessage(this string msg)
            {
                return msg.Equals(InitializationMessage) ||
                       msg.Equals("\"" + InitializationMessage + "\"") ||
                       msg.Equals("null");
            }

            [Serializable]
            private class Wrapper<T>
            {
                public T[] Items;
            }

            public static string FixJSON(this string json)
            {
                string r = json.Replace("\"event\"", "\"evt\"");
                if (r.StartsWith("\"{\\\""))
                {
                    r = json.Replace("\\", "");
                    r = r.Substring(1, r.Length - 2);
                }

                return r;
            }

            public static T[] FromJsonList<T>(this string json)
            {

                if (json.StartsWith("["))
                {
                    json = json.FixJSON();
                    //if (!json.Contains("_Result"))
                        json = FixJavascriptJson(json);

                    string fixJSON = "{\"Items\":" + json.FixJSON() + "}";
                    
                    Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(fixJSON);
                    return wrapper.Items;
                }
                return new T[] { JsonUtility.FromJson<T>(json.FixJSON()) };
            }

            private static string FixJavascriptJson(string json)
            {
               
                Queue<bool> llaves = new Queue<bool>();
                int indexOfValue = 0;

                while ((indexOfValue = json.IndexOf("value", indexOfValue)) != -1)
                {
                   
                   // !json[indexOfValue + 7].Equals('"') && !json[indexOfValue + 7].Equals(':')
                    if (json[indexOfValue + 7].Equals('{'))
                    {
                        json = json.Insert(indexOfValue + 7, "\"");
                        for (int i = indexOfValue + 8; i < json.Length; i++)
                        {
                            if (json[i].Equals('\"'))
                            {                                
                                json = json.Remove(i, 1);
                                json = json.Insert(i, "\\\"");
                                i += 2;
                            }
                            if (json[i].Equals('{') || json[i].Equals('['))
                            {
                                llaves.Enqueue(true);
                            }
                            else if (json[i].Equals('}') || json[i].Equals(']'))
                            {
                                if (llaves.Count > 0)
                                {
                                    llaves.Dequeue();
                                }
                                else
                                {
                                    json = json.Insert(i, "\"");
                                    indexOfValue = i;
                                    break;
                                }
                            }

                        }

                    }
                    else
                        indexOfValue += 8;
                }

                return json;
            }
            public static string ToJson<T>(this T[] array, bool prettyPrint = false)
            {
                return JsonUtility.ToJson(new Wrapper<T>
                {
                    Items = array
                }, prettyPrint);
            }

            public static string ToJSON(this IPacket packet)
            {
                string json = JsonUtility.ToJson(packet);
                return json.Replace("\"evt\"", "\"event\"");
            }

            public static void Print(this IPacket packet)
            {
                try
                {
                    // Get calling method name
                    StackTrace stackTrace = new StackTrace();
                    string className = stackTrace.GetFrame(3).GetMethod().DeclaringType.FullName;
                    string methodName = stackTrace.GetFrame(3).GetMethod().Name;
                    //if (EnableParserDebugger) UnityEngine.Debug.LogWarning("[" + className + "." + methodName + "] " + packet.ToJSON());
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e);
                }
            }

            public static void InstantiateWithJSON(this IPacket packet, string json, bool showDefWarning = true)
            {
                string fixJSON = json.FixJSON();
                try
                {
                    JsonUtility.FromJsonOverwrite(fixJSON, packet);
                    if (EnableParserDebugger)
                    {
                        /*if (showDefWarning && !fixJSON.Equals(packet.ToJSON()))
                        {
                            UnityEngine.Debug.LogWarning("Packet not defined properly: " + json);
                        }*/
                        packet.Print();
                    }
                }
                catch (Exception)
                {
                    UnityEngine.Debug.LogError("Cannot create message with " + fixJSON);
                }
            }
        }
    }
}
