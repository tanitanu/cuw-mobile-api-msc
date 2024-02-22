using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using United.Utility.Helper;

namespace United.Mobile.Test.Product.Api.Tests
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

        public static IEnumerable<object[]> RegisterOffersForBooking_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set1();

        }
        public static IEnumerable<object[]> RegisterOffersForReshop_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set2();

        }
        public static IEnumerable<object[]> RegisterSeatsForBooking_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set3();

        }
        public static IEnumerable<object[]> RegisterSeatsForReshop_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set4();

        }

        public static IEnumerable<object[]> RemoveAncillaryOffer_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set5();

        }
        public static IEnumerable<object[]> ClearCartOnBackNavigation_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set6();

        }
        public static IEnumerable<object[]> RegisterOffersForOmniCartSavedTrip_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set7();

        }
        public static IEnumerable<object[]> UnRegisterAncillaryOffersForBooking_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set8();

        }
        public static IEnumerable<object[]> RegisterSeatsForReshop_Flow()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set9();

        }
        public static IEnumerable<object[]> RegisterOffersForReshop_Flow()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set10();

        }
    }
}

