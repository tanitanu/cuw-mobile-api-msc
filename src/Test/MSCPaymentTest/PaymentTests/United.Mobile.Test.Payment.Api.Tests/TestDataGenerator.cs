using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using United.Utility.Helper;

namespace United.Mobile.Test.Payment.Api.Tests
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


            public static IEnumerable<object[]> GetPaymentToken_Test()
            {
                TestDataSet testDataSet = new TestDataSet();
                yield return testDataSet.Set1();

            }

             public static IEnumerable<object[]> GetPaymentToken_Test1()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set1_1();

        }

             public static IEnumerable<object[]> GetPaymentToken_Test1_1()
             {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.set1_2();

             }

             public static IEnumerable<object[]> PersistFormofPaymentDetails_Test()
            {
                TestDataSet testDataSet = new TestDataSet();
                yield return testDataSet.Set2();

            }
            public static IEnumerable<object[]> GetCreditCardToken_Test()
            {
                TestDataSet testDataSet = new TestDataSet();
                yield return testDataSet.Set3();

            }
            public static IEnumerable<object[]> GetCartInformation_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set4();

        }
            public static IEnumerable<object[]> TravelBankCredit_Test()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set5();

        }
        public static IEnumerable<object[]> GetPaymentToken_Flow()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set1_3();

        }
        public static IEnumerable<object[]> PersistFormofPaymentDetails_Flow()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set2_1();

        }
        public static IEnumerable<object[]> GetCreditCardToken_Flow()
        {
            TestDataSet testDataSet = new TestDataSet();
            yield return testDataSet.Set3_1();

        }
    }
    }


