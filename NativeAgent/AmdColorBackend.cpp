#include "AmdColorBackend.h"

#include <algorithm>
#include <cmath>

namespace Chroma
{
namespace
{
constexpr double kMinimumSaturation = 0.0;
constexpr double kNormalSaturation = 1.0;
constexpr double kMaximumSaturation = 3.0;
}

AmdColorBackend::~AmdColorBackend()
{
    Shutdown();
}

std::wstring_view AmdColorBackend::GetId() const noexcept
{
    return L"amd.adlx";
}

std::wstring_view AmdColorBackend::GetDisplayName() const noexcept
{
    return L"AMD Device Library eXtra";
}

bool AmdColorBackend::Initialize()
{
    if (initialized_)
    {
        return true;
    }

#if !defined(CHROMA_HAS_ADLX)
    return false;
#else
    if (ADLX_FAILED(helper_.Initialize()))
    {
        return false;
    }

    adlx::IADLXDisplayServicesPtr displayServices;
    if (ADLX_FAILED(helper_.GetSystemServices()->GetDisplaysServices(&displayServices)))
    {
        Shutdown();
        return false;
    }

    adlx::IADLXDisplayListPtr displayList;
    if (ADLX_FAILED(displayServices->GetDisplays(&displayList)))
    {
        Shutdown();
        return false;
    }

    displays_.clear();
    for (adlx_uint index = 0; index < displayList->Size(); ++index)
    {
        adlx::IADLXDisplayPtr display;
        if (ADLX_FAILED(displayList->At(index, &display)))
        {
            continue;
        }

        adlx::IADLXDisplayCustomColorPtr customColor;
        if (ADLX_FAILED(displayServices->GetCustomColor(display, &customColor)))
        {
            continue;
        }

        adlx_bool supported = false;
        if (ADLX_FAILED(customColor->IsSaturationSupported(&supported)) || !supported)
        {
            continue;
        }

        DisplayColor entry;
        entry.customColor = customColor;
        if (ADLX_FAILED(customColor->GetSaturationRange(&entry.range)))
        {
            continue;
        }

        adlx_int current = 0;
        if (ADLX_SUCCEEDED(customColor->GetSaturation(&current)))
        {
            entry.normalValue = current;
        }
        else
        {
            entry.normalValue =
                entry.range.minValue +
                ((entry.range.maxValue - entry.range.minValue) / 2);
        }

        displays_.push_back(entry);
    }

    initialized_ = !displays_.empty();
    if (!initialized_)
    {
        Shutdown();
    }
    return initialized_;
#endif
}

void AmdColorBackend::Shutdown() noexcept
{
#if defined(CHROMA_HAS_ADLX)
    displays_.clear();
    if (initialized_)
    {
        (void)helper_.Terminate();
    }
#endif
    initialized_ = false;
}

bool AmdColorBackend::IsInitialized() const noexcept
{
    return initialized_;
}

bool AmdColorBackend::SetSaturation(double saturation)
{
#if !defined(CHROMA_HAS_ADLX)
    (void)saturation;
    return false;
#else
    if (!initialized_ || displays_.empty())
    {
        return false;
    }

    bool changedAnyDisplay = false;
    for (const DisplayColor& display : displays_)
    {
        const adlx_int value = ToAdlxSaturation(saturation, display);
        if (ADLX_SUCCEEDED(display.customColor->SetSaturation(value)))
        {
            changedAnyDisplay = true;
        }
    }
    return changedAnyDisplay;
#endif
}

#if defined(CHROMA_HAS_ADLX)
adlx_int AmdColorBackend::ToAdlxSaturation(
    double saturation,
    const DisplayColor& display) noexcept
{
    const double clamped = std::clamp(
        saturation,
        kMinimumSaturation,
        kMaximumSaturation);

    double value = static_cast<double>(display.normalValue);
    if (clamped <= kNormalSaturation)
    {
        const double amount = clamped / kNormalSaturation;
        value = display.range.minValue +
            amount * (display.normalValue - display.range.minValue);
    }
    else
    {
        const double amount =
            (clamped - kNormalSaturation) /
            (kMaximumSaturation - kNormalSaturation);
        value = display.normalValue +
            amount * (display.range.maxValue - display.normalValue);
    }

    adlx_int rounded = static_cast<adlx_int>(std::lround(value));
    if (display.range.step > 1)
    {
        const adlx_int offset = rounded - display.range.minValue;
        rounded = display.range.minValue +
            static_cast<adlx_int>(std::lround(
                static_cast<double>(offset) / display.range.step)) *
                display.range.step;
    }

    return std::clamp(
        rounded,
        display.range.minValue,
        display.range.maxValue);
}
#endif
}
