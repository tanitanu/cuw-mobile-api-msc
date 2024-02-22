using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using United.Utility.Helper;

namespace United.Mobile.Test.MSCCheckOut.Api.Tests
{
    public class TestDataGenerator
    {
        public static T GetXmlData<T>(string filename)
        {



            var persistedReservation1Json = TestDataGenerator.GetFileContent(filename);
            return XmlSerializerHelper.Deserialize<T>(persistedReservation1Json);



        }
        public static string GetFileContent(string fileName)
        {
            fileName = string.Format("..\\..\\..\\TestData\\{0}", fileName);
            var path = Path.IsPathRooted(fileName) ? fileName : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }


        public static IEnumerable<object[]> CheckOut_Test()
        {
            TestDataSet testDataSet = new TestDataSet();

            yield return testDataSet.Set1();

        }

        public static IEnumerable<object[]> CheckOut_Test1()
        {
            TestDataSet testDataSet = new TestDataSet();

            yield return testDataSet.Set2();

        }
        public static IEnumerable<object[]> CheckOut_Test2()
        {
            TestDataSet testDataSet = new TestDataSet();

            yield return testDataSet.Set3();

        }
        public static IEnumerable<object[]> CheckOut_Flow()
        {
            TestDataSet testDataSet = new TestDataSet();

            yield return testDataSet.Set1_1();

        }
    }
}


