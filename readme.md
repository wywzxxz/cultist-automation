这是一个邪教模拟器的自动化mod。不同于以前的mod，这个模式的工作自动无需人工干预，并且可以在solution(方块)行动的完整生命周期中记录每个阶段的插槽选择。这意味着它可以用来购买书籍、拍卖或使用多个理智进行文书工作。。



(当打开一个solution时)按F1键：学习这个solution当前的工作直到其结束，然后自动尝试重现它。



(当打开一个solution时)按F2键：取消这个solution的自动化。




(当打开一个solution时)按F3键：跳过这个solution的下一轮。用来暂停日常工作以处理一些紧急问题，如疾病、绝望。



# 安装



您必须安装https://github.com/BepInEx/BepInEx/releases/tag/v5.0 。安装后，必须关闭harmony支持mod才能正常工作。为此，编辑Cultist Simulator/BepInEx/config/BepInEx.cfg并将如下几行附加到文件尾部。


>[Preloader]

>ApplyRuntimePatches=false



如果配置文件不存在，您可能需要在安装BEPIPEX之后运行一次游戏。如果这对您不起作用，您可能安装了错误版本的bepinex（应该是X86）


然后将https://github.com/wywzxxz/cultist-automation/releases 中下载dll放入Cultist Simulator/BepInEx/plugins目录下

-------------------------------


This is an automaiton mod for cultist simulation, unlike predecessors, this mod works automatically require no manual interfere, and will learn full lifecircle of an specific solution action, record slot choise for each state. Which means it can be used to purchase book or work at Glover & Glover (using double  Passion) .

press F1 with an open solution: learn current unstart & ongoing action until it ends. This (try to) automated reproduce it.

press F2 with an open solution: cancel automation for this solution.

press F3 with an open solution: Skip next turn of this solution. Designed for temporary pause routine work to deal emergency issues, such as illness, despair

#Installation

You must install https://github.com/BepInEx/BepInEx/releases/tag/v5.0 . Once you installed, harmony support must be turned off or no plugins willbe load. To do this, edit Cultist Simulator/BepInEx/config/BepInEx.cfg  and append it to the end.

>[Preloader]
>ApplyRuntimePatches = false

if config file not exists, you may need to run game once after bepinex installed. If that didn't working for you , you may installed wrong version of bepinex ( should be X86).


-----------------------

inspired by https://github.com/RoboPhred/cultist-recipe-hotkeys, which basicly solved all game API relatived issues.  
