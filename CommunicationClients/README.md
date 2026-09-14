# AVCoders.Crestron.CommunicationClients

AV Coders communication clients backed by Crestron hardware. Part of the [AV Coders crestron-components repository](https://github.com/AV-Coders/crestron-components). Targets **.NET 8.0**.

## Install

```bash
dotnet add package AVCoders.Crestron.CommunicationClients
```

Available from the AV Coders GitHub Packages feed (`https://nuget.pkg.github.com/AV-Coders/index.json`) and nuget.org. MIT licensed.

## What's inside

- `AvCodersSerialClient` - a `SerialClient` over a Crestron `ComPort`
- `AvCodersIrAsSerialClient` - drives an IR port as a serial channel
- `CrestronCecStream` - a `SerialClient` over a DM CEC stream; only forwards `CecMessageReceivedEventId` frames (NACK and physical-address events go to `NackHandlers` / the log, never replayed as responses) and tracks the owning device's online status as `ConnectionState`
- `CrestronMulticastClient` - multicast over the Crestron `UDPServer`
- `CrestronGenericDeviceCommunicationClient` - online/IP status for a `GenericDevice`
