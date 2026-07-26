# Local vendor SDKs

This directory is reserved for vendor SDK checkouts that are needed at build time but should not be committed to the Chroma repository.

## AMD ADLX

Clone the official ADLX repository into `third_party/ADLX` to enable the AMD backend in local builds:

```powershell
git clone https://github.com/GPUOpen-LibrariesAndSDKs/ADLX.git third_party/ADLX
```

CMake also accepts an explicit SDK path through `CHROMA_ADLX_ROOT` or the `ADLX_SDK_ROOT` environment variable.

The ADLX source remains governed by AMD's original license and notices. See [`../THIRD_PARTY_NOTICES.md`](../THIRD_PARTY_NOTICES.md).
