#include "ColorBackendFactory.h"

#include "AmdColorBackend.h"
#include "IntelColorBackend.h"
#include "NvidiaColorBackend.h"

#include <Windows.h>
#include <algorithm>
#include <array>
#include <cwctype>
#include <string>

namespace Chroma
{
namespace
{
enum class GpuPreference : DWORD
{
    Automatic = 0,
    Intel = 1,
    Nvidia = 2,
    Amd = 3
};

GpuPreference ReadGpuPreference()
{
    DWORD value = static_cast<DWORD>(GpuPreference::Automatic);
    DWORD size = sizeof(value);
    const LSTATUS status = RegGetValueW(
        HKEY_CURRENT_USER,
        L"Software\\Chroma",
        L"GpuPreference",
        RRF_RT_REG_DWORD,
        nullptr,
        &value,
        &size);

    if (status != ERROR_SUCCESS || value > static_cast<DWORD>(GpuPreference::Amd))
    {
        return GpuPreference::Automatic;
    }

    return static_cast<GpuPreference>(value);
}

GpuPreference DetectVendor(const std::wstring& name, const std::wstring& deviceId)
{
    std::wstring value = name + L" " + deviceId;
    std::transform(value.begin(), value.end(), value.begin(),
        [](wchar_t character) { return static_cast<wchar_t>(std::towupper(character)); });

    if (value.find(L"VEN_8086") != std::wstring::npos ||
        value.find(L"INTEL") != std::wstring::npos)
    {
        return GpuPreference::Intel;
    }

    if (value.find(L"VEN_10DE") != std::wstring::npos ||
        value.find(L"NVIDIA") != std::wstring::npos ||
        value.find(L"GEFORCE") != std::wstring::npos)
    {
        return GpuPreference::Nvidia;
    }

    if (value.find(L"VEN_1002") != std::wstring::npos ||
        value.find(L"AMD") != std::wstring::npos ||
        value.find(L"RADEON") != std::wstring::npos)
    {
        return GpuPreference::Amd;
    }

    return GpuPreference::Automatic;
}

GpuPreference GetPrimaryDisplayPreference()
{
    for (DWORD index = 0; ; ++index)
    {
        DISPLAY_DEVICEW adapter{};
        adapter.cb = sizeof(adapter);
        if (!EnumDisplayDevicesW(nullptr, index, &adapter, 0))
        {
            break;
        }

        const bool active = (adapter.StateFlags & DISPLAY_DEVICE_ACTIVE) != 0;
        const bool primary = (adapter.StateFlags & DISPLAY_DEVICE_PRIMARY_DEVICE) != 0;
        if (active && primary)
        {
            return DetectVendor(adapter.DeviceString, adapter.DeviceID);
        }
    }

    return GpuPreference::Automatic;
}

std::unique_ptr<IColorBackend> CreateBackend(GpuPreference preference)
{
    switch (preference)
    {
    case GpuPreference::Intel:
        return std::make_unique<IntelColorBackend>();
    case GpuPreference::Nvidia:
        return std::make_unique<NvidiaColorBackend>();
    case GpuPreference::Amd:
        return std::make_unique<AmdColorBackend>();
    default:
        return nullptr;
    }
}

std::unique_ptr<IColorBackend> TryCreateInitializedBackend(GpuPreference preference)
{
    std::unique_ptr<IColorBackend> backend = CreateBackend(preference);
    if (backend != nullptr && backend->Initialize())
    {
        return backend;
    }
    return nullptr;
}
}

std::unique_ptr<IColorBackend> CreateDefaultColorBackend()
{
    const GpuPreference selected = ReadGpuPreference();
    if (selected != GpuPreference::Automatic)
    {
        // Return only the selected vendor. ChromaRuntime owns initialization and
        // its retry timer will keep trying this backend if the driver is not ready.
        return CreateBackend(selected);
    }

    const GpuPreference primary = GetPrimaryDisplayPreference();
    const std::array<GpuPreference, 4> order = {
        primary,
        GpuPreference::Intel,
        GpuPreference::Nvidia,
        GpuPreference::Amd
    };

    for (std::size_t index = 0; index < order.size(); ++index)
    {
        const GpuPreference preference = order[index];
        if (preference == GpuPreference::Automatic)
        {
            continue;
        }

        bool alreadyTried = false;
        for (std::size_t earlier = 0; earlier < index; ++earlier)
        {
            if (order[earlier] == preference)
            {
                alreadyTried = true;
                break;
            }
        }
        if (alreadyTried)
        {
            continue;
        }

        if (std::unique_ptr<IColorBackend> backend = TryCreateInitializedBackend(preference))
        {
            return backend;
        }
    }

    // Keep a concrete backend object so the runtime can remain alive and retry
    // initialization if the display driver becomes available later.
    return CreateBackend(primary == GpuPreference::Automatic
        ? GpuPreference::Intel
        : primary);
}
}