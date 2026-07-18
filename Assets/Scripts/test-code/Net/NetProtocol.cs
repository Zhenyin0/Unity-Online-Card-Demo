using System;
using System.Buffers.Binary;

public static class NetProtocol
{
    // 协议常量（与Python完全一致）
    public const byte PACK_TYPE_WHOLE = 0;
    public const byte MSG_TYPE_CLIENT_REQ = 2;
    public const int HeaderSize = 8; // 内层包头长度
    public const int OuterLenSize = 2; // 外层长度字段长度

    /// <summary>
    /// 生成会话ID（服务端要求奇数）
    /// </summary>
    private static int MakeSession()
    {
        return 1;
    }

    /// <summary>
    /// 封包：外层长度 + 内层包头 + Proto数据
    /// </summary>
    public static byte[] MakePacket(ushort packId, byte[] protoBytes)
    {
        // 内层包：包头 + 协议体
        int innerLength = HeaderSize + protoBytes.Length;
        byte[] innerPacket = new byte[innerLength];
        
        // 大端序写入包头（对应Python pack('>BBHI')）
        innerPacket[0] = PACK_TYPE_WHOLE;
        innerPacket[1] = MSG_TYPE_CLIENT_REQ;
        BinaryPrimitives.WriteUInt16BigEndian(innerPacket.AsSpan(2, 2), packId);
        BinaryPrimitives.WriteInt32BigEndian(innerPacket.AsSpan(4, 4), MakeSession());
        
        // 写入协议体
        Buffer.BlockCopy(protoBytes, 0, innerPacket, HeaderSize, protoBytes.Length);

        // 外层包：2字节大端长度 + 内层包
        byte[] fullPacket = new byte[OuterLenSize + innerLength];
        BinaryPrimitives.WriteUInt16BigEndian(fullPacket.AsSpan(0, 2), (ushort)innerLength);
        Buffer.BlockCopy(innerPacket, 0, fullPacket, OuterLenSize, innerLength);

        return fullPacket;
    }

    /// <summary>
    /// 解包：从缓存中解析一个完整包
    /// 返回：包ID、包体字节、总包长（含外层长度）；解析失败返回默认值
    /// </summary>
    public static bool TryParsePacket(byte[] buffer, int dataLength, out ushort packId, out byte[] body, out int totalPacketLength)
    {
        packId = 0;
        body = null;
        totalPacketLength = 0;

        // 连外层长度都不够
        if (dataLength < OuterLenSize)
            return false;

        // 读取内层包总长度
        ushort innerLen = BinaryPrimitives.ReadUInt16BigEndian(buffer.AsSpan(0, 2));
        totalPacketLength = OuterLenSize + innerLen;
        
        // 数据不足一个完整包
        if (dataLength < totalPacketLength)
            return false;

        // 包头长度校验
        if (innerLen < HeaderSize)
            return false;

        // 解析包头
        // byte packType = buffer[2];
        // byte msgType = buffer[3];
        packId = BinaryPrimitives.ReadUInt16BigEndian(buffer.AsSpan(4, 2));
        // int session = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan(6, 4));

        // 提取包体
        int bodyLength = innerLen - HeaderSize;
        body = new byte[bodyLength];
        Buffer.BlockCopy(buffer, OuterLenSize + HeaderSize, body, 0, bodyLength);

        return true;
    }
}