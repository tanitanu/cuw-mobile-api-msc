using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using United.Common.Helper;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.MSCPayment.Services;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.DataAccess.Product.Services;
using United.Mobile.InFlightPurchase.Domain;
using United.Mobile.Model;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Middleware;

namespace United.Mobile.InFlightPurchase.Api
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
            services.AddControllers()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddScoped<ISessionHelperService, SessionHelperService>();            
            services.AddScoped<IHeaders, Headers>();
            services.AddTransient<IInFlightPurchaseBusiness, InFlightPurchaseBusiness>();
            services.AddHttpContextAccessor();
            services.AddScoped<CacheLogWriter>();
            services.AddScoped(typeof(ICacheLog<>), typeof(CacheLog<>));
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

            container.Register(c => new ResilientClient(Configuration.GetSection("ReferencedataClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("ReferencedataClientKey");
            container.RegisterType<ReferencedataService>().As<IReferencedataService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("DataVaultTokenClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("DataVaultTokenClientKey");
            container.RegisterType<DataVaultService>().As<IDataVaultService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("MerchandizingClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("MerchandizingClientKey");
            container.RegisterType<PurchaseMerchandizingService>().As<IPurchaseMerchandizingService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("GetTravelersClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("GetTravelersClientKey");
            container.RegisterType<GetTravelersService>().As<IGetTravelersService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("GetMPNumberClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("GetMPNumberClientKey");
            container.RegisterType<GetMPNumberService>().As<IGetMPNumberService>().WithAttributeFiltering();

            container.Register(c => new ResilientClient(Configuration.GetSection("GetSDLKeyValuePairContentClient").Get<ResilientClientOpitons>())).Keyed<IResilientClient>("GetSDLKeyValuePairContentClientKey");
            container.RegisterType<GetSDLKeyValuePairContentService>().As<IGetSDLKeyValuePairContentService>().WithAttributeFiltering();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IApplicationEnricher applicationEnricher)
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
                    await context.Response.WriteAsync("Welcome from InFlightPurchase Microservice");
                });
            });

            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            //app.UseSerilogRequestLogging(opts
            //=> opts.EnrichDiagnosticContext = (diagnosticsContext, httpContext) => {
            //    var request = httpContext.Request;
            //    diagnosticsContext.Set("gzip", request.Headers["Content-Encoding"]);
            //    System.Net.ServicePointManager.ServerCertificateValidationCallback = (o, certificate, arg3, arg4) => { return true; };
            //});

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
