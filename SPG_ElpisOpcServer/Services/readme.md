# RMS Services

## Docker setup
```docker-compose -f docker-composer.yml up -d```

## Deploy RMS.MqttBroker
FTP Configuration

| Name | URL | Username | Password |
| ----- | ------ |----- | ------- |
| Mochohost | 50.31.147.20 | rmsmqttbroker | Testing123$ |
| Azure Mochohost | 50.31.147.20 | rmsazuremqtt | Testing123$ |

## Deploy RMS.DataParser
- Change GroupId `rms_dev` for RMS Data Parser in `appSettings.json => KafkaOptions => consumer => groupId` to `rms`

    ```json
    "KafkaOptions": {
        "producer": {
            "bootstrapServers": "106.51.74.7:9092"
         },
        "consumer": {
            "bootstrapServers": "106.51.74.7:9092",
            "brokerAddressFamily": "V4",
            "groupId": "rms"
        }
  },
    ```
- First build PlugIns Solution using below command

    ```sudo dotnet build "PlugIns.sln" -c Release```

- Run DataParser

    ```sudo dotnet run "RMS.DataParser/RMS.DataParser.csproj```

- Publish DataParser

    ```sudo dotnet publish "RMS.DataParser -o /var/www/rmsdataparser```

## Deploy RMS.SocketServer
- Publish DataParser

    ```sudo dotnet publish "RMS.SocketServer -o /var/www/rmssocketserver```


### Service EndPoints ###

| Name | URL |
|---|-------|
|  [RMS DataParser](RMS.DataParser) | [106.51.74.7:5003](106.51.74.7:5003) | 
|  [RMS Socket Server](RMS.SocketServer) | `TCP` 106.51.74.7:5008, `Web` 106.51.74.7:5004 | 
|  [RMS MqttBroker](RMS.MqttBroker) | [rmsmqttbroker.elpisitsolutions.com](https://rmsmqttbroker.elpisitsolutions.com/api)| 

## See More Info 
1. [RMS DataParser](RMS.DataParser/readme.md)

2. [RMS MqttBroker](RMS.MqttBroker/readme.md)

3. [RMS Socket Server](RMS.SocketServer/readme.md)