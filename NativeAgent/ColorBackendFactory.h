#pragma once

#include <memory>

#include "IColorBackend.h"

namespace Chroma
{
// Central backend selection point. Vendor detection and external plugin
// discovery can be added here without coupling ChromaRuntime to them.
std::unique_ptr<IColorBackend> CreateDefaultColorBackend();
}
