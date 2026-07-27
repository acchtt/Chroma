#pragma once

#include <Windows.h>
#include <string>

namespace Chroma
{
class DisplayModeManager
{
public:
    ~DisplayModeManager();

    bool Apply(HWND targetWindow, int width, int height);
    bool Restore();
    bool IsApplied() const noexcept;

private:
    std::wstring deviceName_;
    DEVMODEW originalMode_{};
    int appliedWidth_ = 0;
    int appliedHeight_ = 0;
    bool hasOriginalMode_ = false;
};
}
