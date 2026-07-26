cmake_minimum_required(VERSION 3.20)

project(ChromaNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

find_program(DOTNET_EXECUTABLE dotnet REQUIRED)
set(CHROMA_ICON_SOURCE "${CMAKE_CURRENT_SOURCE_DIR}/../Chroma.WinUI/Assets/Chroma.png")
set(CHROMA_ICON_OUTPUT "${CMAKE_CURRENT_SOURCE_DIR}/../Chroma.WinUI/Assets/Chroma.ico")
set(CHROMA_ICON_BUILDER "${CMAKE_CURRENT_SOURCE_DIR}/../tools/Chroma.IconBuilder/Chroma.IconBuilder.csproj")

add_custom_target(ChromaIcon
    COMMAND "${DOTNET_EXECUTABLE}" run
        --project "${CHROMA_ICON_BUILDER}"
        --configuration Release
        --
        "${CHROMA_ICON_SOURCE}"
        "${CHROMA_ICON_OUTPUT}"
    BYPRODUCTS "${CHROMA_ICON_OUTPUT}"
    COMMENT "Generating current multi-resolution Chroma icon"
    VERBATIM
)

set(CHROMA_ADLX_ROOT "" CACHE PATH "Path to the root of the AMD ADLX SDK checkout")
if(NOT CHROMA_ADLX_ROOT AND DEFINED ENV{ADLX_SDK_ROOT})
    set(CHROMA_ADLX_ROOT "$ENV{ADLX_SDK_ROOT}")
endif()
if(NOT CHROMA_ADLX_ROOT AND EXISTS "${CMAKE_CURRENT_SOURCE_DIR}/../third_party/ADLX/SDK")
    set(CHROMA_ADLX_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../third_party/ADLX")
endif()

set(CHROMA_AGENT_SOURCES
    agent_main.cpp
    AgentTray.cpp
    AppState.cpp
    AgentClient.cpp
    Logger.cpp
    Settings.cpp
    ChromaRuntime.cpp
    ColorBackendFactory.cpp
    IntelColorBackend.cpp
    NvidiaColorBackend.cpp
    AmdColorBackend.cpp
    IGCLManager.cpp
    GameMonitor.cpp
    ForegroundWatcher.cpp
    HueSaturation.cpp
    cApiWrapper.cpp
    ColorAlgorithms_App.cpp
    Chroma.rc
)

if(CHROMA_ADLX_ROOT)
    set(CHROMA_ADLX_HELPER "${CHROMA_ADLX_ROOT}/SDK/ADLXHelper/Windows/Cpp/ADLXHelper.cpp")
    set(CHROMA_ADLX_WIN_APIS "${CHROMA_ADLX_ROOT}/SDK/Platform/Windows/WinAPIs.cpp")
    if(EXISTS "${CHROMA_ADLX_HELPER}" AND EXISTS "${CHROMA_ADLX_WIN_APIS}")
        list(APPEND CHROMA_AGENT_SOURCES
            "${CHROMA_ADLX_HELPER}"
            "${CHROMA_ADLX_WIN_APIS}"
        )
        set(CHROMA_HAS_ADLX ON)
        message(STATUS "AMD ADLX support enabled from ${CHROMA_ADLX_ROOT}")
    else()
        message(WARNING "CHROMA_ADLX_ROOT is set, but required ADLX helper/platform sources were not found; AMD support is disabled")
    endif()
else()
    message(STATUS "AMD ADLX SDK not found; building without AMD support")
endif()

add_executable(ChromaAgent WIN32 ${CHROMA_AGENT_SOURCES})
add_dependencies(ChromaAgent ChromaIcon)

set_target_properties(ChromaAgent PROPERTIES OUTPUT_NAME "Chroma.Agent")
if(MSVC)
    # Keep the release agent portable when the WinUI runtime is bundled into
    # Chroma.exe instead of shipping VC runtime DLLs beside the agent.
    set_property(
        TARGET ChromaAgent
        PROPERTY MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>"
    )
endif()
target_compile_definitions(ChromaAgent PRIVATE UNICODE _UNICODE)
if(CHROMA_HAS_ADLX)
    target_compile_definitions(ChromaAgent PRIVATE CHROMA_HAS_ADLX=1)
    target_include_directories(ChromaAgent PRIVATE "${CHROMA_ADLX_ROOT}")
endif()
target_link_libraries(ChromaAgent PRIVATE
    Comctl32
    Shell32
    Advapi32
    Dwmapi
    UxTheme
    Version
)

include(CTest)
if(BUILD_TESTING)
    add_executable(ChromaProfileMatchingTests
        tests/ProfileMatchingTests.cpp
        GameMonitor.cpp
    )
    target_compile_definitions(ChromaProfileMatchingTests PRIVATE UNICODE _UNICODE)
    target_link_libraries(ChromaProfileMatchingTests PRIVATE Shell32)
    add_test(NAME ProfileMatching COMMAND ChromaProfileMatchingTests)
endif()
