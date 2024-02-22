using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;
using United.Common.Helper;
using United.Common.Helper.Product;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Ancillary.Services;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.MSCPayment.Services;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.DataAccess.Product.Services;
using United.Mobile.Model;
using United.Mobile.Product.Domain;
using United.Utility.Http;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Common.Helper.MSCPayment.Services;
using United.Utility.Middleware;
using Serilog;
using United.Utility.Helper;
using United.Mobile.DataAccess.DynamoDB;
using System;
using System.Threading.Tasks;
using United.Definition;

namespace United.Mobile.Product.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers().AddNewtonsoftJson();
            services.AddControllers();
            services.AddScoped<ISessionHelperService, SessionHelperService>();
            services.AddScoped<IHeaders, Headers>();
            services.AddTransient<IProductBusiness, ProductBusiness>();
            services.AddTransient<IMSCShoppingSessionHelper, MSCShoppingSessionHelper>();
            services.AddTransient<IShoppingUtility, ShoppingUtility>();
            services.AddTransient<IMSCFormsOfPayment, MSCFormsOfPayment>();
            services.AddTransient<IMSCPageProductInfoHelper, MSCPageProductInfoHelper>();
            services.AddTransient<ISeatMapCSL30Helper, SeatMapCSL30Helper>();
            services.AddTransient<IProductUtility, ProductUtility>();
            services.AddTransient<IMoneyPlusMilesUtility, MoneyPlusMilesUtility>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IDataPowerFactory, DataPowerFactory>();
            services.AddTransient<IShoppingBuyMilesHelper, ShoppingBuyMilesHelper>();
            services.AddTransient<IShoppingCartUtility, ShoppingCartUtility>();
            services.AddTransient<IMSCPkDispenserPublicKey, MSCPkDispenserPublicKey>();
            
            services.AddScoped<CacheLogWriter>();
            services.AddScoped(typeof(ICacheLog<>), typeof(CacheLog<>));
            services.AddControllers().AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.Converters.Add(new StringEnumConverter());
            });
            if (Configuration.GetValue<bool>("SwitchToDynamoDB"))
                services.AddTransient<ILegalDocumentsForTitlesService, LegalDocumentForTitleServiceDynamoDB>();
            services.AddSingleton<IFeatureSettings, FeatureSettings>();
            services.AddSingleton<IAuroraMySqlService, AuroraMySqlService>();
            services.AddSingleton<IAWSSecretManager, AWSSecretManager>();
            services.AddSingleton<IDataSecurity, DataSecurity>();
            services.AddSingleton<IFeatureToggles, FeatureToggles>();
        }

        public void ConfigureContainer(ContainerBuilder container)
        {

            container.Register(c => new ResilientClient(Configuration.GetSection("cachingConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("cachingConfigKey");
            container.RegisterType<CachingService>().As<ICachingService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("sessionConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("sessionOnCloudConfigKey");
            container.RegisterType<SessionOnCloudService>().As<ISessionOnCloudService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("sessionConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("sessionConfigKey");
            container.RegisterType<SessionService>().As<ISessionService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("DynamoDBClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DynamoDBClientKey");
            container.RegisterType<DynamoDBService>().As<IDynamoDBService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("dpTokenConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpTokenConfigKey");
            container.RegisterType<DPService>().As<IDPService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("ShoppingCartClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("ShoppingCartClientKey");
            container.RegisterType<ShoppingCartService>().As<IShoppingCartService>().WithAttributeFiltering();
            
            container.Register(c => new ResilientClient(Configuration.GetSection("PKDispenserClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("PKDispenserClientKey");
            container.RegisterType<DataAccess.Common.PKDispenserService>().As<IPKDispenserService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("FlightShoppingClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("FlightShoppingClientKey");
            container.RegisterType<FlightShoppingProductsService>().As<IFlightShoppingProductsService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("SeatMapClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("SeatMapClientKey");
            container.RegisterType<SeatMapService>().As<ISeatMapService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("CMSContentClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("CMSContentClientKey");
            container.RegisterType<CMSContentService>().As<ICMSContentService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("PaymentServiceClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("PaymentServiceClientKey");
            container.RegisterType<PaymentService>().As<IPaymentService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("SeatMapCSL30Client").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("SeatMapCSL30ClientKey");
            container.RegisterType<SeatMapCSL30Service>().As<ISeatMapCSL30Service>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("SQLDBComplimentaryUpgradeClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("SQLDBComplimentaryUpgradeClientKey");
            container.RegisterType<ComplimentaryUpgradeService>().As<IComplimentaryUpgradeService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("OnPremSQLServiceClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("ValidateHashPinOnPremSqlClientKey");
            container.RegisterType<ValidateHashPinService>().As<IValidateHashPinService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("LookUpTravelCreditClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("LookUpTravelCreditClientKey");
            container.RegisterType<LookUpTravelCreditService>().As<ILookUpTravelCreditService>().WithAttributeFiltering();
            
            container.Register(c => new ResilientClient(Configuration.GetSection("ReferencedataClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("ReferencedataClientKey");
            container.RegisterType<ReferencedataService>().As<IReferencedataService>().WithAttributeFiltering();

            if (!Configuration.GetValue<bool>("SwitchToDynamoDB"))
            {
                container.Register(c => new ResilientClient(Configuration.GetSection("LegalDocumentsOnPremSqlClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("LegalDocumentsOnPremSqlClientKey");
                container.RegisterType<LegalDocumentsForTitlesService>().As<ILegalDocumentsForTitlesService>().WithAttributeFiltering();
            }
            container.Register(c => new ResilientClient(Configuration.GetSection("dpTokenValidateConfig").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("dpTokenValidateKey");
            container.RegisterType<DPTokenValidationService>().As<IDPTokenValidationService>().WithAttributeFiltering();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IApplicationEnricher applicationEnricher, IFeatureSettings featureSettings, IHostApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            applicationEnricher.Add(Constants.ServiceNameText, Program.Namespace);
            applicationEnricher.Add(Constants.EnvironmentText, env.EnvironmentName);

            app.MapWhen(context => string.IsNullOrEmpty(context.Request.Path) || string.Equals("/", context.Request.Path), appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    await context.Response.WriteAsync("Welcome from Product Microservice");
                });
            });

            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            //app.UseSerilogRequestLogging(opts
            //=> opts.EnrichDiagnosticContext = (diagnosticsContext, httpContext) => {
            //    var request = httpContext.Request;
            //    diagnosticsContext.Set("gzip", request.Headers["Content-Encoding"]);
            //    System.Net.ServicePointManager.ServerCertificateValidationCallback = (o, certificate, arg3, arg4) => { return true; };
            //});
            if (Configuration.GetValue<bool>("EnableFeatureSettingsChanges"))
            {
                applicationLifetime.ApplicationStarted.Register(async () => await OnStart(featureSettings));
                applicationLifetime.ApplicationStopping.Register(async () => await OnShutDown(featureSettings));
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        private async Task OnStart(IFeatureSettings featureSettings)
        {
            try
            {
                await featureSettings.LoadFeatureSettings(ServiceNames.PRODUCT.ToString());
            }
            catch (Exception) { }

        }
        private async Task OnShutDown(IFeatureSettings featureSettings)
        {
            try
            {
                await featureSettings.DeleteContainerIPAdress(ServiceNames.PRODUCT.ToString(), StaticDataLoader._ipAddress);
            }
            catch { }
        }
    }
}
