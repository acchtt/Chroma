#include "ColorBackendFactory.h"

#include "IntelColorBackend.h"

namespace Chroma
{
std::unique_ptr<IColorBackend> CreateDefaultColorBackend()
{
    return std::make_unique<IntelColorBackend>();
}
}
