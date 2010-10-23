'
'
' 代码分析使用此文件来维护应用到此项目的 SuppressMessage 
' 特性。
' 项目级禁止显示或者没有目标，或者已给定 
' 一个特定目标且其范围为命名空间、类型和成员等。
'
' 若要向此文件添加禁止显示，请右击 
' 错误列表中的消息，再指向“禁止显示消息”，然后单击 
'“在项目禁止显示文件中”。
' 无需手动向此文件添加禁止显示。

<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Scope:="type", Target:="Firefly.Texting.InvalidTextFormatOrEncodingException")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Scope:="type", Target:="Firefly.Texting.InvalidTextFormatException")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Texting.LOC.#WriteFile(Firefly.ZeroLengthStreamPasser,Firefly.Texting.LOCText,System.Boolean)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Texting.LOC.#ReadFile(Firefly.ZeroPositionStreamPasser)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope:="member", Target:="Firefly.Imaging.BitmapEx.#SetRectangle(System.Drawing.Bitmap,System.Int32,System.Int32,System.Int32[,])")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Scope:="member", Target:="Firefly.Imaging.BitmapEx.#GetRectangle(System.Drawing.Bitmap,System.Int32,System.Int32,System.Int32,System.Int32)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope:="member", Target:="Firefly.Packaging.PCK.#.ctor(Firefly.ZeroPositionStreamPasser)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:不要多次释放对象", Scope:="member", Target:="Firefly.Setting.Xml.#ReadFile`1(System.String,System.Collections.Generic.IEnumerable`1<System.Type>,System.Collections.Generic.IEnumerable`1<Firefly.Setting.Xml+IMapper>)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope:="member", Target:="Firefly.Packaging.FileDB.#.ctor()")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope:="member", Target:="Firefly.Packaging.FileDB.#.ctor(System.String,Firefly.Packaging.FileDB+FileType,System.Int64,System.Int64,System.String)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope:="member", Target:="Firefly.Packaging.ISO.#.ctor(Firefly.ZeroPositionStreamPasser)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Texting.Txt.#CreateTextWriter(System.String,System.Text.Encoding,System.Boolean)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Texting.Txt.#CreateTextReader(Firefly.ZeroPositionStreamPasser,System.Text.Encoding,System.Boolean)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Texting.Txt.#CreateTextReader(System.String,System.Text.Encoding,System.Boolean)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Packaging.PackageRegister.#Open(System.Int32,System.String)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Packaging.PackageRegister.#Create(System.Int32,System.String,System.String)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Texting.GlyphText.#GetBitmap(System.Int32,System.Int32,System.Collections.Generic.Dictionary`2<System.String,System.String>)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Packaging.ISO.#Open(System.String)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Imaging.Bmp.#Open(Firefly.ZeroPositionStreamPasser)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Imaging.Bmp.#Open(System.String)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Imaging.Bmp.#.ctor(System.String,System.Int32,System.Int32,System.Int16)")> 
<Assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:丢失范围之前释放对象", Scope:="member", Target:="Firefly.Imaging.Bmp.#.ctor(System.Int32,System.Int32,System.Int16)")> 

