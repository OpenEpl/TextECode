***Only Chinese document is available.***

# TextECode
用于实现从易语言工程文件 `*.e` 导出文本代码，或从文本代码中还原 `*.e` 文件的 *第三方工具*  
该工具可以使得对易语言代码进行 版本管理（如使用 Git 或 SVN）、代码比较（Diff）、自动代码生成 等操作变得容易  

## 编译
```bash
msbuild "TextECode.sln" /p:Configuration=Release /t:Restore
msbuild "TextECode.sln" /p:Configuration=Release;Version=0.0.1
```

## 安装
所有用户均需要先 [安装 .NET 桌面运行时 3.1](https://dotnet.microsoft.com/zh-cn/download/dotnet/3.1) 才可使用本工具  
用户应安装易语言环境，且保证注册表信息正确，以便本工具能够正确读取本地的支持库信息  

Win10及以上用户可以使用 Appx 包安装（从 Releases 下载或者自行编译），由于本项目既没有上架 Microsoft Store 也没有购买可信第三方的证书，因此在安装前您**需要信任我们的自签名证书**。  
Appx 包安装后自动注册应用别名 `TextECode.exe` ，无需设置环境变量即可在 CMD 或 PowerShell 中使用  

Win7/8/8.1用户可下载绿色版，自行存放文件后设置环境变量，使 `TextECode.exe` 可被直接调用  

XP用户无法使用本工具

## 使用
- `TextECode.exe help` ：查看帮助信息
- `TextECode.exe generate Foo.e Foo.eproject` ：从 `*.e` 文件生成文本代码
- `TextECode.exe restore Foo.eproject Foo.e` ：将 文本代码 还原为 `*.e` 文件
- `TextECode.exe view Foo.eproject` ：临时将 文本代码 还原为 `*.e` 文件并打开易语言环境，将对 `*.e` 文件的修改**自动同步**回文本代码，且在易语言环境关闭后**自动删除** `*.e` 文件

## 注意
本项目的生成及还原均不完善，可能造成数据损坏，**使用前请自行备份好源文件**，作者不对可能的损害负任何责任

## 交流
一般的 bug 反馈 与 feature 请求，请用 GitHub 的 Issues 模块反馈  
如果您希望对本项目做出贡献，请使用标准 GitHub 工作流：Fork + Pull request  
进一步的快速讨论：请加入 QQ 群 `605310933` 或 QQ 频道 `e81tgd8w3m` *（注意不要在群中反馈 bug，这很可能导致反馈没有被记录。聊天消息较 Issues 模块比较混乱）*  

## 许可
本项目使用 [MIT License](./LICENSE.txt) 许可证