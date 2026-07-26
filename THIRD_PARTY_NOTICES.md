# Third-party notices

Chroma integrates with graphics-vendor display-control interfaces for Intel, NVIDIA, and AMD hardware. Vendor files and runtime components remain subject to their original licenses and are not relicensed under Chroma's MIT License.

## Intel Graphics Control Library

Chroma uses Intel Graphics Control Library (IGCL) API materials, including the API header and wrapper implementation.

The official IGCL project is maintained by Intel at `intel/drivers.gpu.control-library`. Intel distributes the IGCL binaries with the Intel graphics driver; applications should use the installed driver library rather than packaging the IGCL runtime binary.

The IGCL API header is governed by Intel's Control API / IGCL license rather than the Chroma MIT License. Intel's license permits use and redistribution of the Control API software and IGCL header files solely for use on Intel platforms, subject to its stated conditions. Any redistributed Intel files must retain their Intel copyright and license notices.

Official references:

- https://github.com/intel/drivers.gpu.control-library
- https://github.com/intel/drivers.gpu.control-library/blob/master/License.txt
- https://intel.github.io/drivers.gpu.control-library/

## AMD ADLX

Chroma can be built with AMD ADLX headers and helper sources when the ADLX SDK is available under `third_party/ADLX` or through the configured SDK path. The ADLX checkout retains AMD's original copyright notices and license terms.

Chroma does not relicense AMD ADLX source or documentation. Distributors who include ADLX-derived files must keep the applicable AMD notices and comply with the ADLX repository license.

Official reference:

- https://github.com/GPUOpen-LibrariesAndSDKs/ADLX

## NVIDIA NVAPI

Chroma dynamically loads the NVIDIA driver-provided NVAPI library at runtime. NVIDIA runtime binaries are not bundled with Chroma.

The Digital Vibrance backend uses NVAPI interface entry points resolved from the installed NVIDIA driver. Some Digital Vibrance interfaces are not part of NVIDIA's fully documented public SDK surface and may vary between driver versions. NVIDIA names, APIs, and driver components remain subject to NVIDIA's terms.

Official reference:

- https://developer.nvidia.com/drive/nvapi

## Chroma-authored code

Files written specifically for Chroma are covered by the repository's MIT License unless a file contains a different notice. Files derived from or copied from a vendor SDK must keep their original headers and applicable license terms.
