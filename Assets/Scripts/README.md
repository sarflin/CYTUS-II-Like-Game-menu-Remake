# Cytus 2 菜单系统 - 项目说明

## 已创建的文件

### 核心脚本 (`Assets/Scripts/`)

1. **CharacterData.cs** - 角色数据结构
   - 角色名称、描述、头像
   - Wwise音效配置
   - UI主题颜色

2. **SongData.cs** - 歌曲数据结构
   - 歌曲信息（名称、艺术家、BPM）
   - 难度等级（Easy/Hard/Special）
   - 所属角色关联

3. **MenuManager.cs** - 菜单管理器
   - 控制界面切换
   - 管理界面状态
   - 单例模式，全局访问

4. **CharacterSelectUI.cs** - 角色选择界面
   - 动态生成角色列表
   - 显示角色详情
   - 选择确认逻辑

5. **SongSelectUI.cs** - 曲目选择界面
   - 根据角色过滤歌曲
   - 显示歌曲信息和难度
   - 播放预览音效

6. **MainMenuUI.cs** - 主菜单
   - 开始游戏、设置、退出按钮

7. **WwiseIntegration.cs** - Wwise集成接口
   - 事件发送
   - RTPC参数设置
   - 音乐状态切换

8. **UIAnimator.cs** - UI动画组件
   - 淡入淡出效果
   - 缩放动画

9. **MenuSetupHelper.cs** - 菜单创建工具
   - Unity编辑器菜单：`Cytus2 > 创建菜单系统`
   - 自动创建UI层级结构

10. **SampleDataCreator.cs** - 示例数据创建工具
    - Unity编辑器菜单：`Cytus2 > 创建示例数据`
    - 创建3个示例角色和3首示例歌曲

### 数据配置 (`Assets/ScriptableObjects/`)

- **Characters/** - 角色数据资产
- **Songs/** - 歌曲数据资产

## 使用步骤

### 第一步：创建菜单系统

1. 在Unity编辑器中，点击菜单 `Cytus2 > 创建菜单系统`
2. 系统会自动创建：
   - MenuCanvas（包含所有UI）
   - MainMenu（主菜单）
   - CharacterSelectUI（角色选择）
   - SongSelectUI（曲目选择）
   - MenuManager（管理器）
   - WwiseIntegration（音效接口）

### 第二步：创建示例数据

1. 点击菜单 `Cytus2 > 创建示例数据`
2. 会自动创建示例角色和歌曲数据

### 第三步：配置UI

1. 选中 `CharacterSelectUI` 对象
2. 在Inspector中找到 `Character Data` 数组
3. 添加创建的角色数据

4. 选中 `SongSelectUI` 对象
5. 在Inspector中找到 `All Songs` 数组
6. 添加创建的歌曲数据

### 第四步：配置Wwise

1. 安装Wwise Unity Integration
2. 在 `WwiseIntegration` 组件中配置事件名称
3. 在各UI组件的Inspector中配置对应的音效事件

## 界面流程

```
主菜单 (MainMenu)
    ↓ 点击"开始游戏"
角色选择 (CharacterSelectUI)
    ↓ 选择角色并确认
曲目选择 (SongSelectUI)
    ↓ 选择歌曲并点击"开始"
游戏场景 (待实现)
```

## Wwise集成说明

### 需要创建的Wwise事件

1. **UI音效**
   - `UI_Hover` - 鼠标悬停
   - `UI_Click` - 按钮点击
   - `UI_Confirm` - 确认选择
   - `UI_Back` - 返回操作

2. **角色音效**
   - `Character_Select` - 选择角色
   - `Character_Confirm` - 确认角色

3. **歌曲音效**
   - `Song_Preview` - 歌曲预览
   - `Song_Select` - 选择歌曲
   - `Song_Start` - 开始游戏

4. **音乐状态**
   - Switch Group: `Music_State`
     - States: Menu, CharacterSelect, SongSelect, Gameplay

### 互动音乐控制

```csharp
// 切换音乐状态
WwiseIntegration.SwitchMusic("Music_State", "CharacterSelect");

// 设置音乐强度
WwiseIntegration.SetRTPC("Menu_Transition", 50f);
```

## 自定义扩展

### 添加新角色

1. 在 `Assets/ScriptableObjects/Characters/` 右键
2. 选择 `Create > Cytus2 > Character Data`
3. 填写角色信息
4. 拖拽到 `CharacterSelectUI` 的角色列表

### 添加新歌曲

1. 在 `Assets/ScriptableObjects/Songs/` 右键
2. 选择 `Create > Cytus2 > Song Data`
3. 填写歌曲信息
4. 选择所属角色
5. 拖拽到 `SongSelectUI` 的歌曲列表

## 注意事项

1. 所有UI脚本依赖 TextMeshPro，请确保已安装
2. Wwise Integration需要单独安装
3. 示例数据仅供参考，请替换为实际资源
4. 游戏场景需要自行创建和实现

## 下一步

1. 导入角色头像和歌曲封面图片
2. 安装并配置Wwise
3. 创建游戏场景
4. 实现实际的音乐游戏玩法
5. 添加更多角色和歌曲
