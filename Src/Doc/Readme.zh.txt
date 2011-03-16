萤火虫框架(Firefly)

地狱门神(F.R.C.)


1 概论

本框架的原本目标是为汉化的整个生命周期中的每一件没有已有工具或已有工具不完善的重复劳动提供工具或框架支持，但目前已不限于此。
这里列出汉化的流程结构，即本框架已经支持的部分(以限定名的形式表达)。也列出部分流程的其他工具。
程序加[]，命名空间或库不加[]。

  文件包|分散文件
  ↓↑
包格式分析：[UltraEdit]
解包、替换包中文件、压缩解压：Firefly.Packaging，Firefly.Compressing，Firefly.GUI.PackageManager、Zlib.Net、[WQSG_UMD]、[UMDGen]
  ↓↑
  字符映射表|字库|文本|图片

  字符映射表
  ↓↑
字符映射表分析：[UltraEdit]
字符映射表格式转换：Firefly.TextEncoding
  ↓↑
  .tbl字符表文件
  ↓↑←通用格式文本
字符映射表生成：[Firefly.MappingGen]

  字库
  ↓↑
字库分析：[UltraEdit]、[CrystalTile]、[CrystalTile2]
字库生成、字模提取：[Firefly.FontGen]、Firefly.Glyphing、Firefly.Imaging
  ↓↑
  .fd字库描述文件+字库图片
  ↓←文本导出
  .loc图形文本

  文本
  ↓↑
文本分析：[UltraEdit]、[CrystalTile]、[CrystalTile2]
文本导出、文本导入：Firefly.Texting、Firefly.TextEncoding、[WQSG导出(导入)]
  ↓↑
  通用编码通用格式文本(UTF-16、GB18030)x(Txt、Agemo、WQSG、Plain)
  ↓↑
直接修改：[Notepad]

  图片
  ↓↑
图片分析：[UltraEdit]、[CrystalTile]、[CrystalTile2]
图片转换：Firefly.Imaging、DevIL.Net
  ↓↑
  通用格式图片(bmp、png、dds)
  ↓↑
直接修改：[Photoshop]

  通用编码通用格式文本
  ↓↑→ .tbl
  ↓↑←字符表文件
文本转码：[Firefly.TransEncoding]
生成字符表：[Firefly.CharAdder]
汉字字形转换：[Firefly.TransVariant]

  .loc图形文本
  ↓↑
文本显示、修改：[Firefly.Eddy]
  ↓↑
  通用编码通用格式文本


2 各库功能介绍

2.1 核心库(Firefly.Core.dll)
本库集成了汉化中常常需要用到的一些工具类。
主要有如下方面：
├─Compressing     压缩：LZ77、RLE算法辅助类
├─Core            核心：在文件流中读写各种类型的整数、把文件流的一部分看作文件流、提取文件名的一部分、
│                        从整数中提取位、从多个文件位置计算多个文件长度、遍历多个区间中的整数索引等
├─Glyphing        字形：字形表示、字库生成。
├─Imaging         图像：在Bitmap类与颜色数组之间传送数据、Bmp文件类、Gif文件类、颜色空间转换
├─Packaging       文件包：轻松构造文件包解包打包器
├─Setting         设置：INI文件支持
├─TextEncoding    编码：从文本按频率提取需编码字符、文本码表格式读写
└─Texting         文件：Agemo文本读写、WQSG文本读写、LOC图形文本读写、基于字节的文本正则表达式搜索、简繁日字形转换
详细用法请参见Firefly.chm文档。

2.2 界面库(Firefly.GUI.dll)
放置主要与界面有关的内容。
│  FileDialog             将打开、保存文件合并的文件对话框，同时也可用于打开文件夹。用于替代(OpenFileDialog、
│                         SaveFileDialog、FolderBrowserDialog)三个System.Windows.Forms控件。
│  FileSelectBox          文件选取框，一个文本框和一个按钮的组合，用于选取文件或文件夹路径。
│  PackageManager         包管理器，用于配合Firefly.Packing中的类，实现快速包管理器的编写。
│  ProgressDialog         进度框，用于显示进度。
│  ScrollablePictureBox   可滚动图片框。

2.3 漩涡文本本地化工具(Firefly.Eddy)
用于显示和保存各种本地化文本，GUI工具。
该工具已分离为单独的漩涡(Eddy)项目。

2.4 正则表达式文件重命名工具(RegexRename.exe)
用于批量重命名文件，命令行工具。
详细用法请参见运行命令时的帮助信息，以/?选项获得。

2.5 字符入库器(CharAdder.exe)
用于提取生成字库所需的字符，命令行工具。
详细用法请参见运行命令时的帮助信息，以/?选项获得。

2.6 编码转换器(TransEncoding.exe)
用于批量转换文件编码，命令行工具。
详细用法请参见运行命令时的帮助信息，以/?选项获得。

2.7 Agemo文本验证器(AgemoValidator.exe)
用于批量验证Agemo文本格式，GUI工具。
详细用法请参见运行时的提示信息。

2.8 简繁日汉字异体字转换器(TransVariant.exe)
用于批量转换简繁日汉字字形，命令行工具。
详细用法请参见运行命令时的帮助信息，以/?选项获得。

2.9 字符映射表生成器(MappingGen.exe)
用于进行最接近字符映射表生成，命令行工具。
详细用法请参见运行命令时的帮助信息，以/?选项获得。

2.10 WQSG文本验证器(WQSGValidator.exe)
用于批量验证WQSG文本格式，GUI工具。
详细用法请参见运行时的提示信息。

2.11 字库图片生成器(FontGen.exe)
用于生成fd字库描述文件和字库图片，命令行和图形界面工具。
详细用法请参见运行命令时的帮助信息，以/?选项获得。

2.12 暴力文本导出器(BruteForceExporter.exe)
用于使用正则表达式或自定义标记按指定编码从字节文件中暴力提取WQSG文本。
详细用法请参见运行命令时的帮助信息，以/?选项获得。

2.13 WQSG文本导入器(WQSGImporter.exe)
用于导入WQSG格式的文本，特别是暴力提取的文本。
详细用法请参见运行命令时的帮助信息，以/?选项获得。

2.14 正则表达式字符串替换工具(RegexReplace.exe)
用于命令行替换文件中的字符串，命令行工具。
详细用法请参见运行命令时的帮助信息，以/?选项获得。

2.15 元类型结构处理工具(MetaSchemaManipulator.exe)
用于从元类型结构生成代码。
详细用法请参见运行命令时的帮助信息，以/?选项获得。


3 环境要求

本框架使用 Visual Basic 10.0 编写，开发时需要 Microsoft .Net Framework 4.0 编译器 或 Visual Studio 2010 支持。
本框架运行时需要 Microsoft .Net Framework 4 或 Microsoft .Net Framework 4 Client Profile 运行库支持。
Microsoft .Net Framework 4 (x86/x64，48.1MB)
http://download.microsoft.com/download/9/5/A/95A9616B-7A37-4AF6-BC36-D6EA96C8DAAE/dotNetFx40_Full_x86_x64.exe
Microsoft .NET Framework 4 Client Profile (x86，28.8MB)
http://download.microsoft.com/download/3/1/8/318161B8-9874-48E4-BB38-9EB82C5D6358/dotNetFx40_Client_x86.exe


4 用户使用协议

以下协议不针对示例(Examples文件夹)：
本框架是免费自由软件，所有源代码和可执行程序按照BSD许可证授权，详见License.zh.txt。
本框架的所有文档不按照BSD许可证授权，你可以不经修改的复制、传播这些文档，你还可以引用、翻译这些文档，其他一切权利保留。

以下协议针对示例(Examples文件夹)：
本框架的示例进入公有领域，可以随意修改使用。


5 备注

若你使用C++编程，需要使用C++/CLI才能调用本框架的内容。

如果发现了BUG，或者有什么意见或建议，请到以下网址与我联系。
http://www.cnblogs.com/Rex/Contact.aspx?id=1
常见的问题将在今后编制成Q&A。
