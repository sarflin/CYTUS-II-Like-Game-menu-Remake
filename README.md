# Cytus2-Like Game Menu Remake

> 基于 **Unity + Wwise** 的音乐游戏菜单交互系统 —— 从音频系统构建到全流程质量保障的个人项目。
> An interactive-audio menu system demo (character select → song select → settings) built with Unity + Wwise.

![Unity](https://img.shields.io/badge/Unity-2022.3-000000) ![Wwise](https://img.shields.io/badge/Wwise-2025.1-1E90FF) ![C%23](https://img.shields.io/badge/C%23-239120)

---

## 项目简介

复刻音乐游戏《Cytus II》的主菜单 → 角色选择 → 曲目选择 → 设置流程，重点实现「**音乐随界面切换**」的交互式音频系统，并打通 Unity ↔ Wwise 音频中间件的完整管线。项目同时沉淀了一套编辑器自动化工具链与完整的测试/缺陷文档。

---

## 技术栈

| 类别 | 技术 |
|---|---|
| 引擎 | Unity 2022.3 LTS（UGUI + TextMeshPro） |
| 音频中间件 | Audiokinetic Wwise 2025.1 |
| 语言 | C# |
| 数据驱动 | ScriptableObject |
| 编辑器 | 自定义 EditorWindow / MenuItem 工具链 |
| 音频制作 | Reaper、FL Studio、Wwise 101/201/301 |

---

## 系统架构

### 音频（Wwise）

```
Main Audio Bus (Master → System)
├── Music Bus   ← RTPC: Music_Volume (0–100)
└── SFX Bus     ← RTPC: SFX_Volume  (0–100)

MenuMusic (Music Switch Container, Switch Group = Menu_Music)
├── MainMenu      (Playlist, Loop = Infinite)
├── CharacterSelect (嵌套 Music Switch Container, Switch Group = Character_Theme)
│   └── paff / neko / Cherry / ... / Non (各角色 Playlist, Loop = Infinite)
├── SongSelect    (Playlist)
└── InGame        (Playlist)

SoundBank: Init / Music / SongPreviews / UI
```

### 逻辑（Unity / C#）

```
MenuManager（界面状态机 + 转场调度）
├── MainMenuUI / CharacterSelectUI / SongSelectUI / SettingsUI
├── AudioSystemManager（Bank 加载 / 音乐状态 / 角色主题 / 预览 / UI 音效）
├── WwiseIntegration（Wwise API 封装：PostEvent / SetRTPC / SetSwitch / LoadBank）
└── ScreenTransition（遮罩 + 圆形进度条转场）

数据层：CharacterData / SongData（ScriptableObject）
编辑器工具：MenuSetupHelper / SampleDataCreator / PreviewLinker / DataEditorWindow / AudioSceneSetup
```

---

## 核心功能

- **交互式音乐**：主菜单 / 选角 / 选歌 / 游戏 四状态音乐随界面无缝切换与无限循环；选角界面按角色切换主题曲。
- **UI 音效反馈**：悬停 / 点击 / 返回 / 确认四类音效统一派发。
- **歌曲预览**：优先走 Wwise 事件并用 PlayingID 精确停播，事件缺失时回退 AudioSource；进入选歌界面即播放上次选中歌曲。
- **音量设置**：音乐 / 音效两个 0–100 滑块，通过 Wwise RTPC 实时控制，PlayerPrefs 持久化。
- **界面转场**：圆形进度条读条 + 界面淡入，音频在「退出 / 到达」两个时机精确衔接。
- **窗口控制**：全屏 / 窗口化切换，窗口化支持拖动边框调整大小。

---

## 技术亮点

1. **Wwise ↔ Unity 全链路集成**：封装 `AkUnitySoundEngine` 的 PostEvent / SetSwitch / SetRTPC / LoadBank，隔离业务层与音频引擎。
2. **跨层问题定位**：定位并解决「总线层级错误导致运行时全静音」「RTPC 作用域不生效」「GameSync 命名不一致导致静默失效」「转场时序导致预览被误停」等跨 Wwise/Unity 的疑难问题（详见 `Portfolio/缺陷记录.txt`）。
3. **编辑器自动化工具链**：`PreviewLinker` 批量解析音频 → 自动生成并关联数据资产、匹配事件/封面、清理冗余，把人工配置流程工具化。
4. **数据驱动设计**：角色 / 曲目 / 音频事件全部由 ScriptableObject 与 Inspector 配置，界面自动从 `Resources/` 加载。

---

## 仓库说明

为保证仓库洁净并尊重第三方版权，以下内容**不纳入仓库**（`.gitignore` 已配置）：

- `Assets/Wwise/`：Audiokinetic Wwise Unity Integration（按机器安装，克隆后需自行集成 Wwise 2025.1）；
- `Assets/SongsPreview/`、`Assets/StreamingAssets/**/*.bnk`、Wwise `Originals/`：音频素材与生成的 SoundBank（体积大且含版权音乐）；
- `Assets/Art/`：角色立绘、曲绘等第三方版权美术素材；
- `Library/ Temp/ obj/ Build/` 等引擎生成目录；字体文件；打包产物与演示视频。

> 因此克隆后需自行：集成 Wwise Unity Integration → 导入音频素材 → 在 Wwise 中 Generate SoundBanks → 同步到 `StreamingAssets`，方可运行。

---

## 如何运行

1. 用 **Unity 2022.3 LTS** 打开工程（需先安装并集成 **Wwise 2025.1** Unity Integration）。
2. 打开 `Assets/Scenes/SampleScene.unity`。
3. 若菜单未就绪，执行编辑器菜单：`Cytus2 > Create Menu System`、`Cytus2 > Create Sample Data`、`Cytus2 > Audio > Setup Audio Scene`、`Cytus2 > Audio > Link Song Previews`。
4. 运行体验；打包：`File > Build Settings > Build`。

---

## 项目文档

| 文档 | 内容 |
|---|---|
| [项目说明](Portfolio/项目说明.txt) | 架构、技术栈、功能与运行说明 |
| [缺陷记录](Portfolio/缺陷记录.txt) | 9 类缺陷的现象 / 根因 / 修复 / 验证 |
| [测试用例](Portfolio/测试用例.txt) | 功能 / 表现 / 性能 / 兼容测试用例 |
| [音频测试技术文档](Portfolio/音频测试技术文档.md) | 以音频测试工程师视角的技术拆解与测试策略 |
| [音乐游戏菜单音频策划案](Portfolio/音乐游戏菜单音频策划案.md) | 音频方案与交互设计 |

---

## 声明

本项目为个人学习与求职作品（非商业），复刻自《Cytus II》的菜单交互体验。仓库中的音乐、角色美术等素材版权归原作者 / 版权方所有，仅用于学习交流；如涉及侵权请联系删除。

## 联系方式

- 简历 / 作品集：见投递材料
- GitHub：[@sarflin](https://github.com/sarflin)
