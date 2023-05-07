#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Services/RMS.DataParser/RMS.DataParser.csproj", "RMS.DataParser/"]
COPY ["nuget.config", "nuget.config"]
COPY ["Services/PlugIns.sln", "PlugIns.sln"]
COPY ["Services/PlugIns/", "PlugIns/"]
RUN dotnet restore "PlugIns.sln"
RUN dotnet restore "RMS.DataParser/RMS.DataParser.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PlugIns.sln" -c Release
WORKDIR "/src/RMS.DataParser"
RUN dotnet build "RMS.DataParser.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RMS.DataParser.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RMS.DataParser.dll"]