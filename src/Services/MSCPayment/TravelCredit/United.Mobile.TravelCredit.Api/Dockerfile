#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM artifactorycloud.ual.com/l-docker-releases/datadog/dotnet/aspnet:6.0.5-2.10.0 AS base

WORKDIR /app
EXPOSE 80

FROM artifactorycloud.ual.com/l-docker/ms/dotnet/core/sdk:6.0 AS build
WORKDIR /

COPY ["src/Services/MSCPayment/TravelCredit/United.Mobile.TravelCredit.Api/United.Mobile.TravelCredit.Api.csproj", "src/Services/MSCPayment/TravelCredit/United.Mobile.TravelCredit.Api/"]
COPY ["src/Services/MSCPayment/TravelCredit/United.Mobile.TravelCredit.Domain/United.Mobile.TravelCredit.Domain.csproj", "src/Services/MSCPayment/TravelCredit/United.Mobile.TravelCredit.Domain/"]
COPY ["src/DataAccess/Ancillary/United.Mobile.DataAccess.Ancillary/United.Mobile.DataAccess.Product.csproj", "src/DataAccess/Ancillary/United.Mobile.DataAccess.Ancillary/"]
COPY ["src/Models/United.Mobile.Model.MSC/United.Mobile.Model.MSC.csproj", "src/Models/United.Mobile.Model.MSC/"]
COPY ["src/Models/United.Mobile.Model.Common/United.Mobile.Model.Common.csproj", "src/Models/United.Mobile.Model.Common/"]
COPY ["src/DataAccess/United.Mobile.DataAccess.Common/United.Mobile.DataAccess.Common.csproj", "src/DataAccess/United.Mobile.DataAccess.Common/"]
COPY ["src/Core/United.Utility/United.Utility.csproj", "src/Core/United.Utility/"]
COPY ["src/Core/United.Ebs.Logging/United.Ebs.Logging.csproj", "src/Core/United.Ebs.Logging/"]
COPY ["src/Core/United.Common.Helper/United.Common.Helper.csproj", "src/Core/United.Common.Helper/"]
COPY ["src/DataAccess/United.Mobile.DataAccess.DynamoDB/United.Mobile.DataAccess.DynamoDB.csproj", "src/DataAccess/United.Mobile.DataAccess.DynamoDB/"]
COPY ["src/Core/Common/United.Common.Helper.MSCPayment/United.Common.Helper.MSCPayment.csproj", "src/Core/Common/United.Common.Helper.MSCPayment/"]
COPY ["src/DataAccess/United.DataAccess.MerchandizeService/United.DataAccess.MerchandizeService.csproj", "src/DataAccess/United.DataAccess.MerchandizeService/"]
COPY ["src/DataAccess/MSCPayment/United.Mobile.DataAccess.MSCPayment/United.Mobile.DataAccess.MSCPayment.csproj", "src/DataAccess/MSCPayment/United.Mobile.DataAccess.MSCPayment/"]
COPY ["src/Core/United.Utility.Middleware/United.Utility.Middleware.csproj", "src/Core/United.Utility.Middleware/"]

ARG NUGET_PASSWORD
ARG NUGET_USERNAME


RUN dotnet nuget add source https://artifactorycloud.ual.com/artifactory/api/nuget/v3/v-nuget \
            -n artifactory \
            -u ${NUGET_USERNAME}  \
            -p ${NUGET_PASSWORD} \
            --store-password-in-clear-text

COPY . .
WORKDIR "/src/Services/MSCPayment/TravelCredit/United.Mobile.TravelCredit.Api"

FROM build AS publish
RUN dotnet publish "United.Mobile.TravelCredit.Api.csproj" -c Release --self-contained -r linux-x64 -o /app/publish

FROM base AS final
RUN rm -f /etc/apt/apt.conf.d/proxy.conf
RUN apt-get update && apt-get install -y awscli && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "United.Mobile.TravelCredit.Api.dll"]
