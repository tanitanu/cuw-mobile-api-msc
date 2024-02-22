using Microsoft.Extensions.Configuration;
using System;
using United.Mobile.DataAccess.Common;

namespace United.Mobile.DataAccess.DynamoDB
{
    public class DeviceDynamDB
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;

        public DeviceDynamDB(IConfiguration configuration
            , IDynamoDBService dynamoDBService)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
        }


        public async System.Threading.Tasks.Task<bool> ValidateDeviceIDAPPID(string deviceId, int applicationId, string mpNumber, string sessionId)
        {
            #region
            /* //Database database = DatabaseFactory.CreateDatabase("ConnectionString - iPhone");
            //DbCommand dbCommand = (DbCommand)database.GetStoredProcCommand("usp_Validate_DeviceID_ApplicatinID");
            //database.AddInParameter(dbCommand, "@DeviceID", DbType.String, deviceId);
            //database.AddInParameter(dbCommand, "@ApplicationId", DbType.Int32, applicationId);
            ////database.ExecuteNonQuery(dbCommand);
            //using (IDataReader dataReader = database.ExecuteReader(dbCommand))
            //{
            //    while (dataReader.Read())
            //    {
            //        if (Convert.ToInt32(dataReader["DeviceCount"]) > 0)
            //        {
            //            ok = true;
            //        }
            //    }
            //}*/
            #endregion
            try
            {
                //need to verify the key while testing
                string key = string.Format("{0}::{1}", deviceId, applicationId);
                string tableName = _configuration.GetSection("DynamoDBTables").GetValue<string>("uatb_Device");
                int DeviceCount = await _dynamoDBService.GetRecords<int>(tableName, "Device001", key, sessionId).ConfigureAwait(false);
                if (DeviceCount > 0)
                {
                    return true;
                }
            }
            catch { }
            return false;

        }
    }
}
