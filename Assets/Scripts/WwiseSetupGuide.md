# Cytus 2 菜单系统 Wwise 集成指南

## 1. 安装 Wwise Unity Integration

1. 从 [Audiokinetic](https://www.audiokinetic.com) 下载 Wwise Unity Integration
2. 导入 Unity Package: `Assets > Import Package > Custom Package`
3. 选择需要的模块（推荐：Core, SoundEngine, Communication）

## 2. 创建 Wwise 项目结构

### 2.1 创建 Actor-Mixer Hierarchy

```
Default Work Unit
├── UI
│   ├── UI_Hover
│   ├── UI_Click
│   ├── UI_Confirm
│   └── UI_Back
├── Music
│   ├── Menu_Music
│   └── Character_Music
└── SFX
    ├── Character_Select
    └── Song_Select
```

### 2.2 创建 Switch Group

**Music_State** (用于互动音乐切换)
- Menu
- CharacterSelect
- SongSelect
- Gameplay

**SFX_State** (用于音效状态)
- Normal
- Highlight
- Pressed

### 2.3 创建 RTPC (Game Parameter)

- **Menu_Transition**: 控制菜单切换过渡效果 (0-100)
- **Character_Interest**: 角色选择兴趣度 (0-100)
- **Song_Progress**: 歌曲播放进度 (0-100)

## 3. 配置互动音乐

### 3.1 菜单音乐层级结构

```
Menu_Music
├── Layer_Base (基础旋律)
├── Layer_Percussion (打击乐层)
├── Layer_Synth (合成器层)
└── Layer_Vocals (人声层)
```

### 3.2 使用 Switch 控制音乐状态

当用户在菜单中导航时，通过 Switch 切换音乐状态：

```csharp
// 示例：切换到角色选择音乐
WwiseIntegration.SwitchMusic("Music_State", "CharacterSelect");
```

### 3.3 使用 RTPC 控制音乐强度

根据用户交互动态调整音乐强度：

```csharp
// 示例：根据鼠标位置调整音乐强度
float intensity = Mathf.Lerp(0, 100, mouseX / screenWidth);
WwiseIntegration.SetRTPC("Menu_Transition", intensity);
```

## 4. 音效设计

### 4.1 UI 音效

| 事件名称 | 用途 | 建议音效 |
|---------|------|---------|
| UI_Hover | 鼠标悬停 | 轻微电子音 |
| UI_Click | 按钮点击 | 短促点击声 |
| UI_Confirm | 确认选择 | 上升音效 |
| UI_Back | 返回上一级 | 下降音效 |

### 4.2 角色选择音效

| 事件名称 | 用途 | 建议音效 |
|---------|------|---------|
| Character_Select | 选择角色 | 角色专属音效 |
| Character_Confirm | 确认角色 | 转场音效 |

### 4.3 歌曲选择音效

| 事件名称 | 用途 | 建议音效 |
|---------|------|---------|
| Song_Preview | 歌曲预览 | 3秒歌曲片段 |
| Song_Select | 选择歌曲 | 选中音效 |
| Song_Start | 开始游戏 | 游戏开始音效 |

## 5. Unity 集成代码

### 5.1 初始化 Wwise

```csharp
// 在 Awake 中初始化
AkSoundEngine.Init();
AkSoundEngine.LoadBank("Init");
AkSoundEngine.LoadBank("MainMenu");
```

### 5.2 播放事件示例

```csharp
// 播放 UI 点击音效
AkSoundEngine.PostEvent("UI_Click", gameObject);

// 播放角色选择音效
AkSoundEngine.PostEvent("Character_Select", characterObject);
```

### 5.3 互动音乐控制

```csharp
// 切换音乐状态
public void SwitchMusicState(string state)
{
    AkSoundEngine.SetSwitch("Music_State", state, gameObject);
}

// 设置 RTPC 值
public void SetMusicIntensity(float value)
{
    AkSoundEngine.SetRTPCValue("Menu_Transition", value, gameObject);
}
```

## 6. 调试技巧

1. 使用 Wwise Profiler 连接 Unity 进行实时调试
2. 检查 Console 窗口中的 Wwise 日志
3. 使用 `AkSoundEngine.RenderAudio()` 手动触发音频更新

## 7. 性能优化

1. 使用 LoadBank 异步加载大型音效库
2. 限制同时播放的音效数量
3. 使用 Event-based 音效而非 Sample-based
4. 合理使用 RTPC 控制音效参数

## 8. 参考资源

- [Wwise Unity Integration 文档](https://www.audiokinetic.com/library/edge/?source=Unity)
- [Wwise-101 教程](https://www.audiokinetic.com/courses/wwise101/)
- [Unity 音频系统](https://docs.unity3d.com/Manual/class-AudioSource.html)
