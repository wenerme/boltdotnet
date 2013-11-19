boltdotnet
==========

mirror for https://boltdotnet.codeplex.com/

官方 [BOLT_SDK](https://github.com/lurenpluto/BOLT_SDK)

说明
----

bolt.net is a .net wrapper 4 bolt ui library(http://bolt.xunlei.com)

BOLT.NET 封装了BOLT UI库与C# 交互的常用API  
BOLT.NET 由千寻动漫团队(http://manhuahe.net)开发并维护。


从2012年11月开始，[千寻动漫](http://manhuahe.net)团队开始开发千寻漫画盒2.0版本，我们选择了迅雷的BOLT界面引擎开发新版界面。由于我们项目一开始主要使用.NET平台上的C#语言开发，所以直接选择了在.NET平台上使用BOLT引擎开发新版界面。迅雷BOLT SDK里有一个简单版本的dotNetBoltDemo，我们从这个简单的例子出发，在开发过程中按需封装和改进，逐渐形成了比较稳定的一个封装类库。考虑到很多开发者同样需要使用.NET平台开发BOLT界面，本着分享和避免重复发明轮子的精神，我们将BOLT.NET类库项目开源出来，希望大家一起改进它，以提供一个在.NET平台上使用简单、稳定高效、接口优雅的BOLT封装，为在.NET上通过BOLT开发产品界面提供有品质的保证。


更多说明见文档：https://boltdotnet.codeplex.com/documentation  
论坛讨论:http://bolt.xunlei.com/bbs/forum.php?mod=forumdisplay&fid=43&page=1

版本历史
--------

* ★v1.0.1.0
* ◇更新时间:20130722
* ◇更新内容：
* →XLBolt的Run方法添加了可选参数，用来指定是否初始化XGP
* →XLBolt类添加AddMesssageFilter/RemoveMessageFilter
* →LuaExtension添加PushBitmap、GetBitmap扩展
* →LuaExtension添加GetTuple扩展方法，支持从Lua端返回元素个数不超过6个的table
* →改进了后台窗口的实现代码
* →将P/Invoke代码按分类分离成Win32.cs、XLGraphics.cs、XLLuaRuntime.cs、XLUE.cs
* →小幅改进了下代码质量

* ★v1.0.1.1
* ◇更新时间:20130802
* ◇更新内容：
* →重构LuaBaseClass，使得注册工厂、单例给Bolt环境的代码更清晰。
* →重构LuaBaseCoClass，使得注册CoClass给Bolt环境的代码更清晰。
* →重构LuaFunc、LuaAction，使得代码更精简
* →重构LuaClassAttribute、LuaClassMemberAtttributge，使得代码更精简，使用更安全
* →使用SortedList代替Dictionay存储Lua函数存根
* →重构变量命名，代码风格。
