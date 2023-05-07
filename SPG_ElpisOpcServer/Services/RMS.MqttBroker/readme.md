# RMS Mqtt Broker
RMS Broker is a ASP NET Core app for MQTT based communication. It provides a MQTT client and a MQTT server (broker) for RMS Application.

## Flow Diagram

![Flow Diagram](/Images/rms_mqttbroker_flow.svg)

## Class Diagram

![Class Diagram](/Images/rms_mqtt_class_diagram.svg)

## Supported Framworks
* NET Core 3.1

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
cd RMS.Broker
dotnet watch run
```
## App Configuration

#### Configure Tcp Endpoint for MQTT

```json
"TcpEndPoint": {
      "Enabled": true,
      "IPv4": "*",
      "IPv6": "*",
      "Port": 1883
    },
```
- Change tcp port 1883 to use differenct port

#### Configure WebSocket for MQTT
```json
 "WebSocketEndPoint": {
      "Enabled": true,
      "Path": "/mqtt",
      "KeepAliveInterval": 120, // In seconds.
      "ReceiveBufferSize": 4096,
      "AllowedOrigins": [] // List of strings with URLs.
    }
```
- Change mqtt path which websocket will 

#### Configure Kafka for Data Parse
```json
 "Kafka": {
    "Consumer": {
      "Bootstrap": "106.51.74.7:9092",
      "GroupId": "mqtt-consumer_dev",
      "Topics": [ "rmsparsed" ]
    }
  }
```
- Change bootstrap server to use different kafka
- change Group Id for consume in perticular group
- topics is from data parser


## References
This library is used in the following projects

- [MQTTnet](https://www.nuget.org/packages/MQTTnet/)
- [Confluent.Kafka](https://www.nuget.org/packages/Confluent.Kafka/)