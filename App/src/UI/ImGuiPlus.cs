using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;

namespace MinecraftCloneSilk.UI;

public static class ImGuiPlus
{
    public static unsafe bool BeginTabItemNoClose(string label, ImGuiTabItemFlags flags)
    {
        int utf8ByteCount = 0;
        byte* numPtr;
        if (label != null)
        {
            utf8ByteCount = Encoding.UTF8.GetByteCount(label);
            numPtr = Allocate(utf8ByteCount + 1);
            int utf8 = GetUtf8(label, numPtr, utf8ByteCount);
            numPtr[utf8] = (byte) 0;
        }
        else
            numPtr = (byte*) null;
        int num2 = (int) ImGuiNative.igBeginTabItem(numPtr, null, flags);
        Free(numPtr);
        return (uint) num2 > 0U;
    }
    internal static unsafe void Free(byte* ptr) => Marshal.FreeHGlobal((IntPtr) (void*) ptr);
    internal static unsafe byte* Allocate(int byteCount)
    {
        return (byte*) (void*) Marshal.AllocHGlobal(byteCount);
    }
    internal static unsafe int GetUtf8(string s, byte* utf8Bytes, int utf8ByteCount)
    {
        IntPtr chars;
        if (s == null)
        {
            chars = IntPtr.Zero;
        }
        else
        {
            fixed (char* chPtr = &s.GetPinnableReference())
                chars = (IntPtr) chPtr;
        }
        return Encoding.UTF8.GetBytes((char*) chars, s.Length, utf8Bytes, utf8ByteCount);
    }
 
}