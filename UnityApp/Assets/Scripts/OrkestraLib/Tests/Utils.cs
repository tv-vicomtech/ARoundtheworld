using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System.Linq;
using System;
using OrkestraLib.Utilities;
using static OrkestraLib.Utilities.StringExtensions;

namespace Tests
{
    [TestFixture]
    public class Utils
    {

        [Test]
        public void IsCharCorrect()
        {
            var stringToCheck = StringExtensions.RandomString(5);
            Assert.That(stringToCheck.Length, Is.EqualTo(5));
            var allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var result = stringToCheck.All(allowedChars.Contains);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void CheckSpliceString()
        {
            List<string> auxList = new List<string>() { "a", "b", "c", "d", "e" };
            var result = auxList.Splice(2, 2);
            Assert.That(result, Is.EqualTo(new List<string>() { "c", "d" }));
            Assert.That(auxList, Is.EqualTo(new List<string>() { "a", "b", "e" }));
        }

        [Test]
        public void CheckSpliceActions()
        {
            void func1(string a) { Debug.Log(a); };
            void func2(string b) { Debug.Log(b); };
            void func3(string c) { Debug.Log(c); };
            void func4(string d) { Debug.Log(d); };
            void func5(string d) { Debug.Log(d); };

            List<Action<string>> auxList = new List<Action<string>>() { func1, func2, func3, func4, func5 };
            var result = auxList.Splice(2, 2);
            Assert.That(result, Is.EqualTo(new List<Action<string>>() { func3, func4 }));
            Assert.That(auxList, Is.EqualTo(new List<Action<string>>() { func1, func2, func5 }));
        }
    }
}
