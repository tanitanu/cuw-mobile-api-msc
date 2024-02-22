using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
//using United.Mobile.Test.InFlightPurchase.Tests;

namespace United.Mobile.Test.InFlightPurchase.Api.Tests
{
    public class TestDataGenerator
    {
        public static string GetFileContent(string fileName)
        {
            fileName = string.Format("..\\..\\..\\TestData\\{0}", fileName);
            var path = Path.IsPathRooted(fileName) ? fileName : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }

        public static IEnumerable<object[]> InFlightPurchase_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set1();

        }
        public static IEnumerable<object[]> InFlightPurchase_Test1()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set2();

        }
        public static IEnumerable<object[]> InFlightPurchase_Test2()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set3();

        }




    }
}
