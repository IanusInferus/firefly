'==========================================================================
'
'  File:        HanziVariantTableGen.vb
'  Location:    Firefly.Texting <Visual Basic .Net>
'  Description: 简繁日汉字异体对应表生成器
'  Version:     2010.10.11.
'  Copyright(C) F.R.C.
'
'==========================================================================

Imports System
Imports System.Math
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports Firefly
Imports Firefly.TextEncoding
Imports Firefly.Texting
Imports Firefly.Texting.UniHanDatabase

Namespace Texting
    ''' <summary>
    ''' 简繁日汉字异体对应表生成器
    ''' </summary>
    ''' <remarks>
    ''' 本类按照UniHan数据库生成简繁日汉字异体对应表。
    ''' ftp://ftp.unicode.org/Public/UNIDATA/Unihan.zip
    ''' http://www.unicode.org/reports/tr38/
    ''' 
    ''' 所有涉及的汉字仅在以下三级常用汉字中选取
    ''' U+4E00..U+9FA5   : CJK Unified Ideographs
    ''' U+9FA6..U+9FBB   : CJK Unified Ideographs (4.1)
    ''' U+9FBC..U+9FC3   : CJK Unified Ideographs (5.1)
    ''' 
    ''' 简体字规范(GN):
    ''' 现代汉语通用字表
    ''' http://www.china-language.gov.cn/wenziguifan/shanghi/014.htm
    ''' 修改了12个错字，《现代汉语通用字表》的错字
    ''' http://www.pkucn.com/viewthread.php?tid=226484&amp;extra=page%3D1
    ''' 
    ''' 繁体字规范(TN)：
    ''' 常用国字标准字体表 次常用国字标准字体表(暂缺)
    ''' 代替方案为以下字集取并集
    ''' CNS 11643-1992第一字面字
    ''' 台湾国小字表
    ''' http://hanzi.unihan.com.cn/Coolhanzi/#down_paper
    ''' 
    ''' 日体字规范(JN)：
    ''' 日本常用漢字表
    ''' http://bjkoro.net/chinese/joyokanji.html
    ''' 字数明显不足，使用J0中所有有字源异体关系的字在J0中编号最小的。
    ''' 
    ''' 简体字源(G):
    ''' G0 GB2312-80
    ''' G1 GB12345-90 with 58 Hong Kong and 92 Korean "Idu" characters
    ''' G7 General Purpose Hanzi List for Modern Chinese Language, and General List of Simplified Hanzi
    ''' 
    ''' 繁体字源(T):
    ''' T1 CNS 11643-1992, plane 1
    ''' T2 CNS 11643-1992, plane 2
    ''' 
    ''' 日体字源(J):
    ''' J0 JIS X 0208:1990
    ''' J1 JIS X 0212:1990
    ''' 
    ''' 为了校正一些UniHan中未包含的繁简多一对应和多一对应别字，添加了下表的资料。
    ''' 簡繁轉換別字表
    ''' http://input.foruto.com/ccc/data/proof/02.htm
    ''' 但是，从中去除了不用的二简字(少量仍通用的未去除，如挨捱)、G源异体字(如昆仑崑崙、家私傢俬、村邨、借藉)，并对排序进行了调整。
    ''' 
    ''' 
    ''' 简日转换表优先使用中日简化字对照表(费锦昌)数据
    ''' http://www.pkucn.com/viewthread.php?tid=166825&amp;page=1&amp;authorid=70246
    ''' 
    ''' 
    ''' 落入到三个规范的字认为是该字源的规范字。
    ''' 落入到三个字源的字认为是该字源的常用字。
    ''' 
    ''' 两个字g、t(可能是一个字)，若g(-G，t(-T，g(-t.kSimplifiedVariant，t(-g.kTraditionalVariant，则两者是繁简异体字。
    ''' 两个字j、t(可能是一个字)，若j(-J，t(-T，t(-j.kZVariant，则两者是繁日异体字。
    ''' 两个字g、j(可能是一个字)，若存在t(-T，(g、t是一一对应繁简异体字或同一个字，j、t是一一对应繁日异体字或同一个字)或(g、t是一一对应繁简异体字或同一个字，j、t是一多对应繁日异体字或同一个字)或(g、t是一多对应繁简异体字或同一个字，j、t是一一对应繁日异体字或同一个字)，或g(-G，j(-J，g(-j.kSimplifiedVariant，j(-g.kTraditionalVariant，或g(-j.kTraditionalVariant，j(-g.kSimplifiedVariant，则称两者是简日异体字。
    ''' 三种关系统称字源异体字。
    ''' 
    ''' 字源异体字中，只做规范字->规范字、非规范字->非规范字的转换，不做规范字->非规范字、非规范字->规范字的转换。
    ''' 且对于非规范字，需要参考在其他字源中，是否一个是规范字，一个不是规范字，如果是，则也不转换。
    ''' 
    ''' 在此规则下，一些字由于不在标准中出现，转换会出现问题，比如日本常用字表中无“丑”字(子丑寅卯)，仅有“醜”字，则在GJ、JG转换中，不会像GT、TG中那样出现丑丑转换，而仅有丑醜转换。
    ''' 如果发生此类错误，则需要在生成的转换表中加入此类字。不过这同样意味着，在一一对应转换时，丑醜转换不会进行。
    ''' 
    ''' </remarks>
    Public Class HanziVariantTableGen
        Private GNorm As New HashSet(Of Char32)
        Private TNorm As New HashSet(Of Char32)
        Private JNorm As New HashSet(Of Char32)

        Private AllChars As New Dictionary(Of Char32, UniHanChar)
        Private GChars As New Dictionary(Of Char32, UniHanChar)
        Private TChars As New Dictionary(Of Char32, UniHanChar)
        Private JChars As New Dictionary(Of Char32, UniHanChar)

        Private GTTable As New List(Of GTPair)
        Private JTTable As New List(Of JTPair)
        Private GJTable As New List(Of GJPair)

        Public Structure GTPair
            Public G As Char32
            Public T As Char32
        End Structure

        Public Structure JTPair
            Public J As Char32
            Public T As Char32
        End Structure

        Public Structure GJPair
            Public G As Char32
            Public J As Char32
        End Structure

        Private Sub LoadNorms(ByVal Path As String)
            Dim uhdb As New UniHanDatabase
            uhdb.Load(Path, AddressOf IsToLoad)
            Dim Chars = uhdb.GetChars
            AllChars = Chars.ToDictionary(Function(c) c.Unicode, Function(c) c)
            GChars = (From c In Chars Where c.HasField("kIRG_GSource")).ToDictionary(Function(c) c.Unicode, Function(c) c)
            TChars = (From c In Chars Where c.HasField("kIRG_TSource")).ToDictionary(Function(c) c.Unicode, Function(c) c)
            JChars = (From c In Chars Where c.HasField("kIRG_JSource")).ToDictionary(Function(c) c.Unicode, Function(c) c)

            GNorm = New HashSet(Of Char32)("一乙二十丁厂七卜八人入乂儿九匕几刁了乃刀力又乜三干亍于亏士土工才下寸丈大兀与万弋上小口山巾千乞川亿彳个么久勺丸夕凡及广亡门丫义之尸已巳弓己卫孑子孓也女飞刃习叉马乡幺丰王井开亓夫天元无韦云专丐扎廿艺木五支厅卅不仄太犬区历友歹尤匹厄车巨牙屯戈比互切瓦止少曰日中贝内水冈见手午牛毛气壬升夭长仁仃什片仆仉化仇币仂仍仅斤爪反兮刈介父爻从仑今凶分乏公仓月氏勿风欠丹匀乌勾殳凤卞六文亢方闩火为斗忆计订户讣认讥冗心尹尺夬引丑爿巴孔队办以允邓予劝双书毋幻玉刊末未示击邗戋打巧正扑卉扒邛功扔去甘世艾艽古节艿本术札可叵匝丙左厉丕石右布夯龙戊平灭轧东匜劢卡北占凸卢业旧帅归目旦且叮叶甲申号电田由卟叭只央史叱叽兄叼叩叫叻叨另叹冉皿凹囚四生失矢氕乍禾仨仕丘付仗代仙仟仡仫伋们仪白仔他仞斥卮瓜乎丛令用甩印氐乐尔句匆犰册卯犯外处冬鸟务刍包饥主市庀邝立冯邙玄闪兰半汀汁汇头汈汉忉宁穴宄它讦讧讨写让礼讪讫训必议讯记永司尻尼民弗弘阢出阡辽奶奴尕加召皮边孕发圣对弁台矛纠驭母幼丝匡耒邦玎玑式迂刑邢戎动圩圬圭扛寺吉扣扦圪考托圳老圾巩执扩圹扪扫圯圮地扬场耳芋芏共芊芍芨芄芒亚芝芎芑芗朽朴机权过亘臣吏再协西压厌厍戌在百有存而页匠夸夺夼灰达戍尥列死成夹夷轨邪尧划迈毕至此乩贞师尘尖劣光当吁早吐吓旯曳虫曲团同吕吊吃因吸吗吆屿屹岌帆岁回岂屺则刚网肉凼囝囡钆钇年朱缶氘氖牝先丢廷舌竹迁乔迄伟传乒乓休伍伎伏伛优臼伢伐仳延佤仲仵件任伤伥价伦份伧华仰伉仿伙伪伫自伊血向囟似后行甪舟全会杀合兆企汆氽众爷伞创刖肌肋朵杂夙危旬旭旮旨负犴刎犷匈犸舛各名多凫争邬色饧冱壮冲妆冰庄庆亦刘齐交次衣产决亥邡充妄闭问闯羊并关米灯州汗污江汕汔汲汐汛汜池汝汤汊忖忏忙兴宇守宅字安讲讳讴军讵讶祁讷许讹论讼农讽设访诀聿寻那艮厾迅尽导异弛阱阮孙阵阳收阪阶阴防丞奸如妁妇妃好她妈戏羽观牟欢买纡红纣驮纤纥驯纨约级纩纪驰纫巡寿玕弄玙麦玖玚玛形进戒吞远违韧运扶抚坛抟技坏抔抠坜扰扼拒找批扯址走抄汞坝贡攻赤圻折抓扳坂抡扮抢扺孝坎坍均坞抑抛投抃坟坑抗坊抖护壳志扭块抉声把报拟抒却劫毐芙芫芜苇邯芸芾芰苈苊苣芽芷芮苋芼苌花芹芥苁芩芬苍芪芴芡芟苄芳严苎芦芯劳克芭苏苡杆杜杠材村杖杌杏杉巫杓极杧杞李杨杈求忑孛甫匣更束吾豆两邴酉丽医辰励邳否还矶奁豕尬歼来忒连欤轩轪轫迓邶忐芈步卤卣邺坚肖旰旱盯呈时吴呋助县里呓呆吱吠呔呕园呖呃旷围呀吨旸吡町足虬邮男困吵串呙呐呗员听吟吩呛吻吹呜吭吣吲吼邑吧囤别吮岍帏岐岖岈岗岘帐岑岚兕财囵囫钉针钊钋钌迕氙氚牡告我乱利秃秀私岙每佞兵邱估体何佐伾佑攸但伸佃佚作伯伶佣低你佝佟住位伴佗身皂伺佛伽囱近彻役彷返佘余希佥坐谷孚妥豸含邻坌岔肝肟肛肚肘肠邸龟甸奂免劬狂犹狈狄角删狃狁鸠条彤卵灸岛邹刨饨迎饩饪饫饬饭饮系言冻状亩况亨庑床庋库庇疔疖疗吝应冷这庐序辛肓弃冶忘闰闱闲闳间闵闶闷羌判兑灶灿灼炀弟沣汪沅沄沐沛沔汰沤沥沌沘沏沚沙汩汨汭汽沃沂沦汹汾泛沧沨沟没汴汶沆沩沪沈沉沁泐怃忮怀怄忧忡忤忾怅忻忪怆忭忱快忸完宋宏牢究穷灾良证诂诃启评补初社祀祃诅识诈诉罕诊诋诌词诎诏诐译诒君灵即层屁屃尿尾迟局改张忌际陆阿孜陇陈阽阻阼附坠陀陂陉妍妩妓妪妣妙妊妖妗姊妨妫妒妞姒妤努邵劭忍刭劲甬邰矣鸡纬纭驱纯纰纱纲纳纴纵驳纶纷纸纹纺纻驴纽纾奉玩玮环玡武青责现玫玠玢玥表玦甙盂忝规匦抹卦邽坩坷坯拓垅拢拔抨坪拣拤拈坫垆坦担坤押抻抽拐拃拖拊者拍顶坼拆拎拥抵坻拘势抱拄垃拉拦幸拌拧坨坭抿拂拙招坡披拨择拚抬拇坳拗耵其耶取茉苷苦苯昔苛苤若茂茏苹苫苴苜苗英苒苘茌苻苓茚苟茆茑苑苞范茓茔茕直苠茀茁茄苕茎苔茅枉林枝杯枢枥柜枇杪杳枘枧杵枚枨析板枞松枪枫构杭枋杰述枕杻杷杼丧或画卧事刺枣雨卖矸郁矻矾矽矿砀码厕奈刳奔奇奄奋态瓯欧殴垄殁郏妻轰顷转轭斩轮软到郅鸢非叔歧肯齿些卓虎虏肾贤尚盱旺具昊昙果味杲昃昆国哎咕昌呵咂畅呸昕明易咙昀昂旻昉炅咔畀虮迪典固忠咀呷呻黾咒咋咐呱呼呤咚鸣咆咛咏呢咄呶咖呦咝岵岢岸岩帖罗岿岬岫帜帙帕岭岣峁刿峂迥岷剀凯帔峄沓败账贩贬购贮囹图罔钍钎钏钐钓钒钔钕钗邾制知迭氛迮垂牦牧物乖刮秆和季委竺秉迤佳侍佶岳佬佴供使侑佰侉例侠臾侥版侄岱侦侣侗侃侧侏凭侨侩佻佾佩货侈侪佼依佯侬帛卑的迫阜侔质欣郈征徂往爬彼径所舍金刽郐刹命肴郄斧怂爸采籴觅受乳贪念贫忿瓮戗肼肤朊肺肢肽肱肫肿肭胀朋肷股肮肪肥服胁周剁昏迩郇鱼兔狉狙狎狐忽狝狗狍狞狒咎备炙枭饯饰饱饲饳饴冽变京享冼庞店夜庙府底庖疟疠疝疙疚疡剂卒郊兖庚废净妾盲放於刻劾育氓闸闹郑券卷单炜炬炖炒炝炊炕炎炉炔沫浅法泔泄沽沭河泷沾泸沮泪油泱泅泗泊泠泜泺泃沿泖泡注泣泫泮泞沱泻泌泳泥泯沸泓沼波泼泽泾治怔怯怙怵怖怦怛怏性怍怕怜怩怫怊怿怪怡学宝宗定宕宠宜审宙官空帘穸穹宛实宓诓诔试郎诖诗诘戾肩房诙戽诚郓衬衫衩祆祎祉视祈诛诜话诞诟诠诡询诣诤该详诧诨诩建肃隶录帚屉居届刷鸤屈弧弥弦承孟陋戕陌孤孢陕亟降函陔限卺妹姑姐妲妯姓姗妮始帑弩孥驽姆虱迦迢驾叁参迨艰线绀绁绂练驵组绅细驶织驷驸驹终绉驺驻绊驼绋绌绍驿绎经骀贯甾砉耔契贰奏春帮珏珐珂珑玷玳珀顸珍玲珊珉珈玻毒型韨拭挂封持拮拷拱垭挝垣项垮挎垯挞城挟挠垤政赴赵赳贲垱挡拽垌哉垲挺括挢埏郝垍垧垢拴拾挑垛指垫挣挤垓垟拼垞挖按挥挦挪垠拯拶某甚荆茸革茜茬荐荙巷荚荑贳荛荜茈带草茧茼莒茵茴茱莛荞茯荏荇荃荟茶荀茗荠茭茨荒垩茳茫荡荣荤荥荦荧荨茛故荩胡荪荫茹荔南荬荭药柰标栈柑枯栉柯柄柘栊柩枰栋栌相查柙枵柚枳柞柏柝栀柃柢栎枸栅柳柱柿栏柈柠柁枷柽树勃剌郚剅要酊郦柬咸威歪甭研砖厘砗厚砑砘砒砌砂泵砚斫砭砜砍面耐耍奎耷牵鸥虺残殂殃殇殄殆轱轲轳轴轵轶轷轸轹轺轻鸦虿皆毖韭背战觇点虐临览竖尜省削尝哐昧眄眍盹是郢眇眊盼眨昽眈哇咭哄哑显冒映禺哂星昨咴曷昴咧昱昵咦哓昭哔畎畏毗趴呲胄胃贵畋畈界虹虾虼虻蚁思蚂盅咣虽品咽骂哕剐郧勋咻哗囿咱咿响哌哙哈哚咯哆咬咳咩咪咤哝哪哏哞哟峙炭峡峣罘帧罚峒峤峋峥峧帡贱贴贶贻骨幽钘钙钚钛钝钞钟钡钠钢钣钤钥钦钧钨钩钪钫钬钭钮钯卸缸拜看矩矧毡氡氟氢牯怎郜牲选适秕秒香种秭秋科重复竽竿笈笃俦段俨俅便俩俪叟垡贷牮顺修俏俣俚保俜促俄俐侮俭俗俘信皇泉皈鬼侵禹侯追俑俟俊盾逅待徊徇徉衍律很须舢舣叙俞弇郗剑逃俎郤爰郛食瓴盆胚胧胨胩胪胆胛胂胜胙胍胗胝朐胞胖脉胫胎鸨匍勉狨狭狮独狯狰狡飐飑狩狱狠狲訇訄逄昝贸怨急饵饶蚀饷饸饹饺饻胤饼峦弯孪娈将奖哀亭亮庤度弈奕迹庭庥疬疣疥疭疮疯疫疢疤庠咨姿亲竑音彦飒帝施闺闻闼闽闾闿阀阁阂差养美羑姜迸叛送类籼迷籽娄前酋首逆兹总炳炻炼炟炽炯炸烀烁炮炷炫烂烃剃洼洁洱洪洹洒洧洌浃柒浇泚浈浉浊洞洇洄测洙洗活洑涎洎洫派浍洽洮染洵洚洺洛浏济洨浐洋洴洣洲浑浒浓津浔浕洳恸恃恒恹恢恍恫恺恻恬恤恰恂恪恼恽恨举觉宣宦宥宬室宫宪突穿窀窃客诫冠诬语扁扃袆衲衽袄衿袂祛祜祓祖神祝祚诮祗祢祠误诰诱诲诳鸩说昶诵郡垦退既屋昼咫屏屎弭费陡逊牁眉胥孩陛陟陧陨除险院娃姞姥娅姨娆姻姝娇姚姽姣姘姹娜怒架贺盈怼羿勇炱怠癸蚤柔矜垒绑绒结绔骁绕骄骅绗绘给绚彖绛络骆绝绞骇统骈耕耘耖耗耙艳挈恝泰秦珥珙顼珰珠珽珩珧珣珞琤班珲敖素匿蚕顽盏匪恚捞栽捕埔埂捂振载赶起盐捎捍埕捏埘埋捉捆捐埚埙损袁挹捌都哲逝耆耄捡挫捋埒换挽贽挚热恐捣垸壶捃捅盍埃挨耻耿耽聂莰茝荸莆恭莽莱莲莳莫莴莪莉莠莓荷莜莅荼莶莩荽获莸荻莘晋恶莎莞莹莨莺真莙鸪莼框梆桂桔栲栳郴桓栖桡桎桢桄档桐桤株梃栝桥桕桦桁栓桧桃桅栒格桩校核样栟桉根栩逑索逋彧哥速鬲豇逗栗贾酐酎酌配酏逦翅辱唇厝孬夏砝砹砸砺砰砧砷砟砼砥砾砣础破硁恧原套剞逐砻烈殊殉顾轼轾轿辀辁辂较鸫顿趸毙致剕龀柴桌鸬虔虑监紧逍党眬唛逞晒晟眩眠晓眙唝哧哳哮唠鸭晃哺哽唔晔晌晁剔晏晖晕鸮趵趿畛蚌蚨蚜蚍蚋蚬畔蚝蚧蚣蚊蚪蚓哨唢哩圃哭圄哦唣唏恩盎唑鸯唤唁哼唧啊唉唆帱崂崃罡罢罟峭峨峪峰圆觊峻贼贿赂赃赅赆钰钱钲钳钴钵钷钹钺钻钼钽钾钿铀铁铂铃铄铅铆铈铉铊铋铌铍铎眚缺氩氤氦氧氨毪特牺造乘敌舐秣秫秤租秧积盉秩称秘透笄笕笔笑笊笫笏笋笆俸倩债俵倻借偌值倚俺倾倒俳俶倬倏倘俱倡候赁恁倭倪俾倜隼隽倞俯倍倦倓倌倥臬健臭射皋躬息郫倨倔衄颀徒徕徐殷舰舨舱般航舫瓞途拿釜耸爹舀爱豺豹奚鬯衾鸰颁颂翁胯胰胱胴胭脍脎脆脂胸胳脏脐胶脑胲胼朕脒胺脓鸱玺鱽鸲逛狴狸狷猁狳猃狺逖狼卿狻逢桀鸵留袅眢鸳皱饽饿馀馁凌凇凄栾挛恋桨浆衰勍衷高亳郭席准座脊症疳疴病疽疸疾痄斋疹痈疼疱疰痃痂疲痉脊效离衮紊唐凋颃瓷资恣凉站剖竞部旁旆旄旅旃畜阃阄阅阆羞羔恙瓶桊拳敉粉料粑益兼朔郸烤烘烜烦烧烛烟烨烩烙烊剡郯烬递涛浙涝浡浦涑浯酒涞涟涉娑消涅涠浞涓涢涡浥涔浩海浜涂浠浴浮涣浼涤流润涧涕浣浪浸涨烫涩涌涘浚悖悚悟悭悄悍悝悃悒悔悯悦悌悢悛害宽宸家宵宴宾窍窅窄容窈剜宰案请朗诸诹诺读扅诼冢扇诽袜袪袒袖袗袍袢被袯祯祧祥课冥诿谀谁谂调冤谄谅谆谇谈谊剥恳展剧屑屐屙弱陵陬勐奘疍牂蚩祟陲陴陶陷陪烝姬娠娱娌娉娟娲恕娥娩娴娣娘娓婀砮哿畚通能难逡预桑剟绠骊绡骋绢绣验绤绥绦骍继绨骎骏邕烝鸶彗耜焘舂琎球琏琐理琇麸琉琅捧掭堵揶措描埴域捺掎埼掩埯捷捯排焉掉掳掴埸堌捶赦赧推堆捭埠晢掀逵授捻埝堋教堍掏掐掬鸷掠掂掖培掊接堉掷掸控捩掮探悫埭埽据掘掺掇掼职聃基聆勘聊聍娶菁菝著菱萁菥菘堇勒黄萘萋勚菲菽菖萌萜萝菌萎萸萑菂菜棻菔菟萄萏菊萃菩菼菏萍菹菠菪菅菀萤营萦乾萧菰菡萨菇械梽彬梵梦婪梗梧梾梢梏梅觋检桴桷梓梳棁梯桫棂桶梭救啬郾匮曹敕副豉票鄄酝酞酗酚厢厣戚戛硎硅硭硒硕硖硗硐硚硇硌鸸瓠匏奢盔爽厩聋龚袭殒殓殍盛赉匾雩雪辄辅辆堑龁颅虚彪雀堂常眶眭唪眦啧匙晡晤晨眺眵睁眯眼眸悬野圊啪啦喏喵啉勖曼晦晞晗晚冕啄啭啡畦趼趺距趾啃跃啮跄略蚶蛄蛎蛆蚰蚺蛊圉蚱蚯蛉蛀蛇蛏蚴唬累鄂唱患啰唾唯啤啥啁啕唿啐唼唷啴啖啵啶啷唳啸啜帻崖崎崦崭逻帼崮崔帷崟崤崩崞崇崆崛赇赈婴赊圈铐铑铒铕铗铘铙铚铛铜铝铞铟铠铡铢铣铤铥铧铨铩铪铫铭铬铮铯铰铱铲铳铴铵银铷矫氪牾甜鸹秸梨犁稆秽移秾逶笺筇笨笸笼笪笛笙笮符笱笠笥第笳笤笾笞敏偾做鸺偃偕袋悠偿偶偈偎偲傀偷您偬售停偻偏躯皑兜皎假衅鸻徘徙徜得衔舸舻舳盘舴舶船鸼舷舵斜龛盒鸽瓻敛悉欲彩领翎脚脖脯豚脶脸脞脬脱脘脲朘匐鱾象够逸猜猪猎猫猗凰猖猡猊猞猄猝斛觖猕猛馗祭馃馄馅馆凑减鸾毫孰烹庶庹麻庵庼庾庳痔痍疵痊痒痕廊康庸鹿盗章竟翊商旌族旎旋望袤率阇阈阉阊阋阌阍阎阏阐着羚羝羟盖眷粝粘粗粕粒断剪兽焐焊烯焓焕烽焖烷烺焌清渍添渚鸿淇淋淅淞渎涯淹涿渠渐淑淖挲淌淏混淠涸渑淮淦淆渊淫淝渔淘淳液淬涪淤淡淙淀涫深渌涮涵婆梁渗淄情惬悻惜惭悱悼惝惧惕惘悸惟惆惚惊惇惦悴惮惋惨惯寇寅寄寂逭宿窒窑窕密谋谌谍谎谏扈皲谐谑裆袱袼裈裉祷祸祲谒谓谔谕谖谗谙谚谛谜谝敝逮逯敢尉屠艴弹隋堕郿随蛋隅隈粜隍隗隆隐婧婊婞婳婕娼婢婚婵婶婉胬袈颇颈翌恿欸绩绪绫骐续骑绮绯绰骒绲绳骓维绵绶绷绸绹绺绻综绽绾绿骖缀缁巢耠琫琵琴琶琪瑛琳琦琢琥琨靓琼斑琰琮琯琬琛琚辇替鼋揳揍款堪堞搽塔搭塃揸堰揠堙揩越趄趁趋超揽提堤揖博揾颉揭喜彭揣塄揿插揪搜煮堠耋揄援搀蛰蛩絷塆裁揞搁搓搂搅揎壹握摒揆搔揉掾葜聒斯期欺联葑葚葫靰靸散葳惹蒇葬蒈募葺葛蒉葸萼蓇萩董葆葩葡敬葱蒋葶蒂蒌葓蒎落萱葖韩戟朝葭辜葵棒楮棱棋椰植森棼焚椟椅椒棹棵棍椤棰椎棉椑鹀赍棚椋椁棬棕棺榔楗棣椐椭鹁惠惑逼覃粟棘酣酤酢酥酡酦鹂觌厨厦硬硝硪硷确硫雁厥殖裂雄殚殛颊雳雯辊辋椠暂辌辍辎雅翘辈斐悲紫凿黹辉敞棠牚赏掌晴睐暑最晰量睑睇鼎睃喷戢喋嗒喃喳晶喇遇喊喱喹遏晷晾景喈畴践跖跋跌跗跞跚跑跎跏跛跆遗蛙蛱蛲蛭蛳蛐蛔蛛蜓蛞蜒蛤蛴蛟蛘蛑畯喁喝鹃喂喟斝喘啾嗖喤喉喻喑啼嗟喽嗞喧喀喔喙嵌嵘嵖幅崴遄詈帽嵎崽嵚嵬嵛翙嵯嵝嵫幄嵋赋赌赎赐赑赔黑铸铹铺铻铼铽链铿销锁锃锄锂锅锆锇锈锉锊锋锌锎锏锐锑锒锓锔锕甥掣掰短智矬氰毳毯氮毽氯犊犄犋鹄犍鹅颋剩嵇稍程稀黍稃税稂筐等筘筑策筚筛筜筒筅筏筵筌答筋筝傣傲傅傈舄牍牌傥堡集焦傍傧储遑皓皖粤奥傩遁街惩御徨循舾艇舒畲弑逾颌翕釉番释鹆禽舜貂腈腊腌腓腆腴脾腋腑腙腚腔腕腱腒鱿鲀鲁鲂鲃颍猢猹猩猥猬猾猴飓觞觚猸猱惫飧然馇馈馉馊馋亵装蛮脔就敦裒廋斌痣痨痦痘痞痢痤痪痫痧痛鄌赓竦童瓿竣啻颏鹇阑阒阔阕善翔羡普粪粞尊奠遒道遂孳曾焯焜焰焙焱鹈湛港渫滞湖湘渣渤湮湎湝湨湜渺湿温渴渭溃湍溅滑湃湫溲湟溆渝湲湾渡游溠溇湔滋湉渲溉渥湄滁愤慌惰愠惺愦愕惴愣愀愎惶愧愉愔慨喾割寒富寓窜窝窖窗窘寐谟扉遍棨雇扊裢裎裣裕裤裥裙祾祺祼谠禅禄幂谡谢谣谤谥谦谧塈遐犀属屡孱弼强粥巽疏隔骘隙隘媒媪絮嫂媛婷媚婿巯毵翚登皴婺骛缂缃缄缅彘缆缇缈缉缌缎缏缑缒缓缔缕骗编缗骙骚缘飨耢瑟瑚鹉瑁瑞瑰瑀瑜瑗瑄瑕遨骜瑙遘韫魂髡肆摄摸填搏塥塬鄢趔趑摅塌摁鼓摆赪携塮蜇搋搬摇搞搪塘搒搐搛搠摈彀毂搌搦摊搡聘蓁戡斟蒜蓍鄞勤靴靳靶鹊蓐蓝墓幕蓦鹋蒽蓓蓖蓊蒯蓟蓬蓑蒿蒺蓠蒟蒡蓄蒹蒴蒲蒗蓉蒙蓂蓥颐蒸献蓣楔椿楠禁楂楚楝楷榄想楫榀楞楸椴槐槌楯榆榇榈槎楼榉楦概楣楹椽裘赖剽甄酮酰酯酪酩酬蜃感碛碍碘碓碑硼碉碎碚碰碇碗碌碜鹌尴雷零雾雹辏辐辑辒输督频龃龄龅龆觜訾粲虞鉴睛睹睦瞄睚嗪睫韪嗷嗉睡睨睢雎睥睬嘟嗜嗑嗫嗬嗔鄙嗦嗝愚戥嗄暖盟煦歇暗暅暄暇照遢暌畸跬跨跶跷跸跐跣跹跳跺跪路跻跤跟遣蛸蜈蜎蜗蛾蜊蜍蜉蜂蜣蜕畹蛹嗣嗯嗅嗥嗲嗳嗡嗌嗍嗨嗤嗵嗓署置罨罪罩蜀幌嵊嵩嵴骰锖锗错锘锚锛锜锝锞锟锡锢锣锤锥锦锧锨锪锫锩锬锭键锯锰锱矮雉氲犏辞歃稞稚稗稔稠颓愁筹筠筢筮筻筲筼筱签简筷毁舅鼠牒煲催傻像躲鹎魁敫僇衙微徭愆艄觎毹愈遥貊貅貉颔腻腠腩腰腼腽腥腮腭腹腺腧鹏塍媵腾腿詹鲅鲆鲇鲈鲉鲊稣鲋鲌鲍鲏鲐肄猿颖鹐飔飕觥触解遛煞雏馌馍馏馐酱鹑禀亶廒瘃痱痹痼廓痴痿瘐瘁瘅痰瘆廉鄘麂裔靖新鄣歆韵意旒雍阖阗阘阙羧豢誊粳粮数煎猷塑慈煤煳煜煨煅煌煊煸煺滟溱溘滠满漭漠滢滇溥溧溽源滤滥裟溻溷溦滗滫溴滏滔溪滃溜滦漓滚溏滂溢溯滨溶滓溟滘溺滍粱滩滪愫慑慎慥慊誉鲎塞骞寞窥窦窠窣窟寝谨裱褂褚裸裼裨裾裰禊福谩谪谫谬群殿辟障媾嫫媳媲嫒嫉嫌嫁嫔媸叠缙缜缚缛辔缝骝缟缠缡缢缣缤骟剿耥璈静碧瑶璃瑭瑢獒赘熬觏慝嫠韬髦墈墙摽墟撇墁撂摞嘉摧撄赫截翥踅誓銎摭墉境摘墒摔榖撖摺綦聚蔫蔷靺靼鞅靽鞁靿蔌蔽慕暮摹蔓蔑甍蔸蓰蔹蔡蔗蔟蔺戬蔽蕖蔻蓿蔼斡熙蔚鹕兢嘏蓼榛榧模槚槛榻榫槜榭槔榴槁榜槟榨榕槠榷榍歌遭僰酵酽酾酲酷酶酴酹酿酸厮碶碡碟碴碱碣碳碲磋磁碹碥愿劂臧豨殡需霆霁辕辖辗蜚裴翡雌龇龈睿弊裳颗夥瞅瞍睽墅嘞嘈嗽嘌嘁嘎暧暝踌踉跽踊蜻蜞蜡蜥蜮蜾蝈蜴蝇蜘蜱蜩蜷蝉蜿螂蜢嘘嘡鹗嘣嘤嘚嘛嘀嗾嘧罴罱幔嶂幛赙罂赚骷骶鹘锲锴锶锷锸锹锻锽锾锵锿镀镁镂镃镄镅舞犒舔稳熏箐箦箧箍箸箨箕箬算箅箩箪箔管箜箢箫箓毓舆僖儆僳僚僭僬劁僦僮僧鼻魄魅魃魆睾艋鄱貌膜膊膈膀膑鲑鲔鲙鲚鲛鲜鲟疑獐獍飗觫雒孵夤馑馒銮裹敲豪膏塾遮麽廙腐瘩瘌瘗瘟瘦瘊瘥瘘瘙廖辣彰竭韶端旗旖膂阚鄯鲞精粼粹粽糁歉槊鹚弊熄熘熔煽熥潢潆潇漤漆漕漱漂滹漫漯漶潋潴漪漉漳滴漩漾演澉漏潍慢慷慵寨赛搴寡窬窨窭察蜜寤寥谭肇綮谮褡褙褐褓褛褊褪禚谯谰谱谲暨屣鹛隧嫣嫱嫩嫖嫦嫚嫘嫜嫡嫪鼐翟翠熊凳瞀鹜骠缥缦缧骡缨骢缩缪缫慧耦耧瑾璜璀璎璁璋璇璆奭撵髯髫撷撕撒撅撩趣趟撑撮撬赭播墦擒撸鋆墩撞撤撙增撺墀撰聩聪觐鞋鞑蕙鞒鞍蕈蕨蕤蕞蕺瞢蕉劐蕃蕲蕰蕊赜蔬蕴鼒槿横樯槽槭樗樘樱樊橡槲樟橄敷鹝豌飘醋醌醇醉醅靥魇餍磕磊磔磙磅碾磉殣慭震霄霉霈辘龉龊觑憋瞌瞒题暴瞎瞑嘻嘭噎嘶噶嘲颙暹嘹影踔踝踢踏踟踬踩踮踣踯踪踺踞蝽蝶蝾蝴蝻蝠蝰蝎蝌蝮螋蝗蝓蝣蝼蝤蝙噗嘬颚嘿噍噢噙噜噌嘱噀噔颛幞幡嶓幢嶙嶝墨骺骼骸镊镆镇镈镉镋镌镍镎镏镐镑镒镓镔靠稽稷稻黎稿稼箱箴篑篁篌篓箭篇篆僵牖儇儋躺僻德徵艘磐虢鹞鹟膝膘膛滕鲠鲡鲢鲣鲥鲤鲦鲧鲩鲪鲫鲬橥獗獠觯鹠馓馔熟摩麾褒廛鹡瘛瘼瘪瘢瘤瘠瘫齑鹡凛颜毅羯羰糊糇遴糌糍糈糅翦遵鹣憋熜熵熠潜澍澎澌潵潮潸潭潦鲨潲鋈潟澳潘潼澈澜潽潺澄潏懂憬憔懊憧憎寮窳额谳翩褥褴褫禤谴鹤谵憨熨慰劈履屦嬉勰戮蝥豫缬缭缮缯骣畿耩耨耪璞璟靛璠璘聱螯髻髭髹擀撼擂操熹甏擐擅擞磬鄹颞蕻鞘燕黇颟薤蕾薯薨薛薇檠擎薪薏蕹薮薄颠翰噩薜薅樾橱橛橇樵檎橹橦樽樨橙橘橼墼整橐融翮瓢醛醐醍醒醚醑觱磺磲赝飙殪霖霏霓霍霎錾辙辚臻冀餐遽氅瞥瞟瞠瞰嚄嚆噤暾曈蹀蹅踶踹踵踽嘴踱蹄蹉蹁蹂螨蟒蟆螈螅螭螗螃螠螟噱器噪噬噫噻噼幪罹圜鹦赠默黔镖镗镘镚镛镜镝镞镠氇氆赞憩穑穆穄篝篚篥篮篡簉篦篪篷篙篱盥儒劓翱魉魈邀徼衡歙盦膨膪膳螣膦膙雕鲭鲮鲯鲰鲱鲲鲳鲴鲵鲷鲸鲺鲹鲻獴獭獬邂憝亸鹧磨廨赟癀瘭瘰廪瘿瘵瘴癃瘾瘸瘳斓麇麈凝辨辩嬴壅羲糙糗糖糕瞥甑燎燠燔燃燧燊燏濑濒濉潞澧澡澴激澹澥澶濂澼憷懒憾懈黉褰寰窸窿褶禧壁避嬖犟隰嬗鹨翯颡缰缱缲缳缴璨璩璐璪戴螫擤壕擦觳罄擢藉薹鞡鞠藏薷薰藐藓藁檬檑檄檐檩檀懋醢翳繄礁礅磷磴鹩霜霞龋龌豳壑黻瞭瞧瞬瞳瞵瞩瞪嚏曙嚅蹑蹒蹋蹈蹊蹓蹐蟥螬螵疃螳螺蟋蟑蟀嚎嚓羁罽罾嶷赡黜黝髁髀镡镢镣镤镥镦镧镨镩镪镫罅穗黏魏簧簌篾簃篼簏簇簖簋繁鼢黛儡鹪鼾皤魍徽艚龠爵繇貘邈貔臌朦臊膻臁臆臃鲼鲽鲾鳀鳁鳂鳃鳄鳅鳆鳇鳈鳉鳊獯螽燮鹫襄糜縻膺癍癌麋辫赢糟糠馘燥懑濡濮濞濠濯懦豁蹇謇邃襕襁臀檗甓臂擘孺隳嬷翼蟊鹬鍪骤鏊鳌鬶鬈鬃瞽藕鞯鞨鞭鞫鞧鞣藜藠藤藩鹲檫檵覆醪蹙礞礓礌燹餮蹩瞿瞻曛颢曜躇蹦鹭蹢蹜蟛蟪蟠蟮嚚嚣鹮黠黟髅髂镬镭镯镰镱馥簠簟簪簦鼫鼬鼩雠艟翻臑鳍鳎鳏鳐鳐鳑鹱鹰癞癔癜癖糨冁瀑瀍瀌鎏懵襟璧戳彝邋鬏攉攒鞲鞴藿蘧孽蘅警蘑藻麓攀醭醮醯礤酃霪霭黼鳖曝嚯蹰蹶蹽蹼蹯蹴蹾蹲蹭蹿蹬蠖蠓蠋蟾蠊巅黢髋髌镲籀簸籁簿鳘齁魑艨鼗鳓鳔鳕鳗鳙鳚蟹颤靡癣麒鏖瓣蠃羸羹爆瀚瀣瀛襦谶襞疆骥缵瓒鬓壤攘馨蘩蘖蘘醵醴霰颥酆耀矍曦躁躅蠕鼍嚼嚷巍巉黩黥镳镴黧籍纂鼯犨臜鳜鳝鳞鳟獾魔糯灌瀹瀵譬孀骧耰蠢瓘鼙醺礴礳霸露霹颦曩躏黯髓鼱鳡鳢癫麝赣夔爝灏禳鐾羼蠡耲耱懿韂蘸鹳糵蘼囊霾氍饕躔躐髑镵镶穰鳤瓤饔鬻鬟趱攫攥颧躜罐鼹鼷癯麟蠲矗蠹醾躞衢鑫灞襻纛鬣攮囔馕戆蠼爨齉".ToUTF32)
            Dim TWPrim = "的一是了不我有在人來大上這到們個小你子他以好為就生要說中天和時可麼看會地家下出學著著國得也用多成年裡過去能後長都分很老動起物公水什心把面作方那十想樣自所然如三發事對沒還同高道師氣種而回二開於頭現做文媽體些最寫又因前從法明行問意花日形知四快之常經教活太兩果本兒點當美校進等但樂部再數像只力色外真各山題表請課月呢第位定比與字身相間手王主西電五話見才使名走打聲邊將海給白正幾愛音並加怎少新吃習她變被光情重或由記謝爸民親聽全化空向其嗎實每風望別原畫星候次口先答度哪東平它無工石合此及元已母特六己女球馬讓安帶感圖龍書叫樹友類理您路放覺代內應張住量立更許喜關機歡便完世解造利流處算期臺弟車入北業眼例找百直照朋式節接目讀指神飛非希七條吧陽食南八結門展連今河通容觀玩興句傳認隻詞性卻木近萬金選清火信填孩者父魚受號亮收林熱品班斯牠稱肉腦難念線思片語故市角網場提草啊遠九跟麗區笑術影滿達且建辦早園管往注除黃死示段科保器服紅留曲何味單界站命包告產導交練拿土該拉運義古共歌育奇計灣布祝列鳥功求城寶報約改深較爾象福幫格葉細設任始晚哈苦春識久步千強溫跑落至輕積則雨參根整巴統轉里總首錢具哥精試雖考離士言底華夫阿藝演呀件青送組皮半夜級遊舉反腳刻筆童怕骨縣程忙室蟲克跳台鄉論娘族誰假怪植環料黑響裝牛英禮紀唱資倒排越朝備船坐病鐵視史妹板紙詩狀農趕啦錯房夠戲養亞員李畢斷型雄衣消奶左恐塊座尺終燈緊團層油院察助準決伯周失易依妳吸政停訴羅必婆綠右令景雲買軍似護引驗甲掉剛圓取查基調隨害境野康息竹群戰置狗極推漸居德絲足冷館即傷製臉集社卡歲舞賽香琴慢牙曾沙穿睡需般蘭歷質切爺洲孔突源震靜充錄午講系急藏速升持討短客迷順米另低帝呼富貴續存態驚洋務健費玉背維姊盡廣仙創據究彩優辛喔裏供永田官待男修雙陳測標鼓毛案飯針旅含竟領顆研屬材拜適善枝增頁眾睛岩密醫趣姑附劇盤治乾乾項確村武波救省專顧印摩寒藍雞屋嘴未模技鼠血休射喝陸鐘配湖兵倍議貝旁洞雪賞素忘簡端孫圍敬搖貓隊菜藥顯朵商獲皇迎露仍幸鏡哇絕支制良銀壞宮忽夏復止投探冬爬奏係博乙守局乎液構際須範頂殺獨初透漢鬼蛋燒哭兔若破差顏織尾規壁築吹酒痛防勞珍愈換漂岸昨倫遇江微幼郎榮歐聯懷願互羊游派梅蓋壓警丁粉擔免偷豐偉繼季施亂袋窗斤份努島減魔巨仔負追險劉衛靠輪靈楚叔齒洗敢甚桃陣章散樓拍府符介庭祖伸按鬧刀否灰森揮彈秋尋編營堅移談擊賣暖爭紛盛替餐志虎桌夢耳尼堡殼杯堂圈吉店乘值烈異聖暗獎嚴鎮麻兄猴忍毒悄秦採床茶滑聞齊哦雅箱堆熟沉價操姐典序肯俗閃票頓奮壯唐醒佛豆幹豬偶蛇勢括紹餘徵巧某釋欣勇率寬暴妙災索責納瓶預儀懂臨橫伴珠冰降鮮雜智雕濟泥嚇彎泡軟勝距架激抱秀況抓婚超刺描惡尤戶吳逃氧困述浪威染街篇縮聰隔銅州封致貼箭橋危孝限溪鞋姓烏牆猜淡敵固掃逐勤棒載悟效普姆既掛盒惜硬播慶宜妻獵舊后司缺淚證蓮蘇享財尊躲滴翻寄遍繁捕登煩瓜弄抽淨窮鋼乳厚膠權讚傑髮覽均剪禁汽肥雷狼僅摸鼻糖默婦鹿池胡剩聊避折冒套付繩宇革楊舒協招敗蒙億廳獅肌攻訊退徒幣佳版核粗腿喊補辨繞域滾獻昆洛咪弱陰莉溶滅阻訪湯磁遺宗冠紋隆潔郵尚粒斜裂沿誠碼慧擺側借掌奧稻丹松抬揚潮犯呈挑胞貨脫嘛鈴藉末君衝魯鵝尖束恩胸貌臟觸仁垂娃殊概閱肚祥閒蒸璃擦丟柱啟棉誕濃鹽爛憶甘悲莊蒂玻龜檢卵芬徑益煙階緣歸宙股捉旋鳳複鄰糕勵燃亡臣谷律陶莫膀疑聚績繪飄昏涼累途混港瑪鋒融隱週酸砍巢萊坡浮輸址柔搬餓蝶獸汗曹毫棵嗯戴鷹碎籃延抗恭插塔鴨仿判翅愉晨蛙慈耕筋漁燕騎芳偏訓敘陪暑墨燭臂匹佩迪秒評塑輔磨螺職珊誤奔拔析朗梯遭鬆蘿○奉屆霧宋捨碰瞧炮礦凡託略握闊副脈嘉廠衡川央丈彼鬥雀熊廟趙承扇嘆憐噴囉穴氏托佈埋柴徐壺番罪炎虛蔔賴謎挖麥塞椅縫擠穫哲飽曉頸蟻吐妮攝莖蜜晶紫飾猩廢橡邀黏帽孟曆庫零寧芽叢覆京耐腹夕擇胃筒軸漫惠鑽瓦恆祭跡蟹灌妖咬甜搭矮棲策慌監羽忠諸耶猛損殖埃膚擁井哺蜂舅懶墓鉛劍蕭翠沖貧嫁綱蔡賓撞爵麵耀吵幽翁貫敲緩壤騙腸侍貢疏詳柳伙叮呵委牧航勒幅牌欄占寺伊刷訂侵捲傻賢呆舍晴搶翔瑞菌劃幕撒蠟籠斗蚊閉遲胎蘋丙朱唉倆迴旗罵鳴諾騰怒授慮遷餅障辭促幻吞私郊汁循奪輩蝦扁偵廚縱斑渡塵棄飲疊冊俊姿哩榕槍勸鞭仰拖振羞塗綿壽憑躺雌翼額爆杜咦恨荊慣輝簿躍殘伏竿泉席巡煤溼蘆匠拼肺骼洪釘凱厲恥薄舌夾肩枯帥稀柏殿晃渾培嘿荷擴傘傲狐娥峽敝喻于汙刊姜宣屏淺崇潑煮賀豪踏絡譜蹟艾乃孤虹詹嘗映喲悶泳荒腰鄭蹤爐競勉澳拾髒笨腔稍碳嘻撲遙澤屍牽亦咚乖侯貞媒譯勁恰皆臭棋敏焦搜款盜嗨黎儘橘穩牢患抵喪桶郭胖嗚敦罰漿擾灘齡伍丘扮扶淑詢粽嫦漆撿黨譽爪呂眉茂恢狸凍純疾瓷稚蒼韓頻屁坦疼脖笛蝕穀凝澆蝴錦藤旦峰悅陷梨瑟厭腐撥慕療遵膽串拇泊捐娜械棚碗駛螞礎籍鬍申屈廷岳盆羿桂割韻欲愚軻溜傾琳徹董輛糟喂予摘狂范盼役翰蕉鍵襲礙宏箏狠添喬蜥苗瘦踢懸匆伐批夥祈茄秘帳采潛嶼簧鍋繡贊慰憲邦駕哀謂梁窩漠醉膜犬抹桿趁帆弗圾垃炸泌祕啄惱瞪滔碧誌闆猶糧脊唯爽控訝辯慘銳劑謙鴉鱷召干弓盈疲執蛛筷諒芒尿牲茲莎愁跨歉睜賺龐罐仇巫戒返姨臥眠曼購悉欺鉤澎痕蹈薩撐巾划奴玲迫茫堪塘溝裁頑暢哎赫遮糊銘魏吊凸咐肢宴唷爹猿綁蒐暫颱澱馨斧抖怡赤逢悠跌湧螢跪踐盧璧嘩鎖蠶繫旨凶佔逆拓怨柯挺佑寂淋逛菲截誇轟瑜墳櫃籌獄鯊顎耍砂悔喃纏婿憂艱鍊賜顱兇宅押泛挨酷兼豎莓審萄嘰賊鴿孵儲羨惰督葡葛婷玄札扯帕貪豚棧註腺邁灑叉企叭卷卓玫烤匙鋪盪措渴嫩株廉乏舟汪吩韋紂遜宵釣喚蜘哼擬醜驕藻囊昌畜泣軌芭邱妨傍閔椎瑰擅遞寵憤嬰謀嚷罷驅鑿掰扣仗征宿牡昭浩泰枚滋漲漏噹嬌魂瞭褐遼霜嶺隙闖藺枕忌浴紗裙砝傢慎綜絃碌摔榜潤遣濫簽驟臘纖歹趴豹娶飢陵掘瘋禍皺燥燦贏扔怖胚涉紐晏崖捷禱壹豔瑚潭頗稿嬸艘鑑鱗寓綺匈岡吱弧辰彷氛沸歪巷姪樸袁螃晒犧喇綻廖墊銷鄧准啼揭乞亨拋芝陌偕脆梳宰症騫淘蘊捧嫌飼裕踩墾鍾謹涵搞妥昇沼俠彥耘唸秤廊喉愧閣蔗貳賭僕蔬禽濕熄薪誼鑄溉熔馳撫蔣嫂仲彿奈併披吼蜴亭擋衰螂慨窯覓噢葬禧兢虧浸署嘍紡梭譬軀拆吻畏炒趟挫劈卿緻逝聳掩鋸援儡罩礫媚閎葫栽傀攜轎刑叩昂咒嘲拒蝸眨磚惑瀑毯薇緒餵撈鹼晉滲撕盾絮懼霸瓣鑼污迅脾診皂煉郁舜柄棟凌雯彗鈔吟鼎津喘脹冶勃碑澄儒緯毅薯贈蕩蹄濱鶯澡饒嚼驢劣屯卑娟坊桑售秧哄凋啪廁崩掠棍糞焰蕊嗡嚕蛾鯨頌蠻誓攀謠鷺摺懈擲暈寞劭孕丐抄沃肖賈邪靖框違唔駐躬噪袖蕾蜓蹲窄戚塌赴辱鵑菇欠凹寸杉毀嗅沾肆洽彰租瑩剖辣狹鴻侏礁俄耗逗缸埔戀允坑沈咱俯彌倉颼萎韌棘窟頒盞蜻蓬拳催扎估刮弦拘姍契姻削倦剝歇矩聘郡樑笠噸梢膝筑膨溢檔揉酥湊恕喀瞬濁謊簾蠅旱竊櫻衍茜淒緞惹誘肅蔥菊嚀槓螳蜢顫琪逼裳蔭傭瀝蹦杖娛硫肋炭丸豈函殷拌堊旺偽宛栗斥咕妒盲妝御淵眶訣媳渦藹焚噠棺膏僧槌盟鋤輯襯募碟兮戈丫杞虐咽峻軒蚓烘哨竭陛碘惟澗趾潘烽鞠匯霞遂懇崑蚱酬畦販錫磷禦攪籬鰭篷豫劫篩拯籤祀鹹涓翹挽醬蚯鷗椰樞旬蓄吝澈糾鋁衙弊駝嬉胰婉崙煌裹肝峭畝扭氫屑涯茱刪叛蚩陡寇紮喳傅鈍爍雇顛葦鏈墜鰓樵闢穎轍頰鏽憾橙黛暨暮搧矛禾吏妃禿鈕剎煎秩撇彭鉅崗馴逮褒隋儂掙嫉菩咩淹豌翩錐駱穌蟀麟嚮鵬鬚雛攔躁浞朽丞余坪怠吾奕拭咳咀陀衷挾砰堵淅釉碩喙槳暉霄逸誨稅噓甄蝠堯颳廈蒜隕眺琢蟋穗燙磺諧禪釐櫥髓黴躡蠢疆艦壼籲啥吒呱奸乍曰券兆杆砌秉咸徊倘萍凰菸捆棕笙裘敕箕徜慚雁蒲聆徘絨脂晝寢蛻壇賦蝙誦攤儉鰻凜壩瞎鵡壘巒橢鸚罈賠鍛鴕擎諄汀勿勾沛罕拂茅悽斬砲頃栩愣耽琅盔搓喧筍沫楓啞揍惶脅堤稜窪蓉蜿嘎寮襟蔚薦閩欖濾蟬踴酶薔鬣懿蜒樟徽曬慾諫摀廿吁怯盯杰桐抑媛垢揀拱奠倩辜琉腎冤溺禹俱畔徉泄腕湘氮菁稠睹償痴骸樁鮭幟瞿撮霖魄濤蕨瞞蔓瞇賤萱瘤糰繽齦鑰鑲戊倡庸坤淳抒紳伺梧恍唾砸崎卉舵咖陋啡杏疫嵌債鈞搗煞詠癮晰鏟筏繭溯鶴靴舖鉀輻輾骯瀉槽瀰嘶鐺簫魁僻橄潰詛竣鄒摟熬囚昔咿皿咻卸芸伶侮狄咏呎姒氾茁苑冥眩畚捏隧糜彬錘徙癌琵檬鈣橇蜀錶雍囑瞄懲絹譏懊糯幢癢濛靡圃艷菱竇楣鬱裔欒骰椿縷莽黍塾綴敷蕙勻亙枉沐洶茉苔稽竺潦炙樺奢擱脣襄稈閻欽諦漩鎚滯癡墅瓊綢騷腥筱魅瑋荀呦掏浣暇腫寡嘟嘹狩偎聒慷丑卜仕坎拙灸迄忱迢昧祿摧俏翡耿筠虔瑤匪酵焉鄙淮瓢皎頹廂瞰袱蕪嗇罹凳簇洩檸盎諷崁磯捺竄羚繳甥闡祇籟逍敞艇剔淇簷襪蠍嚨聶簸瀕嚥驪鸛厘亥曳叨屹伕芋冽宦妾吠拚扳吶垣侶迦罔啃氦袍兜庶倚睏荐渝珮愴屠貯梗棗祟冕烹峨唇萌貿渣僑蓑撤璨錳葵窺綽犢跤鵲稟蟾漳聾摹攘銜貘瘡喵磊哞薛孢嚐璇漱璀嘈鈸詮撰蓓鳶膩癒遴滌戍狡弘呻帖灼佗垮籽枸俐柿芙餚諱咯鰍玟邏祠麓哽鐲梵曠蛀儸淪襖惋瓏煥韁嗓瀚蛟蜍跋郯毽轆詭搨腱繇肇鴞綵茸舐摯缽裸崔嘯屜餃絆矯湍濺鈉衫氯輟椒瞌痘蕃擄褚羲迭虞屢嗽禎霉匕阱灶甩吭扒妍扛苟刃肱毋竽肛咧佐胄亢柵几唧甸夸冉卞卒疹茹叟蚤逞揣麝廬釵鐸搏黯嗜繚馮鞣甦鐮楞纔綸饑寨掂滷纍廓尪憫鰹磅楂鄱恒蝗髖斃矚霍掀褲皓櫛渺頤貸簍貶臀瑙羹嶇朧蓆琍瑛趨僵諺閥癟繆隸褶攏駁撼鞍踱朦汐苞吔峪厄桅臼浙兀陝玖豺歧蚜姚舀俘梓鰕愕卦厥炯妄吋扼柑彪莒荼瑯滄瀏犄檀跛嚎剷甕漬褪蜷蘑嘖蠕舔蹺漓臏硼鬢餒藷瑾曦蟑隴煦鄲膈㕷熙覷粱鈎誣蹧猾箍嘔歎憩蛹幔邯螯鼹云佇旭倖桓沌勘帛悍奄茵矣哮拐晁拴涕恬娓姦厝穹荔矽倭婢挪矢媧佰悴庇逕芯淙剃淫侷惕侈淆咆屎郝肘柩揪犀楨渲詐詔燻曜湛櫚痠櫓搔臍晷饗遁靂綑攬跺霹萼闐熒謬僥瀟撓薰蔽藩締瑄緝庄噬硅瞳嗝齋黽鎂祂頷弇熾鑾檯栖鮑苡蟒嘓絞漉楷樊噎鉗蹋祺霎嗶篤醋駿磋錠詣僱潺箔閘諭諮蟆痣愾乓孜拎乒佣妤呃汲甫伽兌囪肪剁杵坍卅岔觔隹枴疚剌邵驛哉壢匍靨盹鯽炫齣峙齲蚪騾埂藕訌巔砥囓匾戇胳椏埤翊荻龕毬哆帷箬眸斌扈噁烯芮悵窸蛤窣萸沏逵蕁皴葳萃菖琺鵜鈾蟄辟楸僚鶘馱鵰睬筯痰矗嬴攙輒昱賑邸毆疙嫻肴噗婁駭朔斂笆嚏氨壑紊檜胱竅俺闌匐邃彫辮淤礪掖擂尉磕眷履涸諳舷檣飧簪棣霽絢斕榔瘩硝憔榭糙葆鵪墟橈獷釀箸瘧蒞痺蝌刁唬仟陞吆耙夷狽汝啕夭芻呸蚌匣晌咎匿侑迸邑赦咨悸祁赧氟掬冑涎沮斛泵悼匡莢聿蛄岱痊岌舶沂犁炬惚芾寐拷惦疋庵庚湃沽嗦俞壬岐殃悚硯琶渠搾渤饞弒洒粟蠵榴鈺雉獾誅峯睦搥膊饋踞嗒篆煒踝澇撩鮒瘟鑷皚掐骷笊墮琇墩鎏駙戩噩粘勳叡冀槐礡遏餡粹穢睞篡醇檳熨艙戮繃璋謐瘠篙賬澀嫵髏擒鏘颯饅憊鎳噯醺嘮囂駟癩螫贖薑癱虜攫髻鰾蹂癲鼾髂霓粲螻垛嶽彘縛搐谿鷲懦窘轄鱉鏢鸝鎊骶闔晟疇鼴忡佼椹鯰曝璽籮巖鱖戌弩么杭䲳忿䲁汞圭侖牟珀叼疤弛玳汶胛拄恤汨胥獴洮丰炳尸芥坷殆勺夙忪抉嗬韭宥栓釗祐尹敖姬腑倪斟捎詁浦徬蚵粥莧瑁惺窖徨蛭扉斐俾毓酗滂袈貂窒烊揆寅蚣琛琦琥鄂瑕羔醞鳩黝褂燄肄嬪慵踵筵韆嘀靄綾鞦裊蹼斡羈裴鼬憧譚頡籐憬闕緬懺閤麒銨攣醃鑒獰蘸蕈羶鍬霾膿贓癖儼擷鯉謗櫈瀆謅氈唰戳体謁謳諼懃縐鎌滬螽腮瀧僮隼蜈涂摻欸寰譊膛蹠鞏憨璦驥璣吽瘸猝瞥魨濘嚭曇昶燼鄯瘍飆稔笵裟彧廝鰲燉姵璿鬟殯鑫璟躪詫顳鋅傜懋寥懍瘴騁鐙怜宕芃銥睫枱鴷琮伎摁愔嗩蓁圮漚跗汕挵戎丕硓并鯝叚帚艮拗怱呶糸仆圳况弔佻怵佬戾阮帘胤昕芷汰柢妞俑囤剋彤拽芹虱軋恃盂垠阜赳尬苯汴炊禺奎紉氓癸舢沁奐玷酋迥閂冇倌旻罟呯胭椽捍侄恣町涅縧訕皤狷紜璐啤謔倔踽埠蘞桔蘦悻蕎孳鰈喫葚廄饌弼糗舂獼粕蠑掣蛸翌纜涿粧蛉銹筐獒楠欉証籗榆噘萋櫫慄蠃跚榧嗆魆鈐餑湣鬈隅皸揖鴽慇鍙煽葎琿鶇酪槤蜆緄榻忤蜊蒡蒿蒟魷鱒儕蒴緹蒻遨劄誡廨諉韙稼蜾諂懨噶薈餌胺獗珩嚅苳輿茌燬苹褸嵋鮪眈穆囟靛珈篦枷瞻拏褥祛諜鈦轡邳躊亓殲狒蹙酌懾耆贅偌蹣舨夔浥鷥浹鎗娩蹶唆矇奚鎬秣簣啜鎔涇臚朕繹惘鏤瓠椋疵脛崧旒梱瓞耜軫猖膾翎燠幀螟晤霏暄翱楔臻彙瀛楹禳媲顥棠譁塢囁琨罌痙譎腋餽腴傕腓紓雹睇軾擘槃簑雋豁詰櫺榨贗榛鱔瘁襤銬氪嬋尷蝨簌閨巍誹醴碾寔蠡躇瀾璜銖羯閡稷暱諪兗耥恪氐沱汎泗刎咫倣叱姣牤浬阡啣朴悖絝挈弁珞皋玘奘佯倨抿倏枋冢杓娉咋偃邢窈孚鬲芍茴圻茗吮茨刨婪泠荃苛逅泯俸祉孰泅窕洌旎虻捩牴晦疣嵐洵蛆柚蚶釧碱捫咔愜仃硃乩啻苷淌甬捱侃惻戕絀妓崛杳愎佃脯羌惇苧訢泓笞洱猙柒笮氖焊揩淄酉恿沅崢昊訟酊豉亟堰蝽揹䲅棹攸嵩苓幌甭昀杼盅葱痢塭賁軼溴幄嗉湡搆碴菠裱煜淼隍蜚渭裰暘撂塚榖馭潁渥徫棻潢幗銎嶄觚遐儆逾璉葷瞍漾酚暝魟賂瞅瑣頎鈷蓽稞蔞鈽蝟艋褙鉋舺蓊眥犒瑢痳腊粵閫瘀萣蜩滸僖噀貲琄墀嬃憎圜撚嶙冪淖慝淩頜摰韶靳噤槲鄞葯蔑螓遛彀赭蒍頫槿劊扙潼遶懣錛踹錩賸閹鍚僉孺鮐覦梇駢鴝縈晢殮詒瀋殭鍥澔蹊喏曙碇盥睚濠罅錨緡醣崚濬婕邂乜瞼襁遽謖檻偲篠陟薊蓴驍隰矓荇囈鋯鐳茛譴謾蟯嚙韜网驃燿鵠蟳贛黿謨瀅蟠癤顰笈瀘砱齜蹔齧砣鶉珣馥閬邇騅孿眭霰珥甯鵟焙鼩閑厴隄涘嗣嬿滇櫧賄烷貉桎旖捅酩穨嫣糬筮嗐蓀哧睪埕隘蹭靶鏄睿娳緘耷霆鯔慫鶆箴紈滕鵯犛鱸褓曣諍淀嶸藿鴛蘄壕嗔鴦蠖縊籸罄螗鍍眄瞟螅癘鰆蹉洄臃鶿餞鶩壟櫸齪洨灞毖鐫螈騵鸞醮殂瓚鶺鬃鼱覲攢纓暐琬梏摒恂酣糴跎驊蜃鱈幛扃嫗躕鉑峋髡攥壅屌銼轤踟姮誑濨攆芫錚坌鴒顴燾剉瞽鷿縹糶贍妏囌俬鏃鸕孽玠繕欞蜣鱺氬炔箋瑗睨泂賃枒鉻崌癆嫺縑扠覬豸獺岬霪夌齷咂愷冼榷烟緲緜糠玕濰虬鷂躭綬汩褊蘚餾摑臆粿緋犂鮟鱇頫".ToUTF32
            TNorm = New HashSet(Of Char32)((From c In TChars Where GetLevel(c.Value, "kIRG_TSource") = 0 Select c.Key).Union(TWPrim))
            JNorm = New HashSet(Of Char32)("亜哀愛悪握圧扱安暗案以位依偉囲委威尉意慰易為異移維緯胃衣違遺医井域育一壱逸稲芋印員因姻引飲院陰隠韻右宇羽雨渦浦運雲営影映栄永泳英衛詠鋭液疫益駅悦謁越閲円園宴延援沿演炎煙猿縁遠鉛塩汚凹央奥往応押横欧殴王翁黄沖億屋憶乙卸恩温穏音下化仮何価佳加可夏嫁家寡科暇果架歌河火禍稼箇花荷華菓課貨過蚊我画芽賀雅餓介会解回塊壊快怪悔懐戒拐改械海灰界皆絵開階貝劾外害慨概涯街該垣嚇各拡格核殻獲確穫覚角較郭閣隔革学岳楽額掛潟割喝括活渇滑褐轄且株刈乾冠寒刊勘勧巻喚堪完官寛干幹患感慣憾換敢棺款歓汗漢環甘監看管簡緩缶肝艦観貫還鑑間閑関陥館丸含岸眼岩頑顔願企危喜器基奇寄岐希幾忌揮机旗既期棋棄機帰気汽祈季紀規記貴起軌輝飢騎鬼偽儀宜戯技擬欺犠疑義議菊吉喫詰却客脚虐逆丘九久休及吸宮弓急救朽求泣球究窮級糾給旧牛去居巨拒拠挙虚許距漁魚享京供競共凶協叫境峡強恐恭挟教橋況狂狭矯胸脅興郷鏡響驚仰凝暁業局曲極玉勤均斤琴禁筋緊菌襟謹近金吟銀句区苦駆具愚虞空偶遇隅屈掘靴繰桑勲君薫訓群軍郡係傾刑兄啓型契形径恵慶憩掲携敬景渓系経継茎蛍計警軽鶏芸迎鯨劇撃激傑欠決潔穴結血月件倹健兼券剣圏堅嫌建憲懸検権犬献研絹県肩見謙賢軒遣険顕験元原厳幻弦減源玄現言限個古呼固孤己庫弧戸故枯湖誇雇顧鼓五互午呉娯後御悟碁語誤護交侯候光公功効厚口向后坑好孔孝工巧幸広康恒慌抗拘控攻更校構江洪港溝甲皇硬稿紅絞綱耕考肯航荒行衡講貢購郊酵鉱鋼降項香高剛号合拷豪克刻告国穀酷黒獄腰骨込今困墾婚恨懇昆根混紺魂佐唆左差査砂詐鎖座債催再最妻宰彩才採栽歳済災砕祭斎細菜裁載際剤在材罪財坂咲崎作削搾昨策索錯桜冊刷察撮擦札殺雑皿三傘参山惨散桟産算蚕賛酸暫残仕伺使刺司史嗣四士始姉姿子市師志思指支施旨枝止死氏祉私糸紙紫肢脂至視詞詩試誌諮資賜雌飼歯事似侍児字寺慈持時次滋治璽磁示耳自辞式識軸七執失室湿漆疾質実芝舎写射捨赦斜煮社者謝車遮蛇邪借勺尺爵酌釈若寂弱主取守手朱殊狩珠種趣酒首儒受寿授樹需囚収周宗就州修愁拾秀秋終習臭舟衆襲週酬集醜住充十従柔汁渋獣縦重銃叔宿淑祝縮粛塾熟出術述俊春瞬准循旬殉準潤盾純巡遵順処初所暑庶緒署書諸助叙女序徐除傷償勝匠升召商唱奨宵将小少尚床彰承抄招掌昇昭晶松沼消渉焼焦照症省硝礁祥称章笑粧紹肖衝訟証詔詳象賞鐘障上丈乗冗剰城場壌嬢常情条浄状畳蒸譲醸錠嘱飾植殖織職色触食辱伸信侵唇娠寝審心慎振新森浸深申真神紳臣薪親診身辛進針震人仁刃尋甚尽迅陣酢図吹垂帥推水炊睡粋衰遂酔錘随髄崇数枢据杉澄寸世瀬畝是制勢姓征性成政整星晴正清牲生盛精聖声製西誠誓請逝青静斉税隻席惜斥昔析石積籍績責赤跡切拙接摂折設窃節説雪絶舌仙先千占宣専川戦扇栓泉浅洗染潜旋線繊船薦践選遷銭銑鮮前善漸然全禅繕塑措疎礎祖租粗素組訴阻僧創双倉喪壮奏層想捜掃挿操早曹巣槽燥争相窓総草荘葬藻装走送遭霜騒像増憎臓蔵贈造促側則即息束測足速俗属賊族続卒存孫尊損村他多太堕妥惰打駄体対耐帯待怠態替泰滞胎袋貸退逮隊代台大第題滝卓宅択拓沢濯託濁諾但達奪脱棚谷丹単嘆担探淡炭短端胆誕鍛団壇弾断暖段男談値知地恥池痴稚置致遅築畜竹蓄逐秩窒茶嫡着中仲宙忠抽昼柱注虫衷鋳駐著貯丁兆帳庁弔張彫徴懲挑朝潮町眺聴脹腸調超跳長頂鳥勅直朕沈珍賃鎮陳津墜追痛通塚漬坪釣亭低停偵貞呈堤定帝底庭廷弟抵提程締艇訂逓邸泥摘敵滴的笛適哲徹撤迭鉄典天展店添転点伝殿田電吐塗徒斗渡登途都努度土奴怒倒党冬凍刀唐塔島悼投搭東桃棟盗湯灯当痘等答筒糖統到討謄豆踏逃透陶頭騰闘働動同堂導洞童胴道銅峠匿得徳特督篤毒独読凸突届屯豚曇鈍内縄南軟難二尼弐肉日乳入如尿任妊忍認寧猫熱年念燃粘悩濃納能脳農把覇波派破婆馬俳廃拝排敗杯背肺輩配倍培媒梅買売賠陪伯博拍泊白舶薄迫漠爆縛麦箱肌畑八鉢発髪伐罰抜閥伴判半反帆搬板版犯班畔繁般藩販範煩頒飯晩番盤蛮卑否妃彼悲扉批披比泌疲皮碑秘罷肥被費避非飛備尾微美鼻匹必筆姫百俵標氷漂票表評描病秒苗品浜貧賓頻敏瓶不付夫婦富布府怖扶敷普浮父符腐膚譜負賦赴附侮武舞部封風伏副復幅服福腹複覆払沸仏物分噴墳憤奮粉紛雰文聞丙併兵塀幣平弊柄並閉陛米壁癖別偏変片編辺返遍便勉弁保舗捕歩補穂募墓慕暮母簿倣俸包報奉宝峰崩抱放方法泡砲縫胞芳褒訪豊邦飽乏亡傍剖坊妨帽忘忙房暴望某棒冒紡肪膨謀貿防北僕墨撲朴牧没堀奔本翻凡盆摩磨魔麻埋妹枚毎幕膜又抹末繭万慢満漫味未魅岬密脈妙民眠務夢無矛霧婿娘名命明盟迷銘鳴滅免綿面模茂妄毛猛盲網耗木黙目戻問紋門匁夜野矢厄役約薬訳躍柳愉油癒諭輸唯優勇友幽悠憂有猶由裕誘遊郵雄融夕予余与誉預幼容庸揚揺擁曜様洋溶用窯羊葉要謡踊陽養抑欲浴翌翼羅裸来頼雷絡落酪乱卵欄濫覧利吏履理痢裏里離陸律率立略流留硫粒隆竜慮旅虜了僚両寮料涼猟療糧良量陵領力緑倫厘林臨輪隣塁涙累類令例冷励礼鈴隷零霊麗齢暦歴列劣烈裂廉恋練連錬炉路露労廊朗楼浪漏老郎六録論和話賄惑枠湾腕".ToUTF32)
        End Sub

        Private Sub GenGTTable()
            Dim GTAdditionalG As Char32() = "厂厂卜卜几几乃乃了了丫吖广广干干干干乾于于才才万万千千么么么幺幺尸尸斗斗丰丰云云巨巨扎扎历历升升升昇仆仆凶凶丑丑汇汇它它术术札札布布占占叶叶只只只只祇叹叹出出冬冬咚饥饥台台台台发发冲冲并并并夹夹夹扣扣托托朴朴夸夸划划当当吁吁曲曲曲同同吊吊团团回回朱朱伙伙价价向向后后尽尽奸奸纤纤沈沈闲闲证证志志坛坛坛折折克克克芸芸苏苏苏两两杆杆杠杠戒戒卤卤里里里困困呆呆别别针针佑佑佣佣谷谷余余馀系系系局局局注注沾沾帘帘卷卷表表幸幸拓拓拈拈拐拐抵抵范范苹苹杯杯板板松松郁郁采采采采制制刮刮岳岳征征径径念念舍舍肮肮周周周弦弦弥弥哗哗姜姜炼炼迹迹胡胡胡荡荡药药咸咸厘厘面面尝尝哄哄响钟钟锺复复适适秋秋皇须须凄凄消涌涌席席准准症症挨挨挽挽恶恶获获栗栗致致党党脏脏娴娴淡淡淀淀梁梁麻麻旋旋捻捻据据菇菇戚戚累累彩彩偷偷衔衔欲欲游游羡羡雇雇确确筑筑铺铺链链御御腊腊漓漓摆摆蒙蒙蒙蒙蒙蒙鉴鉴暗暗愈愈签签稗稗辟辟熔熔蔑蔑酸酸愿愿蜡蜡熏熏糊糊霉霉噪噪赞赞雕雕雕".ToUTF32
            Dim GTAdditionalT As Char32() = "廠厂蔔卜幾几乃迺了瞭丫丫廣广乾幹干榦乾於于才纔萬万千韆麼麽么幺么屍尸鬥斗豐丰雲云巨鉅扎紮歷曆升昇陞昇僕仆凶兇醜丑匯彙它牠朮術札劄布佈佔占葉叶只衹祇隻祇嘆歎出齣冬鼕鼕飢饑台臺檯颱發髮沖衝並併并夾裌袷扣釦托託朴樸誇夸劃划當噹籲吁曲麴麯同衕吊弔團糰回迴朱硃伙夥價价向嚮后後盡儘奸姦纖縴瀋沈閑閒證証志誌壇罎罈折摺克剋尅芸蕓蘇甦囌兩両杆桿杠槓戒誡鹵滷里裏裡困睏呆獃別彆針鍼佑祐佣傭谷穀余餘餘系係繫局侷跼注註沾霑帘簾卷捲表錶幸倖拓搨拈撚拐枴抵牴范範苹蘋杯盃板闆松鬆郁鬱采採彩綵制製刮颳岳嶽征徵徑逕念唸舍捨骯肮周週賙弦絃彌瀰嘩譁薑姜煉鍊跡蹟胡鬍衚蕩盪藥葯咸鹹厘釐面麵嘗嚐哄鬨響鍾鐘鍾復複適适秋鞦皇須鬚淒悽消涌湧席蓆准準症癥挨捱挽輓惡噁獲穫栗慄致緻黨党臟髒嫻嫺淡澹淀澱梁樑麻痲旋鏇捻撚據据菇菰戚慼累纍彩綵偷媮銜啣欲慾游遊羡羨僱雇確确築筑鋪舖鏈鍊御禦臘腊漓灕擺襬蒙矇濛懞曚幪鑑鑒暗闇愈瘉簽籤稗粺辟闢熔鎔蔑衊酸痠愿願蜡蠟熏燻糊餬霉黴噪譟贊讚雕鵰彫".ToUTF32
            If GTAdditionalG.Length <> GTAdditionalT.Length Then Throw New InvalidDataException
            For n = 0 To GTAdditionalG.Length - 1
                Dim gc = GTAdditionalG(n)
                Dim tc = GTAdditionalT(n)

                If Not GChars.ContainsKey(gc) Then Continue For
                If Not TChars.ContainsKey(tc) Then Continue For

                GTTable.Add(New GTPair With {.G = gc, .T = tc})
            Next
            For Each g In From p In GChars Where p.Value.HasField("kTraditionalVariant") Select p.Value
                Dim gc = g.Unicode
                For Each tc In ParseUnicodes(g.Field("kTraditionalVariant"))
                    If Not TChars.ContainsKey(tc) Then Continue For
                    Dim t = TChars(tc)
                    If Not t.HasField("kSimplifiedVariant") Then Continue For
                    If Not ParseUnicodes(t.Field("kSimplifiedVariant")).Contains(gc) Then Continue For

                    If gc <> tc Then
                        Dim GBi = GNorm.Contains(gc) AndAlso TNorm.Contains(gc)
                        Dim TBi = GNorm.Contains(tc) AndAlso TNorm.Contains(tc)
                        If GBi Then GTTable.Add(New GTPair With {.G = gc, .T = gc})
                        If TBi Then GTTable.Add(New GTPair With {.G = tc, .T = tc})
                    End If

                    GTTable.Add(New GTPair With {.G = gc, .T = tc})
                Next
            Next
            GTTable = (From gtp In GTTable Distinct).ToList
        End Sub

        Private Sub GenJTTable()
            For Each j In From p In JChars Where p.Value.HasField("kZVariant") Select p.Value
                Dim jc = j.Unicode
                For Each tc In ParseUnicodes(j.Field("kZVariant"))
                    If Not TChars.ContainsKey(tc) Then Continue For
                    Dim t = TChars(tc)

                    If jc <> tc Then
                        Dim JBi = JNorm.Contains(jc) AndAlso TNorm.Contains(jc)
                        Dim TBi = JNorm.Contains(tc) AndAlso TNorm.Contains(tc)
                        If JBi Then JTTable.Add(New JTPair With {.J = jc, .T = jc})
                        If TBi Then JTTable.Add(New JTPair With {.J = tc, .T = tc})
                    End If

                    JTTable.Add(New JTPair With {.J = j.Unicode, .T = t.Unicode})
                Next
            Next
            JTTable = (From jtp In JTTable Distinct).ToList
        End Sub

        Private Sub GenGJTable()
            Dim GJAdditionalG As Char32() = "边变残禅称单弹对画姬践茎径举恋蛮浅厅团湾稳压隐栈滨迟齿处传从递读恶儿贰发废丰关观广归怀坏欢绘击鸡剂济继价俭检剑将奖经据觉矿扩览劳乐垒两猎灵龄龙泷卖满恼脑酿齐气钱纤轻驱权劝让荣涩烧摄绳实释兽丝肃铁听图为围伪牺戏显险县晓续亚严盐验样谣药艺译驿应樱萤营圆杂赞脏择泽斋战证铸专转庄总纵拜藏乘粹稻叠佛拂罐惠假娘缺壤剩收髓碎穗溪陷摇壹豫醉爱罢败颁饱报贝备辈笔币闭编标宾钵补财仓侧测层产长场肠偿车彻陈诚惩冲铳丑础创锤纯词赐错达带贷诞导岛敌缔电钓顶订锭东动冻栋斗笃锻队钝夺额饿罚阀烦饭范贩访纺飞费纷坟奋愤风缝肤负妇复复赋缚该干干绀刚纲钢个个阁给贡沟规轨贵过汉贺红后护华话缓还环挥辉贿货获获祸饥机积绩级极几计记纪际坚间监茧简见荐舰渐鉴讲绞矫较阶节诘洁结紧谨进惊鲸竞镜纠剧绢绝军开壳课垦恳库夸块宽赖濑栏滥泪类离里历历丽隶连练粮疗邻临赁铃领陆虏录虑绿伦论轮罗络马买贸门梦绵灭鸣铭谋亩纳难拟鸟宁农浓诺盘赔喷贫频评朴仆铺谱骑启迁铅谦缲桥亲倾请庆穷确热认软锐润伞丧骚扫杀缮伤赏绍设舍绅审圣胜师诗时识势饰试视适书输术树帅顺说饲讼诉岁孙损缩锁态坛昙谈叹汤讨誊腾题调统铜头涂驮顽网违维伟纬卫纹闻问涡无务误雾习袭玺系细辖吓鲜闲贤铣现线宪乡详响项协胁谢兴许绪轩悬选勋寻训颜扬阳养业叶谒仪遗亿义忆议阴银饮拥优忧邮犹诱鱼渔语狱预谕园员缘远愿约阅跃云运载暂则责贼赠诈债张帐胀诏贞针侦诊阵镇征只织执职纸志制质滞终钟种众轴诸贮驻筑妆坠准浊资谘渍组户辨瓣辩".ToUTF32
            Dim GJAdditionalJ As Char32() = "辺変残禅称単弾対画姫践茎径挙恋蛮浅庁団湾穏圧隠桟浜遅歯処伝従逓読悪児弍発廃豊関観広帰懐壊歓絵撃鶏剤済継価倹検剣将奨経拠覚鉱拡覧労楽塁両猟霊齢竜滝売満悩脳醸斉気銭繊軽駆権勧譲栄渋焼摂縄実釈獣糸粛鉄聴図為囲偽犠戯顕険県暁続亜厳塩験様謡薬芸訳駅応桜蛍営円雑賛臓択沢斎戦証鋳専転荘総縦拝蔵乗粋稲畳仏払缶恵仮嬢欠壌剰収髄砕穂渓陥揺壱予酔愛罷敗頒飽報貝備輩筆幣閉編標賓鉢補財倉側測層産長場腸償車徹陳誠懲衝銃醜礎創錘純詞賜錯達帶貸誕導島敵締電釣頂訂錠東動凍棟鬪篤鍛隊鈍奪額餓罸閥煩飯範販訪紡飛費紛墳奮憤風縫膚負婦復複賦縛該乾幹紺剛綱鋼個箇閣給貢溝規軌貴過漢賀紅後護華話緩還環揮輝賄貨獲穫禍飢機積績級極幾計記紀際堅間監繭簡見薦艦漸鑑講絞矯較階節詰潔結緊謹進驚鯨競鏡糾劇絹絶軍開殼課墾懇庫誇塊寬頼瀬欄濫涙類離裏歴暦麗隷連練糧療隣臨賃鈴領陸虜録慮緑倫論輪羅絡馬買貿門夢綿滅鳴銘謀畝納難擬鳥寧農濃諾盤賠噴貧頻評樸僕舗譜騎啓遷鉛謙繰橋親傾請慶窮確熱認軟鋭潤傘喪騒掃殺繕傷賞紹設捨紳審聖勝師詩時識勢飾試視適書輸術樹帥順説飼訟訴歳孫損縮鎖態壇曇談嘆湯討謄騰題調統銅頭塗馱頑網違維偉緯衛紋聞問渦無務誤霧習襲璽係細轄嚇鮮閑賢銑現線憲郷詳響項協脅謝興許緒軒懸選勲尋訓顔揚陽養業葉謁儀遺億義憶議陰銀飲擁優憂郵猶誘魚漁語獄預諭園員縁遠願約閲躍雲運載暫則責賊贈詐債張帳脹詔貞針偵診陣鎮徴隻織執職紙誌製質滞終鍾種衆軸諸貯駐築粧墜準濁資諮漬組戸弁弁弁".ToUTF32
            If GJAdditionalG.Length <> GJAdditionalJ.Length Then Throw New InvalidDataException
            For n = 0 To GJAdditionalG.Length - 1
                Dim gc = GJAdditionalG(n)
                Dim jc = GJAdditionalJ(n)

                If Not GChars.ContainsKey(gc) Then Continue For
                If Not JChars.ContainsKey(jc) Then Continue For

                GJTable.Add(New GJPair With {.G = gc, .J = jc})
            Next
            Dim IsOneOnOneGT = Function(a As Char32, b As Char32) GChars.ContainsKey(a) AndAlso TChars.ContainsKey(b) AndAlso GChars(a).HasField("kTraditionalVariant") AndAlso TChars(b).HasField("kSimplifiedVariant") AndAlso ParseUnicodes(GChars(a)("kTraditionalVariant")).Count = 1 AndAlso ParseUnicodes(TChars(b)("kSimplifiedVariant")).Count = 1
            Dim IsOneOnOneTJ = Function(a As Char32, b As Char32) TChars.ContainsKey(a) AndAlso JChars.ContainsKey(b) AndAlso TChars(a).HasField("kZVariant") AndAlso JChars(b).HasField("kZVariant") AndAlso ParseUnicodes(TChars(a)("kZVariant")).Count = 1 AndAlso ParseUnicodes(JChars(b)("kZVariant")).Count = 1
            Dim IsMultiOnOneGT = Function(a As Char32, b As Char32) GChars.ContainsKey(a) AndAlso TChars.ContainsKey(b) AndAlso GChars(a).HasField("kTraditionalVariant") AndAlso ParseUnicodes(GChars(a)("kTraditionalVariant")).Count = 1
            Dim IsOneOnMultiTJ = Function(a As Char32, b As Char32) TChars.ContainsKey(a) AndAlso JChars.ContainsKey(b) AndAlso JChars(b).HasField("kZVariant") AndAlso ParseUnicodes(JChars(b)("kZVariant")).Count = 1
            For Each p In From gtp In GTTable Join jtp In JTTable On gtp.T Equals jtp.T Where (IsMultiOnOneGT(gtp.G, gtp.T) AndAlso IsOneOnOneTJ(jtp.T, jtp.J)) OrElse (IsOneOnOneGT(gtp.G, gtp.T) AndAlso IsOneOnMultiTJ(jtp.T, jtp.J)) Select gtp.G, jtp.J
                GJTable.Add(New GJPair With {.G = p.G, .J = p.J})
            Next
            For Each p In From gtp In GTTable Where TNorm.Contains(gtp.T) AndAlso JNorm.Contains(gtp.T)
                GJTable.Add(New GJPair With {.G = p.G, .J = p.T})
            Next
            For Each p In From jtp In JTTable Where GNorm.Contains(jtp.T) AndAlso TNorm.Contains(jtp.T)
                GJTable.Add(New GJPair With {.G = p.T, .J = p.J})
            Next
            For Each g In From p In GChars Where p.Value.HasField("kTraditionalVariant") Select p.Value
                Dim gc = g.Unicode
                For Each jc In ParseUnicodes(g.Field("kTraditionalVariant"))
                    If Not JChars.ContainsKey(jc) Then Continue For
                    Dim j = JChars(jc)
                    If Not j.HasField("kSimplifiedVariant") Then Continue For
                    If Not ParseUnicodes(j.Field("kSimplifiedVariant")).Contains(gc) Then Continue For

                    If gc <> jc Then
                        Dim GBi = GNorm.Contains(gc) AndAlso JNorm.Contains(gc)
                        Dim JBi = GNorm.Contains(jc) AndAlso JNorm.Contains(jc)
                        If GBi Then GJTable.Add(New GJPair With {.G = gc, .J = gc})
                        If JBi Then GJTable.Add(New GJPair With {.G = jc, .J = jc})
                    End If

                    GJTable.Add(New GJPair With {.G = gc, .J = jc})
                Next
            Next
            For Each g In From p In GChars Where p.Value.HasField("kSimplifiedVariant") Select p.Value
                Dim gc = g.Unicode
                For Each jc In ParseUnicodes(g.Field("kSimplifiedVariant"))
                    If Not JChars.ContainsKey(jc) Then Continue For
                    Dim j = JChars(jc)
                    If Not j.HasField("kTraditionalVariant") Then Continue For
                    If Not ParseUnicodes(j.Field("kTraditionalVariant")).Contains(gc) Then Continue For

                    If gc <> jc Then
                        Dim GBi = GNorm.Contains(gc) AndAlso JNorm.Contains(gc)
                        Dim JBi = GNorm.Contains(jc) AndAlso JNorm.Contains(jc)
                        If GBi Then GJTable.Add(New GJPair With {.G = gc, .J = gc})
                        If JBi Then GJTable.Add(New GJPair With {.G = jc, .J = jc})
                    End If

                    GJTable.Add(New GJPair With {.G = gc, .J = jc})
                Next
            Next
            GJTable = (From gjp In GJTable Distinct).ToList
        End Sub

        Private Sub UpdateJNorm()
            Dim J0Chars = (From c In JChars Where GetLevel(c.Value, "kIRG_JSource") = 0).ToDictionary(Function(c) c.Key, Function(c) c.Value)
            Dim AJNorm As New HashSet(Of Char32)
            Dim Iterated As New HashSet(Of Char32)
            For Each jc In J0Chars.Keys
                If JNorm.Contains(jc) Then Continue For
                If Iterated.Contains(jc) Then Continue For

                Dim jc2 = jc
                Dim Varients = (From gjp In GJTable Where gjp.J = jc2 Join gjp2 In GJTable On gjp.G Equals gjp2.G Select gjp2.J Distinct).ToArray

                Dim BestVarient As Char32 = jc
                For Each v In Varients.Except(New Char32() {jc})
                    If JNorm.Contains(v) Then
                        BestVarient = v
                        Exit For
                    End If
                    If Not J0Chars.ContainsKey(v) Then Continue For
                    If GetCodeDifference(J0Chars(v), J0Chars(BestVarient), "kIRG_JSource") < 0 Then
                        BestVarient = v
                    End If
                Next

                For Each v In Varients.Except(New Char32() {BestVarient})
                    If Not Iterated.Contains(v) Then Iterated.Add(v)
                    If AJNorm.Contains(v) Then AJNorm.Remove(v)
                Next

                If Not AJNorm.Contains(BestVarient) Then
                    AJNorm.Add(BestVarient)
                End If
            Next
            For Each jc In AJNorm
                If JNorm.Contains(jc) Then Continue For
                JNorm.Add(jc)
            Next
        End Sub

        Public Sub New(ByVal Path As String)
            LoadNorms(Path)
            GenGTTable()
            GenJTTable()
            GenGJTable()
            UpdateJNorm()
        End Sub

        Private Shared Sub StableSort(ByVal m As List(Of KeyValuePair(Of Char32, Char32)))
            Dim kd As New Dictionary(Of Char32, Integer)
            Dim vd As New Dictionary(Of Char32, Integer)
            For Each p In m
                If kd.ContainsKey(p.Key) Then
                    kd(p.Key) += 1
                Else
                    kd.Add(p.Key, 1)
                End If
                If vd.ContainsKey(p.Value) Then
                    vd(p.Value) += 1
                Else
                    vd.Add(p.Value, 1)
                End If
            Next
            Dim l = (From p In m Where (kd.ContainsKey(p.Key) AndAlso kd(p.Key) >= 2) OrElse (vd.ContainsKey(p.Value) AndAlso vd(p.Value) >= 2)).ToArray
            Dim g = (From p In m.Except(l) Order By p.Key).ToArray
            m.Clear()
            m.AddRange(l)
            m.AddRange(g)
        End Sub

        Private Shared Sub RemoveLoop(ByVal m As List(Of KeyValuePair(Of Char32, Char32)))
            Dim Looped = (From p1 In m Where p1.Key <> p1.Value Join p2 In m On p1.Key Equals p2.Value And p2.Key Equals p1.Value Select New KeyValuePair(Of Char32, Char32)(p1.Key, p1.Value)).ToArray
            Dim g = m.Except(Looped).ToArray
            m.Clear()
            For Each p In g
                m.Add(p)
            Next
        End Sub

        Private Shared Sub RemoveSame(ByVal m As List(Of KeyValuePair(Of Char32, Char32)))
            Dim LeftOne = From p2 In (From p In m Order By p.Key Group By p.Key Into Grouped = Group) Where p2.Grouped.Count = 1 Select p2.Grouped.First
            Dim RightOne = From p2 In (From p In m Order By p.Value Group By p.Value Into Grouped = Group) Where p2.Grouped.Count = 1 Select p2.Grouped.First
            Dim One = LeftOne.Intersect(RightOne)
            Dim Same = (From p In One Where p.Key = p.Value Select New KeyValuePair(Of Char32, Char32)(p.Key, p.Value)).ToArray
            Dim g = m.Except(Same).ToArray
            m.Clear()
            For Each p In g
                m.Add(p)
            Next
        End Sub

        Private Function GetMap(Of Pair)(ByVal Table As IEnumerable(Of Pair), ByVal DN As HashSet(Of Char32), ByVal RN As HashSet(Of Char32), ByVal MN As HashSet(Of Char32), ByVal GetDChar As Func(Of Pair, Char32), ByVal GetRChar As Func(Of Pair, Char32)) As IEnumerable(Of KeyValuePair(Of Char32, Char32))
            Dim Map As New List(Of KeyValuePair(Of Char32, Char32))
            For Each p In Table
                Dim dc = GetDChar(p)
                Dim rc = GetRChar(p)

                '字源异体字中，只做规范字->规范字、非规范字->非规范字的转换，不做规范字->非规范字、非规范字->规范字的转换。
                '且对于非规范字，需要参考在其他字源中，是否一个是规范字，一个不是规范字，如果是，则也不转换。

                '键或值不在IICore精简字符集中
                If Not AllChars(dc).HasField("kIICore") Then
                    'Continue For
                End If
                If Not AllChars(rc).HasField("kIICore") Then
                    'Continue For
                End If

                '键在定义域规范中，值在值域规范中
                If DN.Contains(dc) AndAlso RN.Contains(rc) Then
                    Map.Add(New KeyValuePair(Of Char32, Char32)(dc, rc))
                    Continue For
                End If

                '键不在定义域规范中，值在值域规范中
                If RN.Contains(rc) Then Continue For

                '键在定义域规范中，值不在值域规范中
                If DN.Contains(dc) Then Continue For

                '键不在定义域规范中，值不在值域规范中
                '键在值域规范中
                If RN.Contains(dc) Then Continue For
                '值在定义域规范中
                If DN.Contains(rc) Then Continue For
                '键与值均在参考规范中
                If MN.Contains(dc) AndAlso MN.Contains(rc) Then Continue For

                Map.Add(New KeyValuePair(Of Char32, Char32)(dc, rc))
            Next

            RemoveLoop(Map)
            RemoveSame(Map)
            StableSort(Map)

            Return Map
        End Function

        Public Function GetGTMap() As IEnumerable(Of KeyValuePair(Of Char32, Char32))
            Return GetMap(GTTable, GNorm, TNorm, JNorm, Function(p) p.G, Function(p) p.T)
        End Function

        Public Function GetTGMap() As IEnumerable(Of KeyValuePair(Of Char32, Char32))
            Return GetMap(GTTable, TNorm, GNorm, JNorm, Function(p) p.T, Function(p) p.G)
        End Function

        Public Function GetJTMap() As IEnumerable(Of KeyValuePair(Of Char32, Char32))
            Return GetMap(JTTable, JNorm, TNorm, GNorm, Function(p) p.J, Function(p) p.T)
        End Function

        Public Function GetTJMap() As IEnumerable(Of KeyValuePair(Of Char32, Char32))
            Return GetMap(JTTable, TNorm, JNorm, GNorm, Function(p) p.T, Function(p) p.J)
        End Function

        Public Function GetGJMap() As IEnumerable(Of KeyValuePair(Of Char32, Char32))
            Return GetMap(GJTable, GNorm, JNorm, TNorm, Function(p) p.G, Function(p) p.J)
        End Function

        Public Function GetJGMap() As IEnumerable(Of KeyValuePair(Of Char32, Char32))
            Return GetMap(GJTable, JNorm, GNorm, TNorm, Function(p) p.J, Function(p) p.G)
        End Function

        Public Function GenerateTranslateTables() As String
            Dim l As New List(Of String)

            Dim GTMap = GetGTMap()
            Dim TGMap = GetTGMap()
            l.Add("Private Shared G2T_G As Char32() = " & Quote & String.Join("", (From p In GTMap Select CStr(p.Key)).ToArray) & Quote & ".ToUTF32")
            l.Add("Private Shared G2T_T As Char32() = " & Quote & String.Join("", (From p In GTMap Select CStr(p.Value)).ToArray) & Quote & ".ToUTF32")
            l.Add("Private Shared T2G_T As Char32() = " & Quote & String.Join("", (From p In TGMap Select CStr(p.Key)).ToArray) & Quote & ".ToUTF32")
            l.Add("Private Shared T2G_G As Char32() = " & Quote & String.Join("", (From p In TGMap Select CStr(p.Value)).ToArray) & Quote & ".ToUTF32")
            l.Add("")

            Dim TJMap = GetTJMap()
            Dim JTMap = GetTJMap()
            l.Add("Private Shared T2J_T As Char32() = " & Quote & String.Join("", (From p In TJMap Select CStr(p.Key)).ToArray) & Quote & ".ToUTF32")
            l.Add("Private Shared T2J_J As Char32() = " & Quote & String.Join("", (From p In TJMap Select CStr(p.Value)).ToArray) & Quote & ".ToUTF32")
            l.Add("Private Shared J2T_J As Char32() = " & Quote & String.Join("", (From p In JTMap Select CStr(p.Key)).ToArray) & Quote & ".ToUTF32")
            l.Add("Private Shared J2T_T As Char32() = " & Quote & String.Join("", (From p In JTMap Select CStr(p.Value)).ToArray) & Quote & ".ToUTF32")
            l.Add("")

            Dim GJMap = GetGJMap()
            Dim JGMap = GetJGMap()
            l.Add("Private Shared G2J_G As Char32() = " & Quote & String.Join("", (From p In GJMap Select CStr(p.Key)).ToArray) & Quote & ".ToUTF32")
            l.Add("Private Shared G2J_J As Char32() = " & Quote & String.Join("", (From p In GJMap Select CStr(p.Value)).ToArray) & Quote & ".ToUTF32")
            l.Add("Private Shared J2G_J As Char32() = " & Quote & String.Join("", (From p In JGMap Select CStr(p.Key)).ToArray) & Quote & ".ToUTF32")
            l.Add("Private Shared J2G_G As Char32() = " & Quote & String.Join("", (From p In JGMap Select CStr(p.Value)).ToArray) & Quote & ".ToUTF32")
            l.Add("")

            Return String.Join(CrLf, l.ToArray)
        End Function

        Public Function GetGChars() As IEnumerable(Of Char32)
            Return GChars
        End Function

        Public Function GetTChars() As IEnumerable(Of Char32)
            Return TChars
        End Function

        Public Function GetJChars() As IEnumerable(Of Char32)
            Return JChars
        End Function

        Public Function GetGNorm() As IEnumerable(Of Char32)
            Return GNorm
        End Function

        Public Function GetTNorm() As IEnumerable(Of Char32)
            Return TNorm
        End Function

        Public Function GetJNorm() As IEnumerable(Of Char32)
            Return JNorm
        End Function

        Private Shared Function ParseUnicodes(ByVal Value As String) As IEnumerable(Of Char32)
            Static r As New Regex("U\+(?<Unicode>[0-9A-F]{4,5})", RegexOptions.ExplicitCapture)
            Return From m As Match In r.Matches(Value) Select New Char32(Integer.Parse(m.Result("${Unicode}"), Globalization.NumberStyles.HexNumber))
        End Function

        Private Shared r As New Regex("^(?<Level>[0-9A-Z_]*)-(?<Code>[0-9A-F]{4})$", RegexOptions.ExplicitCapture)
        Private Shared Function GetLevelIdentifier(ByVal c As UniHanChar, ByVal Source As String) As String
            If Not c.HasField(Source) Then Return ""
            Dim LevelCode = c.Field(Source)
            Dim m = r.Match(LevelCode)
            If Not m.Success Then Return ""
            Return r.Match(LevelCode).Result("${Level}")
        End Function
        Private Shared Function GetLevel(ByVal c As UniHanChar, ByVal Source As String) As Integer
            Dim LevelIdentifier = GetLevelIdentifier(c, Source)
            Select Case Source
                Case "kIRG_GSource"
                    Select Case LevelIdentifier
                        Case "0"
                            Return 0
                        Case "7"
                            Return 1
                        Case Else
                            Return Integer.MaxValue
                    End Select
                Case "kIRG_TSource"
                    Select Case LevelIdentifier
                        Case "1"
                            Return 0
                        Case "2"
                            Return 1
                        Case Else
                            Return Integer.MaxValue
                    End Select
                Case "kIRG_JSource"
                    Select Case LevelIdentifier
                        Case "0"
                            Return 0
                        Case "1"
                            Return 1
                        Case Else
                            Return Integer.MaxValue
                    End Select
                Case Else
                    Throw New NotSupportedException
            End Select
        End Function
        Private Shared Function GetCodeDifference(ByVal a As UniHanChar, ByVal b As UniHanChar, ByVal Source As String) As Integer
            Dim la = GetLevel(a, Source)
            Dim lb = GetLevel(b, Source)
            If la > lb Then Return &H10000
            If la < lb Then Return -&H10000

            If Not a.HasField(Source) AndAlso Not b.HasField(Source) Then Return 0

            Dim ca As String = r.Match(a(Source)).Result("${Code}")
            Dim cb As String = r.Match(b(Source)).Result("${Code}")
            Return Integer.Parse(ca, Globalization.NumberStyles.HexNumber) - Integer.Parse(cb, Globalization.NumberStyles.HexNumber)
        End Function

        Public Shared Function IsToLoad(ByVal t As UniHanTriple) As Boolean
            Static h As New HashSet(Of String)(New String() {"kIICore", "kIRG_GSource", "kIRG_TSource", "kIRG_JSource", "kZVariant", "kSimplifiedVariant", "kTraditionalVariant", "kSemanticVariant", "kSpecializedSemanticVariant", "kCompatibilityVariant"})
            Return h.Contains(t.FieldType)
        End Function
    End Class
End Namespace
