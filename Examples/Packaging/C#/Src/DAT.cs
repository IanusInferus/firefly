//==========================================================================
//
//  File:        DAT.cs
//  Location:    Firefly.Examples <Visual C#>
//  Description: プリニ DAT格式
//  Version:     2010.12.01.
//  Author:      F.R.C.
//  Copyright(C) public Domain
//
//==========================================================================

//这里主要提供对《プリニ》这个游戏的文件包的支持。
//示例中已包含该包类型的样例SCRIPT.DAT。
//プリニ DAT格式如下。

//プリニ DAT格式

//Header 00h
//12      String          Identifier      "NISPACK"
//4       Int32           NumFile         07 00 00 00

//Index 10h
//32      String          Name            "debug.nsf"
//4       Int32           Address         00 08 00 00
//4       Int32           Length          F5 03 00 00
//4       Int32           ?               E8 03 47 39

//Data
//(
//*       Byte()          FileData
//800h对齐
//)


using System;
using System.Collections.Generic;
using System.IO;
using Firefly;
using Firefly.Streaming;
using Firefly.Packaging;

public class DAT : PackageDiscrete
{
    //使用离散文件包接口，表示文件数据不一定非要连续，即通过位置和长度来确定，连续文件一般只有长度一个数值

    public DAT(NewReadingStreamPasser sp)
        : base(sp)
    {
        Initialize();
    }
    public DAT(NewReadingWritingStreamPasser sp)
        : base(sp)
    {
        Initialize();
    }

    //在构造函数中填入文件包读取的部分，对每个文件需要调用PushFile以构造路径信息和各种映射信息
    public void Initialize()
    {
        var s = Readable;

        //判断文件头部是否正常
        if (s.ReadSimpleString(12) != "NISPACK") { throw new InvalidDataException(); }

        var NumFile = s.ReadInt32();

        RootValue = FileDB.CreateDirectory("", "");

        for (var n = 0; n < NumFile; n += 1)
        {

            //读取索引的各部分
            var Name = s.ReadSimpleString(32);
            var Address = s.ReadInt32();
            var Length = s.ReadInt32();
            var Unknown = s.ReadInt32();

            //创建一个文件描述信息，包括文件名、文件大小、文件地址
            var f = new FileDB(Name, FileDB.FileType.File, Length, Address, null);

            //将文件描述信息传递到框架内部
            //框架内部能够自动创建文件树(将文件名中以'\'或者'/'表示的文件路径拆开)
            //框架内部能够自动创建IndexOfFile映射表，能够将文件描述信息映射到文件索引的出现顺序
            //框架内部还记录一些数据用于寻找能放下数据的空洞
            PushFile(f);
        }

        //离散文件在打开的时候应该寻找空洞，以供导入文件使用
        //寻找的起始地址是从当前位置的下一个块开始的位置
        ScanHoles(GetSpace(s.Position));
    }

    //提供格式在打开文件包窗口中的过滤器
    public static String Filter
    {
        get
        {
            return "プリニ DAT格式(*.DAT)|*.DAT";
        }
    }

    //打开文件包的函数
    public static PackageBase Open(String Path)
    {
        IStream s = null;
        IReadableSeekableStream sRead = null;
        try
        {
            s = StreamEx.Create(Path, FileMode.Open);
        }
        catch
        {
            sRead = StreamEx.CreateReadable(Path, FileMode.Open);
        }
        if (s != null)
        {
            return new DAT(s.AsNewReadingWriting());
        }
        else
        {
            return new DAT(sRead.AsNewReading());
        }
    }

    //读取文件在索引中的地址信息，所有索引中的地址信息应该在这里更新
    public override Int64 get_FileAddressInPhysicalFileDB(FileDB File)
    {
        Readable.Position = 16 + 44 * IndexOfFile[File] + 32;
        return Readable.ReadInt32();
    }
    public override void set_FileAddressInPhysicalFileDB(FileDB File, Int64 Value)
    {
        Writable.Position = 16 + 44 * IndexOfFile[File] + 32;
        Writable.WriteInt32(Convert.ToInt32(Value));
    }

    //读取文件在索引中的长度信息，所有索引中的长度信息应该在这里更新
    public override Int64 get_FileLengthInPhysicalFileDB(FileDB File)
    {
        Readable.Position = 16 + 44 * IndexOfFile[File] + 36;
        return Readable.ReadInt32();
    }
    public override void set_FileLengthInPhysicalFileDB(FileDB File, Int64 Value)
    {
        Writable.Position = 16 + 44 * IndexOfFile[File] + 36;
        Writable.WriteInt32(Convert.ToInt32(Value));
    }

    //提供文件数据的对齐的计算函数
    protected override Int64 GetSpace(Int64 Length)
    {
        return ((Length + 0x800 - 1) / 0x800) * 0x800;
    }
}

//在提供上述几个函数的基础上，一个简单的提供自动扩展的可读写文件包系统就可以通过PackageManager来打开了
