# Cytus 2 菜单互动音频 - Wwise 配置指南

菜单系统已接入 Wwise（`WwiseIntegration.cs` / `AudioSystemManager.cs`），
本文说明如何在 Wwise Authoring（2025.1）中创建音频结构、事件、互动音乐与 SoundBank，
并在 Unity 中完成对应配置。

> **版本注意**：2025.1 已将 Actor-Mixer Hierarchy 与 Interactive Music Hierarchy
> 合并为 **Containers** 单一层级。文中所有"Containers"即旧版的 Actor-Mixer Hierarchy。
>
> **关键规则（旧版到 2025.1 都成立）**：**Music Track 只能创建在 Music Segment 内部**，
> 它不会出现在 Music Switch Container 或文件夹的 New Child 菜单里。
> 每个分支的正确结构是 **Music Switch Container → Music Segment → Music Track → 音频**。
> 若你在某容器上右键看不到 "Music Track"，说明你不是在 Music Segment 上右键。
>
> **另一条关键规则**：**Music Clip 不通过 Track 的 Contents Editor 创建**，而是出现在
> **Music Segment Editor**（音乐时间轴）里。给 Track 添加音频的推荐做法是选中 Track 后用
> **Project > Import Audio Files** 导入 wav（见 4.4 第 3 步），Wwise 会自动建出 Clip。
>
> **第三关键规则（循环）**：**Music Segment 本身没有循环开关，播完 Entry→Exit 一遍就停**。
> 想让某段音乐无限循环，必须把它放进 **Music Playlist Container**，并把这个容器的
> **Loop Count 设为 Infinite（无限）**。官方 WAAPI 示例就是
> `MusicSwitchContainer → MusicPlaylistContainer(@LoopCount=0) → MusicSegment → MusicTrack`。
> 所以凡是需要"一直循环"的分支，正确层级是：
> **Music Switch Container → Music Playlist Container（Loop Count=无限）→ Music Segment → Music Track**。

> **当前进度（写于互动音乐结构验收后）**：
> - ✅ 已完成并验证：互动音乐层级（MenuMusic + 内层 CharacterSelect）、
>   每角色"一首一个 Playlist"（Loop Count = Infinite）、切角色无缝转场、无限循环。
>   结构对应 4.6 的树；排查清单见 4.5.2 / 4.4.3。
> - ⚠️ 尚未创建的事件（**下一步先在 Wwise 里补**，见第 5 节）：`Play_MenuMusic`
>   和 5 个 UI 事件 `UI_Click` `UI_Hover` `UI_Back` `Character_Confirm` `Song_Select`
>   （工程里目前只有 24 个 `Play_Preview_*`）。
> - ▶️ 之后的 Wwise 操作就是**生成 SoundBank**（第 7 节）→ 再回 Unity 验证（第 8~10 节）。

## 1. Wwise 工程现状

- Wwise 工程：`E:\cytus2menuremake\cytus2menuremake_WwiseProject\cytus2menuremake_WwiseProject.wproj`
- Wwise 只支持导入 WAV/AMB（**不支持 MP3**）。所有音频源必须是真 WAV（RIFF 文件头）。
- Originals 目录规划：
  - `Originals\SFX\SongPreviews\` — 24 个歌曲预览（已就位）
  - `Originals\Music\mainmenu.wav` — 主菜单 BGM
  - `Originals\Music\CharacterThemes\` — 8 个角色主题曲
  - `Originals\UI\` — 5 个 UI 音效

## 2. 导入音频

在 Wwise 的 **Containers > Default Work Unit** 下逐类导入：

1. **UI 音效**：右键 Default Work Unit → New Child > **Folder**，命名 `UI`
   → 选中 `UI` 文件夹右键 → **Import Audio Files** → Add Files
   → 从 `Originals\UI\` 选 5 个 wav → Import
   → `UI` 下生成 5 个 Sound SFX（`UI_Click` `UI_Hover` `UI_Back` `Character_Confirm` `Song_Select`）
2. **角色主题曲**：新建文件夹 `CharacterThemes` → 导入 `Originals\Music\CharacterThemes\` 的 8 个 wav
   → 生成 `Cherry` `ConneR` `NaiLong` `neko` `paff` `Robo_Head` `sarflin` `Xenon` 8 个 Sound
3. **歌曲预览**：`SongPreviews` 文件夹（已导入 24 个 Sound）

> 命名提醒：Wwise 对象/Switch 名必须是合法标识符（无空格、无特殊字符）。
> 角色 `Nai Long` 在 Wwise 中命名 **NaiLong**（不含空格）。
> 其余角色名已合法：Cherry / ConneR / neko / paff / Robo_Head / sarflin / Xenon。

## 3. Switch Groups（Game Syncs）

在 **Project Explorer > Game Syncs > Switch Groups > Default Work Unit** 下创建两个 Switch Group：

**Switch Group A：`Menu_Music`**（界面层级 BGM 切换）
```
Menu_Music
├── MainMenu
├── CharacterSelect
├── SongSelect
└── InGame
```

**Switch Group B：`Character_Theme`**（角色选择界面按角色切主题曲）
```
Character_Theme
├── Cherry
├── ConneR
├── NaiLong
├── neko
├── paff
├── Robo_Head
├── sarflin
├── Xenon
└── None          ← 离开角色选择界面时使用（切回静音/主菜单曲）
```

**2025.1 具体操作（一步不差）：**

创建 Switch Group：
1. 左侧 **Project Explorer** 点选 **Game Syncs** 标签（不是 Audio 标签）
2. 展开 **Switch Groups > Default Work Unit**
3. 右键 `Default Work Unit` → **New Group**（弹出输入框）→ 命名 `Menu_Music` → 回车
4. 右键 `Default Work Unit` → **New Group** → 命名 `Character_Theme` → 回车

给 `Menu_Music` 加 4 个值：
5. 右键 `Menu_Music` → **New Switch** → 命名 `MainMenu` → 回车内联改名；同法建 `CharacterSelect` `SongSelect` `InGame`
   - 若弹出的是属性面板而不是内联输入，直接改顶部 **Name** 字段后回车即可

给 `Character_Theme` 加 9 个值：
6. 右键 `Character_Theme` → **New Switch** → 依次建 `Cherry` `ConneR` `NaiLong` `neko` `paff` `Robo_Head` `sarflin` `Xenon` `None`
   - **注意 `NaiLong` 无空格、`Robo_Head` 有下划线**，必须与 Unity 端一致

> Switch 名必须与 Unity `CharacterData.wwiseSwitchName` 一致（见第 6 节表）。
> 其中 NaiLong 与 Robo_Head 是唯一用覆盖名的角色。

### 3.1 ⚠️ 只建了 MusicPlaylistContainer 没建 Segment？先看这里

若你在 `MenuMusic` 下右键时看到的是 **Music Playlist Container**（或文件夹里只看到 PlaylistContainer），
说明建错了层级对象。Music Switch Container 的子项应为 **Music Segment**（音乐段落）：
Music Playlist Container 是"顺序/随机播放播放列表"用的，本方案不需要，**请右键按需删除**
（Build: 在容器上右键 → Delete 或直接键盘 Delete 键）后用 Music Segment 重建（见 4.4）。

## 4. 互动音乐层级（Containers）详细步骤

这是核心结构。目标是：主菜单放 mainmenu；角色选择界面放角色主题曲（随角色切换）；
选曲界面、游戏中放其它 BGM。全部由 `Play_MenuMusic` 一个事件驱动。

> **如果你已经建错了（很可能）**：当前工程里 `MenuMusic` 下已有 4 个 **Music Playlist Container**
> （`MainMenu` `ChraracterSelect`⚠️ `SongSelect` `InGame`），这是之前按旧步骤建的**无效结构**：
> 不能用、且 `ChraracterSelect` 还拼错了。做法：
> 1. 逐一右键这 4 个 → **Delete**（保留 `MenuMusic` 容器本身）
> 2. 从下方 4.3 开始用 **Music Segment + Music Track** 重建

最终层级目标（这里是你要建成的样子）：
```
Containers
└── Default Work Unit
    ├── UI              （文件夹，5 个 Sound，步骤 2 已建）
    ├── CharacterThemes （文件夹，8 个 Sound，步骤 2 已建）
    ├── SongPreviews    （文件夹，24 个 Sound，已建）
    ├── mainmenu_Sound  （Sound SFX → mainmenu.wav）
    └── MenuMusic       （Music Switch Container）★ 本步骤重点
```

### 4.1 导入 mainmenu 并建一个普通 Sound

1. 右键 `Default Work Unit` → New Child → **Sound SFX**，命名 `mainmenu_Sound`
2. 在下方 **Contents Editor**（或双击该 Sound）→ **Add Source** → 选 `mainmenu.wav`
   （或直接右键文件夹 Import Audio Files 导入 mainmenu.wav，自动生成 Sound）
3. 选中 `mainmenu_Sound`，右侧 **Sound Property Editor**：
   - Loop: 勾选 **Loop**（背景音乐循环播放）
   - Output Bus: Master Audio Bus

### 4.2 新建 Music Switch Container

1. 右键 `Default Work Unit` → New Child → **Music Switch Container**，命名 `MenuMusic`
   - 一个 Music Switch Container 的作用：**根据某个 Switch Group 的当前值，播放对应子项**
   - 这里我们用两个层级：外层看 `Menu_Music`（选界面），内层角色选择分支再看 `Character_Theme`（选角色）

### 4.3 在外层 Switch Container 上绑定 `Menu_Music`

1. 选中 `MenuMusic`
2. 右侧 **Music Switch Container Property Editor** 中找到 **Switch Group** 属性
   → 点击旁边的 **>>**（Pick）按钮 → 在弹窗里展开 **Game Syncs > Switch Groups**
   → 选 `Menu_Music` → 确定
3. （2025.1）绑定成功后，容器属性区会显示选中的 Switch Group 名。
   若该字段是空的/灰色，说明 Switch Group 还没建（回第 3 步建好再回来选）。
4. 现在 `MenuMusic` 会因为 `Menu_Music` 的不同值播放不同子项；
   每个子项在编辑器里会显示它是哪个 Switch 值的"路由"（名称匹配自动分配）。
5. 怎么确认子项绑到了正确的 Switch 值：选中 `MenuMusic`，点下方
   **Contents Editor/Assigned Objects** 视图，看到每个 Music Segment 旁标着
   `MainMenu`/`CharacterSelect`/`SongSelect`/`InGame` 之一即可；
   若标错，可在该视图里把 Segment 拖到正确的 Switch 值上（或右键重命名对齐）。

> 经验：Wwise 会按**子项名 = Switch 值名**自动分配，所以 Segment 名必须与
> `Menu_Music` 的值一字不差（大小写、空格都要一致）才最省事。

### 4.4 创建外层 4 个子项（对应 Menu_Music 的 4 个值）

在 `MenuMusic` 上右键 → 新建 **4 个 Music Segment**（不是 Music Track！），
每个 Segment 名字与 Menu_Music 的值一致。Music Track 要在**每个 Segment 内部**再建：

```
MenuMusic (Music Switch Container, Switch Group = Menu_Music)
├── MainMenu       ← Music Segment（内部含 Music Track）
├── CharacterSelect ← 这一项最后要变成内层容器（见 4.5）
├── SongSelect     ← Music Segment（内部含 Music Track）
└── InGame         ← Music Segment（内部含 Music Track）
```

以 MainMenu 为例，完整建法（其余 3 项同法）：

1. 右键 `MenuMusic` → New Child → **Music Segment**，命名 `MainMenu`
   - 若右键菜单里看不到 **Music Segment**，说明你选的父对象不对（应右键 Music Switch Container）。
     2025.1 的 Music Switch Container 子项右键菜单会出现 Music Segment；
     若反而出现 **Music Playlist Container** 且能建，那是另一类对象，本方案不用它（建完需删掉）。
2. 右键 `MainMenu`（此时才出现）→ New Child → **Music Track**，命名 `MainMenu_Track`
   - Music Track 是 Segment 专属子对象，所以只能在 Segment 上右键才见得到
3. 给 Track 放上音频（用 **Audio File Importer**，不要再在 Contents Editor 里找 "New Clip"）：
   - 选中 `MainMenu_Track` → 菜单 **Project > Import Audio Files…**
     （或右键 `MainMenu_Track` → **Import Audio Files**）→ **Add Files**
     → 选 `Originals\Music\mainmenu.wav` → **Import**
   - Wwise 会在 **Music Segment Editor（音乐时间轴）** 里于该 Track 行上自动生成一个
     **Music Clip**（矩形音频块）。选中 `MainMenu` Segment 时，下方/中央出现的就是
     **Music Segment Editor**；Clip 出现在轨道那一条横向区域里。
   - Music Clip 直接引用 wav 源，**不需要** mainmenu_Sound 这个普通 Sound。
     已建了也可用：把 `mainmenu_Sound` 从 Project Explorer **拖进** Segment Editor
     的轨道区，同样自动生成 Clip。
   - 循环：把 Clip 右侧白色把手向外拖可拉出循环段（对应源文件循环区域）；音乐是否
     无限循环由 **Music Playlist Container 的 Loop Count** 决定（见 4.4.2，Segment 无 Loop 开关）。
   - ⚠️ 之前"Contents Editor 右键 → New Clip > Music Clip"的写法在 2025.1 走不通——
     **Clip 属于 Music Segment Editor 时间轴，不是 Track 的 Contents Editor**。
     若右键 Track 的 Contents Editor 后没有 New Clip 菜单，属正常现象，用上面的导入法即可。
4. 循环：**Music Segment 没有循环开关**（播完一遍会停）。要让 `MainMenu` 无限循环，
   需要把它包进一个 **Music Playlist Container** 并设 **Loop Count = Infinite**
   （见下方 4.4.2，先把整棵结构搭完再统一包）。
5. 确保该 Segment 属于 `Menu_Music` 的 `MainMenu` 值：
   - 若 Wwise 已自动按名字匹配则无需操作；
   - 否则在 `MenuMusic` 容器的 Switch 分配视图里确认每个 Segment 绑定的值正确（见 4.3 第 5 步）。

`SongSelect` 和 `InGame` 暂时可先留空或复用 mainmenu，后续再换正式 BGM。

### 4.4.1 为什么播放只响一点 / 时间轴上没波形？

这是 Wwise 互动音乐的常见现象，原因基本是下面两种之一（先判断属于哪种）：

> 播放范围 = **Entry Cue（绿标）到 Exit Cue（红标）**。用 Transport 试播 Segment 时，
> 默认只播 Entry→Exit 这段。新建的 Segment 默认长度很短（可能就 1~4 小节），
> 即使导入的音频是整首歌，只要 Exit Cue 停在开头附近，你就只能听到开头那一小点。

**判断方法（在 Music Segment Editor 里）：**

1. 选中 `MainMenu` Segment → 时间轴视图出现（若没有，菜单 **Layouts > Music** 切到音乐布局）。
2. 在时间轴空白处**右键 → 选 `Seconds`**（或 `Bars/Beats`），再**滚轮缩放到合适大小**
   （或右键选择 **Zoom to Fit / Fit**）看整段范围。
3. 找到 Track 上的音频块（Music Clip）：
   - **Clip 块本身只有一小段** → 是 Clip 太短：选中它，把**右侧白色把手向外拖**，
     拉到与整首歌一样长（向外拖是"重复/循环"该音频）。或**双击 Clip** 打开其 Source，
     重设 Trim/Loop 区域 = 整首歌。
   - **Clip 块已覆盖整首歌，但播放仍只响开头** → 是播放范围问题：把时间轴上的
     **Exit Cue（红标）向右拖到 Clip 结尾**，或直接把 Segment 的 **Length** 拉长
     （也可在 **Music Segment Property Editor** 里改 Length）。
4. 波形显示：Clip 块里应能看到波形（官方文档：Clip 会以波形显示在时间轴上）。
   若整首歌都对但块里是空白 → 可能还没转换音频，运行 **Project > Convert All Audio Files**
   或按 **F7** 生成缓存后再看。
5. 确认无误后点 Transport 的 **Play** 试听；若仍只想听"正式区域"，可切换 Transport 上的
   **播放范围模式**（play segment / play entry-exit 区域）。

> 我们最终需要的是**整段循环播放**：Exit Cue 覆盖整首歌 + 把 Segment 包进
> **Music Playlist Container（Loop Count = Infinite）**（见 4.4.2）。之后由 Switch 负责无缝换曲，
> 循环由 Playlist 提供。

### 4.4.2 让每段音乐无限循环（Music Playlist Container）

Wwise 中**循环的位置在 Music Playlist Container，不在 Segment**：
`Music Switch Container → Music Playlist Container → Music Segment → Music Track → 音频`。
直接挂在 Switch Container 下的裸 Segment **只播一遍就停**——这就是"角色主题曲播完不循环"的原因。

> ⚠️ **禁止 Playlist 套 Playlist**：不要在 Music Playlist Container 里**再放一个**
> Music Playlist Container。每角色只需要**一个** Playlist 直接挂在 Switch Container 下，
> 里面只放它自己的 Segment。两层的坏处：每一层的条目/分组都有独立 Loop Count，默认有限，
> 外层播完一轮后整条分支就结束——表现为"切歌正常，但循环失效 / 又变回只能切一次"。
> 把 Segment 轨道拉长来"伪造循环"只是临时骗过耳朵，不是真正的循环，别依赖它。

**把现有 Segment 包进 Playlist（每支需要循环的曲子都这样做）：**

1. 右键选中 `MenuMusic`（或内层 `CharacterSelect`）→ **New Child → Music Playlist Container**，
   命名为原来的分支名（如 `MainMenu`、`Cherry`……），保证 Switch 自动匹配不变。
2. 在 Project Explorer 里把对应 **Music Segment 拖进这个 Playlist Container**（Segment 成为其 Playlist Item）。
3. 选中该 Playlist Container → 下方出现 **Music Playlist Editor** 视图（若没有：
   **Layouts > Music**）。里面有一行/一列是 **Loop Count（循环次数）**。
4. 把该行的 **Loop Count 设为 Infinite（无限，0 = 无限）**：
   点击该单元格或在行上右键 → 把 Loop/Repeat 改成 Infinite。
5. 重复 1~4 处理所有分支（MainMenu、SongSelect、CharacterSelect 的 9 个角色……）。
6. 确认 Switch 分配仍正确：Switch Container 的分配视图里，每个 Switch 值现在应指向
   **Playlist Container**（名字一致时通常自动迁移，否则手动把 Playlist 拖到对应值上）。

> 试听验证：Transport 点 Play，主题曲应播完一遍后**自动从头循环**；切 Switch 仍按
> 4.5.1 的 Next Bar/Next Beat 转场同步。若 Playlist Editor 里没有 Loop Count 列，
> 右键该行 → **Local/Total Duration** 附近找 **Repeat/Loop** 相关选项，或查看该
> Playlist Container 的 Property Editor 里的 Loop 区域。

### 4.4.3 为什么"切歌好了循环又没了"？——Playlist 要"一首一个"，不能共用一个大 Playlist

> **结构铁律**：Music Switch Container 里**每个 Switch 值各自管一个 Music Playlist Container**，
> 每个 Playlist 里**只有一首**（一个 Segment，Loop Count = Infinite）。这是 Wwise-201 的官方做法
> （Boss/Combat/... 六个分支各是独立 Playlist）。这样"循环"由各自的 Playlist 承担，
> "切歌"由 Switch 的转场承担，两者互不干扰。
>
> **反例（会造成刚切歌正常、随后循环失效）**：`Switch Container → 单个大 Playlist → 全部 Segment`。
> 值无法映射到具体角色，切换只是转场对象"跳转"碰巧生效，循环语义也完全不对——一定要拆开。

让现有 9 个 Segment 改成"每角色一个 Playlist"的最小改动（无需重建工程）：
1. 选中现在那个大 Playlist Container → 把它的 9 个 Music Segment 逐个 **剪切**。
2. 在 `CharacterSelect` Switch 容器上右键 → New Child → **Music Playlist Container**，
   命名与原 Segment 同名（Cherry、ConneR……），把 Segment 粘贴进去；**9 个全建完后删掉旧的大 Playlist**。
3. **关键——给每个 Playlist 加"播放项（Item）"**：Playlist 的**层级子对象** ≠ **播放列表项**。
   只把 Segment 作为 Playlist 的 Child 放进层级，Playlist 里其实是**空的播放列表**，指向它自然没声音。
   选中该角色 Playlist → 打开 **Music Playlist Editor**（Layouts > Music）→ 在播放列表树里
   **右键根分组 → New Item → 选对应 Segment**（或直接把 Segment 从层级拖进播放列表树）。
   确认编辑器里出现一行/一个 Item 后再继续。
4. 把该 Item（以及 Group）的 **Loop Count 都设 Infinite（0 = 无限）**；Playlist Container 属性里
   **播放模式 = Continuous**（不要 Random/Step 且有限）。
5. 每个 Segment 的 **Exit Cue 拉到歌曲结尾**（见 4.4.1），循环点正确后**不需要**靠"把 Track 拖长"来伪装循环。
6. Switch 分配视图：每个 `Character_Theme` 值指向**同名 Playlist**（4.5.2 第 3 条）。
7. 转场默认行（Source 空 + Destination 空）：**Exit Source = Next Bar**（或 Immediate），
   Destination = **从头播放（Start Entrance）**，不要选 post-exit，否则会跳过循环点。
   **验证顺序**：先单独选中某个角色 Playlist 直接 Transport Play → 应无限循环；
   再把 Switch 分配全指回 Playlist，反复切值测试。

> 为什么不能靠"拖长 Segment 轨道"来伪造循环：单个 Segment 本身没有 Loop 开关，轨道再长也只播一遍；
> 循环的承担者必须是"一首一个 Playlist"里的 Loop Count = Infinite。

### 4.5 把 CharacterSelect 项改成内层 Switch Container（角色主题曲）

外层 `CharacterSelect` 必须能随**角色**再切歌，所以它不是普通 Segment，而是一个内层容器（容器可以嵌套）：

1. 在 `MenuMusic` 上右键 → New Child → **Music Switch Container**，命名 `CharacterSelect`
2. 把刚才建的外层 `CharacterSelect` Music Segment 删掉（或直接不建，用这个容器代替）
3. 选中这个内层 `CharacterSelect` 容器 → Switch Group 属性 → 选 **`Character_Theme`**
4. 在它下面新建 **9 个 Music Segment**，一角色一个 + None；在每个 Segment 内再建 Music Track 并放入对应角色的 Sound
   （主题曲要循环的话，最后按 **4.4.2** 给每支曲子包一层 Music Playlist Container 并设 Loop Count = Infinite）：
```
CharacterSelect (Music Switch Container, Switch Group = Character_Theme)
├── Cherry   （Music Segment → Music Track → Sound "Cherry"）   （绑定 Switch: Cherry）
├── ConneR   （Music Segment → Music Track → Sound "ConneR"）   （绑定 Switch: ConneR）
├── NaiLong  （Music Segment → Music Track → Sound "NaiLong"）  （绑定 Switch: NaiLong）
├── neko     （Music Segment → Music Track → Sound "neko"）     （绑定 Switch: neko）
├── paff     （Music Segment → Music Track → Sound "paff"）     （绑定 Switch: paff）
├── Robo_Head（Music Segment → Music Track → Sound "Robo_Head"）（绑定 Switch: Robo_Head）
├── sarflin  （Music Segment → Music Track → Sound "sarflin"）  （绑定 Switch: sarflin）
├── Xenon    （Music Segment → Music Track → Sound "Xenon"）    （绑定 Switch: Xenon）
└── None     （Music Segment → Music Track → 留空/短休止）       （绑定 Switch: None）
```
每个角色 Segment 的建法与 4.4 相同：右键 Segment → New Child → **Music Track**
→ 用 **Audio File Importer**（Project > Import Audio Files）导入对应角色的 wav，
Wwise 自动在该 Track 上生成 Music Clip。
**循环不要找 Segment 的开关（没有）**：按 4.4.2 包 Music Playlist Container，
把其 **Loop Count 设为 Infinite（无限）**。

### 4.5.1 主题曲切换想"丝滑 / 不从头重来"怎么办？

> **先说清楚结论**：Wwise 没有"新 Segment 接着上一次的同一时间点继续播"的原生功能。
> 在 Music Switch Container 里切 Switch 值时，**新 Segment 总是从它自己的 Entry Cue（开头）
> 开始播**（官方社区里"让新曲从当前曲相同位置开始"一直是 feature request，到 2026 仍未实现）。
> 所以你要的"所有曲子都在同一个时间点"最好的实现方式，是官方教程（Wwise-201
> "Using Music Transitions"）教的那套：**统一乐谱网格 + 转场同步到小节/拍**，让旧曲播到
> 下一个重拍再换新曲，听起来就是"无缝换段"而不是"从头硬切"。

**具体步骤（在 `CharacterSelect` 内层容器上操作）：**

1. **统一 Tempo / Time Signature（最关键）**：选中 `CharacterSelect`（Music Switch Container），
   在 Property Editor 的 **General Settings** 里设定 Tempo 与拍号（比如 120 BPM、4/4）。
   然后对**每个角色 Segment**：选中该 Segment → Property Editor → **Time 设置勾
   Override Parent**（继承容器统一的网格）。所有主题曲节奏型一致，换曲才"在拍子上"。
2. **设置音乐转场（Transitions）**：选中 `CharacterSelect` → Property Editor 里找到
   **Transitions** 标签（Music Switch Container / Segment 都有）。
   添加一条 from/to 转场（或编辑默认的空行），把 **Sync** 设为 **Next Bar** 或 **Next Beat**：
   - 效果：Switch 值变化后，旧主题曲**继续播到下一小节/拍的边界**才切到新主题曲，
     而不是立刻从头重开。
   - 若设 Immediate，就是你现在遇到的"一换就从头硬切"。
3. **（可选）淡入淡出**：把同一行的 **Distance** 设为 2~4 拍（或 1 秒左右），
   **Level** 选音量渐变（交叉渐变），新旧主题短暂叠一段，听感更顺。
4. **（推荐）做等长循环段**：把 8 首主题曲都裁成**相同小节数**的循环（例如都以 4 小节为
   一个 Loop）。这样即使新曲从小节 1 开始，重拍位置和结构一致，听感像自然的段落切换。

> **性能/极致同轴方案（可选）**：若你希望多首主题曲真正"共用同一时间轴、绝不重头"，
> 行业标准做法是：把 8 首主题叠在**同一个 Segment 的多条 Music Track** 上，用
> **Music Track 的 Switch/State 关联**（Sub-track 关联 Switch）在某个小节按角色切轨道。
> 这样所有乐器共享一个播放头。本指南先用 Switch Container 方案（简单），这条作为进阶参考。

> **验证**：在 Wwise **Transport Control** 里播放 `MenuMusic`（或 CharacterSelect），
> 边播边切换 `Character_Theme` 的 Switch 值：设置了 Sync=Next Bar 后，应能看到旧曲播到
> 下一小节边界才切走。游戏里 `AkSoundEngine.SetSwitch` 同样走这批转场规则。

### 4.5.2 只能切换一次，之后切 Switch 没反应？（排查清单）

症状：包成 Music Playlist Container 后，**第一次换曲成功，之后改 `Character_Theme` 值再也不切**。
Wwise 官方文档明确说明：切换未完成时再次改值会**撤掉旧转场、重新安排新转场**，所以"只切一次"
不是设计行为，必定是下面某个配置问题。按顺序排查：

**1. Synth 同步点永远等不到（最常见）→ 先用 Immediate 隔离。**
   - Transitions 里若所有规则都用 **Sync = Next Bar** + **Enforce Sync**（强制对齐），而当前播放的
     Segment **没有 Tempo/拍号**（或被 Playlist 挡住了继承链，见 4.5.1 的 Override Parent），
     引擎会一直等"下一小节"，等不到 → 表现为"改值没反应"。
   - **隔离测试**：把 `CharacterSelect` 容器 Transitions 的**默认行（Source 和目标都空白）**
     的 **Sync 改成 Immediate** → 反复切 4~5 次。若每次都切完全程 ✓ → 问题是同步点/网格；
     若还是只切一次 ✗ → 看第 2、3 条。确认没问题后再把 Sync 改回 Next Bar。

**2. 只配了单条 from/to 转场，没配默认行（你"一直循环但切不动"最常见原因）。**
   - 若你只在 Transitions 里加了一条"从某主题→某主题"的规则，那**只有这一对值**有转场；
     其它所有组合落到**默认行（Source 空白 + 目标空白）**。该默认行若没配，Wwise 就用内置
     行为"**等当前 Segment 播完再播新的**"——但 Playlist 是**无限循环、永远播不完**，
     于是后面每一次改值都被挂起，表现就是"只切第一次，之后没反应"。
   - 修法：编辑**默认行**（Source 留空 + Destination 留空）= 通配所有组合：
     - **Exit Source** = **Next Bar**（或 Next Beat），**Play pre-entry 勾掉**；
     - **Destination** 区域 = **Jump to start / 从头播 Playlist**。
     - 需要个别组合特殊处理的，再在默认行之下（Wwise 自下而上扫描，靠下的先命中）加具体规则。
   - 隔离测试：先把默认行 Sync 改成 **Immediate**，连续切 4~5 次都能切 → 确认是规则/同步问题；
     再把 Sync 改回 Next Bar。

**3. Switch 关联指错对象（包 Playlist 后残留旧关联）。**
   - 把 Segment 拖进 Playlist 后，检查 `CharacterSelect` 的 **Association Editor**：
     每个 `Character_Theme` 值应指向**同名的 Music Playlist Container**，而不是指向
     已被挪进 Playlist 里的旧 Segment（残留关联会导致某些值切过去没声音/不触发）。
   - 确认每行只有一个候选对象，没有重复/悬空项。

**4. Playlist 播放模式不是 Continuous。**
   - 选中任一角色 Playlist Container → Property Editor → 播放模式应为 **Continuous**
     （随机/顺序 Step 模式播完一组就停，也会让切换观感变怪）。

**5. Playlist 套了两层（Playlist 里再套 Playlist）→ 循环失效的主要原因。**
   - 见 4.4.2 的警告：`CharacterSelect (MPC) → CharacterSelect (MPC) → 各角色 Segment`
     这种两层结构里，外层 MPC 的条目 Loop Count 是有限的，播完一轮后分支就结束了。
   - 目标结构是**一层**：`CharacterSelect (Music Switch Container, Character_Theme)` 下
     直接挂 **9 个不同的 MPC（Cherry、ConneR…None）**，每个 MPC 里只放自己的 Segment，
     Loop Count = Infinite。删掉多余的嵌套层，循环和切歌就都能同时成立。

> 排查结果请回报：把 Sync 改 Immediate 后能否反复切换——这决定了问题在"同步点"还是"关联/规则"。

### 4.6 验证嵌套层级

最终 `MenuMusic` 应长这样（每个循环分支：脚标 `(PL)` = Music Playlist Container，其 **Loop Count = Infinite**；叶子 = Music Segment → Music Track → Music Clip）：
```
MenuMusic (Switch Group = Menu_Music)
├── MainMenu (PL → Segment)     → Track → mainmenu.wav（循环）
├── CharacterSelect (Switch Container, Switch Group = Character_Theme)
│   ├── Cherry    (PL → Segment) → Track → Cherry.wav（循环）
│   ├── ConneR    (PL → Segment) → Track → ConneR.wav（循环）
│   ├── NaiLong   (PL → Segment) → Track → NaiLong.wav（循环）
│   ├── neko      (PL → Segment) → Track → neko.wav（循环）
│   ├── paff      (PL → Segment) → Track → paff.wav（循环）
│   ├── Robo_Head (PL → Segment) → Track → Robo_Head.wav（循环）
│   ├── sarflin   (PL → Segment) → Track → sarflin.wav（循环）
│   ├── Xenon     (PL → Segment) → Track → Xenon.wav（循环）
│   └── None      (PL → Segment) → Track →（静音）
├── SongSelect (PL → Segment)   → Track →（临时 mainmenu 或一首主题曲；循环）
└── InGame (PL → Segment)       → Track →（留空）
```
判断依据：右键层级里任意对象时，**"Music Track" 只在 Music Segment 的右键菜单出现**。
循环确认：选中任一 `(PL)` 即 Music Playlist Container，**Music Playlist Editor** 里
对应 Playlist Item 的 **Loop Count 应为 Infinite**。

### 4.7 创建驱动事件

在 **Events > Default Work Unit** 右键 → New Event → **Play**，命名 `Play_MenuMusic`，
Target 选 `MenuMusic` 容器。

Unity 端 `AudioSystemManager` 会在启动时触发 `SetSwitch(Menu_Music, ...)`，并在启动 `Start()` 时
`SetSwitch(Character_Theme, None)`。只要 `Play_MenuMusic` 已触发过一次，后续 Switch 就即时换歌。

## 5. 事件清单

**UI 事件**（各 Play 对应 UI 目录的 Sound）：
```
UI_Click
UI_Hover
UI_Back
Character_Confirm
Song_Select
```
创建方法（5 个都这么做）：
1. **Project Explorer > Events > Default Work Unit**
2. 右键 `Default Work Unit` → **New Event** → **Play**
3. 左侧加号 **Add a Target/Items** → 弹窗选对应 Sound（如 `UI_Click`）→ OK
4. 事件名 = Sound 名（`UI_Click` 等），点击目标 Sound 的空白区域给它改名/确认
   - 其实事件建好后默认名就叫目标 Sound 名，若不同改成上图所示名字

> ⚠️ **事件名千万不要带 `Play_` 前缀**：如果你把事件建成了 `Play_UI_Click` 这类名字，
> Unity 端发的 `UI_Click` 会匹配失败（日志报 `事件不存在: UI_Hover`）。命名必须与上表逐字一致。
> 改名方法：Project Explorer 里右键该事件 → **Rename（F2）**，把前面的 `Play_` 去掉 → Ctrl+S 保存 → 重新 Generate。

**音乐事件**：
```
Play_MenuMusic   → MenuMusic 容器
```
创建方法：
1. 右键 `Events > Default Work Unit` → **New Event** → **Play**
2. Add Target → 选 `MenuMusic`（Music Switch Container）→ OK
3. 事件名确保为 `Play_MenuMusic`

**24 个歌曲预览事件**（Play 对应 SongPreviews 目录的 Sound，完整清单见
`cytus2menuremake_WwiseProject\EventList_SongPreviews.txt`）：
```
Play_Preview_Cherry_0scill4torfeatMasKimuraMEG
Play_Preview_Cherry_ComeBackToMeShadowTheHedgehogSong
Play_Preview_ConneR_CarryAllBeforeOne
Play_Preview_ConneR_SeraphimLongVer
Play_Preview_ConneR_V
Play_Preview_ConneR_HistoryDstryeR
Play_Preview_NaiLong_LOVEOVERDREAMSfeatYURiCa
Play_Preview_NaiLong_toAsteroidB612RETALE
Play_Preview_Neko_CALAMITYRHAPSODY
Play_Preview_Neko_StarryEyedDreamerfeatSOLARIAExplici
Play_Preview_Neko_Resonate
Play_Preview_Paff_NatsuNoHate
Play_Preview_Paff_LeavingAllBehindovercast
Play_Preview_Paff_Incyde
Play_Preview_Paff_HyouryuwithKIVCytusver
Play_Preview_RoboHead_SILENTMOMENT
Play_Preview_RoboHead_Midnight
Play_Preview_RoboHead_CHAOS
Play_Preview_Xenon_ImAliveREBiRTHFestivalAnthem2019Re
Play_Preview_Xenon_AscensionMitiSRemix
Play_Preview_Xenon_tellmeifimwrong
Play_Preview_sarflin_InfiniteEvolution
Play_Preview_sarflin_AethersChild
Play_Preview_sarflin_99Glooms
```

> 预览事件名基于文件名生成，若你导入后 Wwise 中的 Sound 名与文件名一致即可直接对应。

## 6. Unity 端名字对照表

| Unity 字段 | 数值 | 说明 |
|---|---|---|
| AudioSystemManager.musicSwitchGroup | `Menu_Music` | 界面 BGM Switch 组 |
| AudioSystemManager.characterThemeSwitchGroup | `Character_Theme` | 角色主题曲 Switch 组 |
| CharacterData.wwiseSwitchName | `NaiLong` / `Robo_Head` | Wwise 角色 Switch 名：Nai Long→NaiLong、RObo_head→Robo_Head，其余留空用 characterName |
| SongData.wwiseEventPreview | `Play_Preview_<角色>_<歌曲>` | 歌曲预览事件 |
| SongData.wwiseEventPlay | （可自定义） | 开始游玩事件 |

> `CharacterData.wwiseSwitchName` 留空时 Unity 用 `characterName` 作为 Switch 名。
> **必须填 wwiseSwitchName 的 2 个角色**（displayName 与 Wwise 名不一致）：
> - `Nai Long`（asset 名带空格）→ 填 **`NaiLong`**
> - `RObo_head`（asset 名带下划线）→ 填 **`Robo_Head`**（大写 R、下划线）
>
> 其余角色（Cherry/ConneR/neko/paff/sarflin/Xenon）displayName 已合法，
> 留空即可，但**必须与 Wwise 里的 Switch 名拼写完全一致**。

## 7. SoundBank 生成

1. 打开 **Project Explorer > SoundBanks**（F7）底部 **Project Settings** 标签
   （或菜单 **Project > Project Settings** → **SoundBanks**）
2. 确认输出目录指向 Unity（2025.1 项目属性里是 **Output 路径/命名** 设置）：
   - 找到 **SoundBank Output / Generated SoundBank path** 字段
   - 改为指向 `E:\cytus2menuremake\Assets\StreamingAssets\Audio\GeneratedSoundBanks`（去掉末尾平台子目录）
   - 勾选 **Copy WAV: No**（不用把源文件拷进工程），**Copy JSON: Yes**（生成 SoundbanksInfo.json）
   - 确认 **Platform** 区里至少勾选 **Windows**
3. 回到 SoundBanks 列表，确认/创建 Bank：
   - **`Init` 是 Wwise 系统全局 Bank，列表里**本来就有**，无需也**不能**新建/改名**——
     若你试图新建一个叫 `Init` 的 Bank 被拒，就是因为已存在。直接用它，默认已勾选。
     （不要再建 `UNIT` 之类的空 Bank；若已建就删掉，见第 8 节。）
   - `UI`（右键 Add Items → 勾选 5 个 UI 事件）
   - `Music`（勾选 `Play_MenuMusic` 与 `MenuMusic` 容器）
   - `SongPreviews`（勾选 24 个 Play_Preview_* 事件）
4. 点击 **Generate All**（Ctrl+F7 或工具栏 Pipeline > Generate）

生成的 `.bnk` 输出到 `Assets/StreamingAssets/Audio/GeneratedSoundBanks/`，
`AkWwiseInitializationSettings` 的 Base Path 已指向此处。
> 若生成后找不到文件夹，多半是第 2 步 Output 路径没改；改完**重新 Generate** 一次。
>
> **目录不用手动建**：改好输出路径后，Wwise 生成时会**自动创建** `Audio\GeneratedSoundBanks\Windows\`
> 及其中所有父目录。唯一可能需要你动手的是：若 Unity 工程里**还没有** `Assets\StreamingAssets\`，
> 先在 Unity Project 面板右键 **Create > Folder** 建一个空的 `StreamingAssets` 即可，其余全部自动。
>
> **当前状态提醒**：`Assets\WwiseSettings.xml` 里 `RootOutputPath` 还是
> `../cytus2menuremake_WwiseProject/GeneratedSoundBanks/`（指向 Wwise 工程），
> **还没改成 Unity 路径**——改路径重新 Generate，或用第 8 节的"拷贝"方式二选一。

## 8. Unity 端 Bank 加载

- **`Init` 是 Wwise 系统 Bank，不需要也不能"改成别的名字"**：生成时它总会输出 `Init.bnk`，
  `AudioSystemManager` 第 66 行硬编码 `LoadBank("Init")`，Unity 里必须有它。
  自定义的 `UNIT.bnk`（48 B，空的）**不要加载**；建议回 Wwise 把那个空 Bank 删掉。
- `AudioSystemManager.startupBanks` 默认 = `UI, Music, SongPreviews`（Init 单独加载，见上）。
  Bank 名必须与 Wwise 里一致。

**Bank 文件到位（重点，别漏）**：Wwise 默认把 `.bnk` 生成在
`cytus2menuremake_WwiseProject\GeneratedSoundBanks\Windows\`，而 Unity 读取的是
`Assets\StreamingAssets\Audio\GeneratedSoundBanks\Windows\`——**两者是不同目录**。
二选一：
- **拷贝**：把 `Init.bnk` `UI.bnk` `Music.bnk` `SongPreviews.bnk`（+ 同名 `.json` 可选）
  从 Wwise 工程的 Windows 目录复制到上面的 Unity StreamingAssets 目录；
- **改路径重新生成**：按第 7 节第 2 步把 Output 指向
  `E:\cytus2menuremake\Assets\StreamingAssets\Audio\GeneratedSoundBanks` 后再次 Generate All。

## 9. 回退方案（无需 Wwise 也能听预览）

`AudioSystemManager.PlaySongPreview`：
1. `SongData.wwiseEventPreview` 非空 → 发 Wwise 事件
2. 否则 → 用 `SongData.previewClip`（AudioClip）走 AudioSource

因此 Wwise 事件未建完时，选曲界面仍能播放预览。运行 `Cytus2 > Audio > Link Song Previews`
会自动填好 `previewClip`。

## 10. Unity 端操作清单（编辑器内准备）

### 10.1 确认 Bank 文件到位

进入 Unity 前先确认 `Assets/StreamingAssets/Audio/GeneratedSoundBanks/Windows/`（或第 7 节改路径后的目录）
里有 `Init.bnk` `UI.bnk` `Music.bnk` `SongPreviews.bnk`。
若缺失，按第 8 节"拷贝或改路径重新生成"处理。禁止出现 `UNIT.bnk` 等多余空 Bank。

### 10.2 生成/装配菜单与音频对象（按顺序跑一次）

1. **Cytus2 > Create Menu System**（仍没有菜单结构时跑）：生成菜单 UI。
2. **Cytus2 > Audio > Setup Audio Scene**：在当前打开的场景里补建
   `WwiseGlobal`（AkInitializer）与 `AudioManager`（WwiseIntegration + AudioSystemManager）。
   - 若场景里已有一个 AudioManager，它不会重复建。
   - 每次打开的新场景都要跑一次（或直接把这两个对象做成预制体放场景）。
3. **Cytus2 > Create Sample Data**（还没有 CharacterData 时跑）：只生成 8 个角色（不再生成假歌曲）。
4. **Cytus2 > Refresh Character Data**：补齐 `wwiseSwitchName` 大小写与 `portrait`（已有角色可重复跑）。
5. **Cytus2 > Audio > Link Song Previews**：扫描 `Assets/SongsPreview/` 的 `*_Preview.mp3`，
   按 `cytus2menuremake_WwiseProject\EventList_SongPreviews.txt` 里的真实事件名，
   生成/更新 `Assets/Resources/Songs/` 下的 SongData（自动填 `wwiseEventPreview` 与 `previewClip`），
   并**自动清理不再对应当前 mp3 的冗余 SongData**。
   - 前置：`Assets/SongsPreview/` 有 24 个 mp3、`Assets/Resources/Characters/` 有 CharacterData。

### 10.3 Inspector 核对（AudioSystemManager）

| 字段 | 值 | 说明 |
|---|---|---|
| startupBanks | `UI, Music, SongPreviews` | Init 无需列出，第 66 行单独加载 |
| musicSwitchGroup | `Menu_Music` | 与 Wwise 一致 |
| mainMenuState / characterSelectState / songSelectState / gameState | `MainMenu` `CharacterSelect` `SongSelect` `InGame` | 与 Wwise Switch 值一致 |
| characterThemeSwitchGroup | `Character_Theme` | 与 Wwise 一致 |
| clickSound / hoverSound / backSound / characterConfirmSound / songSelectSound | `UI_Click` `UI_Hover` `UI_Back` `Character_Confirm` `Song_Select` | 与你刚建的 UI 事件一致 |

### 10.4 角色与歌曲数据字段

- CharacterData（`Assets/Resources/Characters/`）：
  `Nai Long` 填 `NaiLong`、`RObo_head` 填 `Robo_Head`、**`Paff` 填 `paff`、`Neko` 填 `neko`（小写，必须与 Wwise Switch 值一致）**。
  直接跑 **Cytus2 > Refresh Character Data** 会自动补齐 `wwiseSwitchName` 和 `portrait`
  （从 `Assets/Art/Characters/` 匹配同名 png；Nai Long / sarflin 没有现成图则仍为空，需手动指定）。
- SongData（`Assets/Resources/Songs/`）：
  `wwiseEventPreview` 应为 `Play_Preview_<角色>_<歌>`（Link Song Previews 已自动填）。
  若某首显示为 fallback 名，到 Wwise 核对真实事件名后手填。

### 10.5 Play Mode 验证清单

1. Unity Play Mode：主菜单 BGM（mainmenu）循环播放
2. Start → 角色选择：切换到角色主题曲，轮播到不同角色即切换对应主题曲
3. 点角色 → 选曲界面：BGM 切 SongSelect，选曲自动播放对应预览（切换歌曲/返回都会自动停止上一首）
4. 返回主菜单：主题曲切回 None，恢复 mainmenu
5. Console 无 `事件不存在` 告警即表示事件名全部匹配

> 若完全无声：先确认 10.1（Init.bnk 必须存在，`LoadBank("Init")` 成功）；
> 再在 Console 过滤 `[Wwise]` 看 `LoadBank` 返回值。

### 10.6 常见 Bug 与修复记录（2026-08）

1. **主菜单/选人没声音**：`Play_MenuMusic` 从未被触发——`AudioSystemManager.Start` 现在会自动
   `PostEvent(Play_MenuMusic)`（只发一次，之后全靠 Switch 换段）。若仍无声，检查 Music 事件是否在
   `Music` Bank 里（`Music.json` 含 `Play_MenuMusic`）。
2. **Paff/Neko 主题曲不切**：`wwiseSwitchName` 必须是小写 `paff`/`neko`。跑 `Refresh Character Data` 修复。
3. **选曲界面混入旧/示例歌曲**：`Create Sample Data` 已不再生成假歌曲；`Link Song Previews`
   现在会**自动清理 `Resources/Songs/` 里不对应当前 mp3 的冗余 SongData**。
4. **预览重叠 / 退出不停**：预览现在记录 PlayingID，切换/返回前先 `StopPlayingID` 精确停止上一首
   （不会误伤背景音乐）；回退路径的 AudioSource 也设为循环并在停止时清掉。
5. **预览不循环**：Unity 回退路径已循环；走 Wwise 事件时，需要在 Wwise 里把
   **SongPreviews 目录下的 24 个预览 Sound 勾选 Loop: Yes**（Project Explorer → **Containers >
   Default Work Unit > SongPreviews**，Ctrl/Shift 多选全部 24 个 Sound → Property Editor →
   General Settings → **Loop** 打勾 → Ctrl+S 保存），然后重新 Generate + 按第 8 节同步到 StreamingAssets。
6. **UI 音效缺失（事件不存在: UI_Hover）**：Wwise 里事件被建成了 `Play_UI_Click` 这类带
   `Play_` 前缀的名字，与 Unity 端约定（`UI_Click`…）不一致。修法：在 Wwise 把 5 个 UI 事件
   **改名去掉 `Play_` 前缀**（Rename/F2）→ 保存 → 重新 Generate → 同步到 StreamingAssets。
   （若反过来事件不带前缀，则不用改。）
7. **`Failed load Init.bnk ... AK_BankAlreadyLoaded`**：`AkInitializer` 初始化时已自动加载 Init，
   无需再手动 Load。`AudioSystemManager` 已去掉重复的 `LoadBank("Init")`，该报错会自动消失。

## 11. 未来扩展：新角色 / 给角色追加歌曲

整个框架是**数据驱动**的，加角色/加歌**不需要改任何 C# 代码**（选曲列表读
`Resources/Songs/`，预览按 `Play_Preview_<角色>_<歌>` 事件名，主题曲按 `Character_Theme`
Switch 值），只需保证各层命名一致。

### 11.1 给**已有角色**追加一首新歌（最快路径）

1. **给 Wwise 一份预览 wav**：导入进 `Containers > Default Work Unit > SongPreviews`，
   命名 `xxx_Preview`；选中它 → Property Editor → General Settings → **Loop: Yes**。
2. **建预览事件**：Events 下新建 Play 事件，名字必须为
   **`Play_Preview_<角色>_<歌>`**（无空格），Target 选刚导入的预览 Sound。
3. **更新 `EventList_SongPreviews.txt`**：在文件末尾追加两行，格式与现有一致——
   ```
   N. [角色] 艺术家 - 歌名
      Event: Play_Preview_角色_歌名
   ```
   （`Link Song Previews` 靠它把 mp3 映射到真实事件名；不追加只会用"回退生成名"，可能对不上。）
4. **给 Unity 一份预览 mp3**：放进 `Assets/SongsPreview/`，按命名规则
   `角色-艺术家 - 歌名_Preview.mp3`。
5. **Wwise 重新 Generate All**（若预览 Sound/事件加了，只进 `SongPreviews.bnk`）→ 按第 8 节同步。
6. Unity 跑一次 **`Cytus2 > Audio > Link Song Previews`**：会自动为新歌建 `SongData`
   （填好 `wwiseEventPreview` 与 `previewClip`），并清理不再对应当前 mp3 的旧数据。
7. 没给 wav 的话，上面 1/2/5 可跳过：Unity 回退路径（`previewClip` 走 AudioSource 循环）也能响，
   只是预览走不了 Wwise。

### 11.2 新增一个**角色**（含主题曲 + 歌曲）

**Wwise 侧（主题曲 + Switch 值）：**
1. 主题曲 wav：在 `Containers > Default Work Unit > CharacterThemes` 建
   **Music Segment → Music Track**，导入 wav 生成 Clip（4.4 节方法），并把
   **Music Segment 包进 Music Playlist Container**（Loop Count = Infinite，4.4.2）。
2. 把该自定义 Playlist 作为 **`CharacterSelect`（内层 Switch 容器）的子项**，名称 = 角色的
   Switch 值（例如 `NaiLong`、`paff` 这种**合法标识符**，无空格）。
3. `Character_Theme` Switch Group 里**新增一个值**（同名）；Switch 容器分配视图里把该值指向新 Playlist。
4. 主题曲/事件放进 `Music` Bank（自动包含）。

**Unity 侧（数据）：**
1. 大头像/肖像图放 `Assets/Art/Characters/<名字>.png`。
2. 建角色数据：Project 面板右键 **Create > Cytus2 > Character Data**（或复制一个现有角色 .asset），
   填 `characterName`（显示名）、`description`、`portrait`、`themeColor`、`sortOrder`；
   若显示名与 Switch 值不一致（含空格等），填 `wwiseSwitchName` = 上面第 2 步的 Switch 值。
   （手动画像懒得拖，可跑 **`Cytus2 > Refresh Character Data`**，它会按文件名自动配对。）
3. 歌曲预览按 11.1 走。
4. 全部就绪后：Wwise Generate + 同步 Bank → Unity 跑 `Link Song Previews` + `Refresh Character Data`。

> **统一命名三处必须一致**：`Character_Theme` Switch 值 = CharacterData.wwiseSwitchName =
> 角色的新 Playlist 名；预览事件名 = `Play_Preview_<角色>_<歌>`。任何一处大小写/拼写不同，
> 表现就是"主题曲不切 / 预览没声音"，排查时先对这三串名字。

### 11.3 歌曲曲绘（封面）：自动匹配

选曲界面显示的封面来自 `SongData.coverArt`。**所有封面统一丢进 `Assets/Art/SongCovers/`**，
然后 `PreviewLinker` 会按文件名自动匹配，无需手拖 Sprite。

**命名约定（不区分大小写、忽略标点符号）——推荐首选用法：**
```
<角色>-<歌曲名>.png        Cherry-0scill4tor (feat_ Mas Kimura & MEG).png
```
回退匹配：文件名只要**包含完整歌曲名**就能匹配（如 `艺术家 - 歌曲名.png`），
不同角色同名歌曲时，优先选文件名里含角色名的那个。

**操作步骤：**
1. 把所有曲绘图片放进 `Assets/Art/SongCovers/`（png/jpg/jpeg 均可），按上面规则改名。
2. Unity 菜单 **`Cytus2 > Audio > Link Song Previews`**（顺带重建/更新所有 SongData 并自动绑封面）；
   或封面已就位、只想补绑时单独跑 **`Cytus2 > Audio > Link Cover Art`**。
3. 跑完看 Console：`封面匹配 N 张`；若某些歌还是没封面，会有
   `[PreviewLinker] 未找到封面的歌曲:...` 警告，按它列出的歌名修正文件名即可。

> 注意：封面匹配的关键字和 mp3 的歌曲名（文件名里 `_Preview` 前最后一段）比对，
> 所以**不要**把 mp3 改名，否则连歌曲事件/封面都会一起失配。