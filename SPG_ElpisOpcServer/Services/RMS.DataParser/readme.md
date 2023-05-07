# RMS Data Parser
RMS Data Parser is a ASP NET Core app for Parsing data from different customer devices.

## Flow Diagram

![Flow Diagram](/Images/data_parser_flow.svg)

## Class Diagram

![Class Diagram](/Images/data_parser_class_diagram.svg)

## Supported Framworks
* NET Core 3.1
* NET 5.0

## Getting Started ##

##### Install Visual Studio 2019+ #####

VS 2019+ is required for developing ASP NET Core. If you do not already have it installed, you can download it [here](https://www.visualstudio.com/downloads/download-visual-studio-vs). If you are installing VS 2019+ for the first time, select the "Custom" installation type and select the following from the features list to install:

- ASP.NET and Web development 
  - `Workloads > ASP.NET and Web development `.

  - Most current SDK version of .NET Core 3.1
  - Or install the most current .NET Core 3.1 SDK from here https://dotnet.microsoft.com/download


## Run the app

Run the following commands:

```dotnetcli
cd RMS.DataParser
dotnet watch run
```
## EndPoints
Authentication  : `BasicAuthentication`

| Username | Password |
|---|-------|
|  admin | Admin@4321 | 

### 1. `/api/v1/plugins` (Get) ###
- Requires authentication
- Provides list of plugIns available for DataParser
- Returns array plugIn info which Includes
```json
 [{
      "PlugInDir": "" // Directory which plugIn Exist,
      "Id": "" // 32 length GUID,
      "TypeId": 0 // Type for device,
      "Name": "" // Name of the plugIn Provider
    }]
```

### 2. `/api/v1/topics` (Get) ###
- Requires authentication
- Provides list of plugIns topics available for DataParser
- Returns array plugIn info which Includes
```json
 [
     "topic": {
      "PlugInDir": "" // Directory which plugIn Exist,
      "Id": "" // 32 length GUID,
      "TypeId": 0 // Type for device,
      "Name": "" // Name of the plugIn Provider
    }
 ]
```

### 3. `/api/v1/plugins/add` (Post) ###
- Requires authentication
- Add topic for data parsing
- Body 
```json
{
    "topic": "" // Directory which plugIn Exist,
    "pluginId": "" // 32 length GUID,
}
```

### 4. `/api/v1/plugins/remove` (Post) ###
- Requires authentication
- Remove topic for data parsing
- Body 
```json
{
    "topic": "" // Directory which plugIn Exist,
    "pluginId": "" // 32 length GUID,
}
```
   

## Run as Service

```bash
dotnet publish -o Your_Path
sc create RMSWorker binPath=Your_Path/RMS.DataParser.exe
```

## References
This library is used in the following projects

- [Confluent.Kafka](https://www.nuget.org/packages/Confluent.Kafka/)

