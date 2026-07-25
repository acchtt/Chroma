#include "ColorBackendFactory.h"

#include "IntelColorBackend.h"
#include "NvidiaColorBackend.h"

namespace Chroma
{
std::unique_ptr<IColorBackend> CreateDefaultColorBackend()
{
    auto intel = std::make_unique<IntelColorBackend>();
    if (intel->Initialize())
    {
        return intel;
    }

    auto nvidia = std::make_unique<NvidiaColorBackend>();
    if (nvidia->Initialize())
    {
        return nvidia;
    }

    // Preserve the existing Intel backend as the final object so runtime
    // initialization fails in the same way it did before vendor fallback.
    return std::make_unique<IntelColorBackend>();
}
}