using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace United.Mobile.Test.ETC.Api.Tests
{
    public class TestDataGenarator
    {
        public static string GetFileContent(string fileName)
        {
            fileName = string.Format("..\\..\\..\\TestData\\{0}", fileName);
            var path = Path.IsPathRooted(fileName) ? fileName : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }
        public static IEnumerable<object[]> ETC_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set1();



        }
        public static IEnumerable<object[]> ETC1_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set6();



        }


        public static IEnumerable<object[]> ETC_Test1()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set2();

        }

        public static IEnumerable<object[]> ETC_Test2()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set3();



        }
        public static IEnumerable<object[]> ETC2_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set4();



        }
        public static IEnumerable<object[]> ETC3_Test1()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set5();

        }
        public static IEnumerable<object[]> ETC_Negative_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set1();



        }
        public static IEnumerable<object[]> ETC_Flow()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set2_1();

        }
        public static IEnumerable<object[]> ETC4_Flow()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set1_1();



        }
    }
}

