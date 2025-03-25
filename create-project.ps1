# 创建微信朋友圈模拟器三层架构项目脚本

# 步骤1: 创建项目文件夹结构
mkdir WeChatMomentSimulator
cd WeChatMomentSimulator
mkdir src
mkdir tests
mkdir docs

# 步骤2: 创建解决方案文件
dotnet new sln -n WeChatMomentSimulator

# 步骤3: 创建三个子项目
cd src
dotnet new classlib -f net7.0 -n WeChatMomentSimulator.Core
dotnet new classlib -f net7.0 -n WeChatMomentSimulator.Services
dotnet new wpf -f net7.0-windows -n WeChatMomentSimulator.Desktop
cd ..

# 步骤4: 创建测试项目
cd tests
dotnet new xunit -f net7.0 -n WeChatMomentSimulator.Core.Tests
dotnet new xunit -f net7.0 -n WeChatMomentSimulator.Services.Tests
dotnet new xunit -f net7.0 -n WeChatMomentSimulator.Desktop.Tests
cd ..

# 步骤5: 将项目添加到解决方案
dotnet sln WeChatMomentSimulator.sln add src/WeChatMomentSimulator.Core/WeChatMomentSimulator.Core.csproj
dotnet sln WeChatMomentSimulator.sln add src/WeChatMomentSimulator.Services/WeChatMomentSimulator.Services.csproj
dotnet sln WeChatMomentSimulator.sln add src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj
dotnet sln WeChatMomentSimulator.sln add tests/WeChatMomentSimulator.Core.Tests/WeChatMomentSimulator.Core.Tests.csproj
dotnet sln WeChatMomentSimulator.sln add tests/WeChatMomentSimulator.Services.Tests/WeChatMomentSimulator.Services.Tests.csproj
dotnet sln WeChatMomentSimulator.sln add tests/WeChatMomentSimulator.Desktop.Tests/WeChatMomentSimulator.Desktop.Tests.csproj

# 步骤6: 添加项目引用
dotnet add src/WeChatMomentSimulator.Services/WeChatMomentSimulator.Services.csproj reference src/WeChatMomentSimulator.Core/WeChatMomentSimulator.Core.csproj
dotnet add src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj reference src/WeChatMomentSimulator.Core/WeChatMomentSimulator.Core.csproj
dotnet add src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj reference src/WeChatMomentSimulator.Services/WeChatMomentSimulator.Services.csproj
dotnet add tests/WeChatMomentSimulator.Core.Tests/WeChatMomentSimulator.Core.Tests.csproj reference src/WeChatMomentSimulator.Core/WeChatMomentSimulator.Core.csproj
dotnet add tests/WeChatMomentSimulator.Services.Tests/WeChatMomentSimulator.Services.Tests.csproj reference src/WeChatMomentSimulator.Services/WeChatMomentSimulator.Services.csproj
dotnet add tests/WeChatMomentSimulator.Services.Tests/WeChatMomentSimulator.Services.Tests.csproj reference src/WeChatMomentSimulator.Core/WeChatMomentSimulator.Core.csproj
dotnet add tests/WeChatMomentSimulator.Desktop.Tests/WeChatMomentSimulator.Desktop.Tests.csproj reference src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj

# 步骤7: 添加NuGet包依赖
# 服务层依赖项
dotnet add src/WeChatMomentSimulator.Services/WeChatMomentSimulator.Services.csproj package Handlebars.Net --version 2.1.4
dotnet add src/WeChatMomentSimulator.Services/WeChatMomentSimulator.Services.csproj package Newtonsoft.Json --version 13.0.3
dotnet add src/WeChatMomentSimulator.Services/WeChatMomentSimulator.Services.csproj package SkiaSharp --version 2.88.6
dotnet add src/WeChatMomentSimulator.Services/WeChatMomentSimulator.Services.csproj package SixLabors.ImageSharp --version 3.0.2
dotnet add src/WeChatMomentSimulator.Services/WeChatMomentSimulator.Services.csproj package NLog --version 5.2.4

# 桌面应用层依赖项
dotnet add src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj package CommunityToolkit.Mvvm --version 8.2.1
dotnet add src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj package MaterialDesignThemes --version 4.9.0
dotnet add src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj package Microsoft.Extensions.DependencyInjection --version 7.0.0
dotnet add src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj package SkiaSharp.Views.WPF --version 2.88.6
dotnet add src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj package NLog.Extensions.Logging --version 5.3.5

# 测试项目依赖项
dotnet add tests/WeChatMomentSimulator.Core.Tests/WeChatMomentSimulator.Core.Tests.csproj package Moq --version 4.20.69
dotnet add tests/WeChatMomentSimulator.Services.Tests/WeChatMomentSimulator.Services.Tests.csproj package Moq --version 4.20.69
dotnet add tests/WeChatMomentSimulator.Desktop.Tests/WeChatMomentSimulator.Desktop.Tests.csproj package Moq --version 4.20.69

# 步骤8: 创建项目基本文件夹结构
# 创建核心层文件夹结构
mkdir src\WeChatMomentSimulator.Core\Models
mkdir src\WeChatMomentSimulator.Core\Models\Template
mkdir src\WeChatMomentSimulator.Core\Models\Template\Enums
mkdir src\WeChatMomentSimulator.Core\Models\User
mkdir src\WeChatMomentSimulator.Core\Models\User\Enums
mkdir src\WeChatMomentSimulator.Core\Models\Content
mkdir src\WeChatMomentSimulator.Core\Models\Content\Enums
mkdir src\WeChatMomentSimulator.Core\Models\Render
mkdir src\WeChatMomentSimulator.Core\Models\Render\Enums
mkdir src\WeChatMomentSimulator.Core\Models\Project
mkdir src\WeChatMomentSimulator.Core\Interfaces
mkdir src\WeChatMomentSimulator.Core\Interfaces\Services
mkdir src\WeChatMomentSimulator.Core\Interfaces\Repositories
mkdir src\WeChatMomentSimulator.Core\Exceptions
mkdir src\WeChatMomentSimulator.Core\Utils
mkdir src\WeChatMomentSimulator.Core\Utils\Extensions

# 创建服务层文件夹结构
mkdir src\WeChatMomentSimulator.Services\Services
mkdir src\WeChatMomentSimulator.Services\Repositories
mkdir src\WeChatMomentSimulator.Services\Providers
mkdir src\WeChatMomentSimulator.Services\Renderers
mkdir src\WeChatMomentSimulator.Services\Configuration

# 创建桌面应用层文件夹结构
mkdir src\WeChatMomentSimulator.Desktop\ViewModels
mkdir src\WeChatMomentSimulator.Desktop\ViewModels\Base
mkdir src\WeChatMomentSimulator.Desktop\Views
mkdir src\WeChatMomentSimulator.Desktop\Views\Controls
mkdir src\WeChatMomentSimulator.Desktop\Views\Windows
mkdir src\WeChatMomentSimulator.Desktop\Views\Pages
mkdir src\WeChatMomentSimulator.Desktop\Services
mkdir src\WeChatMomentSimulator.Desktop\Converters
mkdir src\WeChatMomentSimulator.Desktop\Resources
mkdir src\WeChatMomentSimulator.Desktop\Resources\Styles
mkdir src\WeChatMomentSimulator.Desktop\Resources\Icons

# 步骤9, 10, 11: 创建基础类文件、.gitignore和README.md
# [此处添加之前设定的文件内容创建代码]

# 步骤12: 构建项目
dotnet build

# 运行项目
# dotnet run --project src/WeChatMomentSimulator.Desktop/WeChatMomentSimulator.Desktop.csproj

Write-Host "微信朋友圈模拟器三层架构项目创建完成!" -ForegroundColor Green