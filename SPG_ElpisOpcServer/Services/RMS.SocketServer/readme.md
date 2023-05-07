# RMS Socket Server
RMS Broker is a ASP NET Core app for Socket based communication. It provides a TCP client and a TCP server between RMS Application and Device.

## Flow Diagram

![Flow Diagram](/Images/socket_server_flow.svg)

## Supported Framworks
* NET Core 3.1

## Getting Started ##

##### Install Visual Studio 2019+ #####

VS 2019+ is required for developing ASP NET Core. If you do not already have it installed, you can download it [here](https://www.visualstudio.com/downloads/download-visual-studio-vs). If you are installing VS 2019+ for the first time, select the "Custom" installation type and select the following from the features list to install:

- ASP.NET and Web development 
  - `Workloads > ASP.NET and Web development `.

  - Most current SDK version of .NET Core 3.1
  - Or install the most current .NET Core 3.1 SDK from here https://dotnet.microsoft.com/download


#### Configure Tcp Endpoint for Server

`appsettings.json`
```json
"Socket": {
    /*
         Wildcard Addresses:
          *             - All local IP addresses
          localhost     - Localhost only
          disable       - Skip address assignment
        */
    "TcpEndPoint": {
      "Enabled": true,
      "Address": "*",
      "Port": 5008
    },
    "WebSocketEndPoint": {
      "Path": "/rms",
      "KeepAliveInterval": 120, // In seconds.
      "ReceiveBufferSize": 1024,
      "AllowedOrigins": [] // List of strings with URLs.
    }
  }
```
- Change tcp port to use differenct port

## Packet Structure

### Packet Format

  `Example: Hello `

| 1 byte | 2 byte | 1 byte | 2 byte | 5 bytes |
|---------|---------|-------|------|---------|
| Send Type | Payload Size | Packet Info | Message Id | Hello 

### Sender Types (`1 Byte`)
- To Client
- To Device
- From Client
- From Device
- Ping

### Packet Info (`1 Byte`)

| 3 bit | 1 bit | 4 bit |
|---|-------|-------|
|  0 | Retain | Quality |

### Quality ###
- None
- Only Once
- Twice
- Synchronize