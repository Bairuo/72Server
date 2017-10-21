using System;
using System.Collections;
using System.Linq;

/****************
 * 
 * 作者：Bairuo
 * 最后修改时间：2017.10.6
 * 
 * 
 * 
 ****************/
namespace PAServer {
public class ProtocolBytes : ProtocolBase {
    //消息长度，消息内容
    //协议名称长度，协议名称，协议内容
    public byte[] Bytes;
    int start;

    public override ProtocolBase Decode(byte[] readbuff, int start, int length)
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.Bytes = new byte[length];
        Array.Copy(readbuff, start, protocol.Bytes, 0, length);
        return protocol;
    }

    public override byte[] Encode()
    {
        return Bytes;
    }
    
    
    public override string GetName()
    {
        start = 0;
        return GetString(start);
    }
    public string GetString()
    {
        return GetString(start, ref start);
    }
    public int GetInt()
    {
        return GetInt(start, ref start);
    }
    public bool GetBool()
    {
        return GetBool(start, ref start);
    }
    public float GetFloat()
    {
        return GetFloat(start, ref start);
    }
    public override string GetDesc()
    {
        string str = "";
        if (Bytes == null) return str;
        for (int i = 0; i < Bytes.Length; i++)
        {
            int b = (int)Bytes[i];
            str += b.ToString() + " ";
        }
        return str;
    }

    public int GetLength()
    {
        return Bytes.Length;
    }
    public ProtocolBytes GetSonProtocol(int start, int length)
    {
        ProtocolBytes proto = new ProtocolBytes();
        if (length == 0) return proto;

        byte[] rest = new byte[length];

        for (int i = 0; i < length; i++)
        {
            rest[i] = Bytes[i + start];
        }

        
        proto.AddByte(rest);
        return proto;
    }
    public ProtocolBytes GetRestProtocol(int start)
    {
        int newLength = Bytes.Length - start;
        return GetSonProtocol(start, newLength);
    }

    public void AddName(string str)
    {
        AddString(str);
    }
    public void AddString(string str)
    {
        Int32 len = str.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
        
        if (Bytes == null)
            Bytes = lenBytes.Concat(strBytes).ToArray();
        else
            Bytes = Bytes.Concat(lenBytes).Concat(strBytes).ToArray();
    }
    public void AddInt(int num)
    {
        byte[] numBytes = BitConverter.GetBytes(num);
        if (Bytes == null)
            Bytes = numBytes;
        else
            Bytes = Bytes.Concat(numBytes).ToArray();
    }
    public void AddBool(bool num)
    {
        byte[] numBytes = BitConverter.GetBytes(num);
        if (Bytes == null)
            Bytes = numBytes;
        else
            Bytes = Bytes.Concat(numBytes).ToArray();
    }
    public void AddFloat(float num)
    {
        byte[] numBytes = BitConverter.GetBytes(num);
        if (Bytes == null)
            Bytes = numBytes;
        else
            Bytes = Bytes.Concat(numBytes).ToArray();
    }
    public void AddByte(byte[] bytes)
    {
        if (Bytes == null)
            Bytes = bytes;
        else
            Bytes = Bytes.Concat(bytes).ToArray();
    }

    public string GetString(int start, ref int end)
    {
        if (Bytes == null)
            return "";
        if (Bytes.Length < start + sizeof(Int32))
            return "";
        Int32 strLen = BitConverter.ToInt32(Bytes, start);
        if (Bytes.Length < start + sizeof(Int32) + strLen)
            return "";

        string str = System.Text.Encoding.UTF8.GetString(Bytes, start + sizeof(Int32), strLen);
        end = start + sizeof(Int32) + strLen;
        return str;
    }
    public string GetString(int start)
    {
        int end = 0;
        return GetString(start, ref end);
    }
    public int GetInt(int start, ref int end)
    {
        if (Bytes == null)
            return 0;
        if (Bytes.Length < start + sizeof(Int32))
            return 0;
        end = start + sizeof(Int32);
        return BitConverter.ToInt32(Bytes, start);
    }
    public bool GetBool(int start, ref int end)
    {
        if (Bytes == null)
            return false;
        if (Bytes.Length < start + sizeof(bool))
            return false;
        end = start + sizeof(bool);
        return BitConverter.ToBoolean(Bytes, start);
    }
    public int GetInt(int start)
    {
        int end = 0;
        return GetInt(start, ref end);
    }
    public float GetFloat(int start, ref int end)
    {
        if (Bytes == null)
            return 0;
        if (Bytes.Length < start + sizeof(float))
            return 0;
        end = start + sizeof(float);
        return BitConverter.ToSingle(Bytes, start);
    }
    public float GetFloat(int start)
    {
        int end = 0;
        return GetFloat(start, ref end);
    }
}
}