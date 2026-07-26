# Local vendor SDKs

This directory is reserved for vendor SDK checkouts that are needed at build time but should not be committed to the Chroma repository.

## AMD ADLX

Clone the official ADLX repository into `third_party/ADLX` to enable the AMD backend in local builds, then check out the same revision used by GitHub Actions:

```powershell
git clone https://github.com/GPUOpen-LibrariesAndSDKs/ADLX.git third_party/ADLX
git -C third_party/ADLX checkout d9f04a9bba022d6cf6333f005dd540b4ad19fb63
```

CMake also accepts an explicit SDK path through `CHROMA_ADLX_ROOT` or the `ADLX_SDK_ROOT` environment variable.

The ADLX source remains governed by AMD's original license and notices. See [`../THIRD_PARTY_NOTICES.md`](../THIRD_PARTY_NOTICES.md).
