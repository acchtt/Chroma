#include "ResolutionOverride.h"

#include "GameMonitor.h"
#include "Logger.h"

#include <Windows.h>
#include <filesystem>
#include <fstream>
#include <iomanip>

namespace Chroma
{
namespace
{
constexpr int kMinimumWidth = 640;
constexpr int kMaximumWidth = 16384;
constexpr int kMinimumHeight = 480;
constexpr int kMaximumHeight = 8640;

bool GetResolutionFilePath(std::filesystem::path& path)
{
    wchar_t localAppData[MAX_PATH] = {};
    const DWORD length = GetEnvironmentVariableW(
        L"LOCALAPPDATA",
        localAppData,
        MAX_PATH);
    if (length == 0 || length >= MAX_PATH)
    {
        return false;
    }

    path = std::filesystem::path(localAppData) / L"Chroma" / L"resolutions.txt";
    return true;
}

bool IsValidResolution(int width, int height)
{
    return width >= kMinimumWidth && width <= kMaximumWidth &&
           height >= kMinimumHeight && height <= kMaximumHeight;
}
}

bool TryLoadResolutionOverride(
    const std::wstring& executablePath,
    ResolutionOverride& resolutionOverride)
{
    std::filesystem::path filePath;
    if (!GetResolutionFilePath(filePath) || !std::filesystem::exists(filePath))
    {
        return false;
    }

    std::wifstream input(filePath);
    if (!input.is_open())
    {
        Log(LogLevel::Warning, L"Could not open the custom resolution file");
        return false;
    }

    std::wstring formatName;
    int formatVersion = 0;
    if (!(input >> formatName >> formatVersion) ||
        formatName != L"ChromaResolutions" ||
        formatVersion != 1)
    {
        Log(LogLevel::Warning, L"Unsupported custom resolution file format");
        return false;
    }

    std::wstring configuredPath;
    int width = 0;
    int height = 0;
    while (input >> std::quoted(configuredPath) >> width >> height)
    {
        if (!IsValidResolution(width, height))
        {
            continue;
        }

        if (GameMonitor::PathsEqual(configuredPath, executablePath))
        {
            resolutionOverride.executablePath = configuredPath;
            resolutionOverride.width = width;
            resolutionOverride.height = height;
            return true;
        }
    }

    if (!input.eof())
    {
        Log(LogLevel::Warning, L"The custom resolution file contains an invalid entry");
    }

    return false;
}
}
