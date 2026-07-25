cmake_minimum_required(VERSION 3.20)

project(ChromaNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

add_executable(ChromaAgent WIN32
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
    IGCLManager.cpp
    GameMonitor.cpp
    ForegroundWatcher.cpp
    HueSaturation.cpp
    cApiWrapper.cpp
    ColorAlgorithms_App.cpp
    Chroma.rc
)

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
