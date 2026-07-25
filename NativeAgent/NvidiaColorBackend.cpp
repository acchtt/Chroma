#include "NvidiaColorBackend.h"

#include <algorithm>
#include <cmath>

namespace Chroma
{
namespace
{
constexpr NvidiaColorBackend::NvStatus kNvApiOk = 0;
constexpr NvidiaColorBackend::NvStatus kNvApiEndEnumeration = -7;

constexpr unsigned int kNvApiInitializeId = 0x0150E828;
constexpr unsigned int kNvApiUnloadId = 0xD22BDD7E;
constexpr unsigned int kNvApiEnumDisplayHandleId = 0x9ABDD40D;
constexpr unsigned int kNvApiGetDvcInfoId = 0x4085DE45;
constexpr unsigned int kNvApiSetDvcLevelId = 0x172409B4;

constexpr unsigned int kDvcInfoVersion = 1;
constexpr double kMinimumSaturation = 0.0;
constexpr double kNormalSaturation = 1.0;
constexpr double kMaximumSaturation = 3.0;
}

NvidiaColorBackend::~NvidiaColorBackend()
{
    Shutdown();
}

std::wstring_view NvidiaColorBackend::GetId() const noexcept
{
    return L"nvidia.nvapi";
}

std::wstring_view NvidiaColorBackend::GetDisplayName() const noexcept
{
    return L"NVIDIA Digital Vibrance Control";
}

bool NvidiaColorBackend::Initialize()
{
    if (initialized_)
    {
        return true;
    }

#if defined(_WIN64)
    module_ = LoadLibraryW(L"nvapi64.dll");
#else
    module_ = LoadLibraryW(L"nvapi.dll");
#endif

    if (module_ == nullptr || !ResolveFunctions())
    {
        Shutdown();
        return false;
    }

    if (initialize_() != kNvApiOk || !EnumerateDisplays())
    {
        Shutdown();
        return false;
    }

    initialized_ = true;
    return true;
}

void NvidiaColorBackend::Shutdown() noexcept
{
    displays_.clear();

    if (unload_ != nullptr)
    {
        (void)unload_();
    }

    initialize_ = nullptr;
    unload_ = nullptr;
    enumDisplayHandle_ = nullptr;
    getDvcInfo_ = nullptr;
    setDvcLevel_ = nullptr;
    initialized_ = false;

    if (module_ != nullptr)
    {
        FreeLibrary(module_);
        module_ = nullptr;
    }
}

bool NvidiaColorBackend::IsInitialized() const noexcept
{
    return initialized_;
}

bool NvidiaColorBackend::SetSaturation(double saturation)
{
    if (!initialized_ || displays_.empty())
    {
        return false;
    }

    bool changedAnyDisplay = false;
    for (DisplayHandle display : displays_)
    {
        DvcInfo info{};
        info.version = sizeof(DvcInfo) | (kDvcInfoVersion << 16);

        if (getDvcInfo_(display, 0, &info) != kNvApiOk)
        {
            continue;
        }

        const int level = ToDvcLevel(saturation, info);
        if (setDvcLevel_(display, 0, level) == kNvApiOk)
        {
            changedAnyDisplay = true;
        }
    }

    return changedAnyDisplay;
}

bool NvidiaColorBackend::ResolveFunctions()
{
    const auto queryInterface = reinterpret_cast<QueryInterfaceFn>(
        GetProcAddress(module_, "nvapi_QueryInterface"));
    if (queryInterface == nullptr)
    {
        return false;
    }

    initialize_ = reinterpret_cast<InitializeFn>(queryInterface(kNvApiInitializeId));
    unload_ = reinterpret_cast<UnloadFn>(queryInterface(kNvApiUnloadId));
    enumDisplayHandle_ = reinterpret_cast<EnumDisplayHandleFn>(queryInterface(kNvApiEnumDisplayHandleId));
    getDvcInfo_ = reinterpret_cast<GetDvcInfoFn>(queryInterface(kNvApiGetDvcInfoId));
    setDvcLevel_ = reinterpret_cast<SetDvcLevelFn>(queryInterface(kNvApiSetDvcLevelId));

    return initialize_ != nullptr &&
           unload_ != nullptr &&
           enumDisplayHandle_ != nullptr &&
           getDvcInfo_ != nullptr &&
           setDvcLevel_ != nullptr;
}

bool NvidiaColorBackend::EnumerateDisplays()
{
    displays_.clear();

    for (int index = 0;; ++index)
    {
        DisplayHandle display = nullptr;
        const NvStatus status = enumDisplayHandle_(index, &display);
        if (status == kNvApiEndEnumeration)
        {
            break;
        }

        if (status != kNvApiOk)
        {
            displays_.clear();
            return false;
        }

        if (display != nullptr)
        {
            displays_.push_back(display);
        }
    }

    return !displays_.empty();
}

int NvidiaColorBackend::ToDvcLevel(
    double saturation,
    const DvcInfo& info) noexcept
{
    const double clamped = std::clamp(
        saturation,
        kMinimumSaturation,
        kMaximumSaturation);

    if (clamped <= kNormalSaturation)
    {
        const double amount = clamped / kNormalSaturation;
        return static_cast<int>(std::lround(
            info.minLevel + amount * (0.0 - info.minLevel)));
    }

    const double amount =
        (clamped - kNormalSaturation) /
        (kMaximumSaturation - kNormalSaturation);
    return static_cast<int>(std::lround(amount * info.maxLevel));
}
}