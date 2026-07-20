# AVCoders.Crestron.Matrix

Crestron NVX AV-over-IP endpoint wrappers for the AV Coders device library. Part of the [AV Coders crestron-components repository](https://github.com/AV-Coders/crestron-components). Targets **.NET 8.0**.

## Install

```bash
dotnet add package AVCoders.Crestron.Matrix
```

Available from the AV Coders GitHub Packages feed (`https://nuget.pkg.github.com/AV-Coders/index.json`) and nuget.org. MIT licensed.

## What's inside

- `NvxBase` - an `AVoIPEndpoint` over `DmNvxBaseClass`
- `NvxEncoder` / `NvxDecoder`
- `Nvx36xEncoder` / `NvxExxEncoder`
- `NvxCommunicationEmulator` - for testing without hardware
