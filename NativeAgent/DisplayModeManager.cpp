#include "DisplayModeManager.h"

#include "Logger.h"

#include <cmath>

namespace Chroma
{
namespace
{
bool RefreshRatesMatch(DWORD first, DWORD second)
{
    if (first == 0 || second == 0)
    {
        return true;
    }

    return std::abs(static_cast<int>(first) - static_cast<int>(second)) <= 1;
}
}

DisplayModeManager::~DisplayModeManager()
{
    Restore();
}

bool DisplayModeManager::Apply(HWND targetWindow, int width, int height)
{
    if (width <= 0 || height <= 0)
    {
        return false;
    }

    if (hasOriginalMode_ && appliedWidth_ == width && appliedHeight_ == height)
    {
        return true;
    }

    if (hasOriginalMode_ && !Restore())
    {
        return false;
    }

    HWND window = targetWindow != nullptr ? targetWindow : GetForegroundWindow();
    HMONITOR monitor = MonitorFromWindow(window, MONITOR_DEFAULTTOPRIMARY);
    if (monitor == nullptr)
    {
        Log(LogLevel::Warning, L"Could not resolve the game display for the custom resolution");
        return false;
    }

    MONITORINFOEXW monitorInfo{};
    monitorInfo.cbSize = sizeof(monitorInfo);
    if (!GetMonitorInfoW(monitor, &monitorInfo))
    {
        LogLastError(LogLevel::Warning, L"GetMonitorInfoW failed", GetLastError());
        return false;
    }

    DEVMODEW currentMode{};
    currentMode.dmSize = sizeof(currentMode);
    if (!EnumDisplaySettingsExW(
            monitorInfo.szDevice,
            ENUM_CURRENT_SETTINGS,
            &currentMode,
            0))
    {
        LogLastError(LogLevel::Warning, L"Could not read the current display mode", GetLastError());
        return false;
    }

    if (static_cast<int>(currentMode.dmPelsWidth) == width &&
        static_cast<int>(currentMode.dmPelsHeight) == height)
    {
        Log(LogLevel::Info,
            L"Custom resolution already active: " +
            std::to_wstring(width) + L"x" + std::to_wstring(height));
        return true;
    }

    DEVMODEW selectedMode{};
    bool found = false;
    for (DWORD index = 0;; ++index)
    {
        DEVMODEW candidate{};
        candidate.dmSize = sizeof(candidate);
        if (!EnumDisplaySettingsExW(monitorInfo.szDevice, index, &candidate, 0))
        {
            break;
        }

        if (static_cast<int>(candidate.dmPelsWidth) != width ||
            static_cast<int>(candidate.dmPelsHeight) != height ||
            candidate.dmBitsPerPel != currentMode.dmBitsPerPel ||
            !RefreshRatesMatch(candidate.dmDisplayFrequency, currentMode.dmDisplayFrequency))
        {
            continue;
        }

        selectedMode = candidate;
        found = true;
        break;
    }

    if (!found)
    {
        Log(LogLevel::Warning,
            L"The graphics driver does not expose " +
            std::to_wstring(width) + L"x" + std::to_wstring(height) +
            L" at the current refresh rate");
        return false;
    }

    const LONG testResult = ChangeDisplaySettingsExW(
        monitorInfo.szDevice,
        &selectedMode,
        nullptr,
        CDS_TEST,
        nullptr);
    if (testResult != DISP_CHANGE_SUCCESSFUL)
    {
        Log(LogLevel::Warning,
            L"Windows rejected the requested custom resolution during validation: " +
            std::to_wstring(testResult));
        return false;
    }

    const LONG applyResult = ChangeDisplaySettingsExW(
        monitorInfo.szDevice,
        &selectedMode,
        nullptr,
        CDS_FULLSCREEN,
        nullptr);
    if (applyResult != DISP_CHANGE_SUCCESSFUL)
    {
        Log(LogLevel::Warning,
            L"Windows could not apply the requested custom resolution: " +
            std::to_wstring(applyResult));
        return false;
    }

    deviceName_ = monitorInfo.szDevice;
    originalMode_ = currentMode;
    appliedWidth_ = width;
    appliedHeight_ = height;
    hasOriginalMode_ = true;

    Log(LogLevel::Info,
        L"Applied custom resolution " +
        std::to_wstring(width) + L"x" + std::to_wstring(height) +
        L" at " + std::to_wstring(selectedMode.dmDisplayFrequency) + L" Hz");
    return true;
}

bool DisplayModeManager::Restore()
{
    if (!hasOriginalMode_)
    {
        return true;
    }

    const LONG result = ChangeDisplaySettingsExW(
        deviceName_.c_str(),
        &originalMode_,
        nullptr,
        CDS_FULLSCREEN,
        nullptr);
    if (result != DISP_CHANGE_SUCCESSFUL)
    {
        Log(LogLevel::Warning,
            L"Windows could not restore the previous desktop display mode: " +
            std::to_wstring(result));
        return false;
    }

    Log(LogLevel::Info,
        L"Restored desktop resolution " +
        std::to_wstring(originalMode_.dmPelsWidth) + L"x" +
        std::to_wstring(originalMode_.dmPelsHeight) + L" at " +
        std::to_wstring(originalMode_.dmDisplayFrequency) + L" Hz");

    deviceName_.clear();
    originalMode_ = {};
    appliedWidth_ = 0;
    appliedHeight_ = 0;
    hasOriginalMode_ = false;
    return true;
}

bool DisplayModeManager::IsApplied() const noexcept
{
    return hasOriginalMode_;
}
}
