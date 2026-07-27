#include "ChromaRuntime.h"

#include "ColorBackendFactory.h"
#include "GameMonitor.h"
#include "Logger.h"
#include "ResolutionOverride.h"

#include <utility>

namespace Chroma
{
namespace
{
constexpr double kNormalSaturation = 1.0;
}

ChromaRuntime::ChromaRuntime()
    : ChromaRuntime(CreateDefaultColorBackend())
{
}

ChromaRuntime::ChromaRuntime(
    std::unique_ptr<IColorBackend> backend)
    : backend_(std::move(backend))
{
}

ChromaRuntime::~ChromaRuntime()
{
    Shutdown();
}

void ChromaRuntime::AttachProfiles(
    std::vector<GameProfile>* profiles)
{
    profiles_ = profiles;

    if (status_.activeProfileIndex >= 0 &&
        (profiles_ == nullptr ||
         status_.activeProfileIndex >= static_cast<int>(profiles_->size())))
    {
        RestoreDesktop();
    }
}

bool ChromaRuntime::Initialize()
{
    if (status_.initialized)
    {
        return true;
    }

    status_.initialized = backend_ != nullptr && backend_->Initialize();
    return status_.initialized;
}

void ChromaRuntime::Shutdown()
{
    if (!status_.initialized)
    {
        displayModeManager_.Restore();
        return;
    }

    RestoreDesktop();
    backend_->Shutdown();
    status_ = {};
}

bool ChromaRuntime::HandleForegroundWindow(
    HWND foregroundWindow)
{
    if (!status_.initialized || profiles_ == nullptr)
    {
        return false;
    }

    HWND targetWindow = foregroundWindow != nullptr
        ? foregroundWindow
        : GetForegroundWindow();

    const std::wstring foregroundPath =
        GameMonitor::GetForegroundExecutablePath();

    const int matchingProfileIndex =
        FindMatchingProfile(foregroundPath);

    if (matchingProfileIndex == status_.activeProfileIndex)
    {
        return false;
    }

    if (matchingProfileIndex >= 0)
    {
        return ApplyProfile(
            matchingProfileIndex,
            foregroundPath,
            targetWindow);
    }

    const bool wasGameActive = status_.gameActive;
    const bool restored = RestoreDesktop();
    return wasGameActive && restored;
}

bool ChromaRuntime::ReapplyActiveProfile()
{
    if (!status_.initialized ||
        profiles_ == nullptr ||
        status_.activeProfileIndex < 0 ||
        status_.activeProfileIndex >= static_cast<int>(profiles_->size()))
    {
        return false;
    }

    return ApplyProfile(
        status_.activeProfileIndex,
        status_.activeExecutablePath,
        GetForegroundWindow());
}

void ChromaRuntime::OnProfileRemoved(
    int removedIndex)
{
    if (removedIndex < 0)
    {
        return;
    }

    if (status_.activeProfileIndex == removedIndex)
    {
        RestoreDesktop();
    }
    else if (status_.activeProfileIndex > removedIndex)
    {
        --status_.activeProfileIndex;
    }
}

bool ChromaRuntime::RestoreDesktop()
{
    const bool displayRestored = displayModeManager_.Restore();

    bool saturationRestored = true;
    if (status_.initialized)
    {
        saturationRestored = backend_ != nullptr &&
            backend_->SetSaturation(kNormalSaturation);
    }

    status_.gameActive = false;
    status_.activeProfileIndex = -1;
    status_.appliedSaturationPercent = 100;
    status_.activeExecutablePath.clear();
    status_.activeExecutableName.clear();
    return displayRestored && saturationRestored;
}

const RuntimeStatus& ChromaRuntime::GetStatus() const noexcept
{
    return status_;
}

const IColorBackend* ChromaRuntime::GetBackend() const noexcept
{
    return backend_.get();
}

bool ChromaRuntime::IsProfileActive(
    int profileIndex) const noexcept
{
    return status_.activeProfileIndex == profileIndex;
}

int ChromaRuntime::FindMatchingProfile(
    const std::wstring& executablePath) const
{
    if (profiles_ == nullptr || executablePath.empty())
    {
        return -1;
    }

    for (std::size_t index = 0; index < profiles_->size(); ++index)
    {
        if ((*profiles_)[index].enabled &&
            GameMonitor::PathsEqual(
                executablePath,
                (*profiles_)[index].executablePath))
        {
            return static_cast<int>(index);
        }
    }

    return -1;
}

bool ChromaRuntime::ApplyProfile(
    int profileIndex,
    const std::wstring& executablePath,
    HWND foregroundWindow)
{
    if (profiles_ == nullptr ||
        profileIndex < 0 ||
        profileIndex >= static_cast<int>(profiles_->size()))
    {
        return false;
    }

    if (displayModeManager_.IsApplied() && !displayModeManager_.Restore())
    {
        Log(LogLevel::Warning,
            L"Could not restore the previous display mode before applying a new profile");
        return false;
    }

    const GameProfile& profile = (*profiles_)[profileIndex];
    ResolutionOverride resolutionOverride;
    if (TryLoadResolutionOverride(profile.executablePath, resolutionOverride) &&
        !displayModeManager_.Apply(
            foregroundWindow,
            resolutionOverride.width,
            resolutionOverride.height))
    {
        Log(LogLevel::Warning,
            L"The profile will continue with saturation only because its custom resolution could not be applied");
    }

    const double saturation =
        static_cast<double>(profile.saturationPercent) / 100.0;

    if (backend_ == nullptr ||
        !backend_->SetSaturation(saturation))
    {
        displayModeManager_.Restore();
        return false;
    }

    status_.gameActive = true;
    status_.activeProfileIndex = profileIndex;
    status_.appliedSaturationPercent = profile.saturationPercent;
    status_.activeExecutablePath = executablePath;
    status_.activeExecutableName = profile.executableName;
    return true;
}
}
