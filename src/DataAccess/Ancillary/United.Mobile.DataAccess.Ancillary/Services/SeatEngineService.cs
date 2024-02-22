using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using UAWSSeatEngine.SeatMapEngineSoap;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Utility.Helper;

namespace United.Mobile.DataAccess.Product.Services
{
    public class SeatEngineService : ISeatEngineService
    {
        private readonly ICacheLog<SeatEngineService> _logger;
        private readonly IConfiguration _configuration;
        public SeatEngineService(ICacheLog<SeatEngineService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<GetMerchandizingOffersOutput> GetSeatMapWithFeesFromCSlResponse(SeatMapEngineSoapClient seatMapEngineSoapClient)
        {
            string merchandizingServiceURL = _configuration.GetValue<string>("BasicHttpBinding_SeatEngineService");
            var binding = new BasicHttpBinding();
            binding.MaxReceivedMessageSize = _configuration.GetValue<long>("maxReceivedMessageSize");
            if (_configuration.GetValue<bool>("BasicHttpSecurityMode"))
            {
                binding.Security.Mode = BasicHttpSecurityMode.Transport;
            }
            var endpoint = new EndpointAddress(new Uri(merchandizingServiceURL));
            var channelFactory = new ChannelFactory<ISeatEngineService>(binding, endpoint);
            var serviceClient = channelFactory.CreateChannel();
            //TODO
            var response = await serviceClient.GetSeatMapWithFeesFromCSlResponse(seatMapEngineSoapClient);
            channelFactory.Close();
            return response;
        }
    }
}
