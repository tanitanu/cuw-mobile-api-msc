using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using United.Utility.Helper;

namespace United.Mobile.MoneyPlusMiles.Api.Tests
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

        public static IEnumerable<object[]> GetMoneyPlusMiles_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set1();
        }

        public static IEnumerable<object[]> ApplyMoneyPlusMiles_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set2();
        }
    }
}
