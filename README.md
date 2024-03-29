# UnlockECU

![Header Image](https://user-images.githubusercontent.com/1116555/156388388-4a81bb7d-4b7d-4424-9220-496147332cfa.png)

Free, open-source ECU seed-key unlocking tool. 

## Getting started

[Try it out here in your browser instantly](https://unlockecu.sn.sg/), or download a local, offline copy:

**Latest Offline**: Select the most recent build from the [automated builds](https://github.com/jglim/UnlockECU/actions), then download the zip artifact.

**Stable Offline**: Download and unarchive the application from the [Releases](https://github.com/jglim/UnlockECU/releases/) page, then run the main application `VisualUnlockECU.exe`.

Ensure that you have *.NET Desktop Runtime 5.0.0*. , available from [here](https://dotnet.microsoft.com/download/dotnet/5.0).

## License

MIT

This application **does not include or require copyrighted or proprietary files**. Security functions and definitions have been reverse-engineered and reimplemented.

*When interacting with this repository (PR, issues, comments), please avoid including copyrighted/proprietary files, as they will be removed without notice.*

## Features

- There is no need for additional files such as security DLLs. The application supports a set of security providers out of the box, and definitions are stored in `db.json`.
- Security functions are completely reverse engineered and re-implemented in C#.
- The project is unencumbered by proprietary binary blobs, and can be shared freely without legal issues.

## Demo

https://user-images.githubusercontent.com/1116555/156387740-27e43a22-e892-4a3a-b024-f9a2f918a50d.mp4



## Adding definitions

Definitions specify a seed-key function for a specific ECU and security level. The input seed's size, output key's length as well as the security provider must be specified. Some security providers require specific parameters to operate. 

Here is an example of a definition:

```
{
  "EcuName": "ME97",
  "Aliases": [],
  "AccessLevel": 1,
  "SeedLength": 2,
  "KeyLength": 2,
  "Provider": "PowertrainBoschContiSecurityAlgo1",
  "Origin": "ME97_ME97_13_10_01_J",
  "Parameters": [
    {
      "Key": "ubTable",
      "Value": "FCAD1E5941992FCD",
      "DataType": "ByteArray"
    },
    {
      "Key": "Mask",
      "Value": "4300",
      "DataType": "ByteArray"
    }
  ]
}
```

Currently, these security providers are available:

- DaimlerStandardSecurityAlgo
- DaimlerStandardSecurityAlgoMod
- DaimlerStandardSecurityAlgoRefG
- DRVU_PROF
- EDIFF290
- EsLibEd25519
- ESPSecurityAlgoLevel1
- IC172Algo1
- IC172Algo2
- MarquardtSecurityAlgo
- OCM172
- PowertrainBoschContiSecurityAlgo1
- PowertrainBoschContiSecurityAlgo2
- PowertrainDelphiSecurityAlgo
- PowertrainSecurityAlgo
- PowertrainSecurityAlgo2
- PowertrainSecurityAlgo3
- PowertrainSecurityAlgoNFZ
- RBTM
- RDU222
- RVC222_MPC222_FCW246_LRR3
- SWSP177
- VGSSecurityAlgo
- VolkswagenSA2
- XorAlgo

The definitions file `db.json` should be found alongside the application's main binary.

## Notes

- If your diagnostics file has unlocking capabilities, usually your diagnostics client can already perform the unlocking without further aid. Check your client's available functions for phrases such as `Entriegeln` , `Zugriffberechtigung` , and `Unlock`.
- Generally, this application operates like most DLL-based seed-key generators. If you already have a DLL-based tool, this application does not offer much more (only includes a few modern targets such as `HU7`).
- Definitions are reverse-engineered from DLLs and SMR-D files. If the definition does not innately exist in those files, they will not be available here (e.g. high-level instrument cluster definitions).
- There are ECUs that share the same seed-key function. For example, `CRD3` and `CRD3S2` appear to share the same function as `CRD3NFZ`.
- The core of this project is a "portable" .NET 5 class library which can be reused on other platforms.
- As the security providers are now written in a high-level language, they can be better studied. For example, `DaimlerStandardSecurityAlgo` performs a XOR with its private key as a final step, which allows the private key to be recovered from a known seed and key.
- `DaimlerStandardSecurityAlgo` is usually used for firmware flashing, and might not unlock other capabilities such as variant-coding.

## Contributing

Contributions in adding security providers and definitions are welcome.
