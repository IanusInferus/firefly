﻿'==========================================================================
'
'  File:        HanziConverter.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 简繁日汉字转换
'  Version:     2009.10.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Firefly
Imports Firefly.TextEncoding

Namespace Texting
    ''' <summary>简繁日汉字转换器</summary>
    Public NotInheritable Class HanziConverter
        Private Sub New()
        End Sub

        ''' <summary>表转换</summary>
        Public Shared Function TableConvert(ByVal s As String, ByVal Dict As Dictionary(Of Char32, Char32)) As String
            Dim s32 = s.ToUTF32
            Dim h As New HashSet(Of Char32)(From c In s32 Distinct)
            Dim sb As New List(Of Char32)
            For Each c In s32
                If Dict.ContainsKey(c) Then
                    Dim Mapped = Dict(c)
                    If Not h.Contains(Mapped) Then
                        sb.Add(Mapped)
                        Continue For
                    End If
                End If
                sb.Add(c)
            Next
            Return sb.ToUTF16B
        End Function

        ''' <summary>表转换，仅一一对应</summary>
        Public Shared Function TableConvertOneOnOne(ByVal s As String, ByVal Dict As Dictionary(Of Char32, Char32), ByVal ReverseDict As Dictionary(Of Char32, Char32)) As String
            Dim s32 = s.ToUTF32
            Dim h As New HashSet(Of Char32)(From c In s32 Distinct)
            Dim sb As New List(Of Char32)
            For Each c In s32
                If Dict.ContainsKey(c) Then
                    Dim Mapped = Dict(c)
                    If Not h.Contains(Mapped) Then
                        If ReverseDict.ContainsKey(Mapped) AndAlso ReverseDict(Mapped) = c Then
                            sb.Add(Mapped)
                            Continue For
                        End If
                    End If
                End If
                sb.Add(c)
            Next
            Return sb.ToUTF16B
        End Function

        ''' <summary>构造转换表</summary>
        Public Shared Function BuildTable(ByVal R As Char32(), ByVal D As Char32()) As Dictionary(Of Char32, Char32)
            Dim Dict As New Dictionary(Of Char32, Char32)
            If R.Length <> D.Length Then Throw New ArgumentException
            For n = 0 To R.Length - 1
                If Dict.ContainsKey(R(n)) Then Continue For
                Dict.Add(R(n), D(n))
            Next
            Return Dict
        End Function

        ''' <summary>构造转换表，仅一一对应</summary>
        Public Shared Function BuildTableOneOnOne(ByVal R As Char32(), ByVal D As Char32()) As Dictionary(Of Char32, Char32)
            Dim Dict As New Dictionary(Of Char32, Char32)
            Dim MultiSet As New HashSet(Of Char32)
            If R.Length <> D.Length Then Throw New ArgumentException
            For n = 0 To R.Length - 1
                If MultiSet.Contains(R(n)) Then Continue For
                If Dict.ContainsKey(R(n)) Then
                    MultiSet.Add(R(n))
                    Dict.Remove(R(n))
                    Continue For
                End If
                Dict.Add(R(n), D(n))
            Next
            Return Dict
        End Function

        ''' <summary>构造转换集</summary>
        Public Shared Function BuildSet(ByVal R As Char32(), ByVal D As Char32()) As HashSet(Of KeyValuePair(Of Char32, Char32))
            Dim Dict As New HashSet(Of KeyValuePair(Of Char32, Char32))
            If R.Length <> D.Length Then Throw New ArgumentException
            For n = 0 To R.Length - 1
                Dict.Add(New KeyValuePair(Of Char32, Char32)(R(n), D(n)))
            Next
            Return Dict
        End Function

        ''' <summary>简繁转换</summary>
        Public Shared Function G2T(ByVal s As String) As String
            Return TableConvert(s, G2T_Dict)
        End Function

        ''' <summary>繁简转换</summary>
        Public Shared Function T2G(ByVal s As String) As String
            Return TableConvert(s, T2G_Dict)
        End Function

        ''' <summary>繁日转换</summary>
        Public Shared Function T2J(ByVal s As String) As String
            Return TableConvert(s, T2J_Dict)
        End Function

        ''' <summary>日繁转换</summary>
        Public Shared Function J2T(ByVal s As String) As String
            Return TableConvert(s, J2T_Dict)
        End Function

        ''' <summary>简日转换</summary>
        Public Shared Function G2J(ByVal s As String) As String
            Return TableConvert(s, G2J_Dict)
        End Function

        ''' <summary>日简转换</summary>
        Public Shared Function J2G(ByVal s As String) As String
            Return TableConvert(s, J2G_Dict)
        End Function

        ''' <summary>简繁转换，仅一一对应</summary>
        Public Shared Function G2TOneOnOne(ByVal s As String) As String
            Return TableConvertOneOnOne(s, G2T_Dict, T2G_Dict)
        End Function

        ''' <summary>繁简转换，仅一一对应</summary>
        Public Shared Function T2GOneOnOne(ByVal s As String) As String
            Return TableConvertOneOnOne(s, T2G_Dict, G2T_Dict)
        End Function

        ''' <summary>繁日转换，仅一一对应</summary>
        Public Shared Function T2JOneOnOne(ByVal s As String) As String
            Return TableConvertOneOnOne(s, T2J_Dict, J2T_Dict)
        End Function

        ''' <summary>日繁转换，仅一一对应</summary>
        Public Shared Function J2TOneOnOne(ByVal s As String) As String
            Return TableConvertOneOnOne(s, J2T_Dict, T2J_Dict)
        End Function

        ''' <summary>简日转换，仅一一对应</summary>
        Public Shared Function G2JOneOnOne(ByVal s As String) As String
            Return TableConvertOneOnOne(s, G2J_Dict, J2G_Dict)
        End Function

        ''' <summary>日简转换，仅一一对应</summary>
        Public Shared Function J2GOneOnOne(ByVal s As String) As String
            Return TableConvertOneOnOne(s, J2G_Dict, G2J_Dict)
        End Function

        ''' <summary>简繁转换表，仅一一对应</summary>
        Public Shared ReadOnly Property G2T_Dict() As Dictionary(Of Char32, Char32)
            Get
                Static Dict As Dictionary(Of Char32, Char32)
                If Dict Is Nothing Then Dict = BuildTableOneOnOne(G2T_G, G2T_T)
                Return Dict
            End Get
        End Property

        ''' <summary>繁简转换表，仅一一对应</summary>
        Public Shared ReadOnly Property T2G_Dict() As Dictionary(Of Char32, Char32)
            Get
                Static Dict As Dictionary(Of Char32, Char32)
                If Dict Is Nothing Then Dict = BuildTableOneOnOne(T2G_T, T2G_G)
                Return Dict
            End Get
        End Property

        ''' <summary>繁日转换表，仅一一对应</summary>
        Public Shared ReadOnly Property T2J_Dict() As Dictionary(Of Char32, Char32)
            Get
                Static Dict As Dictionary(Of Char32, Char32)
                If Dict Is Nothing Then Dict = BuildTableOneOnOne(T2J_T, T2J_J)
                Return Dict
            End Get
        End Property

        ''' <summary>日繁转换表，仅一一对应</summary>
        Public Shared ReadOnly Property J2T_Dict() As Dictionary(Of Char32, Char32)
            Get
                Static Dict As Dictionary(Of Char32, Char32)
                If Dict Is Nothing Then Dict = BuildTableOneOnOne(J2T_J, J2T_T)
                Return Dict
            End Get
        End Property

        ''' <summary>简日转换表，仅一一对应</summary>
        Public Shared ReadOnly Property G2J_Dict() As Dictionary(Of Char32, Char32)
            Get
                Static Dict As Dictionary(Of Char32, Char32)
                If Dict Is Nothing Then Dict = BuildTableOneOnOne(G2J_G, G2J_J)
                Return Dict
            End Get
        End Property

        ''' <summary>日简转换表，仅一一对应</summary>
        Public Shared ReadOnly Property J2G_Dict() As Dictionary(Of Char32, Char32)
            Get
                Static Dict As Dictionary(Of Char32, Char32)
                If Dict Is Nothing Then Dict = BuildTableOneOnOne(J2G_J, J2G_G)
                Return Dict
            End Get
        End Property

        ''' <summary>简繁转换表集</summary>
        Public Shared ReadOnly Property G2T_Set() As HashSet(Of KeyValuePair(Of Char32, Char32))
            Get
                Static Dict As HashSet(Of KeyValuePair(Of Char32, Char32))
                If Dict Is Nothing Then Dict = BuildSet(G2T_G, G2T_T)
                Return Dict
            End Get
        End Property

        ''' <summary>繁简转换表集</summary>
        Public Shared ReadOnly Property T2G_Set() As HashSet(Of KeyValuePair(Of Char32, Char32))
            Get
                Static Dict As HashSet(Of KeyValuePair(Of Char32, Char32))
                If Dict Is Nothing Then Dict = BuildSet(T2G_T, T2G_G)
                Return Dict
            End Get
        End Property

        ''' <summary>繁日转换表集</summary>
        Public Shared ReadOnly Property T2J_Set() As HashSet(Of KeyValuePair(Of Char32, Char32))
            Get
                Static Dict As HashSet(Of KeyValuePair(Of Char32, Char32))
                If Dict Is Nothing Then Dict = BuildSet(T2J_T, T2J_J)
                Return Dict
            End Get
        End Property

        ''' <summary>日繁转换表集</summary>
        Public Shared ReadOnly Property J2T_Set() As HashSet(Of KeyValuePair(Of Char32, Char32))
            Get
                Static Dict As HashSet(Of KeyValuePair(Of Char32, Char32))
                If Dict Is Nothing Then Dict = BuildSet(J2T_J, J2T_T)
                Return Dict
            End Get
        End Property

        ''' <summary>简日转换表集</summary>
        Public Shared ReadOnly Property G2J_Set() As HashSet(Of KeyValuePair(Of Char32, Char32))
            Get
                Static Dict As HashSet(Of KeyValuePair(Of Char32, Char32))
                If Dict Is Nothing Then Dict = BuildSet(G2J_G, G2J_J)
                Return Dict
            End Get
        End Property

        ''' <summary>日简转换表集</summary>
        Public Shared ReadOnly Property J2G_Set() As HashSet(Of KeyValuePair(Of Char32, Char32))
            Get
                Static Dict As HashSet(Of KeyValuePair(Of Char32, Char32))
                If Dict Is Nothing Then Dict = BuildSet(J2G_J, J2G_G)
                Return Dict
            End Get
        End Property

        Private Shared G2T_G As Char32() = "卜卜几几乃乃了了干干干干乾于于才才千千么么幺尸尸斗斗丰丰云云巨巨扎扎历历升升升仆仆凶凶丑丑汇汇它它术术札札布布占占只只只只叹叹出出冬冬咚饥饥台台台台发发冲冲并并并扣扣托托朴朴夸夸划划当当吁吁曲曲吊吊团团回回朱朱伙伙向向后后尽尽奸奸纤纤沈沈闲闲证证志志坛坛折折克克苏苏苏杆杆杠杠戒戒卤卤里里里困困别别佑佑佣佣谷谷余余馀系系系局局局注注沾沾帘帘卷卷表表幸幸拓拓拈拈拐拐抵抵范范苹苹杯杯板板松松郁郁采采采采制制刮刮岳岳征征径径念念舍舍周周弦弦弥弥哗哗姜姜炼炼迹迹胡胡荡荡药药咸咸厘厘面面尝尝哄哄钟钟复复秋秋须须凄凄涌涌席席准准症症挨挨挽挽恶恶获获栗栗致致脏脏娴娴淡淡淀淀梁梁麻麻捻捻菇菇戚戚累累彩彩衔衔欲欲游游雇雇筑筑铺铺链链御御腊腊摆摆蒙蒙蒙鉴鉴愈愈签签辟辟熔熔酸酸愿愿霉霉噪噪赞赞雕雕雕於体体儿儿况况划听听咏咏阪坂坏坏覆复庄庄徵怜怜恒恒扑扑挂挂晒晒杰杰栖栖洒洒涂涂烟烟画著着硷碱碱绦绦网网肴肴荐荐葱葱虫虫诫钩钩锈锈锤锤鼹鼹㔉㖊㖞㛠㟆㧑㧟㨫㱩㱮㲿㶉㶽㺍㻏㻘䁖䅉䇲䌶䌷䌸䌹䌺䌼䌾䍀䍁䓕䗖䜥䜧䝙䞐䦀䦁䯃䯄䯅䲝䴓䴔䴖䴗䴘万与专业丛东丝丢两严丧个临为丽举义乌乐乔习乡书买乱争亏亚产亩亲亵亿仅从仑仓仪们价众优会伞伟传伣伤伥伦伧伪伫佥侠侣侥侦侧侨侩侪侬俦俨俩俪俫俭债倾偬偿傥傧储兑兖党兰关兴兹养兽内冈册写军农冯决冻凉减凑凛凤凭凯击凿刍刘则刚创删刬刹刽剀剂剑剥剧劝办务动励劲劳势勋匀匮区医华协单卖卢卫却厂厅厉压厌厐厕厢厣厦厨厩厮县参双变叙叠叶号叽吓吕吗吨启吴呐呒呓呕员呛呜咙咛咤响哑哔哜哝哟唠唡唢唤啧啬啭啮啸喷喽嗫嗳嘘嘤嘱噜嚣园囱围国图圆圣圹场块坚坜坝坞坟坠垄垒垦垩垫埘埚堑堕墙壮声壳壶处备够头夹夺奁奂奋奖奥妆妇妈妩妪姗娄娆娇娱娲婴婵婶媪嫔嬷孙学孪宁宝实宠审宪宫宽宾寝对寻导寿将尔尘尧尴层屉届属屡屦屿岁岂岖岗岘岚岛岭岽峡峥峦崄崭嵘嵚巅巩币帅师帏帐帜带帧帮帼幂广庆庐库应庙庞废开异弃弑张弪弯弹强归录彝彦彻徕忆忏忧忾怀态怂怃怅怆总恋恳恸恹恺恻恼悦悬悮悯惊惧惨惩惫惬惭惮惯愠愤慑懑懒戆戏战戬戯户执扩扪扫扬扰抚抛抡抢护报担拟拢拣拥拦拧拨择挚挛挜挝挞挟挠挡挣挤挥捝捞损捡换捣据掳掴掷掺揽揿搀搁搂搅携摄摇摊撑撵撷擞攒敌敛数斋斓斩断无旧时旷旸昙昼显晋晓晔晕晖暂暧机杀杂权条来杨杩极构枞枢枣枪枫枭柜柠栀栅标栈栉栋栏树样栾桡桢档桥桦桧桨桩梦检棂椁椟椭楼榄榅榇榈榉槛槟槠横樯樱橥橱橹欢欤欧歼殁殇残殓殡殴毁毂毕毙毡气氢氩氲汉汤汹沟没沤沥沦沧沪泞泪泶泷泸泻泼泽泾洁洼浃浅浆浇浊测济浏浑浒浓浔涛涝涟涡涣涤润涧涨涩渊渍渎渐渑渔渗温湾湿溃溅滚滞满滢滤滥滦滨滩滪潇潋潍潜澜濑濒灭灯灵灾灿炀炉炖炜点炽烁烂烛烦烧烩烫烬热焕焖焘煴熏爱爷牍牵牺犊状犷犹狈狞独狭狮狰狱猎猕猡猪猫猬献獭玑玛玮环现玱玺珐珑珲琏琐琼瑶璎瓒瓯电畅畴疖疗疟疠疡疮疯疱痉痒痖痨痪瘘瘪瘫瘾癞癣癫皑皱皲盏盐监盖盗盘眦眬睁睐睑瞆瞒瞩矫矶矾矿码砖砚砺砾础硕硙确碍碛礼祯祷祸禀禄禅离秃秆种积称秽秾税稣稳穑穷窃窍窎窑窜窝窥窦竖竞笃笋笔笕笺笼筛筝筹简箩箪箫篑篓篮篱籁籴类籼粜粤粪粮糁紧纠纡红纣纥约级纨纪纫纬纭纮纯纰纱纲纳纵纶纷纸纹纺纼纽纾线绁绂练组绅细织终绉绊绋绌绍绎经绐绑绒结绔绕绘给绚绛络绝绞统绢绣绥继绩绪绫绬续绮绯绰绱绲绳维绵绶绷绸综绽绾绿缀缁缂缄缅缆缇缈缉缊缋缎缓缔缕编缗缘缙缚缜缝缞缟缠缢缣缤缥缦缧缨缩缪缫缭缮缯缰缴罂罗罚罢羁羡翘耸耻聂聋职联聪肃肠肤肮肾肿胀胁胆胜胧胪胫胶脉脍脐脑脓脚脱脸腻腼腾膑臜舆舰舱艰艳艺节芈芜芦苇苋苌苍苎茎茔茧荆荚荜荞荟荠荣荤荦荧荨荪荫荮莅莱莲莴莹莺莼萝萤营萦萧萨蒋蒌蓝蓟蓦蔂蔷蔹蔺蔼蕲蕴薮藓虏虑虚虽虾蚀蚁蚂蚕蚬蛊蛎蛮蛰蛲蜕蜗蜡蝇蝈蝉蝼蝾螀蟏衅补衬衮袄袅袜袭装裆裤褛褴见观觃规觅视览觉觊觍觎觐觑觞触訚誉誊计订讣认讥讦讧讨让讪讫训议讯记讱讲讳讴讶讷许讹论讻讼讽设访诀诂诃评诅识诇诈诉诊诋诌词诏诐译诒试诗诘诙诚诛话诞诟诠诡询诣诤该详诧诪诬语诮误诰诱诲诳说诵诶请诸诺读诽课诿谀谁调谄谅谆谇谈谊谋谌谍谎谏谐谑谒谓谕谖谗谙谚谛谜谞谟谠谡谢谣谤谦谧谨谩谪谬谭谯谱谲谴谵谶豮贝贞负贠贡财责贤败账货质贩贪贫贬购贮贯贰贱贲贴贵贷贸费贺贻贼贾贿赁赂赃资赅赈赊赋赌赎赏赐赒赓赔赕赖赗赘赙赚赛赝赠赡赢赣赵赶趋趸跃践跶跷跸跻踊踌踪踬踯蹑蹒蹰躏躯车轧轨轩轫转轭轮软轰轲轳轴轶轸轻轼载轾轿较辄辅辆辇辈辉辊辋辍辎辏辐辑输辔辕辖辗辘辙辚辞辩辫边辽达迁过迈运还这进远违连迟迩适选逊递逦逻遗遥邓邮邹邺邻郑郦郸酂酝酦酱酽酿释銮錾针钉钊钋钏钑钒钓钗钙钛钝钞钠钡钢钣钤钥钦钧钨钮钰钱钳钴钵钶钹钻钾钿铀铁铂铃铄铅铆铉铋铍铎铏铐铓铔铖铙铛铜铝铢铣铦铨铬铭铮铰铱铲铵银铸铿销锁锂锄锅锆锉锋锌锍锐锑错锚锛锜锟锡锢锣锥锦锭键锯锰锱锲锳锵锷锹锻锼锾镀镁镂镇镉镊镌镍镐镑镒镖镗镘镜镝镞镟镫镭镮镯镰镳镶长门闩闪闫闬闭问闯闰闱闳间闵闷闸闹闺闻闽闾阀阁阂阃阅阆阉阎阐阑阓阔阕阖阗阙阛队阳阴阵阶际陆陇陈陉陕陨险随隐隶隽难雏雳雾霁霡霭静靥鞑鞯韦韧韩韪韬韵页顶顷项顺顼顽顾顿颀颁颂预颅领颇颈颉颊颌颍颎颐频颒颓颔颕颖颗题颚颛颜额颞颠颢颤颦颧风飒飓飕飖飘飙飚飞飨餍饤饦饨饪饬饭饮饯饰饱饲饴饵饶饷饺饼饽饾饿馁馂馄馅馆馈馊馋馎馏馑馒馔马驭驮驯驰驱驲驳驴驶驷驸驹驻驼驽驾驿骁骂骃骄骅骆骇骈骉骊骋验骏骑骓骔骕骗骚骛骞骠骡骤骥骦髅髋鬓魇魉鱼鱿鲀鲁鲄鲇鲈鲋鲍鲎鲐鲑鲒鲓鲔鲕鲖鲗鲘鲛鲜鲝鲡鲢鲣鲤鲧鲨鲫鲭鲳鲴鲷鲸鲻鲽鲿鳃鳄鳅鳋鳌鳍鳏鳒鳔鳕鳖鳗鳚鳛鳜鳝鳞鳟鳠鳣鸟鸠鸡鸢鸣鸥鸦鸧鸩鸪鸫鸬鸭鸮鸯鸰鸲鸳鸴鸵鸶鸽鸾鸿鹂鹃鹄鹅鹈鹉鹊鹌鹍鹎鹏鹑鹒鹓鹔鹕鹖鹙鹚鹜鹞鹡鹢鹤鹥鹦鹧鹫鹭鹯鹰鹳鹴鹾麦麸黄黉黡黩黪黾鼋齐齿龂龃龄龇龈龉龊龋龌龙龚龛龟𡒄𨱏𪞝𪡏𪢮𪨗𪻐𪾢𫁡𫂈𫄨𫄸𫌀𫌨𫍟𫍰𫍲𫏋𫐆𫐉𫐐𫐓𫓩𫔎𫗠𫗦𫗧𫗮𫗴𫘝𫘣𫘤𫘨𫚈𫚒𫚔𫚕𫚙𫛛𫛢𫛶𫛸".ToUTF32
        Private Shared G2T_T As Char32() = "蔔卜幾几乃迺了瞭乾幹干榦乾於于才纔千韆麼么么屍尸鬥斗豐丰雲云巨鉅扎紮歷曆升昇陞僕仆凶兇醜丑匯彙它牠朮術札劄布佈佔占只衹祇隻嘆歎出齣冬鼕鼕飢饑台臺檯颱發髮沖衝並併并扣釦托託朴樸誇夸劃划當噹籲吁曲麴吊弔團糰回迴朱硃伙夥向嚮后後盡儘奸姦纖縴瀋沈閑閒證証志誌壇罈折摺克剋蘇甦囌杆桿杠槓戒誡鹵滷里裏裡困睏別彆佑祐佣傭谷穀余餘餘系係繫局侷跼注註沾霑帘簾卷捲表錶幸倖拓搨拈撚拐枴抵牴范範苹蘋杯盃板闆松鬆郁鬱采採彩綵制製刮颳岳嶽征徵徑逕念唸舍捨周週弦絃彌瀰嘩譁薑姜煉鍊跡蹟胡鬍蕩盪藥葯咸鹹厘釐面麵嘗嚐哄鬨鍾鐘復複秋鞦須鬚淒悽涌湧席蓆准準症癥挨捱挽輓惡噁獲穫栗慄致緻臟髒嫻嫺淡澹淀澱梁樑麻痲捻撚菇菰戚慼累纍彩綵銜啣欲慾游遊僱雇築筑鋪舖鏈鍊御禦臘腊擺襬蒙矇濛鑑鑒愈瘉簽籤辟闢熔鎔酸痠愿願霉黴噪譟贊讚雕鵰彫於体體儿兒况況畫听聽咏詠阪阪坏壞覆覆庄莊徵怜憐恒恆扑撲挂掛晒曬杰傑栖棲洒灑涂塗烟煙畫著著鹼碱鹼絛縧网網肴餚荐薦葱蔥虫蟲誡鈎鉤銹鏽錘鎚鼹鼴劚噚喎𡢃㠏撝擓㩜殰殨瀇鸂煱獱𤫩𤪺瞜稏筴䊷紬縳絅䋙綐䋻繿繸薳螮𧩙譅貙賰𨦫𨧜𩣑騧䯀䱽鳾鵁鶄鶪鷈萬與專業叢東絲丟兩嚴喪個臨為麗舉義烏樂喬習鄉書買亂爭虧亞產畝親褻億僅從侖倉儀們價眾優會傘偉傳俔傷倀倫傖偽佇僉俠侶僥偵側僑儈儕儂儔儼倆儷倈儉債傾傯償儻儐儲兌兗黨蘭關興茲養獸內岡冊寫軍農馮決凍涼減湊凜鳳憑凱擊鑿芻劉則剛創刪剗剎劊剴劑劍剝劇勸辦務動勵勁勞勢勛勻匱區醫華協單賣盧衛卻廠廳厲壓厭龎廁廂厴廈廚廄廝縣參雙變敘疊葉號嘰嚇呂嗎噸啟吳吶嘸囈嘔員嗆嗚嚨嚀吒響啞嗶嚌噥喲嘮啢嗩喚嘖嗇囀嚙嘯噴嘍囁噯噓嚶囑嚕囂園囪圍國圖圓聖壙場塊堅壢壩塢墳墜壟壘墾堊墊塒堝塹墮牆壯聲殼壺處備夠頭夾奪奩奐奮獎奧妝婦媽嫵嫗姍婁嬈嬌娛媧嬰嬋嬸媼嬪嬤孫學孿寧寶實寵審憲宮寬賓寢對尋導壽將爾塵堯尷層屜屆屬屢屨嶼歲豈嶇崗峴嵐島嶺崬峽崢巒嶮嶄嶸嶔巔鞏幣帥師幃帳幟帶幀幫幗冪廣慶廬庫應廟龐廢開異棄弒張弳彎彈強歸錄彞彥徹徠憶懺憂愾懷態慫憮悵愴總戀懇慟懨愷惻惱悅懸悞憫驚懼慘懲憊愜慚憚慣慍憤懾懣懶戇戲戰戩戱戶執擴捫掃揚擾撫拋掄搶護報擔擬攏揀擁攔擰撥擇摯攣掗撾撻挾撓擋掙擠揮挩撈損撿換搗據擄摑擲摻攬撳攙擱摟攪攜攝搖攤撐攆擷擻攢敵斂數齋斕斬斷無舊時曠暘曇晝顯晉曉曄暈暉暫曖機殺雜權條來楊榪極構樅樞棗槍楓梟櫃檸梔柵標棧櫛棟欄樹樣欒橈楨檔橋樺檜槳樁夢檢欞槨櫝橢樓欖榲櫬櫚櫸檻檳櫧橫檣櫻櫫櫥櫓歡歟歐殲歿殤殘殮殯毆毀轂畢斃氈氣氫氬氳漢湯洶溝沒漚瀝淪滄滬濘淚澩瀧瀘瀉潑澤涇潔窪浹淺漿澆濁測濟瀏渾滸濃潯濤澇漣渦渙滌潤澗漲澀淵漬瀆漸澠漁滲溫灣濕潰濺滾滯滿瀅濾濫灤濱灘澦瀟瀲濰潛瀾瀨瀕滅燈靈災燦煬爐燉煒點熾爍爛燭煩燒燴燙燼熱煥燜燾熅燻愛爺牘牽犧犢狀獷猶狽獰獨狹獅猙獄獵獼玀豬貓蝟獻獺璣瑪瑋環現瑲璽琺瓏琿璉瑣瓊瑤瓔瓚甌電暢疇癤療瘧癘瘍瘡瘋皰痙癢瘂癆瘓瘺癟癱癮癩癬癲皚皺皸盞鹽監蓋盜盤眥矓睜睞瞼瞶瞞矚矯磯礬礦碼磚硯礪礫礎碩磑確礙磧禮禎禱禍稟祿禪離禿稈種積稱穢穠稅穌穩穡窮竊竅窵窯竄窩窺竇豎競篤筍筆筧箋籠篩箏籌簡籮簞簫簣簍籃籬籟糴類秈糶粵糞糧糝緊糾紆紅紂紇約級紈紀紉緯紜紘純紕紗綱納縱綸紛紙紋紡紖紐紓線紲紱練組紳細織終縐絆紼絀紹繹經紿綁絨結絝繞繪給絢絳絡絕絞統絹繡綏繼績緒綾緓續綺緋綽鞝緄繩維綿綬繃綢綜綻綰綠綴緇緙緘緬纜緹緲緝縕繢緞緩締縷編緡緣縉縛縝縫縗縞纏縊縑繽縹縵縲纓縮繆繅繚繕繒韁繳罌羅罰罷羈羨翹聳恥聶聾職聯聰肅腸膚骯腎腫脹脅膽勝朧臚脛膠脈膾臍腦膿腳脫臉膩靦騰臏臢輿艦艙艱艷藝節羋蕪蘆葦莧萇蒼苧莖塋繭荊莢蓽蕎薈薺榮葷犖熒蕁蓀蔭葤蒞萊蓮萵瑩鶯蓴蘿螢營縈蕭薩蔣蔞藍薊驀虆薔蘞藺藹蘄蘊藪蘚虜慮虛雖蝦蝕蟻螞蠶蜆蠱蠣蠻蟄蟯蛻蝸蠟蠅蟈蟬螻蠑螿蠨釁補襯袞襖裊襪襲裝襠褲褸襤見觀覎規覓視覽覺覬覥覦覲覷觴觸誾譽謄計訂訃認譏訐訌討讓訕訖訓議訊記訒講諱謳訝訥許訛論訩訟諷設訪訣詁訶評詛識詗詐訴診詆謅詞詔詖譯詒試詩詰詼誠誅話誕詬詮詭詢詣諍該詳詫譸誣語誚誤誥誘誨誑說誦誒請諸諾讀誹課諉諛誰調諂諒諄誶談誼謀諶諜謊諫諧謔謁謂諭諼讒諳諺諦謎諝謨讜謖謝謠謗謙謐謹謾謫謬譚譙譜譎譴譫讖豶貝貞負貟貢財責賢敗賬貨質販貪貧貶購貯貫貳賤賁貼貴貸貿費賀貽賊賈賄賃賂贓資賅賑賒賦賭贖賞賜賙賡賠賧賴賵贅賻賺賽贗贈贍贏贛趙趕趨躉躍踐躂蹺蹕躋踴躊蹤躓躑躡蹣躕躪軀車軋軌軒軔轉軛輪軟轟軻轤軸軼軫輕軾載輊轎較輒輔輛輦輩輝輥輞輟輜輳輻輯輸轡轅轄輾轆轍轔辭辯辮邊遼達遷過邁運還這進遠違連遲邇適選遜遞邐邏遺遙鄧郵鄒鄴鄰鄭酈鄲酇醞醱醬釅釀釋鑾鏨針釘釗釙釧鈒釩釣釵鈣鈦鈍鈔鈉鋇鋼鈑鈐鑰欽鈞鎢鈕鈺錢鉗鈷缽鈳鈸鑽鉀鈿鈾鐵鉑鈴鑠鉛鉚鉉鉍鈹鐸鉶銬鋩錏鋮鐃鐺銅鋁銖銑銛銓鉻銘錚鉸銥鏟銨銀鑄鏗銷鎖鋰鋤鍋鋯銼鋒鋅鋶銳銻錯錨錛錡錕錫錮鑼錐錦錠鍵鋸錳錙鍥鍈鏘鍔鍬鍛鎪鍰鍍鎂鏤鎮鎘鑷鐫鎳鎬鎊鎰鏢鏜鏝鏡鏑鏃鏇鐙鐳鐶鐲鐮鑣鑲長門閂閃閆閈閉問闖閏闈閎間閔悶閘鬧閨聞閩閭閥閣閡閫閱閬閹閻闡闌闠闊闋闔闐闕闤隊陽陰陣階際陸隴陳陘陝隕險隨隱隸雋難雛靂霧霽霢靄靜靨韃韉韋韌韓韙韜韻頁頂頃項順頊頑顧頓頎頒頌預顱領頗頸頡頰頜潁熲頤頻頮頹頷頴穎顆題顎顓顏額顳顛顥顫顰顴風颯颶颼颻飄飆飈飛饗饜飣飥飩飪飭飯飲餞飾飽飼飴餌饒餉餃餅餑餖餓餒餕餛餡館饋餿饞餺餾饉饅饌馬馭馱馴馳驅馹駁驢駛駟駙駒駐駝駑駕驛驍罵駰驕驊駱駭駢驫驪騁驗駿騎騅騌驌騙騷騖騫驃騾驟驥驦髏髖鬢魘魎魚魷魨魯魺鯰鱸鮒鮑鱟鮐鮭鮚鮳鮪鮞鮦鰂鮜鮫鮮鮺鱺鰱鰹鯉鯀鯊鯽鯖鯧鯝鯛鯨鯔鰈鱨鰓鱷鰍鰠鰲鰭鰥鰜鰾鱈鱉鰻䲁鰼鱖鱔鱗鱒鱯鱣鳥鳩雞鳶鳴鷗鴉鶬鴆鴣鶇鸕鴨鴞鴦鴒鴝鴛鷽鴕鷥鴿鸞鴻鸝鵑鵠鵝鵜鵡鵲鵪鵾鵯鵬鶉鶊鵷鷫鶘鶡鶖鶿鶩鷂鶺鷁鶴鷖鸚鷓鷲鷺鸇鷹鸛鸘鹺麥麩黃黌黶黷黲黽黿齊齒齗齟齡齜齦齬齪齲齷龍龔龕龜壈鎝凙嗹圞屩瑽睍鴗䉬絺纁襀覼𧦧諰謏蹻轣軨輗輮鏦鐍餦餔餗餭饘駃駻騃騠鱮鮄鮰鰤鯆鳷鸋鶒鶗".ToUTF32
        Private Shared T2G_T As Char32() = "蔔卜幾几乃迺了瞭乾幹干榦乾於于才纔千韆麼么么屍尸鬥斗豐丰雲云巨鉅扎紮歷曆升昇陞僕仆凶兇醜丑匯彙它牠朮術札劄布佈佔占只衹祇隻嘆歎出齣冬鼕鼕飢饑台臺檯颱發髮沖衝並併并扣釦托託朴樸誇夸劃划當噹籲吁曲麴吊弔團糰回迴朱硃伙夥向嚮后後盡儘奸姦纖縴瀋沈閑閒證証志誌壇罈折摺克剋蘇甦囌杆桿杠槓戒誡鹵滷里裏裡困睏別彆佑祐佣傭谷穀余餘餘系係繫局侷跼注註沾霑帘簾卷捲表錶幸倖拓搨拈撚拐枴抵牴范範苹蘋杯盃板闆松鬆郁鬱采採彩綵制製刮颳岳嶽征徵徑逕念唸舍捨周週弦絃彌瀰嘩譁薑姜煉鍊跡蹟胡鬍蕩盪藥葯咸鹹厘釐面麵嘗嚐哄鬨鍾鐘復複秋鞦須鬚淒悽涌湧席蓆准準症癥挨捱挽輓惡噁獲穫栗慄致緻臟髒嫻嫺淡澹淀澱梁樑麻痲捻撚菇菰戚慼累纍彩綵銜啣欲慾游遊僱雇築筑鋪舖鏈鍊御禦臘腊擺襬蒙矇濛鑑鑒愈瘉簽籤辟闢熔鎔酸痠愿願霉黴噪譟贊讚雕鵰彫於体體儿兒况況畫听聽咏詠阪阪坏壞覆覆庄莊徵怜憐恒恆扑撲挂掛晒曬杰傑栖棲洒灑涂塗烟煙畫著著鹼碱鹼絛縧网網肴餚荐薦葱蔥虫蟲誡鈎鉤銹鏽錘鎚鼹鼴㠏㩜䉬䊷䋙䋻䯀䱽䲁丟亂亞佇來侖侶俔俠倀倆倈倉個們倫偉側偵偽傖傘備傯傳債傷傾僅僉僑僥價儀儂億儈儉儐儔儕償優儲儷儻儼兌兗內兩冊冪凍凙凜凱刪則剎剗剛剝剴創劇劉劊劍劑劚勁動務勛勝勞勢勵勸勻匱區協卻厭厲厴參叢吒吳吶呂員問啞啟啢喎喚喪喬單喲嗆嗇嗎嗚嗩嗶嗹嘍嘔嘖嘮嘯嘰嘸噓噚噥噯噴噸嚀嚇嚌嚕嚙嚨嚴嚶囀囁囂囈囑囪國圍園圓圖圞執堅堊堝堯報場塊塋塒塢塵塹墊墜墮墳墾壈壓壘壙壟壢壩壯壺壽夠夢夾奐奧奩奪奮妝姍娛婁婦媧媼媽嫗嫵嬈嬋嬌嬤嬪嬰嬸孫學孿宮寢實寧審寫寬寵寶將專尋對導尷屆屜屢層屨屩屬岡峴島峽崗崢崬嵐嶄嶇嶔嶮嶸嶺嶼巒巔帥師帳帶幀幃幗幟幣幫庫廁廂廄廈廚廝廟廠廢廣廬廳弒弳張強彈彎彞彥從徠徹恥悅悞悵悶惱惻愛愜愴愷愾態慍慘慚慟慣慫慮慶憂憊憑憚憤憫憮憲憶懇應懣懨懲懶懷懸懺懼懾戀戇戩戰戱戲戶拋挩挾捫掃掄掗掙揀揚換揮損搖搗搶摑摟摯摻撈撐撓撝撥撫撳撻撾撿擁擄擇擊擋擓擔據擠擬擰擱擲擴擷擻擾攆攏攔攙攜攝攢攣攤攪攬敗敘敵數斂斃斕斬斷時晉晝暈暉暘暢暫曄曇曉曖曠書會朧東柵梔條梟棄棗棟棧楊楓楨業極榪榮榲構槍槨槳樁樂樅樓標樞樣樹樺橈橋機橢橫檔檜檢檣檳檸檻櫃櫓櫚櫛櫝櫥櫧櫫櫬櫸櫻欄權欒欖欞欽歐歟歡歲歸歿殘殤殨殮殯殰殲殺殼毀毆氈氣氫氬氳決沒洶浹涇涼淚淪淵淺渙減渦測渾湊湯溝溫滄滅滌滬滯滲滸滾滿漁漚漢漣漬漲漸漿潁潑潔潛潤潯潰澀澆澇澗澠澤澦澩濁濃濕濘濟濤濫濰濱濺濾瀅瀆瀇瀉瀏瀕瀘瀝瀟瀧瀨瀲瀾灘灣灤災為烏無煒煥煩煬煱熅熒熱熲熾燈燉燒燙燜營燦燭燴燻燼燾爍爐爛爭爺爾牆牘牽犖犢犧狀狹狽猙猶獄獅獎獨獰獱獵獷獸獺獻獼玀現琺琿瑋瑣瑤瑩瑪瑲瑽璉璣環璽瓊瓏瓔瓚甌產畝畢異疇疊痙瘂瘋瘍瘓瘡瘧瘺療癆癘癟癢癤癩癬癮癱癲皚皰皸皺盜盞監盤盧眥眾睍睜睞瞜瞞瞶瞼矓矚矯硯碩確碼磑磚磧磯礎礙礦礪礫礬祿禍禎禪禮禱禿秈稅稈稏稟種稱穌積穎穠穡穢穩窩窪窮窯窵窺竄竅竇竊競筆筍筧筴箋箏節篤篩簍簞簡簣簫籃籌籟籠籬籮粵糝糞糧糴糶糾紀紂約紅紆紇紈紉紋納紐紓純紕紖紗紘紙級紛紜紡紬細紱紲紳紹紼紿絀終組絅絆結絕絝絞絡絢給絨統絲絳絹絺綁綏綐經綜綠綢綬維綰綱綴綸綺綻綽綾綿緄緇緊緋緒緓緘緙線緝緞締緡緣編緩緬緯緲練緹縈縉縊縐縑縕縗縛縝縞縣縫縮縱縲縳縵縷縹總績繃繅繆繒織繕繚繞繡繢繩繪繭繳繸繹繼繽繿纁續纏纓纜缽罌罰罵罷羅羈羋羨義習翹聖聞聯聰聲聳聶職聾肅脅脈脛脫脹腎腦腫腳腸膚膠膩膽膾膿臉臍臏臚臢臨與興舉舊艙艦艱艷芻苧茲荊莖莢莧華萇萊萬萵葉葤葦葷蒞蒼蓀蓋蓮蓴蓽蔞蔣蔭蕁蕎蕪蕭薈薊薔薩薳薺藍藝藪藹藺蘄蘆蘊蘚蘞蘭蘿虆處虛虜號虧蛻蜆蝕蝟蝦蝸螞螢螮螻螿蟄蟈蟬蟯蟻蠅蠑蠟蠣蠨蠱蠶蠻衛袞裊補裝褲褸褻襀襖襠襤襪襯襲見覎規覓視覥覦親覬覲覷覺覼覽觀觴觸訂訃計訊訌討訐訒訓訕訖記訛訝訟訣訥訩訪設許訴訶診詁詆詐詒詔評詖詗詛詞詢詣試詩詫詬詭詮詰話該詳詼誅認誑誒誕誘誚語誠誣誤誥誦誨說誰課誶誹誼誾調諂諄談諉請諍諒論諛諜諝諦諧諫諭諰諱諳諶諷諸諺諼諾謀謁謂謄謅謊謎謏謐謔謖謗謙講謝謠謨謫謬謳謹謾譅譎譏識譙譚譜譫譯議譴護譸譽讀變讒讓讖讜豈豎豬豶貓貙貝貞貟負財貢貧貨販貪貫責貯貳貴貶買貸費貼貽貿賀賁賂賃賄賅資賈賊賑賒賓賙賜賞賠賡賢賣賤賦賧質賬賭賰賴賵賺賻購賽贅贈贍贏贓贖贗贛趕趙趨踐踴蹕蹣蹤蹺蹻躂躉躊躋躍躑躓躕躡躪軀車軋軌軍軒軔軛軟軨軫軸軻軼軾較載輊輒輔輕輗輛輜輝輞輟輥輦輩輪輮輯輳輸輻輾輿轂轄轅轆轉轍轎轔轟轡轣轤辦辭辮辯農這連進運過達違遙遜遞遠適遲遷選遺遼邁還邇邊邏邐郵鄉鄒鄧鄭鄰鄲鄴酇酈醞醫醬醱釀釁釅釋釗釘釙針釣釧釩釵鈉鈍鈐鈑鈒鈔鈕鈞鈣鈦鈳鈴鈷鈸鈹鈺鈾鈿鉀鉉鉍鉑鉗鉚鉛鉶鉸鉻銀銅銑銓銖銘銛銥銨銬銳銷銻銼鋁鋅鋇鋒鋤鋩鋮鋯鋰鋶鋸鋼錄錏錐錕錙錚錛錠錡錢錦錨錫錮錯錳鍈鍋鍍鍔鍛鍥鍬鍰鍵鎂鎊鎖鎘鎝鎢鎪鎬鎮鎰鎳鏃鏇鏑鏗鏘鏜鏝鏟鏡鏢鏤鏦鏨鐃鐍鐙鐫鐮鐲鐳鐵鐶鐸鐺鑄鑠鑣鑰鑲鑷鑼鑽鑾鑿長門閂閃閆閈閉開閎閏間閔閘閡閣閥閨閩閫閬閭閱閹閻闈闊闋闌闐闔闕闖關闠闡闤陘陝陣陰陳陸陽隊階隕際隨險隱隴隸雋雖雙雛雜雞離難電霢霧霽靂靄靈靜靦靨鞏鞝韁韃韉韋韌韓韙韜韻響頁頂頃項順頊頌頎預頑頒頓頗領頜頡頤頭頮頰頴頷頸頹頻顆題額顎顏顓顛類顥顧顫顯顰顱顳顴風颯颶颻颼飄飆飈飛飣飥飩飪飭飯飲飴飼飽飾餃餅餉養餌餑餒餓餔餕餖餗餛餞餡餦館餭餺餾餿饅饉饋饌饒饗饘饜饞馬馭馮馱馳馴馹駁駃駐駑駒駕駙駛駝駟駢駭駰駱駻駿騁騃騅騌騎騖騙騠騧騫騰騷騾驀驃驅驊驌驍驕驗驚驛驟驢驥驦驪驫骯髏髖鬢鬧魎魘魚魨魯魷魺鮄鮐鮑鮒鮚鮜鮞鮦鮪鮫鮭鮮鮰鮳鮺鯀鯆鯉鯊鯔鯖鯛鯝鯧鯨鯰鯽鰂鰈鰍鰓鰜鰠鰤鰥鰭鰱鰲鰹鰻鰼鰾鱈鱉鱒鱔鱖鱗鱟鱣鱨鱮鱯鱷鱸鱺鳥鳩鳳鳴鳶鳷鳾鴆鴉鴒鴕鴗鴛鴝鴞鴣鴦鴨鴻鴿鵁鵑鵜鵝鵠鵡鵪鵬鵯鵲鵷鵾鶄鶇鶉鶊鶒鶖鶗鶘鶡鶩鶪鶬鶯鶴鶺鶿鷁鷂鷈鷓鷖鷗鷥鷫鷲鷹鷺鷽鸂鸇鸋鸕鸘鸚鸛鸝鸞鹺鹽麗麥麩黃黌點黨黲黶黷黽黿齊齋齒齗齜齟齡齦齪齬齲齷龍龎龐龔龕龜𡢃𤪺𤫩𧦧𧩙𨦫𨧜𩣑".ToUTF32
        Private Shared T2G_G As Char32() = "卜卜几几乃乃了了干干干干乾于于才才千千么么幺尸尸斗斗丰丰云云巨巨扎扎历历升升升仆仆凶凶丑丑汇汇它它术术札札布布占占只只只只叹叹出出冬冬咚饥饥台台台台发发冲冲并并并扣扣托托朴朴夸夸划划当当吁吁曲曲吊吊团团回回朱朱伙伙向向后后尽尽奸奸纤纤沈沈闲闲证证志志坛坛折折克克苏苏苏杆杆杠杠戒戒卤卤里里里困困别别佑佑佣佣谷谷余余馀系系系局局局注注沾沾帘帘卷卷表表幸幸拓拓拈拈拐拐抵抵范范苹苹杯杯板板松松郁郁采采采采制制刮刮岳岳征征径径念念舍舍周周弦弦弥弥哗哗姜姜炼炼迹迹胡胡荡荡药药咸咸厘厘面面尝尝哄哄钟钟复复秋秋须须凄凄涌涌席席准准症症挨挨挽挽恶恶获获栗栗致致脏脏娴娴淡淡淀淀梁梁麻麻捻捻菇菇戚戚累累彩彩衔衔欲欲游游雇雇筑筑铺铺链链御御腊腊摆摆蒙蒙蒙鉴鉴愈愈签签辟辟熔熔酸酸愿愿霉霉噪噪赞赞雕雕雕於体体儿儿况况划听听咏咏阪坂坏坏覆复庄庄徵怜怜恒恒扑扑挂挂晒晒杰杰栖栖洒洒涂涂烟烟画著着硷碱碱绦绦网网肴肴荐荐葱葱虫虫诫钩钩锈锈锤锤鼹鼹㟆㨫𫂈䌶䌺䌾䯅䲝鳚丢乱亚伫来仑侣伣侠伥俩俫仓个们伦伟侧侦伪伧伞备偬传债伤倾仅佥侨侥价仪侬亿侩俭傧俦侪偿优储俪傥俨兑兖内两册幂冻𪞝凛凯删则刹刬刚剥剀创剧刘刽剑剂㔉劲动务勋胜劳势励劝匀匮区协却厌厉厣参丛咤吴呐吕员问哑启唡㖞唤丧乔单哟呛啬吗呜唢哔𪡏喽呕啧唠啸叽呒嘘㖊哝嗳喷吨咛吓哜噜啮咙严嘤啭嗫嚣呓嘱囱国围园圆图𪢮执坚垩埚尧报场块茔埘坞尘堑垫坠堕坟垦𡒄压垒圹垄坜坝壮壶寿够梦夹奂奥奁夺奋妆姗娱娄妇娲媪妈妪妩娆婵娇嬷嫔婴婶孙学孪宫寝实宁审写宽宠宝将专寻对导尴届屉屡层屦𪨗属冈岘岛峡岗峥岽岚崭岖嵚崄嵘岭屿峦巅帅师帐带帧帏帼帜币帮库厕厢厩厦厨厮庙厂废广庐厅弑弪张强弹弯彝彦从徕彻耻悦悮怅闷恼恻爱惬怆恺忾态愠惨惭恸惯怂虑庆忧惫凭惮愤悯怃宪忆恳应懑恹惩懒怀悬忏惧慑恋戆戬战戯戏户抛捝挟扪扫抡挜挣拣扬换挥损摇捣抢掴搂挚掺捞撑挠㧑拨抚揿挞挝捡拥掳择击挡㧟担据挤拟拧搁掷扩撷擞扰撵拢拦搀携摄攒挛摊搅揽败叙敌数敛毙斓斩断时晋昼晕晖旸畅暂晔昙晓暧旷书会胧东栅栀条枭弃枣栋栈杨枫桢业极杩荣榅构枪椁桨桩乐枞楼标枢样树桦桡桥机椭横档桧检樯槟柠槛柜橹榈栉椟橱槠橥榇榉樱栏权栾榄棂钦欧欤欢岁归殁残殇㱮殓殡㱩歼杀壳毁殴毡气氢氩氲决没汹浃泾凉泪沦渊浅涣减涡测浑凑汤沟温沧灭涤沪滞渗浒滚满渔沤汉涟渍涨渐浆颍泼洁潜润浔溃涩浇涝涧渑泽滪泶浊浓湿泞济涛滥潍滨溅滤滢渎㲿泻浏濒泸沥潇泷濑潋澜滩湾滦灾为乌无炜焕烦炀㶽煴荧热颎炽灯炖烧烫焖营灿烛烩熏烬焘烁炉烂争爷尔墙牍牵荦犊牺状狭狈狰犹狱狮奖独狞㺍猎犷兽獭献猕猡现珐珲玮琐瑶莹玛玱𪻐琏玑环玺琼珑璎瓒瓯产亩毕异畴叠痉痖疯疡痪疮疟瘘疗痨疠瘪痒疖癞癣瘾瘫癫皑疱皲皱盗盏监盘卢眦众𪾢睁睐䁖瞒瞆睑眬瞩矫砚硕确码硙砖碛矶础碍矿砺砾矾禄祸祯禅礼祷秃籼税秆䅉禀种称稣积颖秾穑秽稳窝洼穷窑窎窥窜窍窦窃竞笔笋笕䇲笺筝节笃筛篓箪简篑箫篮筹籁笼篱箩粤糁粪粮籴粜纠纪纣约红纡纥纨纫纹纳纽纾纯纰纼纱纮纸级纷纭纺䌷细绂绁绅绍绋绐绌终组䌹绊结绝绔绞络绚给绒统丝绛绢𫄨绑绥䌼经综绿绸绶维绾纲缀纶绮绽绰绫绵绲缁紧绯绪绬缄缂线缉缎缔缗缘编缓缅纬缈练缇萦缙缢绉缣缊缞缚缜缟县缝缩纵缧䌸缦缕缥总绩绷缫缪缯织缮缭绕绣缋绳绘茧缴䍁绎继缤䍀𫄸续缠缨缆钵罂罚骂罢罗羁芈羡义习翘圣闻联聪声耸聂职聋肃胁脉胫脱胀肾脑肿脚肠肤胶腻胆脍脓脸脐膑胪臜临与兴举旧舱舰艰艳刍苎兹荆茎荚苋华苌莱万莴叶荮苇荤莅苍荪盖莲莼荜蒌蒋荫荨荞芜萧荟蓟蔷萨䓕荠蓝艺薮蔼蔺蕲芦蕴藓蔹兰萝蔂处虚虏号亏蜕蚬蚀猬虾蜗蚂萤䗖蝼螀蛰蝈蝉蛲蚁蝇蝾蜡蛎蟏蛊蚕蛮卫衮袅补装裤褛亵𫌀袄裆褴袜衬袭见觃规觅视觍觎亲觊觐觑觉𫌨览观觞触订讣计讯讧讨讦讱训讪讫记讹讶讼诀讷讻访设许诉诃诊诂诋诈诒诏评诐诇诅词询诣试诗诧诟诡诠诘话该详诙诛认诳诶诞诱诮语诚诬误诰诵诲说谁课谇诽谊訚调谄谆谈诿请诤谅论谀谍谞谛谐谏谕𫍰讳谙谌讽诸谚谖诺谋谒谓誊诌谎谜𫍲谧谑谡谤谦讲谢谣谟谪谬讴谨谩䜧谲讥识谯谭谱谵译议谴护诪誉读变谗让谶谠岂竖猪豮猫䝙贝贞贠负财贡贫货贩贪贯责贮贰贵贬买贷费贴贻贸贺贲赂赁贿赅资贾贼赈赊宾赒赐赏赔赓贤卖贱赋赕质账赌䞐赖赗赚赙购赛赘赠赡赢赃赎赝赣赶赵趋践踊跸蹒踪跷𫏋跶趸踌跻跃踯踬蹰蹑躏躯车轧轨军轩轫轭软𫐉轸轴轲轶轼较载轾辄辅轻𫐐辆辎辉辋辍辊辇辈轮𫐓辑辏输辐辗舆毂辖辕辘转辙轿辚轰辔𫐆轳办辞辫辩农这连进运过达违遥逊递远适迟迁选遗辽迈还迩边逻逦邮乡邹邓郑邻郸邺酂郦酝医酱酦酿衅酽释钊钉钋针钓钏钒钗钠钝钤钣钑钞钮钧钙钛钶铃钴钹铍钰铀钿钾铉铋铂钳铆铅铏铰铬银铜铣铨铢铭铦铱铵铐锐销锑锉铝锌钡锋锄铓铖锆锂锍锯钢录铔锥锟锱铮锛锭锜钱锦锚锡锢错锰锳锅镀锷锻锲锹锾键镁镑锁镉𨱏钨锼镐镇镒镍镞镟镝铿锵镗镘铲镜镖镂𫓩錾铙𫔎镫镌镰镯镭铁镮铎铛铸铄镳钥镶镊锣钻銮凿长门闩闪闫闬闭开闳闰间闵闸阂阁阀闺闽阃阆闾阅阉阎闱阔阕阑阗阖阙闯关阓阐阛陉陕阵阴陈陆阳队阶陨际随险隐陇隶隽虽双雏杂鸡离难电霡雾霁雳霭灵静腼靥巩绱缰鞑鞯韦韧韩韪韬韵响页顶顷项顺顼颂颀预顽颁顿颇领颌颉颐头颒颊颕颔颈颓频颗题额颚颜颛颠类颢顾颤显颦颅颞颧风飒飓飖飕飘飙飚飞饤饦饨饪饬饭饮饴饲饱饰饺饼饷养饵饽馁饿𫗦馂饾𫗧馄饯馅𫗠馆𫗮馎馏馊馒馑馈馔饶飨𫗴餍馋马驭冯驮驰驯驲驳𫘝驻驽驹驾驸驶驼驷骈骇骃骆𫘣骏骋𫘤骓骔骑骛骗𫘨䯄骞腾骚骡蓦骠驱骅骕骁骄验惊驿骤驴骥骦骊骉肮髅髋鬓闹魉魇鱼鲀鲁鱿鲄𫚒鲐鲍鲋鲒鲘鲕鲖鲔鲛鲑鲜𫚔鲓鲝鲧𫚙鲤鲨鲻鲭鲷鲴鲳鲸鲇鲫鲗鲽鳅鳃鳒鳋𫚕鳏鳍鲢鳌鲣鳗鳛鳔鳕鳖鳟鳝鳜鳞鲎鳣鲿𫚈鳠鳄鲈鲡鸟鸠凤鸣鸢𫛛䴓鸩鸦鸰鸵𫁡鸳鸲鸮鸪鸯鸭鸿鸽䴔鹃鹈鹅鹄鹉鹌鹏鹎鹊鹓鹍䴖鸫鹑鹒𫛶鹙𫛸鹕鹖鹜䴗鸧莺鹤鹡鹚鹢鹞䴘鹧鹥鸥鸶鹔鹫鹰鹭鸴㶉鹯𫛢鸬鹴鹦鹳鹂鸾鹾盐丽麦麸黄黉点党黪黡黩黾鼋齐斋齿龂龇龃龄龈龊龉龋龌龙厐庞龚龛龟㛠㻘㻏𫍟䜥䦀䦁䯃".ToUTF32

        Private Shared T2J_T As Char32() = "並予豫五伍停千倣布占百幸借藉仿仃家像象象凶塚塚冬鼕沖沖淒兇勤懃匹疋仟佔止合閤向嚮笑笑唇脣座坑阬聖復娘孃娘傢岐歧岩巖岳嶽巖巨鉅佈希稀席蓆帳賬干乾乾並倖坐弁辨弦絃志誌誌淒欲扇煽搭搨斤觔暖煖杯盃漆欠缺慾只育盅淡澹柒照炤疊佰研硎並筋觔箇個個糸絲系繫繡繫繡翻缶罐牴繙堊毓芸藝複復牴証證諮咨避逮迭疊迨遊游辟里裏裏鐘鍾鑑鑒閤乃乘乩亙亞仇仞份伙佛佞侄俎俟俱值假偷傳僭價儉儘兒兔兩冤冰凞函剩劍劑勛勞勸卮卯吝吞吳咒哄唉唧啞啟喃喧單喻嚏嚙嚴囊圈圍圓圖團址堯塈增壓壞壤壺夭奘奩妒姊姬娛媮媯嫻嬝寇實寬專尥峨崖崙嵯嶔巢巴帚帡帶幢幫廁廄廢廣廳彈彿從徵德恂恿悴惠惡慚應懶懷戛戰戲戶戾扎抉拂拉拔拚拜捲插揭搔搖搜搥搧摒擇擊擴攝攢攪收效敪敿晃晚曆曉曳曾朐朘朵查柩桌棕棧棹椫榆榮樂樣樽橢檢檳檼櫻權欣歡步歲歷歸殼每氣汎汙沍泖泛涉涕涸淚淨淫渴溈溉溜溪溯溼滿漾潀潑澀澔澤濟濱瀆瀏瀨灌灔灤灩灶炯焰煉熙燒燕營燻爛牆牠犛犧獵獸玆瑙產畬當瘦發皋盡眾瞭砧碎礦稟稻穗笆箋箝篠簑籐籠籲粥粹糯絕經綠綢緣縱總繈繩繪繼纊續纖罶羹翠聰聽肅胡胭腦臟舉舍茅荔莆莊莓菔菟菴萌萼蒙蓑蕊薰藏藥藪蘗虎處虱螗螢蟆蟒蟹蠅蠟蠢蠹褲襤覸覺觀訶詻說諫謠謭譁譯讀變讓豎豐豔豜豺貉賣賴贊躗躭躲軿輕轉迪遞遲邊鄉鄰醉醋醬醱釆釋釜鉤銳鋪錄錢鎔鏽鑄閱關阱陝陷隋隕險隱隸雁雜雞霍霸靈韌韭頰頸頹顏顯餐饋馱騷騺驅驗驛髓髮鬥鬧鬱鱉鶯鷗鹼鹽黑默鼠齊齋齒齡龍龐龜".ToUTF32
        Private Shared T2J_J As Char32() = "並予予五五停千倣布占百幸借借倣停家像象像凶塚冢冬冬沖冲凄凶勤勤匹匹千占止合合向向笑咲唇唇座坑坑聖復娘娘嬢家岐岐岩岩岳岳巌巨巨布希希席席帳帳干乾干并幸座弁弁弦弦志誌志悽欲扇扇搭搭斤斤暖暖杯杯漆欠欠欲止育沖淡淡漆照照畳百研研竝筋筋箇個箇糸糸系系綉繋繍翻缶缶羝翻聖育芸芸複複觝証証諮諮避逮迭迭逮遊遊避里裏里鐘鐘鑑鑑闔廼乗稽亘亜讐仭彬夥仏侫姪爼竢倶値仮偸伝僣価倹侭児兎両寃氷凞凾剰剣剤勲労勧巵夘悋呑呉呪閧欸喞唖啓娚諠単喩嚔囓厳嚢圏囲円図団阯尭墍増圧壊壌壷殀弉匳妬姉姫娯婾嬀嫺嫋冦実寛専尦峩崕崘嵳崟巣芭菷帲帯橦幇厠廐廃広庁弾髴従徴徳悛慂忰恵悪慙応嬾懐戞戦戯戸戻紮刔払柆抜抃拝巻扱掲掻揺捜捶煽屏択撃拡摂攅撹収効敠敽晄晩暦暁曵曽胊脧朶査柾棹椶桟櫂樿楡栄楽様墫楕検梹櫽桜権忻歓歩歳歴帰殻毎気泛汚冱茅氾渉洟凅涙浄婬渇潙漑澑渓遡湿満瀁潨溌渋浩沢済浜涜嚠瀬潅灎欒灔竃烱焔錬煕焼讌営熏燗墻它髦犠猟獣茲碯産畲当痩発皐尽衆暸碪砕鉱廩稲穂巴牋鉗筱簔籘篭吁鬻粋粫絶経緑紬縁縦総繦縄絵継絖続繊羀羮翆聡聴粛糊臙脳臓挙舎茆茘蒲荘苺蔔莵庵萠蕚朦簑蘂薫蔵薬籔蘖乕処蝨螳蛍蟇蠎蠏蝿蝋惷蠧袴繿覵覚観呵咯説諌謡譾嘩訳読変譲竪豊艷豣犲狢売頼賛躛耽躱輧軽転廸逓遅辺郷隣酔酢醤醗采釈釡鈎鋭舗録銭熔銹鋳閲関穽陜陥陏殞険隠隷鴈雑鶏癨覇霊靭韮頬頚頽顔顕喰餽駄騒驇駆験駅髄髪闘閙欝鼈鴬鴎鹸塩黒黙鼡斉斎歯齢竜厖亀".ToUTF32
        Private Shared J2T_J As Char32() = "並予豫五伍停千倣布占百幸借藉仿仃家像象象凶塚塚冬鼕沖沖淒兇勤懃匹疋仟佔止合閤向嚮笑笑唇脣座坑阬聖復娘孃娘傢岐歧岩巖岳嶽巖巨鉅佈希稀席蓆帳賬干乾乾並倖坐弁辨弦絃志誌誌淒欲扇煽搭搨斤觔暖煖杯盃漆欠缺慾只育盅淡澹柒照炤疊佰研硎並筋觔箇個個糸絲系繫繡繫繡翻缶罐牴繙堊毓芸藝複復牴証證諮咨避逮迭疊迨遊游辟里裏裏鐘鍾鑑鑒閤乃乘乩亙亞仇仞份伙佛佞侄俎俟俱值假偷傳僭價儉儘兒兔兩冤冰凞函剩劍劑勛勞勸卮卯吝吞吳咒哄唉唧啞啟喃喧單喻嚏嚙嚴囊圈圍圓圖團址堯塈增壓壞壤壺夭奘奩妒姊姬娛媮媯嫻嬝寇實寬專尥峨崖崙嵯嶔巢巴帚帡帶幢幫廁廄廢廣廳彈彿從徵德恂恿悴惠惡慚應懶懷戛戰戲戶戾扎抉拂拉拔拚拜捲插揭搔搖搜搥搧摒擇擊擴攝攢攪收效敪敿晃晚曆曉曳曾朐朘朵查柩桌棕棧棹椫榆榮樂樣樽橢檢檳檼櫻權欣歡步歲歷歸殼每氣汎汙沍泖泛涉涕涸淚淨淫渴溈溉溜溪溯溼滿漾潀潑澀澔澤濟濱瀆瀏瀨灌灔灤灩灶炯焰煉熙燒燕營燻爛牆牠犛犧獵獸玆瑙產畬當瘦發皋盡眾瞭砧碎礦稟稻穗笆箋箝篠簑籐籠籲粥粹糯絕經綠綢緣縱總繈繩繪繼纊續纖罶羹翠聰聽肅胡胭腦臟舉舍茅荔莆莊莓菔菟菴萌萼蒙蓑蕊薰藏藥藪蘗虎處虱螗螢蟆蟒蟹蠅蠟蠢蠹褲襤覸覺觀訶詻說諫謠謭譁譯讀變讓豎豐豔豜豺貉賣賴贊躗躭躲軿輕轉迪遞遲邊鄉鄰醉醋醬醱釆釋釜鉤銳鋪錄錢鎔鏽鑄閱關阱陝陷隋隕險隱隸雁雜雞霍霸靈韌韭頰頸頹顏顯餐饋馱騷騺驅驗驛髓髮鬥鬧鬱鱉鶯鷗鹼鹽黑默鼠齊齋齒齡龍龐龜".ToUTF32
        Private Shared J2T_T As Char32() = "並予予五五停千倣布占百幸借借倣停家像象像凶塚冢冬冬沖冲凄凶勤勤匹匹千占止合合向向笑咲唇唇座坑坑聖復娘娘嬢家岐岐岩岩岳岳巌巨巨布希希席席帳帳干乾干并幸座弁弁弦弦志誌志悽欲扇扇搭搭斤斤暖暖杯杯漆欠欠欲止育沖淡淡漆照照畳百研研竝筋筋箇個箇糸糸系系綉繋繍翻缶缶羝翻聖育芸芸複複觝証証諮諮避逮迭迭逮遊遊避里裏里鐘鐘鑑鑑闔廼乗稽亘亜讐仭彬夥仏侫姪爼竢倶値仮偸伝僣価倹侭児兎両寃氷凞凾剰剣剤勲労勧巵夘悋呑呉呪閧欸喞唖啓娚諠単喩嚔囓厳嚢圏囲円図団阯尭墍増圧壊壌壷殀弉匳妬姉姫娯婾嬀嫺嫋冦実寛専尦峩崕崘嵳崟巣芭菷帲帯橦幇厠廐廃広庁弾髴従徴徳悛慂忰恵悪慙応嬾懐戞戦戯戸戻紮刔払柆抜抃拝巻扱掲掻揺捜捶煽屏択撃拡摂攅撹収効敠敽晄晩暦暁曵曽胊脧朶査柾棹椶桟櫂樿楡栄楽様墫楕検梹櫽桜権忻歓歩歳歴帰殻毎気泛汚冱茅氾渉洟凅涙浄婬渇潙漑澑渓遡湿満瀁潨溌渋浩沢済浜涜嚠瀬潅灎欒灔竃烱焔錬煕焼讌営熏燗墻它髦犠猟獣茲碯産畲当痩発皐尽衆暸碪砕鉱廩稲穂巴牋鉗筱簔籘篭吁鬻粋粫絶経緑紬縁縦総繦縄絵継絖続繊羀羮翆聡聴粛糊臙脳臓挙舎茆茘蒲荘苺蔔莵庵萠蕚朦簑蘂薫蔵薬籔蘖乕処蝨螳蛍蟇蠎蠏蝿蝋惷蠧袴繿覵覚観呵咯説諌謡譾嘩訳読変譲竪豊艷豣犲狢売頼賛躛耽躱輧軽転廸逓遅辺郷隣酔酢醤醗采釈釡鈎鋭舗録銭熔銹鋳閲関穽陜陥陏殞険隠隷鴈雑鶏癨覇霊靭韮頬頚頽顔顕喰餽駄騒驇駆験駅髄髪闘閙欝鼈鴬鴎鹸塩黒黙鼡斉斎歯齢竜厖亀".ToUTF32

        Private Shared G2J_G As Char32() = "据艺叠罐娘缺豫冲复复干个个后获获机里历历舍圣系帐征只志制准辨瓣辩斗芸舍刬叠垩滦禀账陨干千斗升升冲并并后志里谷谷系采采制征周周准淡彩覆复予五伍停百借藉仃像象象仟止笑笑座娘岐歧希稀坐弁覆徵扇煽漆欠只育盅澹柒佰研硎缶毓菔避逮迭迨辟刬卜坂廪据机栾殒徵阪㑩㔉㖞㧑㨫㱩㱮㲿㶉㺍䌸䍁䗖䜣䜧䝙䞐䯄䴖䴗丑专业丛东丝两严丧丰临为丽举乃么义乌乐乔乘习乡书乩买亏云亚产亩亲亵亿仅仆仇从仑仓仞仪们价份仿众优伙伛伞伟传伤伦伪伫佛佞佣佥侄侣侥侦侧侨侪侬俎俟俦俨俩俪俭俱债值倾假偷偻偿傥储傩僭儿兑兔兰关兴兹养兽冈册军农冤冯冰决况冻净凉减凑凛几凤凫凭凯击函凿刍划刘则刚创删别刭剀剂剑剧剩剿劝务动劲劳势勋匮华协单卖卢卤卫卮卯卷厂厅压厌厕厢厦厨厩厮县发变叶叹吃吊吓吕吝吞吨听启吴呐呓呕呗员呙呜咏咒咛咨咸哄响哑哒哗哜唉唤唧啖啧啬啭啮啸喃喧喷喻嗫嘤噪嚏嚣囊团园围图圆圈圹场址坏块坚坛坞坟坠垄垒垦埘埚堑墙增壤壳壶壹处备夭头夸夹夺奁奂奋奖奘奸妆妇妈妒妪姊姜姬娄娇娱娴婴婵媪嫔孙宁实宠审宪宫宽宾寇对寻导尔尘尝尧尸层屃屿岁岂岖岚岛岭峣峥峦峨崖崭嵚嵯巢巩巴币帅师帏帘帚帜带帧帮帼幂幢广庄庆庐庑库应庙庞废廿开异弃张弥弯弹强归录彻德忆忏忧忾怀态怂怃怅怆怜总怿恂恳恶恸恻恼恿悫悬悭悮悯悴惊惠惧惩惫惭惮惯愈愠愤愿慑懑懒戋戏战戛户戾扎扑托执扩扪扫扬扰抉抚抟抢护报拂拉拔拚拜拟拣拥拨择挂挚挛挞挠挤挥挽捝捞损换捣掷插揭揽搁搅搔搜摄摆摇摈摊摒攒收效敌敛斋斩无时旷旸昙显晃晋晓晔晕晖晚暂暧曳曾术朵杀杂权杆杠杨杰极构枞枣枥枪枫枭柜柠查柩栀栅标栈栉栋栌栎栏树栖栗样桌桡桤桥桦桧梦检棕棹椁椠椭榄榆榈槛槟樯樱樽橹橼欢欣欤步歼殁殇殚殡毁毂每毕毙毡气汇汉汤汹沟沥沦沧沪泖泛泞泪泷泸泻泼泽洁洒洼浃浆浇浊测济浏浑浒浓浔浚涂涉涌涕涛涟涡涣涤润涧涨涩涸淀淫渊渍渎渐渔渖渗渴游溃溅溉溜溪溯滚满滤滥滨滩漾潇潋潴澜濑濒灌灭灵灶灾灿炀炖炮炯炼炽烁烂烛烟烦烧烬热焕焰煴熙燕爱爷牍牵牺犁犊犹狈狞狮狯狱猎猪猬獭玛环现玱玺珐珑珲琅琐琼瑙瑶瑷璎瓯电畅畴疗疟疠疡疬疮疯疱疴痈痉痒痖痨痫瘘瘦癞癣癫皋皑皱皲盏盐监盖盘眦睑睿瞒瞩瞭矫矶矾矿码砖砚砧砺砾础硕硗确碍碎碛碱祢祯祸禄离秃秆种积秽稳稻穑穗穷窍窎窑窜窝窥窦窭竖竞笃笆笋笔笕笺笼筑筚筛筝筹签简箦箧箫篑篓篮篱籁籴类粜粝粥粪粮粹糯紧纠纡红纣纤约级纩纪纬纭纯纰纱纲纳纵纶纷纸纹纺纻纼纽线绀绁练组绅细织终绊绍绎经绒结绕绖绗绘给绚绛络绝绞统绢绣绥绦继绩绪绫续绮绯绰绳维绵绶绷绸绹绻综绽绾绿缀缁缄缅缆缈缉缊缋缍缎缒缓缔缕编缗缘缙缚缛缝缞缟缠缢缤缥缦缧缨缩缪缬缭缮缲缵罂网罗罚罢罴羁羹翘翠耸耻聂聋职聍联聪肃肠肤肾肿胀胁胄胜胡胧胪胫胭胶脉脍脏脐脑脓脔脸腊腭腻腼腽腾舆舣舰舱艰艳节芜芦苇苌苍苎苏苹范茅茏茑茔茕茧荆荐荔荚荛荞荟荠荡荣荤荦荨荫荮药莆莓莲莳莴莸莹莺莼菟萌萝萤营萧萨萼蒙蓑蓝蓟蓣蓦蔷蔺蔼蕊蕰蕴薮薰藏藓虎虏虑虱虽虾蚀蚁蚬蛊蛎蛏蛰蛲蜕蜗蝇蝼蝾螀螗蟆蟏蟒蟹蠢蠹衅衔补衬衮袄袜袭裆裈裤褛褴见观规觅视觇览觉觊觋觌觍觎觏觐觞訚誊计订讣认讥讦讧讨让讫训议讯记讱讲讳讴讶讷许讹论讼讽设访诀证诂诃评诅识诇诈诉诊诋词诏译诒诔试诗诘诙诚诛话诞诟诠诡询诣诤该详诧诨诪诫诬语诮误诰诱诲诳说诵诶请诸诹诺读诽课谀谁调谄谅谆谈谊谋谍谏谐谑谒谓谔谕谗谙谚谛谜谝谞谟谡谢谣谤谥谦谧谨谩谪谬谭谮谱谲谴谵谶豮豺貉贝贞负贡财责贤败货质贩贪贫贬购贮贯贱贲贳贴贵贷贸费贺贻贼贽贾贿赁赂赃资赆赈赉赋赌赍赎赏赐赑赒赔赖赗赘赙赚赛赝赞赟赠赡赢赵趋跃跄跸跻踌踪踬踯蹑蹒躏躲车轧轨轩转轭轮软轰轲轳轴轶轸轹轻轼载轾轿辂较辄辅辆辇辈辉辍辎辏辐辑输辔辕辖辗辘辙辫边辽达迁过迈运还这进远违连迟迩迪迳迹适选逊递逻遗遥邮邹邻郁郑郸酂酦酱酿醉醋释釜鉴銮錾针钉钏钑钓钖钗钜钝钞钟钢钣钥钦钧钩钮钱钲钳钴钵钶钺钻钿铁铃铄铅铆铉铊铎铏铖铗铙铛铜铠铢铣铨铫铭铮铳银铸铺链铿销锁锄锅锈锋锐锖错锚锠锡锢锣锤锥锦锭键锯锱锳锵锷锹锺锻锽镀镂镇镊镌镐镒镕镘镙镜镝镞镟镠镡镢镣镦镫镴长门闩闪闫闬闭问闯闰闲间闵闷闸闹闺闻闼闾阀阁阄阅阇阈阉阋阎阏阐阑阒阓阔阖阙阛队阱阳阴阵阶际陆陇陈陕险陷隋隐隶隽难雁雏雕雳雾霁霍霭霸靥鞑韦韧韩韬韭韵页顶顷项顺须顽顾顿颁颂颃预颅领颇颈颉颊颍颎颐频颒颓颔颖颗题颚颜额颞颠颤颦颧风飏飒飓飖飘飙飚飞飨餐饣饤饥饦饨饫饬饭饮饯饰饱饲饴饵饶饷饺饼饾饿馁馂馅馆馈馎馏馑馒馔马驭驮驯驰驱驲驳驴驶驷驹驻驼驽驾驿骀骁骂骃骄骆骇骈骊骋验骎骏骐骑骓骔骕骖骗骚骞骠骡骤骥骧髅髓鬓魇魉鱼鲁鲂鲆鲇鲈鲊鲋鲍鲑鲒鲔鲕鲖鲗鲙鲛鲜鲣鲤鲧鲨鲬鲭鲱鲲鲵鲶鲷鲸鲹鲻鲽鲿鳁鳃鳄鳅鳆鳇鳌鳍鳏鳒鳔鳕鳖鳗鳞鳟鳢鳣鸟鸠鸡鸢鸣鸥鸦鸧鸨鸩鸪鸫鸭鸯鸰鸱鸳鸵鸷鸽鸾鸿鹀鹃鹄鹅鹈鹉鹊鹍鹎鹏鹑鹒鹓鹔鹖鹗鹘鹙鹜鹞鹟鹡鹤鹥鹦鹧鹩鹪鹫鹬鹭鹯鹰鹳鹾麸黉黑默黩黪黾鼠齐齑齿龀龂龃龄龆龈龉龊龋龌龙龛龟𡒄𪨗𪾢𫁡𫄨𫄸𫌀𫌨𫍙𫍰𫏋𫐄𫐉𫐐𫐓𫓧𫓩𫔎𫗠𫗧𫗴𫘝𫘤𫘨𫚈𫚒𫚔𫛛𫛶𫛸".ToUTF32
        Private Shared G2J_J As Char32() = "拠芸畳缶嬢欠予衝復複幹個箇後獲穫機裏歴暦捨聖係帳徴隻誌製準弁弁弁闘芸舎鏟迭聖欒廩帳殞干千斗升昇沖並併后志里谷穀系採彩制征周週准淡彩覆覆予五五停百借借停像象像千止笑咲座娘岐岐希希座弁復徴扇扇漆欠止育沖淡漆百研研缶育蔔避逮迭逮避剗蔔坂廩据机欒殞征坂儸劚喎撝㩜殰殨瀇鸂獱縳繸螮訢譅貙賰騧鶄鶪醜専業叢東糸両厳喪豊臨為麗挙廼幺義烏楽喬乗習郷書稽買虧雲亜産畝親褻億僅僕讐従侖倉仭儀們価彬倣衆優夥傴傘偉伝傷倫偽佇仏侫傭僉姪侶僥偵側僑儕儂爼竢儔儼倆儷倹倶債値傾仮偸僂償儻儲儺僣児兌兎蘭関興茲養獣岡冊軍農寃馮氷決況凍淨涼減湊凜幾鳳鳧憑凱撃凾鑿芻劃劉則剛創刪別剄剴剤剣劇剰勦勧務動勁労勢勲匱華協単売盧滷衛巵夘捲廠庁圧厭厠廂廈廚廐廝県発変葉嘆喫弔嚇呂悋呑噸聴啓呉吶囈嘔唄員咼嗚詠呪嚀諮鹹閧響唖噠嘩嚌欸喚喞啗嘖嗇囀囓嘯娚諠噴喩囁嚶譟嚔囂嚢団園囲図円圏壙場阯壊塊堅壇塢墳墜壟塁墾塒堝塹墻増壌殻壷壱処備殀頭誇夾奪匳奐奮奨弉姦粧婦媽妬嫗姉薑姫婁嬌娯嫺嬰嬋媼嬪孫寧実寵審憲宮寛賓冦対尋導爾塵嘗尭屍層屓嶼歳豈嶇嵐島嶺嶢崢巒峩崕嶄崟嵳巣鞏芭幣帥師幃簾菷幟帯幀幇幗冪橦広荘慶廬廡庫応廟厖廃廾開異棄張彌彎弾強帰録徹徳憶懺憂愾懐態慫憮悵愴憐総懌悛懇悪慟惻悩慂愨懸慳悞憫忰驚恵懼懲憊慙憚慣癒慍憤願懾懣嬾戔戯戦戞戸戻紮撲託執拡捫掃揚擾刔撫摶搶護報払柆抜抃拝擬揀擁撥択掛摯攣撻撓擠揮輓挩撈損換搗擲扱掲攬擱撹掻捜摂擺揺擯攤屏攅収効敵斂斎斬無時曠暘曇顕晄晉暁曄暈暉晩暫曖曵曽術朶殺雑権桿槓楊傑極構樅棗櫪槍楓梟櫃檸査柾梔柵標桟櫛棟櫨櫟欄樹棲慄様棹橈榿橋樺檜夢検椶櫂槨槧楕欖楡櫚檻梹檣桜墫櫓櫞歓忻歟歩殲歿殤殫殯毀轂毎畢斃氈気匯漢湯洶溝瀝淪滄滬茅氾濘涙滝瀘瀉溌沢潔灑窪浹漿澆濁測済嚠渾滸濃潯濬塗渉湧洟濤漣渦渙滌潤澗漲渋凅澱婬淵漬涜漸漁瀋滲渇遊潰濺漑澑渓遡滾満濾濫浜灘瀁瀟瀲瀦瀾瀬瀕潅滅霊竃災燦煬燉砲烱錬熾爍燗燭煙煩焼燼熱煥焔熅煕讌愛爺牘牽犠犂犢猶狽獰獅猾獄猟豬蝟獺瑪環現瑲璽琺瓏琿瑯瑣瓊碯瑤璦瓔甌電暢疇療瘧癘瘍癧瘡瘋皰痾癰痙癢瘂癆癇瘻痩癩癬癲皐皚皺皸盞塩監蓋盤眥瞼叡瞞矚暸矯磯礬鉱碼磚硯碪礪礫礎碩磽確礙砕磧鹸禰禎禍祿離禿稈種積穢穏稲穡穂窮竅窵窯竄窩窺竇窶竪競篤巴筍筆筧牋篭築篳篩箏籌簽簡簀篋簫簣簍籃籬籟糴類糶糲鬻糞糧粋粫緊糾紆紅紂繊約級絋紀緯紜純紕紗綱納縦綸紛紙紋紡紵紖紐線紺紲練組紳細織終絆紹繹経絨結繞絰絎絵給絢絳絡絶絞統絹綉綏絛継績緒綾続綺緋綽縄維綿綬繃紬綯綣綜綻綰緑綴緇緘緬纜緲緝縕繢綞緞縋緩締縷編緡縁縉縛縟縫縗縞纏縊繽縹縵縲纓縮繆纈繚繕繰纉罌網羅罰罷羆羈羮翹翆聳恥聶聾職聹聯聡粛腸膚腎腫脹脅冑勝糊朧臚脛臙膠脈膾臓臍脳膿臠臉臘齶膩靦膃騰輿艤艦艙艱艷節蕪蘆葦萇蒼苧蘇蘋範茆蘢蔦塋煢繭荊薦茘莢蕘蕎薈薺蕩栄葷犖蕁蔭葤薬蒲苺蓮蒔萵蕕瑩鴬蓴莵萠蘿蛍営蕭薩蕚朦簑藍薊蕷驀薔藺藹蘂薀蘊籔薫蔵蘚乕虜慮蝨雖蝦蝕蟻蜆蠱蠣蟶蟄蟯蛻蝸蝿螻蠑螿螳蟇蠨蠎蠏惷蠧釁銜補襯袞襖襪襲襠褌袴褸繿見観規覓視覘覧覚覬覡覿覥覦覯覲觴誾謄計訂訃認譏訐訌討譲訖訓議訊記訒講諱謳訝訥許訛論訟諷設訪訣証詁呵評詛識詗詐訴診詆詞詔訳詒誄試詩詰詼誠誅話誕詬詮詭詢詣諍該詳詫諢譸誡誣語誚誤誥誘誨誑説誦誒請諸諏諾読誹課諛誰調諂諒諄談誼謀諜諌諧謔謁謂諤諭讒諳諺諦謎諞諝謨謖謝謡謗謚謙謐謹謾謫謬譚譖譜譎譴譫讖豶犲狢貝貞負貢財責賢敗貨質販貪貧貶購貯貫賤賁貰貼貴貸貿費賀貽賊贄賈賄賃賂贓資贐賑賚賦賭齎贖賞賜贔賙賠頼賵贅賻賺賽贋賛贇贈贍贏趙趨躍蹌蹕躋躊蹤躓躑躡蹣躪躱車軋軌軒転軛輪軟轟軻轤軸軼軫轢軽軾載輊轎輅較輒輔輛輦輩輝輟輜輳輻輯輸轡轅轄輾轆轍辮辺遼達遷過邁運還這進遠違連遅邇廸逕跡適選遜逓邏遺遙郵鄒隣欝鄭鄲酇醗醤醸酔酢釈釡鑑鑾鏨針釘釧鈒釣鍚釵鉅鈍鈔鐘鋼鈑鑰欽鈞鈎鈕銭鉦鉗鈷鉢鈳鉞鑽鈿鉄鈴鑠鉛鉚鉉鉈鐸鉶鋮鋏鐃鐺銅鎧銖銑銓銚銘錚銃銀鋳舗鏈鏗銷鎖鋤鍋銹鋒鋭錆錯錨錩錫錮鑼錘錐錦錠鍵鋸錙鍈鏘鍔鍬鍾鍛鍠鍍鏤鎮鑷鐫鎬鎰鎔鏝鏍鏡鏑鏃鏇鏐鐔钁鐐鐓鐙鑞長門閂閃閆閈閉問闖閏閑間閔悶閘閙閨聞闥閭閥閣鬮閲闍閾閹鬩閻閼闡闌闃闠闊闔闕闤隊穽陽陰陣階際陸隴陳陜険陥陏隠隷雋難鴈雛彫靂霧霽癨靄覇靨韃韋靭韓韜韮韻頁頂頃項順須頑顧頓頒頌頏預顱領頗頚頡頬潁熲頤頻頮頽頷穎顆題顎顔額顳顛顫顰顴風颺颯颶颻飄飆飈飛饗喰飠飣飢飥飩飫飭飯飲餞飾飽飼飴餌饒餉餃餅餖餓餒餕餡館餽餺餾饉饅饌馬馭駄馴馳駆馹駁驢駛駟駒駐駝駑駕駅駘驍罵駰驕駱駭駢驪騁験駸駿騏騎騅騌驌驂騙騒騫驃騾驟驥驤髏髄鬢魘魎魚魯魴鮃鮎鱸鮓鮒鮑鮭鮚鮪鮞鮦鰂鱠鮫鮮鰹鯉鯀鯊鯒鯖鯡鯤鯢鯰鯛鯨鰺鯔鰈鱨鰮鰓鰐鰍鰒鰉鰲鰭鰥鰜鰾鱈鼈鰻鱗鱒鱧鱣鳥鳩鶏鳶鳴鴎鴉鶬鴇鴆鴣鶇鴨鴦鴒鴟鴛鴕鷙鴿鸞鴻鵐鵑鵠鵝鵜鵡鵲鵾鵯鵬鶉鶊鵷鷫鶡鶚鶻鶖鶩鷂鶲鶺鶴鷖鸚鷓鷯鷦鷲鷸鷺鸇鷹鸛鹺麩黌黒黙黷黲黽鼡斉齏歯齔齗齟齢齠齦齬齪齲齷竜龕亀壈屩睍鴗絺纁襀覼訑諰蹻軏軨輗輮鈇鏦鐍餦餗饘駃騃騠鱮鮄鮰鳷鶒鶗".ToUTF32
        Private Shared J2G_J As Char32() = "拠芸畳缶嬢欠予衝復複幹個箇後獲穫機裏歴暦捨聖係帳徴隻誌製準弁弁弁闘芸舎鏟迭聖欒廩帳殞干千斗升昇沖並併后志里谷穀系採彩制征周週准淡彩覆覆予五五停百借借停像象像千止笑咲座娘岐岐希希座弁復徴扇扇漆欠止育沖淡漆百研研缶育蔔避逮迭逮避剗蔔坂廩据机欒殞征坂㩜両乕乗亀亜仏仭仮伝佇侖価侫侶倆倉們倣値倫倶倹偉側偵偸偽傑傘備傭傴債傷傾僂僅僉僑僕僣僥儀儂億儔儕償優儲儷儸儺儻儼兌兎児円冊冑冦冪凅凍凜処凱凾刔別刪剄則剛剣剤剰剴創劃劇劉劚労効勁動務勝勢勦勧勲匯匱匳協単厖厠厭厳収叡叢吶呂呉呑呪呵咼員唄唖問啓啗喎喚喞喩喪喫喬喰営嗇嗚嘆嘔嘖嘗嘩嘯噠噴噸嚀嚇嚌嚔嚠嚢嚶囀囁囂囈囓団囲図圏園圧執堅堝報場塁塊塋塒塗塢塩塵塹増墜墫墳墻墾壇壈壊壌壙壟壱売壷変夘夢夥夾奐奨奪奮妬姉姦姪姫娚娯婁婦婬媼媽嫗嫺嬋嬌嬪嬰嬾孫実宮寃寛寧審寵対専尋導尭屍屏屓層屩岡峩島崕崟崢嵐嵳嶄嶇嶢嶺嶼巒巣巴巵帥師帯帰幀幃幇幗幟幣幺幾庁広庫廂廃廈廐廚廝廟廠廡廬廸廼廾弉弔張強弾彌彎彫彬従徳徹応忰忻恥恵悋悛悞悩悪悵悶惷惻愛愨愴愾慂慄態慍慙慟慣慫慮慳慶憂憊憐憑憚憤憫憮憲憶懇懌懐懣懲懸懺懼懾戔戞戦戯戸戻払扱抃抜択拝拡挙挩捜捫捲掃掛掲掻揀揚換揮揺損搗搶摂摯摶撃撈撓撝撥撫撲撹撻擁擠擬擯擱擲擺擾攅攣攤攬敗敵斂斃斉斎斬時晄晉晩暁暈暉暘暢暫暸曄曇曖曠曵書曽朦朧朶東柆柵査柾栄桜桟桿梔梟梹棄棗棟棲棹検椶楊楓楕楡業極楽榿構槍槓様槧槨樅標権樹樺橈橋橦檜檣檸檻櫂櫃櫓櫚櫛櫞櫟櫨櫪欄欖欝欸欽歓歟歩歯歳歿殀殤殨殫殯殰殲殺殻毀毎氈気氷氾決沢況洟洶浜浹涙涜涼淨淪淵渇済渉渋渓渙減渦測渾湊湧湯満溌溝滄滅滌滝滬滲滷滸滾漁漑漢漣漬漲漸漿潁潅潔潤潯潰澆澑澗澱濁濃濘濤濫濬濺濾瀁瀇瀉瀋瀕瀘瀝瀟瀦瀬瀲瀾灑灘災為烏烱焔無焼煕煙煢煥煩煬熅熱熲熾燉燗燦燭燼爍爺爼爾牋牘牽犂犖犠犢犲狢狽猟猶猾獄獅獣獰獱獺現琺琿瑣瑤瑩瑪瑯瑲璦環璽瓊瓏瓔甌産畝畢異疇痙痩痾瘂瘋瘍瘡瘧瘻療癆癇癒癘癢癧癨癩癬癰癲発皐皚皰皸皺盞監盤盧県眥睍瞞瞼矚矯砕砲硯碩碪碯確碼磚磧磯磽礎礙礪礫礬祿禍禎禰禿稈種稲稽穂積穎穏穡穢穽窩窪窮窯窵窶窺竃竄竅竇竜竢竪競筆筍筧箏節範築篋篤篩篭篳簀簍簑簡簣簫簽簾籃籌籔籟籬粋粛粧粫糊糞糧糲糴糶糸糾紀紂約紅紆紋納紐純紕紖紗紙級紛紜紡紬紮細紲紳紵紹紺終組絆絋経絎結絛絞絡絢給絨絰統絳絵絶絹絺綉綏継続綜綞綣綬維綯綰綱網綴綸綺綻綽綾綿緇緊緋総緑緒緘線緝緞締緡編緩緬緯緲練縁縄縉縊縋縕縗縛縞縟縦縫縮縲縳縵縷縹績繃繆繊織繕繚繞繢繭繰繸繹繽繿纁纈纉纏纓纜罌罰罵罷羅羆羈義羮翆習翹聞聡聯聳聴聶職聹聾脅脈脛脳脹腎腫腸膃膚膠膩膾膿臉臍臓臘臙臚臠臨興舗艙艤艦艱艷芭芻苧苺茅茆茘茲荊荘莢莵華菷萇萠萵葉葤葦葷蒔蒲蒼蓋蓮蓴蔦蔭蔵蕁蕎蕕蕘蕚蕩蕪蕭蕷薀薈薊薑薔薦薩薫薬薺藍藹藺蘂蘆蘇蘊蘋蘚蘢蘭蘿虜虧蛍蛻蜆蝕蝟蝦蝨蝸蝿螮螳螻螿蟄蟇蟯蟶蟻蠎蠏蠑蠣蠧蠨蠱衆術衛袞袴補褌褸褻襀襖襠襪襯襲覇見規覓視覘覚覡覥覦覧親覬覯覲観覼覿觴訂訃計訊訌討訐訑訒訓訖託記訛訝訟訢訣訥訪設許訳訴診証詁詆詐詒詔評詗詛詞詠詢詣試詩詫詬詭詮詰話該詳詼誄誅誇認誑誒誕誘誚語誠誡誣誤誥誦誨説読誰課誹誼誾調諂諄談請諌諍諏諒論諛諜諝諞諠諢諤諦諧諭諮諰諱諳諷諸諺諾謀謁謂謄謎謐謔謖謗謙謚講謝謡謨謫謬謳謹謾譅譎譏譖識譚譜譟譫議譲譴護譸讌讐讒讖豈豊豬豶貙貝貞負財貢貧貨販貪貫責貯貰貴貶買貸費貼貽貿賀賁賂賃賄資賈賊賑賓賙賚賛賜賞賠賢賤賦質賭賰賵賺賻購賽贄贅贇贈贋贍贏贐贓贔贖趙趨跡蹌蹕蹣蹤蹻躊躋躍躑躓躡躪躱車軋軌軍軏軒軛軟転軨軫軸軻軼軽軾較輅載輊輒輓輔輗輛輜輝輟輦輩輪輮輯輳輸輻輾輿轂轄轅轆轍轎轟轡轢轤辮農辺逓逕這連進遅遊運過達違遙遜遠遡適遷選遺遼邁還邇邏郵郷鄒鄭鄲酇酔酢醗醜醤醸釁釈釘針釡釣釧釵鈇鈍鈎鈑鈒鈔鈕鈞鈳鈴鈷鈿鉄鉅鉈鉉鉗鉚鉛鉞鉢鉦鉱鉶銀銃銅銑銓銖銘銚銜銭銷銹鋏鋒鋤鋭鋮鋳鋸鋼錆錐錘錙錚錠錦錨錩錫錬錮錯録鍈鍋鍍鍔鍚鍛鍠鍬鍵鍾鎔鎖鎧鎬鎮鎰鏃鏇鏈鏍鏐鏑鏗鏘鏝鏡鏤鏦鏨鐃鐍鐐鐓鐔鐘鐙鐫鐸鐺鑑鑞鑠鑰鑷鑼鑽鑾鑿钁長門閂閃閆閈閉開閏閑間閔閘閙関閣閥閧閨閭閲閹閻閼閾闃闊闌闍闔闕闖闠闡闤闥阯陏陜陣陥陰陳陸険陽隊階際隠隣隴隷雋雑雖雛離難雲電霊霧霽靂靄靦靨靭鞏韃韋韓韜韮韻響頁頂頃項順須頌頏預頑頒頓頗領頚頡頤頬頭頮頷頻頼頽顆題額顎顔顕願顛類顧顫顰顱顳顴風颯颶颺颻飄飆飈飛飠飢飣飥飩飫飭飯飲飴飼飽飾餃餅餉養餌餒餓餕餖餗餞餡餦館餺餽餾饅饉饌饒饗饘馬馭馮馳馴馹駁駃駄駅駆駐駑駒駕駘駛駝駟駢駭駰駱駸駿騁騃騅騌騎騏騒験騙騠騧騫騰騾驀驂驃驌驍驕驚驟驢驤驥驪髄髏鬢鬩鬮鬻魎魘魚魯魴鮃鮄鮎鮑鮒鮓鮚鮞鮦鮪鮫鮭鮮鮰鯀鯉鯊鯒鯔鯖鯛鯡鯢鯤鯨鯰鰂鰈鰉鰍鰐鰒鰓鰜鰥鰭鰮鰲鰹鰺鰻鰾鱈鱒鱗鱠鱣鱧鱨鱮鱸鳥鳧鳩鳳鳴鳶鳷鴆鴇鴈鴉鴎鴒鴕鴗鴛鴟鴣鴦鴨鴬鴻鴿鵐鵑鵜鵝鵠鵡鵬鵯鵲鵷鵾鶄鶇鶉鶊鶏鶒鶖鶗鶚鶡鶩鶪鶬鶲鶴鶺鶻鷂鷓鷖鷙鷦鷫鷯鷲鷸鷹鷺鸂鸇鸚鸛鸞鹸鹹鹺麗麩黌黒黙黲黷黽鼈鼡齎齏齔齗齟齠齢齦齪齬齲齶齷龕".ToUTF32
        Private Shared J2G_G As Char32() = "据艺叠罐娘缺豫冲复复干个个后获获机里历历舍圣系帐征只志制准辨瓣辩斗芸舍刬叠垩滦禀账陨干千斗升升冲并并后志里谷谷系采采制征周周准淡彩覆复予五伍停百借藉仃像象象仟止笑笑座娘岐歧希稀坐弁覆徵扇煽漆欠只育盅澹柒佰研硎缶毓菔避逮迭迨辟刬卜坂廪据机栾殒徵阪㨫两虎乘龟亚佛仞假传伫仑价佞侣俩仓们仿值伦俱俭伟侧侦偷伪杰伞备佣伛债伤倾偻仅佥侨仆僭侥仪侬亿俦侪偿优储俪㑩傩傥俨兑兔儿圆册胄寇幂涸冻凛处凯函抉别删刭则刚剑剂剩剀创划剧刘㔉劳效劲动务胜势剿劝勋汇匮奁协单庞厕厌严收睿丛呐吕吴吞咒诃呙员呗哑问启啖㖞唤唧喻丧吃乔餐营啬呜叹呕啧尝哗啸哒喷吨咛吓哜嚏浏囊嘤啭嗫嚣呓啮团围图圈园压执坚埚报场垒块茔埘涂坞盐尘堑增坠樽坟墙垦坛𡒄坏壤圹垄壹卖壶变卯梦伙夹奂奖夺奋妒姊奸侄姬喃娱娄妇淫媪妈妪娴婵娇嫔婴懒孙实宫冤宽宁审宠对专寻导尧尸摒屃层𪨗冈峨岛崖嵚峥岚嵯崭岖峣岭屿峦巢笆卮帅师带归帧帏帮帼帜币么几厅广库厢废厦厩厨厮庙厂庑庐迪乃廿奘吊张强弹弥弯雕份从德彻应悴欣耻惠吝恂悮恼恶怅闷蠢恻爱悫怆忾恿栗态愠惭恸惯怂虑悭庆忧惫怜凭惮愤悯怃宪忆恳怿怀懑惩悬忏惧慑戋戛战戏户戾拂插拚拔择拜扩举捝搜扪卷扫挂揭搔拣扬换挥摇损捣抢摄挚抟击捞挠㧑拨抚扑搅挞拥挤拟摈搁掷摆扰攒挛摊揽败敌敛毙齐斋斩时晃晋晚晓晕晖旸畅暂瞭晔昙暧旷曳书曾蒙胧朵东拉栅查柩荣樱栈杆栀枭槟弃枣栋栖桌检棕杨枫椭榆业极乐桤构枪杠样椠椁枞标权树桦桡桥幢桧樯柠槛棹柜橹榈栉橼栎栌枥栏榄郁唉钦欢欤步齿岁殁夭殇㱮殚殡㱩歼杀壳毁每毡气冰泛决泽况涕汹滨浃泪渎凉净沦渊渴济涉涩溪涣减涡测浑凑涌汤满泼沟沧灭涤泷沪渗卤浒滚渔溉汉涟渍涨渐浆颍灌洁润浔溃浇溜涧淀浊浓泞涛滥浚溅滤漾㲿泻渖濒泸沥潇潴濑潋澜洒滩灾为乌炯焰无烧熙烟茕焕烦炀煴热颎炽炖烂灿烛烬烁爷俎尔笺牍牵犁荦牺犊豺貉狈猎犹狯狱狮兽狞㺍獭现珐珲琐瑶莹玛琅玱瑷环玺琼珑璎瓯产亩毕异畴痉瘦疴痖疯疡疮疟瘘疗痨痫愈疠痒疬霍癞癣痈癫发皋皑疱皲皱盏监盘卢县眦𪾢瞒睑瞩矫碎炮砚硕砧瑙确码砖碛矶硗础碍砺砾矾禄祸祯祢秃秆种稻乩穗积颖稳穑秽阱窝洼穷窑窎窭窥灶窜窍窦龙俟竖竞笔笋笕筝节范筑箧笃筛笼筚箦篓蓑简篑箫签帘篮筹薮籁篱粹肃妆糯胡粪粮粝籴粜丝纠纪纣约红纡纹纳纽纯纰纼纱纸级纷纭纺绸扎细绁绅纻绍绀终组绊纩经绗结绦绞络绚给绒绖统绛绘绝绢𫄨绣绥继续综缍绻绶维绹绾纲网缀纶绮绽绰绫绵缁紧绯总绿绪缄线缉缎缔缗编缓缅纬缈练缘绳缙缢缒缊缞缚缟缛纵缝缩缧䌸缦缕缥绩绷缪纤织缮缭绕缋茧缲䍁绎缤褴𫄸缬缵缠缨缆罂罚骂罢罗罴羁义羹翠习翘闻聪联耸听聂职聍聋胁脉胫脑胀肾肿肠腽肤胶腻脍脓脸脐脏腊胭胪脔临兴铺舱舣舰艰艳巴刍苎莓泖茅荔兹荆庄荚菟华帚苌萌莴叶荮苇荤莳莆苍盖莲莼茑荫藏荨荞莸荛萼荡芜萧蓣蕰荟蓟姜蔷荐萨薰药荠蓝蔼蔺蕊芦苏蕴苹藓茏兰萝虏亏萤蜕蚬蚀猬虾虱蜗蝇䗖螗蝼螀蛰蟆蛲蛏蚁蟒蟹蝾蛎蠹蟏蛊众术卫衮裤补裈褛亵𫌀袄裆袜衬袭霸见规觅视觇觉觋觍觎览亲觊觏觐观𫌨觌觞订讣计讯讧讨讦𫍙讱训讫托记讹讶讼䜣诀讷访设许译诉诊证诂诋诈诒诏评诇诅词咏询诣试诗诧诟诡诠诘话该详诙诔诛夸认诳诶诞诱诮语诚诫诬误诰诵诲说读谁课诽谊訚调谄谆谈请谏诤诹谅论谀谍谞谝喧诨谔谛谐谕咨𫍰讳谙讽诸谚诺谋谒谓誊谜谧谑谡谤谦谥讲谢谣谟谪谬讴谨谩䜧谲讥谮识谭谱噪谵议让谴护诪燕仇谗谶岂丰猪豮䝙贝贞负财贡贫货贩贪贯责贮贳贵贬买贷费贴贻贸贺贲赂赁贿资贾贼赈宾赒赉赞赐赏赔贤贱赋质赌䞐赗赚赙购赛贽赘赟赠赝赡赢赆赃赑赎赵趋迹跄跸蹒踪𫏋踌跻跃踯踬蹑躏躲车轧轨军𫐄轩轭软转𫐉轸轴轲轶轻轼较辂载轾辄挽辅𫐐辆辎辉辍辇辈轮𫐓辑辏输辐辗舆毂辖辕辘辙轿轰辔轹轳辫农边递迳这连进迟游运过达违遥逊远溯适迁选遗辽迈还迩逻邮乡邹郑郸酂醉醋酦丑酱酿衅释钉针釜钓钏钗𫓧钝钩钣钑钞钮钧钶铃钴钿铁钜铊铉钳铆铅钺钵钲矿铏银铳铜铣铨铢铭铫衔钱销锈铗锋锄锐铖铸锯钢锖锥锤锱铮锭锦锚锠锡炼锢错录锳锅镀锷钖锻锽锹键锺镕锁铠镐镇镒镞镟链镙镠镝铿锵镘镜镂𫓩錾铙𫔎镣镦镡钟镫镌铎铛鉴镴铄钥镊锣钻銮凿镢长门闩闪闫闬闭开闰闲间闵闸闹关阁阀哄闺闾阅阉阎阏阈阒阔阑阇阖阙闯阓阐阛闼址隋陕阵陷阴陈陆险阳队阶际隐邻陇隶隽杂虽雏离难云电灵雾霁雳霭腼靥韧巩鞑韦韩韬韭韵响页顶顷项顺须颂颃预顽颁顿颇领颈颉颐颊头颒颔频赖颓颗题额颚颜显愿颠类顾颤颦颅颞颧风飒飓飏飖飘飙飚飞饣饥饤饦饨饫饬饭饮饴饲饱饰饺饼饷养饵馁饿馂饾𫗧饯馅𫗠馆馎馈馏馒馑馔饶飨𫗴马驭冯驰驯驲驳𫘝驮驿驱驻驽驹驾骀驶驼驷骈骇骃骆骎骏骋𫘤骓骔骑骐骚验骗𫘨䯄骞腾骡蓦骖骠骕骁骄惊骤驴骧骥骊髓髅鬓阋阄粥魉魇鱼鲁鲂鲆𫚒鲇鲍鲋鲊鲒鲕鲖鲔鲛鲑鲜𫚔鲧鲤鲨鲬鲻鲭鲷鲱鲵鲲鲸鲶鲗鲽鳇鳅鳄鳆鳃鳒鳏鳍鳁鳌鲣鲹鳗鳔鳕鳟鳞鲙鳣鳢鲿𫚈鲈鸟凫鸠凤鸣鸢𫛛鸩鸨雁鸦鸥鸰鸵𫁡鸳鸱鸪鸯鸭莺鸿鸽鹀鹃鹈鹅鹄鹉鹏鹎鹊鹓鹍䴖鸫鹑鹒鸡𫛶鹙𫛸鹗鹖鹜䴗鸧鹟鹤鹡鹘鹞鹧鹥鸷鹪鹔鹩鹫鹬鹰鹭㶉鹯鹦鹳鸾碱咸鹾丽麸黉黑默黪黩黾鳖鼠赍齑龀龂龃龆龄龈龊龉龋腭龌龛".ToUTF32
    End Class
End Namespace