# WeChatMomentSimulator

一个用于创建、编辑和导出微信朋友圈模拟图片的桌面应用程序。

![应用预览](https://i.imgur.com/YMuKI0R.png)

## ✨ 功能特点

- 🖼️ **丰富模板库** - 内置多种朋友圈、聊天记录和个人资料模板
- 🔄 **实时预览** - 所见即所得的编辑体验
- 📱 **真实模拟** - 精确还原微信界面样式和交互
- 🎨 **丰富自定义选项** - 自定义头像、昵称、文本、图片等元素
- 💾 **多格式导出** - 支持PNG、JPG等多种格式导出
- 🛠️ **模板创建** - 创建和保存自定义模板

## 📸 界面展示

### 创建朋友圈内容
![朋友圈创建](https://i.imgur.com/P7QLF3z.png)

### 移动端预览
![移动预览](https://i.imgur.com/xTZCJwb.png)

### 应用功能区
![功能区](https://i.imgur.com/nLbDelw.png)

## 🚀 技术栈

- **框架**: .NET 9.0
- **UI**: WPF (Windows Presentation Foundation)
- **架构**: MVVM (Model-View-ViewModel)
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **图像处理**: SkiaSharp, ImageSharp
- **模板引擎**: Handlebars.Net
- **日志记录**: NLog

## 📥 安装

### 系统要求
- Windows 10/11
- .NET 9.0 Runtime

### 下载与安装
1. 从 [Releases](https://github.com/yourusername/WeChatMomentSimulator/releases) 页面下载最新版本
2. 运行安装程序并按照向导完成安装
3. 启动应用程序

## 💻 使用指南

### 快速开始
1. 从左侧模板库选择一个模板
2. 在中间编辑区填写所需信息
3. 在右侧预览区查看效果
4. 点击"导出图片"保存结果

### 创建自定义模板
1. 点击"文件" > "新建模板"
2. 选择模板类型并设计内容
3. 点击"保存模板"将其添加到您的模板库

## 👨‍💻 开发指南

### 获取源码
```bash
git clone https://github.com/yourusername/WeChatMomentSimulator.git
cd WeChatMomentSimulator
```

### 构建项目
```bash
dotnet restore
dotnet build
```

### 项目结构
- WeChatMomentSimulator.Core - 核心模型和接口
- WeChatMomentSimulator.Services - 服务实现
- WeChatMomentSimulator.Desktop - WPF桌面应用

## 🤝 贡献指南

我们欢迎所有形式的贡献，无论是新功能、bug修复还是文档改进。

1. Fork 这个仓库
2. 创建您的特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交您的更改 (`git commit -m 'Add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 打开一个 Pull Request

## 📄 许可证

该项目基于 MIT 许可证 - 查看 LICENSE 文件了解详情

## 📞 联系方式

如有问题或建议，请通过以下方式联系我们：
- 提交 [Issues](https://github.com/yourusername/WeChatMomentSimulator/issues)
- 发送邮件至: your.email@example.com

---
**注意**: 本工具仅用于学习和娱乐目的，与腾讯微信官方无关。
