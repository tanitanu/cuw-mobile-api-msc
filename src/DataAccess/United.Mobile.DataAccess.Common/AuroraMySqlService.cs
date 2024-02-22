using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.Common;
using United.Mobile.Model.DynamoDb.Common;
using United.Utility.Helper;

namespace United.Mobile.DataAccess.Common
{
    public class AuroraMySqlService : IAuroraMySqlService
    {
        private readonly ILogger<AuroraMySqlService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAWSSecretManager _secretManager;
        private Lazy<Task<string>> connectionStringSource;
        private readonly IDataSecurity _dataSecurity;
        public AuroraMySqlService(ILogger<AuroraMySqlService> logger, IConfiguration configuration, IAWSSecretManager secretManager, IDataSecurity dataSecurity)
        {
            _configuration = configuration;
            _logger = logger;
            _secretManager = secretManager;
            _dataSecurity = dataSecurity;
            connectionStringSource = GetConnectionString();

        }

        public async Task<bool> InsertpaymentRecord(PaymentDB paymentRequest)
        {
            try
            {
                using (var connection = new MySqlConnection(await connectionStringSource.Value))
                {
                    MySqlCommand command = new MySqlCommand("sp_InsertPaymentTable", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new MySqlParameter("par_TransactionId", paymentRequest.TransactionId));
                    command.Parameters.Add(new MySqlParameter("par_ApplicationId", paymentRequest.ApplicationId));
                    command.Parameters.Add(new MySqlParameter("par_ApplicationVersion", paymentRequest.ApplicationVersion));
                    command.Parameters.Add(new MySqlParameter("par_PaymentType", paymentRequest.PaymentType));
                    command.Parameters.Add(new MySqlParameter("par_Amount", paymentRequest.Amount));
                    command.Parameters.Add(new MySqlParameter("par_CurrencyCode", paymentRequest.CurrencyCode));
                    command.Parameters.Add(new MySqlParameter("par_Mileage", paymentRequest.Mileage));
                    command.Parameters.Add(new MySqlParameter("par_Remark", paymentRequest.Remark));
                    command.Parameters.Add(new MySqlParameter("par_InsertBy", paymentRequest.InsertBy));
                    command.Parameters.Add(new MySqlParameter("par_IsTest", paymentRequest.IsTest));
                    command.Parameters.Add(new MySqlParameter("par_sessionId", paymentRequest.SessionId));
                    command.Parameters.Add(new MySqlParameter("par_deviceId", paymentRequest.DeviceId));
                    command.Parameters.Add(new MySqlParameter("par_recordLocator", paymentRequest.RecordLocator));
                    command.Parameters.Add(new MySqlParameter("par_mileagePlusNumber", paymentRequest.MileagePlusNumber));
                    command.Parameters.Add(new MySqlParameter("par_RestAPIVersion", paymentRequest.RestAPIVersion));
                    command.Parameters.Add(new MySqlParameter("par_FormOfPayment", paymentRequest.FormOfPayment));

                    await command.Connection.OpenAsync();
                    var result = await command.ExecuteNonQueryAsync();
                    await command.Connection.CloseAsync();
                    if (result > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Insert into Payment table failed - DataAccess error {message} {stackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            return false;
        }

        private Lazy<Task<string>> GetConnectionString()
        {
            return new Lazy<Task<string>>(async () =>
            {
                var connString = _configuration.GetSection("AuroraDBConnectionString").GetValue<bool>("IsUseCryptography")
                              ? _dataSecurity.DecryptData(_configuration.GetSection("AuroraDBConnectionString").GetValue<string>("ConnectionString-unitedtravelapp"),
                               _configuration.GetSection("AuroraDBConnectionString").GetValue<string>("EncrypKey"),
                               _configuration.GetSection("AuroraDBConnectionString").GetValue<string>("IVValue")
                                )
                              : await _secretManager.GetSecretValue(_configuration.GetSection("AuroraDBConnectionString").GetValue<string>("ConnectionString-SecretKey"));
                return connString;
            });
        }

        public async Task<List<MOBFeatureSetting>> GetFeatureSettingsByAPIName(string apiName)
        {
            List<MOBFeatureSetting> list = new List<MOBFeatureSetting>();
            string connectionString = await connectionStringSource.Value;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(await connectionStringSource.Value))
                {
                    await conn.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand("sp_GetAllFeatureSettingbyAPIName", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("par_Apiname", apiName));
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new MOBFeatureSetting()
                            {

                                Key = reader["Key"].ToString(),
                                Value = reader["Value"].ToString(),
                                Apiname = reader["Apiname"].ToString(),
                                LastUpdatedBy = reader["Lastmodifiedby"].ToString()
                            });
                        }
                        await cmd.Connection.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetFeatureSettings - DataAccess error{@apiName} {@message} {@stackTrace}", apiName, ex.Message, ex.StackTrace);
                 throw ex;
            }
            return list;
        }
        public async Task<MOBFeatureSetting> InsertFeatureSettings(MOBFeatureSetting featureSetting)
        {
            try
            {

                using (var connection = new MySqlConnection(await connectionStringSource.Value))
                {
                    MySqlCommand command = new MySqlCommand("sp_InsertFeatureSettings", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new MySqlParameter("par_Key", featureSetting.Key));
                    command.Parameters.Add(new MySqlParameter("par_Value", featureSetting.Value));
                    command.Parameters.Add(new MySqlParameter("par_Apiname", featureSetting.Apiname.ToUpper()));
                    command.Parameters.Add(new MySqlParameter("par_LastUpdatedBy", featureSetting.LastUpdatedBy));
                    command.Connection.Open();
                    var result = await command.ExecuteNonQueryAsync();
                    await command.Connection.CloseAsync();
                    if (result > 0)
                        return featureSetting;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetFeatureSettings - DataAccess error {message} {stackTrace}", ex.Message, ex.StackTrace);
                
            }
            return null;
        }
        public async Task<bool> InsertContainerIPAddress(MOBContainerIPAddressDetails request)
        {
            try
            {
                _logger.LogInformation("InsertContainerIPAddress - DataAccess {@request}{@ServiceName}", request, request.ServiceName);
                using (var connection = new MySqlConnection(await connectionStringSource.Value))
                {
                    MySqlCommand command = new MySqlCommand("sp_InsertContainerIpAddress", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new MySqlParameter("par_IPAddress", request.IpAddress));
                    command.Parameters.Add(new MySqlParameter("par_ServiceName", request.ServiceName));
                    await command.Connection.OpenAsync();
                    var result = await command.ExecuteNonQueryAsync();
                    await command.Connection.CloseAsync();
                    if (result > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("InsertContainerIPAddress - DataAccess error{@apiName} {@message} {@stackTrace}", request.ServiceName, ex.Message, ex.StackTrace);
               
            }
            return false;
        }
        public async Task<List<MOBContainerIPAddressDetails>> GetContainerIPAddressesByService(string apiName)
        {
            List<MOBContainerIPAddressDetails> list = new List<MOBContainerIPAddressDetails>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(await connectionStringSource.Value))
                {
                    await conn.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand("sp_GetIPAddressByService", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new MySqlParameter("par_ServiceName", apiName));
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new MOBContainerIPAddressDetails()
                            {

                                IpAddress = reader["IPAddress"].ToString(),
                                ServiceName = reader["ServiceName"].ToString(),
                                IsManuallyRefreshed = Convert.ToBoolean(reader["IsManuallyrefreshed"]),

                            });
                        }
                    }
                    await cmd.Connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetContainerIPAddressesByService - DataAccess error {@message} {@stackTrace}", ex.Message, ex.StackTrace);
                
            }
            return list;
        }
        public async Task<bool> UpdateIsManuallyRefreshedToggle(string serviceName, string containerIpAdressList)
        {
            try
            {
                using (var connection = new MySqlConnection(await connectionStringSource.Value))
                {
                    MySqlCommand command = new MySqlCommand("sp_UpdateIsManuallyRefreshedToggle", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new MySqlParameter("par_ipAddresList", containerIpAdressList));
                    command.Parameters.Add(new MySqlParameter("par_ServiceName", serviceName));
                    await command.Connection.OpenAsync();
                    var result = await command.ExecuteNonQueryAsync();
                    await command.Connection.CloseAsync();
                    if (result > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateIsManuallyRefreshedToggle - DataAccess error {@message} {@stackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            return false;
        }
        public async Task<bool> DeleteContainerIPAddress(MOBContainerIPAddressDetails request)
        {
            try
            {
                _logger.LogInformation("DeleteContainerIPAddress - DataAccess {@request}", request);
                using (var connection = new MySqlConnection(await connectionStringSource.Value))
                {
                    MySqlCommand command = new MySqlCommand("sp_DeleteIPAddressByService", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new MySqlParameter("par_ipAddress", request.IpAddress));
                    command.Parameters.Add(new MySqlParameter("par_ServiceName", request.ServiceName));
                    await command.Connection.OpenAsync();
                    var result = await command.ExecuteNonQueryAsync();
                    await command.Connection.CloseAsync();
                    if (result > 0)
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("DeleteContainerIPAddress - DataAccess error{@apiName} {@message} {@stackTrace}", request.ServiceName, ex.Message, ex.StackTrace);
                
            }
            return false;
        }
        public async Task<bool> IsAllContainersRefreshed(string serviceName)
        {
            try
            {
                using (var connection = new MySqlConnection(await connectionStringSource.Value))
                {
                    MySqlCommand command = new MySqlCommand("sp_IsAllContainersRefreshed", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new MySqlParameter("par_ServiceName", serviceName));
                    command.Parameters.Add("@IsRefreshed", MySqlDbType.Bit);
                    command.Parameters["@IsRefreshed"].Direction = ParameterDirection.Output;
                    await command.Connection.OpenAsync();
                    var result = await command.ExecuteNonQueryAsync();
                    await command.Connection.CloseAsync();
                    if (Convert.ToBoolean(command.Parameters["@IsRefreshed"].Value))
                        return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("IsAllContainersRefreshed - DataAccess error {@message} {@stackTrace}", ex.Message, ex.StackTrace);
                throw ex;
            }
            return false;
        }
    }
}
