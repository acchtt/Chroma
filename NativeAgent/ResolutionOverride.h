#pragma once

#include <string>

namespace Chroma
{
struct ResolutionOverride
{
    std::wstring executablePath;
    int width = 0;
    int height = 0;
};

bool TryLoadResolutionOverride(
    const std::wstring& executablePath,
    ResolutionOverride& resolutionOverride);
}
