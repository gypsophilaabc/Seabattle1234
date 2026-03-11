SeaBattle

SeaBattle 是一个基于 Unity 开发的双人回合制海战策略游戏。
玩家通过规划攻击行动，在回合结束时同步结算攻击结果，逐步摧毁对方舰队以取得胜利。

本项目旨在实现一个具有清晰结构与完整 UI 交互的海战策略游戏原型，包括舰船部署、攻击规划、同步结算以及动态战场界面。

Project Overview

SeaBattle 的核心玩法围绕 回合规划与同步结算机制 展开。

每一回合包含两个规划阶段和一个结算阶段：

Player 0 Planning Phase
玩家 0 在敌方海域上规划攻击行动。

Player 1 Planning Phase
玩家 1 在玩家 0 的海域上规划攻击行动。

Resolving Phase
双方的攻击同时结算，系统更新战场状态。

玩家通过不同类型的武器在敌方棋盘上布置攻击，命中敌方舰船并最终击败对手。

Game Features

双人回合制海战对战

同步攻击结算机制

多种攻击类型（Gun / Torpedo / Bomb / Scout）

可视化攻击规划系统

动态棋盘 UI（当前攻击棋盘放大显示）

攻击资源管理系统

撤销与清空攻击规划

回合阶段提示与玩家状态显示

User Interface Layout

游戏界面主要由三个部分组成：

Battlefield

中央区域包含两个海域棋盘：

当前玩家攻击目标的棋盘（高亮并放大）

另一名玩家的棋盘（缩小并变暗）

棋盘由网格构成，玩家通过点击网格选择攻击目标。

Attack List

左侧面板显示当前回合武器使用情况：

Gun: X / N
Torpedo: X / N
Bomb: X / N
Scout: X / N

其中：

X 为当前已规划攻击次数

N 为本回合最大可使用次数

若存在未使用的武器，系统会提示：

Unused weapons remain
Control Panel

右侧面板提供主要操作按钮：

Confirm
确认当前攻击规划并结束规划阶段。

Undo
撤销最近一次攻击规划。

Clear
清空当前回合的攻击计划。

在回合结算阶段，双方玩家需要点击 Ready 按钮进入下一回合。

Controls
操作	功能
鼠标点击	选择攻击位置
1–4	选择武器类型
W / A / S / D	调整鱼雷攻击方向
Confirm	确认当前攻击规划
Undo	撤销最近攻击
Clear	清空攻击计划
Project Structure

主要系统模块如下：

BattleController
负责攻击规划、武器使用与攻击逻辑

BattleFlowController
负责回合阶段切换与游戏流程控制

BattleLayoutController
控制棋盘 UI 的缩放与动画

BattleHUDController
管理界面文本与按钮状态

Placement System
负责游戏开始时的舰船部署
Development Environment

Engine: Unity 2022

Language: C#

UI System: Unity UI + TextMeshPro

Version Control: Git / GitHub

Future Improvements

完整舰船伤害系统

更丰富的攻击类型

AI 对手

游戏结束界面

动画与视觉效果优化

网络对战支持
