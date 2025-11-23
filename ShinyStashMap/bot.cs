using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
namespace ShinyStashMap;

public class bot
{
    private static readonly Encoding Encoder = Encoding.UTF8;
    private static byte[] Encode(string command, bool addrn = true) => Encoder.GetBytes(addrn ? command + "\r\n" : command);
    Socket socket;
    public int MaximumTransferSize = 8192;
    private readonly object _sync = new();
    public bot() 
    {         
        socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    }
    public void Connect(string host, int port)
    {
        socket.Connect(host, port);
    }
    public void Disconnect()
    {
        socket.Disconnect(true);
    }
    public byte[] ReadBytes(long[] jumps, int length)
    {
        
        lock (_sync)
        {
            var cmd = PeekMainPointer(jumps, length);
            SendInternal(cmd);

            // give it time to push data back
            Thread.Sleep((length / 256));
            var buffer = new byte[(length * 2) + 1];
            var _ = ReadInternal(buffer);
            return ConvertHexByteStringToBytes(buffer);
        }
    }
    
    public static byte[] PeekMainPointer(long[] jumps, int count) => Encode($"pointerPeek {count}{string.Concat(jumps.Select(z => $" {z}"))}");
    private int SendInternal(byte[] buffer) => socket.Send(buffer);
    private int ReadInternal(byte[] buffer)
    {
        int br = socket.Receive(buffer, 0, 1, SocketFlags.None);
        while (buffer[br - 1] != (byte)'\n')
            br += socket.Receive(buffer, br, 1, SocketFlags.None);
        return br;
    }
    private static bool IsNum(char c) => (uint)(c - '0') <= 9;
    private static bool IsHexUpper(char c) => (uint)(c - 'A') <= 5;

    public static byte[] ConvertHexByteStringToBytes(byte[] bytes)
    {
        var dest = new byte[bytes.Length / 2];
        for (int i = 0; i < dest.Length; i++)
        {
            int ofs = i * 2;
            var _0 = (char)bytes[ofs + 0];
            var _1 = (char)bytes[ofs + 1];
            dest[i] = DecodeTuple(_0, _1);
        }
        return dest;
    }

    private static byte DecodeTuple(char _0, char _1)
    {
        byte result;
        if (IsNum(_0))
            result = (byte)((_0 - '0') << 4);
        else if (IsHexUpper(_0))
            result = (byte)((_0 - 'A' + 10) << 4);
        else
            throw new ArgumentOutOfRangeException(nameof(_0));

        if (IsNum(_1))
            result |= (byte)(_1 - '0');
        else if (IsHexUpper(_1))
            result |= (byte)(_1 - 'A' + 10);
        else
            throw new ArgumentOutOfRangeException(nameof(_1));
        return result;
    }

    public static byte[] StringToByteArray(string hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }
    public ulong FollowMainPointer(long[] jumps)
    {
        lock (_sync)
        {
            var cmd = MainPointer(jumps);
            SendInternal(cmd);

            // give it time to push data back
            Thread.Sleep(1);
            var buffer = new byte[17];
            var _ = ReadInternal(buffer);
            var bytes = ConvertHexByteStringToBytes(buffer);
            bytes = [.. bytes.Reverse()];
            return BitConverter.ToUInt64(bytes, 0);
        }
    }
    public void WriteBytes(byte[] data, ulong offset)
    {
       
            lock (_sync)
            {
                SendInternal(Poke((uint)offset, data));

                // give it time to push data back
                Thread.Sleep((data.Length / 256));
            }
        
    }

    public static byte[] MainPointer(long[] jumps) => Encode($"pointer{string.Concat(jumps.Select(z => $" {z}"))}");
    public static byte[] Poke(uint offset, byte[] data) => Encode($"poke 0x{offset:X8} 0x{string.Concat(data.Select(z => $"{z:X2}"))}");
}
