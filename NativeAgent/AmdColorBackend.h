#pragma once

#include "IColorBackend.h"

#if defined(CHROMA_HAS_ADLX)
#include "SDK/ADLXHelper/Windows/Cpp/ADLXHelper.h"
#include "SDK/Include/IDisplaySettings.h"
#include "SDK/Include/IDisplays.h"

#include <vector>
#endif

namespace Chroma
{
class AmdColorBackend final : public IColorBackend
{
public:
    ~AmdColorBackend() override;

    std::wstring_view GetId() const noexcept override;
    std::wstring_view GetDisplayName() const noexcept override;

    bool Initialize() override;
    void Shutdown() noexcept override;
    bool IsInitialized() const noexcept override;
    bool SetSaturation(double saturation) override;

private:
#if defined(CHROMA_HAS_ADLX)
    struct DisplayColor
    {
        adlx::IADLXDisplayCustomColorPtr customColor;
        ADLX_IntRange range{};
        adlx_int normalValue = 0;
    };

    static adlx_int ToAdlxSaturation(
        double saturation,
        const DisplayColor& display) noexcept;

    adlx::ADLXHelper helper_;
    std::vector<DisplayColor> displays_;
#endif
    bool initialized_ = false;
};
}
