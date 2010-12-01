//==========================================================================
//
//  File:        DAT.fs
//  Location:    Firefly.Examples <Visual F#>
//  Description: プリニ DAT格式
//  Version:     2010.12.01.
//  Author:      F.R.C.
//  Copyright(C) Public Domain
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

namespace PackageManager
open System
open System.Collections.Generic
open System.IO
open Firefly
open Firefly.Streaming
open Firefly.Packaging

type DAT =
    inherit PackageDiscrete //使用离散文件包接口，表示文件数据不一定非要连续，即通过位置和长度来确定，连续文件一般只有长度一个数值

    new(sp : NewReadingStreamPasser) as this = { inherit PackageDiscrete(sp) } then
        this.Initialize ()
    new(sp : NewReadingWritingStreamPasser) as this = { inherit PackageDiscrete(sp) } then
        this.Initialize ()

    //在构造函数中填入文件包读取的部分，对每个文件需要调用PushFile以构造路径信息和各种映射信息
    member this.Initialize () =
        let s = this.Readable

        //判断文件头部是否正常
        if s.ReadSimpleString(12) <> "NISPACK" then raise (InvalidDataException())

        let NumFile = s.ReadInt32 ()
        for i in 0 .. (NumFile - 1) do

            //读取索引的各部分
            let Name = s.ReadSimpleString(32)
            let Address = s.ReadInt32 ()
            let Length = s.ReadInt32 ()
            let Unknown = s.ReadInt32 ()

            //创建一个文件描述信息，包括文件名、文件大小、文件地址
            let f = new FileDB (Name, FileDB.FileType.File, int64 Length, int64 Address)

            //将文件描述信息传递到框架内部
            //框架内部能够自动创建文件树(将文件名中以'\'或者'/'表示的文件路径拆开)
            //框架内部能够自动创建IndexOfFile映射表，能够将文件描述信息映射到文件索引的出现顺序
            //框架内部还记录一些数据用于寻找能放下数据的空洞
            base.PushFile (f)

        //离散文件在打开的时候应该寻找空洞，以供导入文件使用
        //寻找的起始地址是从当前位置的下一个块开始的位置
        base.ScanHoles (this.GetSpace(s.Position))


    //提供格式在打开文件包窗口中的过滤器
    static member Filter = "プリニ DAT格式(*.DAT)|*.DAT"

    //打开文件包的函数
    static member Open (path : String) : PackageBase =
        let s =
            try
                (StreamEx.Create (path, FileMode.Open)).AsNewReadingWriting ()
            with _ ->
                null
        match s with
        | null -> new DAT ((StreamEx.CreateReadable (path, FileMode.Open)).AsNewReading ()) :> PackageBase
        | _ -> new DAT (s) :> PackageBase

    //读取文件在索引中的地址信息，所有索引中的地址信息应该在这里更新
    override this.FileAddressInPhysicalFileDB
        with get (file : FileDB) : int64 =
            this.Readable.Position <- 16L + 44L * int64 this.IndexOfFile.[file] + 32L
            int64 (this.Readable.ReadInt32())
        and set (file : FileDB) (value : int64) =
            this.Writable.Position <- 16L + 44L * int64 this.IndexOfFile.[file] + 32L
            this.Writable.WriteInt32(int32 value)

    //读取文件在索引中的长度信息，所有索引中的长度信息应该在这里更新
    override this.FileLengthInPhysicalFileDB
        with get (file : FileDB) : int64 =
            this.Readable.Position <- 16L + 44L * int64 this.IndexOfFile.[file] + 36L
            int64 (this.Readable.ReadInt32())
        and set (file : FileDB) (value : int64) =
            this.Writable.Position <- 16L + 44L * int64 this.IndexOfFile.[file] + 36L
            this.Writable.WriteInt32(int32 value)

    //提供文件数据的对齐的计算函数
    override this.GetSpace (length : int64) : int64 = ((length + 0x800L - 1L) / 0x800L) * 0x800L

//在提供上述几个函数的基础上，一个简单的提供自动扩展的可读写文件包系统就可以通过PackageManager来打开了
()
