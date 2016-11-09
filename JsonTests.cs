using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniJson;

namespace UnitTests
{
    [TestClass]
    public class JsonTests
    {
        [TestMethod]
        public void TestParseString()
        {
            Assert.AreEqual("asdf\ubeef\r\n", Json.Parse("\"asdf\\ubeef\\r\\n\""));
            Assert.AreEqual("\"", Json.Parse("\"\\\"\" "));
            Assert.AreEqual("", Json.Parse("\"\""));
        }

        [TestMethod]
        public void TestParseDouble()
        {
            Assert.AreEqual(0.12345, (double)Json.Parse("0.12345"));
            Assert.AreEqual(3.0, (double)Json.Parse(" 3 "));
            Assert.AreEqual(0.001, (double)Json.Parse("  1e-3"));
            Assert.AreEqual(1000.0, (double)Json.Parse("1e+3 "));
            Assert.AreEqual(1000.0, (double)Json.Parse("1e3"));
        }

        [TestMethod]
        public void TestParseArray()
        {
            Assert.AreEqual(0, ((List<object>)Json.Parse("[]")).Count);
            Assert.IsTrue(new List<object> { 1.0, 2.0, 3.0 }.SequenceEqual((List<object>)Json.Parse("[1 , 2,3]")));
        }

        [TestMethod]
        public void TestParseObject()
        {
            Assert.AreEqual(0, ((Dictionary<string, object>)Json.Parse("{}")).Count);
            var target = new Dictionary<string, object>
            {
                {"asdf", 4.0},
                {"qwer", "ooo"},
            };
            var result = (Dictionary<string, object>)Json.Parse("{\"asdf\":  4,\t\"qwer\"\t\t:\"ooo\"}");
            Assert.AreEqual(target.Count, result.Count);
            foreach (var kv in target)
            {
                Assert.AreEqual(kv.Value, result[kv.Key]);
            }
        }

        [TestMethod]
        public void TestParseTrueFalseNull()
        {
            Assert.AreEqual(true, Json.Parse("true"));
            Assert.AreEqual(false, Json.Parse("false"));
            Assert.AreEqual(null, Json.Parse("null"));
        }

        [TestMethod]
        public void TestStringifyString()
        {
            Assert.AreEqual("\"asdf\\r\\n\"", Json.Stringify("asdf\r\n"));
            Assert.AreEqual("\"\\\"\"", Json.Stringify("\""));
        }

        private void AssertStringifyFails(object obj)
        {
            try
            {
                Assert.Fail(Json.Stringify(obj));
            }
            catch (ArgumentException)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestStringifyDouble()
        {
            Assert.AreEqual("3", Json.Stringify(3.0));
            Assert.AreEqual("0.001", Json.Stringify(0.001));
            Assert.AreEqual("1000", Json.Stringify(1000.0));
            AssertStringifyFails(double.NaN);
            AssertStringifyFails(double.PositiveInfinity);
            AssertStringifyFails(double.NegativeInfinity);
        }

        [TestMethod]
        public void TestStringifyArray()
        {
            Assert.AreEqual("[1,2,3]", Json.Stringify(new List<object> { 1.0, 2.0, 3.0 }));
        }

        [TestMethod]
        public void TestStringifyObject()
        {
            var result = Json.Stringify(new Dictionary<string, object>
            {
                {"asdf", 4.0},
                {"qwer", "ooo"},
            });
            Assert.IsTrue(result == "{\"asdf\":4,\"qwer\":\"ooo\"}" || result == "{\"qwer\":\"ooo\",\"asdf\":4}");
        }

        [TestMethod]
        public void TestStringifyTrueFalseNull()
        {
            Assert.AreEqual("true", Json.Stringify(true));
            Assert.AreEqual("false", Json.Stringify(false));
            Assert.AreEqual("null", Json.Stringify(null));
        }

        [TestMethod]
        public void TestNested()
        {
            Assert.AreEqual("{\"\":[[],[1,{\"a\":2},[3,4]]]}", Json.Stringify(new Dictionary<string, object>{
                {"", new List<object>{
                    new List<object>(),
                    new List<object>{
                        1.0,
                        new Dictionary<string, object>{
                            {"a", 2.0}
                        },
                        new List<object>{ 3.0, 4.0 }
                    },
                }}
            }));
        }

        [TestMethod]
        public void TestEmpty()
        {
            try
            {
                Assert.Fail(Json.Parse("").ToString());
            }
            catch (FormatException)
            {
                Assert.IsTrue(true);
            }
        }
    }
}
