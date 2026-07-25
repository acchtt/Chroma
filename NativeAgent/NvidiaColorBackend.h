#pragma once

#include "IColorBackend.h"

#include <windows.h>

#include <vector>

namespace Chroma
{
class NvidiaColorBackend final : public IColorBackend
{
public:
    ~NvidiaColorBackend() override;

    std::wstring_view GetId() const noexcept override;
    std::wstring_view GetDisplayName() const noexcept override;

    bool Initialize() override;
    void Shutdown() noexcept override;
    bool IsInitialized() const noexcept override;
    bool SetSaturation(double saturation) override;

private:
    struct DvcInfo
    {
        unsigned int version = 0;
        int currentLevel = 0;
        int minLevel = 0;
        int maxLevel = 0;
    };

    using NvStatus = int;
    using DisplayHandle = void*;
    using QueryInterfaceFn = void*(__cdecl*)(unsigned int);
    using InitializeFn = NvStatus(__cdecl*)();
    using UnloadFn = NvStatus(__cdecl*)();
    using EnumDisplayHandleFn = NvStatus(__cdecl*)(int, DisplayHandle*);
    using GetDvcInfoFn = NvStatus(__cdecl*)(DisplayHandle, int, DvcInfo*);
    using SetDvcLevelFn = NvStatus(__cdecl*)(DisplayHandle, int, int);

    bool ResolveFunctions();
    bool EnumerateDisplays();
    static int ToDvcLevel(double saturation, const DvcInfo& info) noexcept;

    HMODULE module_ = nullptr;
    InitializeFn initialize_ = nullptr;
    UnloadFn unload_ = nullptr;
    EnumDisplayHandleFn enumDisplayHandle_ = nullptr;
    GetDvcInfoFn getDvcInfo_ = nullptr;
    SetDvcLevelFn setDvcLevel_ = nullptr;
    std::vector<DisplayHandle> displays_;
    bool initialized_ = false;
};
}