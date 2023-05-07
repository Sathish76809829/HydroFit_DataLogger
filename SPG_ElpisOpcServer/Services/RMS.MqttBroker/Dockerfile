#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["RMS.MqttBroker/RMS.Broker.csproj", "RMS.MqttBroker/"]
RUN dotnet restore "RMS.MqttBroker/RMS.Broker.csproj"
COPY . .
WORKDIR "/src/RMS.MqttBroker"
RUN dotnet build "RMS.Broker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RMS.Broker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RMS.Broker.dll"]