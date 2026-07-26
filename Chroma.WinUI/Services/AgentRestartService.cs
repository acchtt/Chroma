using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace Chroma.Services;

internal static class AgentRestartService
{
    private const string PipeName = "Chroma.Agent.v1";
    private const uint ProtocolVersion = 1;

    public static async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        AgentResponse? response = await SendAsync(AgentCommand.Shutdown, cancellationToken);
        if (response is not { Success: true })
        {
            return;
        }

        // The agent acknowledges Shutdown before its hidden window finishes
        // destroying the runtime and tray icon. Wait until the pipe disappears so
        // the UI cannot accidentally reconnect to the old backend instance.
        for (int attempt = 0; attempt < 40; attempt++)
        {
            await Task.Delay(100, cancellationToken);
            if (await SendAsync(AgentCommand.GetStatus, cancellationToken, TimeSpan.FromMilliseconds(120)) is null)
            {
                return;
            }
        }
    }

    private static Task<AgentResponse?> SendAsync(
        AgentCommand command,
        CancellationToken cancellationToken) =>
        SendAsync(command, cancellationToken, TimeSpan.FromSeconds(2));

    private static async Task<AgentResponse?> SendAsync(
        AgentCommand command,
        CancellationToken cancellationToken,
        TimeSpan timeoutValue)
    {
        var request = new AgentRequest
        {
            Size = (uint)Marshal.SizeOf<AgentRequest>(),
            ProtocolVersion = ProtocolVersion,
            Command = command,
            RequestId = unchecked((uint)Environment.TickCount)
        };

        try
        {
            using var pipe = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(timeoutValue);

            await pipe.ConnectAsync(timeout.Token);
            byte[] requestBytes = StructureToBytes(request);
            await pipe.WriteAsync(requestBytes, timeout.Token);
            await pipe.FlushAsync(timeout.Token);

            byte[] responseBytes = new byte[Marshal.SizeOf<AgentResponse>()];
            int total = 0;
            while (total < responseBytes.Length)
            {
                int read = await pipe.ReadAsync(responseBytes.AsMemory(total), timeout.Token);
                if (read == 0)
                {
                    return null;
                }
                total += read;
            }

            AgentResponse response = BytesToStructure<AgentResponse>(responseBytes);
            return response.Size == (uint)Marshal.SizeOf<AgentResponse>() &&
                   response.ProtocolVersion == ProtocolVersion &&
                   response.RequestId == request.RequestId
                ? response
                : null;
        }
        catch (Exception exception) when (
            exception is IOException or TimeoutException or OperationCanceledException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static byte[] StructureToBytes<T>(T value) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] bytes = new byte[size];
        IntPtr pointer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(value, pointer, false);
            Marshal.Copy(pointer, bytes, 0, size);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private static T BytesToStructure<T>(byte[] bytes) where T : struct
    {
        IntPtr pointer = Marshal.AllocHGlobal(bytes.Length);
        try
        {
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            return Marshal.PtrToStructure<T>(pointer);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private enum AgentCommand : uint
    {
        GetStatus = 1,
        Shutdown = 5
    }

    private enum AgentError : uint
    {
        None = 0,
        InvalidRequest = 1,
        UnsupportedProtocol = 2,
        RuntimeFailure = 3,
        ProfileLoadFailure = 4
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct AgentRequest
    {
        public uint Size;
        public uint ProtocolVersion;
        public AgentCommand Command;
        public uint RequestId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    private struct NativeAgentStatus
    {
        public uint Size;
        public uint ProtocolVersion;
        [MarshalAs(UnmanagedType.Bool)] public bool AgentRunning;
        [MarshalAs(UnmanagedType.Bool)] public bool RuntimeInitialized;
        [MarshalAs(UnmanagedType.Bool)] public bool GameActive;
        public int ActiveProfileIndex;
        public int AppliedSaturationPercent;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string ActiveExecutableName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    private struct AgentResponse
    {
        public uint Size;
        public uint ProtocolVersion;
        public uint RequestId;
        [MarshalAs(UnmanagedType.Bool)] public bool Success;
        public AgentError Error;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 160)] public string ErrorMessage;
        public NativeAgentStatus Status;
    }
}
