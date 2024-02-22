using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using United.Utility.Helper;

namespace United.Mobile.Test.MSCRegister.Api.Tests
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


        public static IEnumerable<object[]> RegisterOffers_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set1();

        }

        public static IEnumerable<object[]> RegisterOffers_Test1()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set5();

        }
        public static IEnumerable<object[]> RegisterBags_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set2();

        }
        public static IEnumerable<object[]> RegisterSameDayChange_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set3();

        }
        public static IEnumerable<object[]> RegisterCheckinSeats_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set4();

        }
        public static IEnumerable<object[]> RegisterOffers_Flow()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set1_1();

        }
    }
}
