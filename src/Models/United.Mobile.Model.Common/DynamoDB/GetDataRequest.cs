namespace United.Mobile.Model.DynamoDb.Common
{
    public class GetDataRequest
    {
        public string TransactionId { get; set; }
        public string TableName { get; set; }
        public string Key { get; set; }
    }
}