using System;
using System.Runtime.InteropServices;

//this file is up to date with renderdoc api version 1.4.0.2





public static class RenderDoc
{
    [StructLayout(LayoutKind.Sequential)]
    public struct API 
    {
        #region enums
        public enum InputButton
        {
            // '0' - '9' matches ASCII values
            Key_0 = 0x30,
            Key_1 = 0x31,
            Key_2 = 0x32,
            Key_3 = 0x33,
            Key_4 = 0x34,
            Key_5 = 0x35,
            Key_6 = 0x36,
            Key_7 = 0x37,
            Key_8 = 0x38,
            Key_9 = 0x39,

            // 'A' - 'Z' matches ASCII values
            Key_A = 0x41,
            Key_B = 0x42,
            Key_C = 0x43,
            Key_D = 0x44,
            Key_E = 0x45,
            Key_F = 0x46,
            Key_G = 0x47,
            Key_H = 0x48,
            Key_I = 0x49,
            Key_J = 0x4A,
            Key_K = 0x4B,
            Key_L = 0x4C,
            Key_M = 0x4D,
            Key_N = 0x4E,
            Key_O = 0x4F,
            Key_P = 0x50,
            Key_Q = 0x51,
            Key_R = 0x52,
            Key_S = 0x53,
            Key_T = 0x54,
            Key_U = 0x55,
            Key_V = 0x56,
            Key_W = 0x57,
            Key_X = 0x58,
            Key_Y = 0x59,
            Key_Z = 0x5A,

            // leave the rest of the ASCII range free
            // in case we want to use it later
            Key_NonPrintable = 0x100,

            Divide,
            Multiply,
            Subtract,
            Plus,

            F1,
            F2,
            F3,
            F4,
            F5,
            F6,
            F7,
            F8,
            F9,
            F10,
            F11,
            F12,

            Home,
            End,
            Insert,
            Delete,
            PageUp,
            PageDn,

            Backspace,
            Tab,
            PrtScrn,
            Pause,

            Max,
        };

        public enum CaptureOption
        {
            // Allow the application to enable vsync
            //
            // Default - enabled
            //
            // 1 - The application can enable or disable vsync at will
            // 0 - vsync is force disabled
            AllowVSync = 0,

            // Allow the application to enable fullscreen
            //
            // Default - enabled
            //
            // 1 - The application can enable or disable fullscreen at will
            // 0 - fullscreen is force disabled
            AllowFullscreen = 1,

            // Record API debugging events and messages
            //
            // Default - disabled
            //
            // 1 - Enable built-in API debugging features and records the results into
            //     the capture, which is matched up with events on replay
            // 0 - no API debugging is forcibly enabled
            APIValidation = 2,

            // Capture CPU callstacks for API events
            //
            // Default - disabled
            //
            // 1 - Enables capturing of callstacks
            // 0 - no callstacks are captured
            CaptureCallstacks = 3,

            // When capturing CPU callstacks, only capture them from actions.
            // This option does nothing without the above option being enabled
            //
            // Default - disabled
            //
            // 1 - Only captures callstacks for actions.
            //     Ignored if CaptureCallstacks is disabled
            // 0 - Callstacks, if enabled, are captured for every event.
            CaptureCallstacksOnlyActions = 4,

            // Specify a delay in seconds to wait for a debugger to attach, after
            // creating or injecting into a process, before continuing to allow it to run.
            //
            // 0 indicates no delay, and the process will run immediately after injection
            //
            // Default - 0 seconds
            //
            DelayForDebugger = 5,

            // Verify buffer access. This includes checking the memory returned by a Map() call to
            // detect any out-of-bounds modification, as well as initialising buffers with undefined contents
            // to a marker value to catch use of uninitialised memory.
            //
            // NOTE: This option is only valid for OpenGL and D3D11. Explicit APIs such as D3D12 and Vulkan do
            // not do the same kind of interception & checking and undefined contents are really undefined.
            //
            // Default - disabled
            //
            // 1 - Verify buffer access
            // 0 - No verification is performed, and overwriting bounds may cause crashes or corruption in
            //     RenderDoc.
            VerifyBufferAccess = 6,

            // Hooks any system API calls that create child processes, and injects
            // RenderDoc into them recursively with the same options.
            //
            // Default - disabled
            //
            // 1 - Hooks into spawned child processes
            // 0 - Child processes are not hooked by RenderDoc
            HookIntoChildren = 7,

            // By default RenderDoc only includes resources in the final capture necessary
            // for that frame, this allows you to override that behaviour.
            //
            // Default - disabled
            //
            // 1 - all live resources at the time of capture are included in the capture
            //     and available for inspection
            // 0 - only the resources referenced by the captured frame are included
            RefAllResources = 8,

            // In APIs that allow for the recording of command lists to be replayed later,
            // RenderDoc may choose to not capture command lists before a frame capture is
            // triggered, to reduce overheads. This means any command lists recorded once
            // and replayed many times will not be available and may cause a failure to
            // capture.
            //
            // NOTE: This is only true for APIs where multithreading is difficult or
            // discouraged. Newer APIs like Vulkan and D3D12 will ignore this option
            // and always capture all command lists since the API is heavily oriented
            // around it and the overheads have been reduced by API design.
            //
            // 1 - All command lists are captured from the start of the application
            // 0 - Command lists are only captured if their recording begins during
            //     the period when a frame capture is in progress.
            CaptureAllCmdLists = 10,

            // Mute API debugging output when the API validation mode option is enabled
            //
            // Default - enabled
            //
            // 1 - Mute any API debug messages from being displayed or passed through
            // 0 - API debugging is displayed as normal
            DebugOutputMute = 11,

            // Option to allow vendor extensions to be used even when they may be
            // incompatible with RenderDoc and cause corrupted replays or crashes.
            //
            // Default - inactive
            //
            // No values are documented, this option should only be used when absolutely
            // necessary as directed by a RenderDoc developer.
            AllowUnsupportedVendorExtensions = 12,

        };

        public enum OverlayBits
        {
            // This single bit controls whether the overlay is enabled or disabled globally
            Enabled = 0x1,

            // Show the average framerate over several seconds as well as min/max
            FrameRate = 0x2,

            // Show the current frame number
            FrameNumber = 0x4,

            // Show a list of recent captures, and how many captures have been made
            CaptureList = 0x8,

            // Default values for the overlay mask
            Default = (Enabled | FrameRate |
                                          FrameNumber | CaptureList),

            // Enable all bits
            All = int.MaxValue,

            // Disable all bits
            None = 0,
        };
        #endregion

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GetApiVersionDelegate(IntPtr major, IntPtr minor, IntPtr patch);
        private IntPtr _GetApiVersion;
        public struct ApiVersion { public int major, minor, patch; }
        public ApiVersion GetApiVersion() 
        {
            ApiVersion version = new ApiVersion();
            IntPtr majorMemory = Marshal.AllocHGlobal(Marshal.SizeOf(version.major));
            IntPtr minorMemory = Marshal.AllocHGlobal(Marshal.SizeOf(version.minor));
            IntPtr patchMemory = Marshal.AllocHGlobal(Marshal.SizeOf(version.patch));
            try
            {
                Marshal.GetDelegateForFunctionPointer<GetApiVersionDelegate>(_GetApiVersion)(majorMemory, minorMemory, patchMemory);
                version.major = Marshal.ReadInt32(majorMemory);
                version.minor = Marshal.ReadInt32(minorMemory);
                version.patch = Marshal.ReadInt32(patchMemory);
            }
            finally
            {
                Marshal.FreeHGlobal(majorMemory);
                Marshal.FreeHGlobal(minorMemory);
                Marshal.FreeHGlobal(patchMemory);
            }
            return version;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SetCaptureOptionU32Delegate();
        private IntPtr _SetCaptureOptionU32;
        public int SetCaptureOptionU32() 
        {
            return Marshal.GetDelegateForFunctionPointer<SetCaptureOptionU32Delegate>(_SetCaptureOptionU32)();
        }

        //not supported, pretty useless tbh
        private IntPtr SetCaptureOptionF32;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetCaptureOptionU32Delegate(CaptureOption opt, uint val);
        private IntPtr _GetCaptureOptionU32;
        public int GetCaptureOptionU32(CaptureOption opt, uint val) 
        {
            return Marshal.GetDelegateForFunctionPointer<GetCaptureOptionU32Delegate>(_GetCaptureOptionU32)(opt, val);
        }

        //not supported, pretty useless too
        private IntPtr GetCaptureOptionF32;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SetKeysDelegate(IntPtr keys, int count);
        private IntPtr _SetFocusToggleKeys;
        public void SetFocusToggleKeys(InputButton[] keys)
        {
            IntPtr keysMemory = Marshal.AllocHGlobal(Marshal.SizeOf<InputButton>() * keys.Length);
            try
            {
                Marshal.GetDelegateForFunctionPointer<SetKeysDelegate>(_SetFocusToggleKeys)(keysMemory, keys.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(keysMemory);
            }
        }

        private IntPtr _SetCaptureKeys;
        public void SetCaptureKeys(InputButton[] keys)
        {
            IntPtr keysMemory = Marshal.AllocHGlobal(Marshal.SizeOf<InputButton>() * keys.Length);
            try
            {
                Marshal.GetDelegateForFunctionPointer<SetKeysDelegate>(_SetCaptureKeys)(keysMemory, keys.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(keysMemory);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint GetOverlayBitsDelegate();
        private IntPtr _GetOverlayBits;
        public uint GetOverlayBits() 
        {
            return Marshal.GetDelegateForFunctionPointer<GetOverlayBitsDelegate>(_GetOverlayBits)();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MaskOverlayBitsDelegate(uint and, uint or);
        private IntPtr _MaskOverlayBits;
        public void MaskOverlayBits(uint and, uint or)
        {
            Marshal.GetDelegateForFunctionPointer<MaskOverlayBitsDelegate>(_MaskOverlayBits)(and, or);
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void RemoveHooksDelegate();
        private IntPtr _RemoveHooks; 
        public void RemoveHooks()
        {
            Marshal.GetDelegateForFunctionPointer<RemoveHooksDelegate>(_RemoveHooks)();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void UnloadCrashHandlerDelegate();
        private IntPtr _UnloadCrashHandler;
        public void UnloadCrashHandler() 
        {
            Marshal.GetDelegateForFunctionPointer<UnloadCrashHandlerDelegate>(_UnloadCrashHandler)();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void SetCaptureFilePathTemplateDelegate(string pathTemplate);
        private IntPtr _SetCaptureFilePathTemplate;
        public void SetCaptureFilePathTemplate(string pathTemplate)
        {
            Marshal.GetDelegateForFunctionPointer<SetCaptureFilePathTemplateDelegate>(_SetCaptureFilePathTemplate)(pathTemplate);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate string GetCaptureFilePathTemplateDelegate();
        private IntPtr _GetCaptureFilePathTemplate;
        public string GetCaptureFilePathTemplate() 
        {
            return Marshal.GetDelegateForFunctionPointer<GetCaptureFilePathTemplateDelegate>(_GetCaptureFilePathTemplate)();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint GetNumCapturesDelegate();
        private IntPtr _GetNumCaptures;
        public uint GetNumCaptures()
        {
            return Marshal.GetDelegateForFunctionPointer<GetNumCapturesDelegate>(_GetNumCaptures)();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint GetCaptureDelegate(uint index, IntPtr filename, IntPtr pathLength, IntPtr timestamp);
        private IntPtr _GetCapture;
        public uint GetCapture(uint index, IntPtr filename, IntPtr pathLength, IntPtr timestamp) 
        {
            return Marshal.GetDelegateForFunctionPointer<GetCaptureDelegate>(_GetCapture)(index, filename, pathLength, timestamp);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void TriggerCaptureDelegate();
        private IntPtr _TriggerCapture;
        public void TriggerCapture()
        {
            Marshal.GetDelegateForFunctionPointer<TriggerCaptureDelegate>(_TriggerCapture)();
        }

        //deprecated
        private IntPtr IsTargetControlConnected;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate uint LaunchReplayUIDelegate(uint connectTargetControl, string cmdLine);
        private IntPtr _LaunchReplayUI;
        public uint LaunchReplayUI(uint connectTargetControl, string cmdLine) 
        {
            return Marshal.GetDelegateForFunctionPointer<LaunchReplayUIDelegate>(_LaunchReplayUI)(connectTargetControl, cmdLine);
        }

        //useless for Unity applications
        private IntPtr SetActiveWindow;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void StartFrameCaptureDelegate(IntPtr devicePointer, IntPtr windowHandle);
        private IntPtr _StartFrameCapture;
        public void StartFrameCapture(IntPtr devicePointer, IntPtr windowHandle) 
        {
            Marshal.GetDelegateForFunctionPointer<StartFrameCaptureDelegate>(_StartFrameCapture)(devicePointer, windowHandle);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void IsFrameCapturingDelegate();
        private IntPtr _IsFrameCapturing;
        public void IsFrameCapturing()
        {
            Marshal.GetDelegateForFunctionPointer<IsFrameCapturingDelegate>(_IsFrameCapturing)();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint EndFrameCaptureDelegate(IntPtr devicePointer, IntPtr windowHandle);
        private IntPtr _EndFrameCapture;
        public uint EndFrameCapture(IntPtr devicePointer, IntPtr windowHandle) 
        {
            return Marshal.GetDelegateForFunctionPointer<EndFrameCaptureDelegate>(_EndFrameCapture)(devicePointer, windowHandle);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void TriggerMultiFrameCaptureDelegate(uint numFrames);
        private IntPtr _TriggerMultiFrameCapture;
        public void TriggerMultiFrameCapture(uint numFrames) 
        {
            Marshal.GetDelegateForFunctionPointer<TriggerMultiFrameCaptureDelegate>(_TriggerMultiFrameCapture)(numFrames);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
        private delegate uint SetCaptureFileCommentsDelegate(string filePath, string comments);
        private IntPtr _SetCaptureFileComments;
        public uint SetCaptureFileComments(string filePath, string comments) 
        {
            return Marshal.GetDelegateForFunctionPointer<SetCaptureFileCommentsDelegate>(_SetCaptureFileComments)(filePath, comments);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint DiscardFrameCaptureDelegate(IntPtr devicePointer, IntPtr windowHandle);
        private IntPtr _DiscardFrameCapture;
        public uint DiscardFrameCapture(IntPtr devicePointer, IntPtr windowHandle) 
        {
            return Marshal.GetDelegateForFunctionPointer<DiscardFrameCaptureDelegate>(_DiscardFrameCapture)(devicePointer, windowHandle);
        }
    }

    const int RENDERDOC_VERSION = 10402;


    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    delegate int GetRenderDocAPIDelegate(int version, out IntPtr ptrToAPI);
    private static bool apiSet = false;
    private static API api = new API();
    public static API? Api
    {
        get
        {
            if (!apiSet)
            {
                //load the dll
                IntPtr module = LoadLibrary("renderdoc.dll");

                if (module == IntPtr.Zero)
                {
                    Console.WriteLine("Could not load renderdoc dll!");
                    return null;
                }

                //get the address to the "get api" function from the dll
                IntPtr address = GetProcAddress(module, "RENDERDOC_GetAPI");

                if (address == IntPtr.Zero)
                {
                    Console.WriteLine("Could not get RENDERDOC_GetAPI function from renderdoc dll!");
                    return null;
                }

                //get a delegate which will call the "get api" function
                var getRenderDocAPI = Marshal.GetDelegateForFunctionPointer<GetRenderDocAPIDelegate>(address);
                //allocate some unmanaged memory to hold the API struct
                IntPtr apiMemory = IntPtr.Zero;
                //actually call the "get api" function
                if (getRenderDocAPI(RENDERDOC_VERSION, out apiMemory) == 1)
                {
                    //apiMemory now contains the filled API struct, we can assign that to api
                    api = Marshal.PtrToStructure<API>(apiMemory);
                    apiSet = true;
                }
                else
                {
                    Console.WriteLine("RENDERDOC_GetAPI did not fill the API struct, something has gone terribly wrong!");
                    return null;
                }
            }
            return api;
        }
    }

    public class CaptureScope : IDisposable
    {
        string comment;
        IntPtr devicePointer;
        IntPtr windowHandle;
        public CaptureScope(string comment = null, int devicePointer = 0, int windowHandle = 0) 
        {
            this.comment = comment;
            this.devicePointer = (IntPtr)devicePointer;
            this.windowHandle = (IntPtr)windowHandle;
            Api?.StartFrameCapture(this.devicePointer, this.windowHandle);
        }

        public void Dispose()
        {
            Api?.EndFrameCapture(devicePointer, windowHandle);
            if(comment != null) 
            {
                //null means "choose the most recent file"
                Api?.SetCaptureFileComments(null, comment);
            }
        }
    }
}
