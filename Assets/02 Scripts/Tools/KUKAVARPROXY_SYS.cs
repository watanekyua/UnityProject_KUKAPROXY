// 20220905

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Text;
using System;

public class KUKAVARPROXY_SYS : MonoBehaviour
{
    /*
    The proxy reveives read/write requests from clients over the network. The proxy has the ip address of the robot and is listening on port 7000.

    The format of the messages exchange between clients and proxy must follow this protocol :
    
    Read Request Message Format
    ---------------------------
    2 bytes Id(uint16)
    2 bytes for content length(uint16)
    1 byte for read/write mode(0=Read)
    2 bytes for the variable name length(uint16)
    N bytes for the variable name to be read(ASCII)

    Write Request Message Format
    ---------------------------
    2 bytes Id(uint16)
    2 bytes for content length(uint16)
    1 byte for read/write mode(1=Write)
    2 bytes for the variable name length(uint16)
    N bytes for the variable name to be written(ASCII)
    2 bytes for the variable value length(uint16)
    M bytes for the variable value to be written(ASCII)

    Answer Message Format
    ---------------------------
    2 bytes Id(uint16)
    2 bytes for content length(uint16)
    1 byte for read/write mode(0=Read, 1=Write, 2=ReadArray, 3=WriteArray)
    2 bytes for the variable value length(uint16)
    N bytes for the variable value(ASCII)
    3 bytes for tail(000 on error, 011 on success)
    */


    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Unity Functions

    public enum MotionType
    {
        userE6Pos,
        userPTP_REL
    }

    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Unity Functions

    // ▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰▰ Custom Functions
    // E.X.
    // motionType = userE6Pos(絕對) or userPTP_REL(相對)
    // value = {E6POS: X 1, Y 0, Z 0, A 0, B 0, C 0, E1 0.0, E2 0.0, E3 0.0, E4 0.0, E5 0.0, E6 0.0}
    public byte[] WriteRequestMessage(MotionType motionType, string value)
    {
        int length = 9 + motionType.ToString().Length + value.Length;
        byte[] nameBytes = Encoding.ASCII.GetBytes(motionType.ToString());
        byte[] valueBytes = Encoding.ASCII.GetBytes(value);

        byte[] data = new byte[length];

        // ID
        int id = GetMessageId();
        data[0] = (byte)(id >> 8);
        data[1] = (byte)(id);

        // content length
        data[2] = (byte)((length - 4) >> 8);
        data[3] = (byte)(length - 4);

        // Write
        data[4] = 1;

        // variable name lengt
        data[5] = (byte)(motionType.ToString().Length >> 8);
        data[6] = (byte)(motionType.ToString().Length);

        // variable name to be written
        for (int i = 0; i < nameBytes.Length; i++)
        {
            int _a = i + 7;
            data[_a] = nameBytes[i];
        }

        // value length
        data[7 + nameBytes.Length] = (byte)(value.Length >> 8);
        data[8 + nameBytes.Length] = (byte)(value.Length);

        // variable value to be written
        for (int i = 0; i < value.Length; i++)
        {
            int _a = i + 9 + nameBytes.Length;
            data[_a] = valueBytes[i];
        }

        return data;
    }

    public int GetMessageId()
    {
        int _a = System.DateTime.Now.Millisecond;
        return _a;
    }

    public byte[] ReadRequestMessage(MotionType motionType)
    {
        int length = 9 + motionType.ToString().Length;
        byte[] nameBytes = Encoding.ASCII.GetBytes(motionType.ToString());

        byte[] data = new byte[length];

        // ID
        int id = GetMessageId();
        data[0] = (byte)(id >> 8);
        data[1] = (byte)(id);

        // content length
        data[2] = (byte)((length - 4) >> 8);
        data[3] = (byte)(length - 4);

        // Write
        data[4] = 0;

        // variable name lengt
        data[5] = (byte)(motionType.ToString().Length >> 8);
        data[6] = (byte)(motionType.ToString().Length);

        // variable name to be written
        for (int i = 0; i < nameBytes.Length; i++)
        {
            int _a = i + 7;
            data[_a] = nameBytes[i];
        }

        return data;
    }
}
