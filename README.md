<div align="center">
<p><img src="https://www.wpe64.com/web_images/wpe.png" height="150"></p>

# Winsock Packet Editor（WPE x64）

**商业生产级 · 白主题 WPF 重构版** &nbsp;|&nbsp; **Commercial-grade · White-theme WPF Edition**

<img src="https://img.shields.io/github/license/x-nas/WinsockPacketEditor" alt="License"></img>
[![Visitors](https://visitor-badge.laobi.icu/badge?page_id=x-nas.WinsockPacketEditor&title=Visitors)](https://github.com/x-nas/WinsockPacketEditor)
![GitHub Repo stars](https://img.shields.io/github/stars/x-nas/WinsockPacketEditor?style=dark)
![GitHub Repo forks](https://img.shields.io/github/forks/x-nas/WinsockPacketEditor?style=dark)
[![Release](https://img.shields.io/github/v/release/x-nas/WinsockPacketEditor?sort=semver)](https://github.com/x-nas/WinsockPacketEditor/releases)

![Platform](https://img.shields.io/badge/platform-Windows%20x64-0078D6)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4)
![UI](https://img.shields.io/badge/UI-WPF%20%C2%B7%20MVVM-2563EB)

&bull; <a href="https://www.wpe64.com">官方网站</a>
&bull; <a href="https://www.wpe64.com">Official website</a>

</div>

## [⭐] 星星历史 · Star History

[![Star History Chart](https://api.star-history.com/svg?repos=x-nas/WinsockPacketEditor&type=Date)](https://www.star-history.com/#x-nas/WinsockPacketEditor&Date)

## [📚] 软件简介

**WPE x64** 是一款可拦截并修改 WinSock 封包的 Windows 软件，自适应支持 32 位与 64 位目标程序，提供 **进程注入** 与 **SOCKS 代理** 两种拦截模式，内置高级滤镜、自动化机器人、封包对比、端口映射与代理账号体系。核心基于 C# 多线程与消息队列（MQ）设计，实测拦截百万级封包依旧流畅稳定。

本版本在 **完整保留原生拦截内核** 的前提下，将主程序界面整体重构为 **WPF（.NET Framework 4.8，MVVM）白主题**：无边框自绘标题栏（内置最小化/最大化/关闭、无系统白条）、矢量定制图标、硬件加速的丝滑过渡，力求商业生产级的排版与观感。注入模式下运行于目标进程内部的封包编辑器仍沿用成熟的原生实现，确保功能 1:1、行为最稳。

## [📚] Introduction

**WPE x64** is a Windows tool that intercepts and modifies WinSock packets, with adaptive support for both 32-bit and 64-bit targets. It offers two capture modes — **process injection** and **SOCKS proxy** — plus advanced filters, automation robots, packet comparison, port mapping and a proxy account system. Built on C# multithreading and a message-queue (MQ) pipeline, it stays smooth and stable after intercepting millions of packets.

This edition keeps the original native capture kernel intact while rebuilding the main UI in **WPF (.NET Framework 4.8, MVVM) with a white theme**: a borderless custom title bar (built-in minimize/maximize/close, no system white strip), vector icons and hardware-accelerated transitions for a commercial-grade look and feel. The in-process packet editor used in injection mode still relies on the mature native implementation, guaranteeing 1:1 feature parity and rock-solid behavior.

## [🏗️] 架构 · Architecture

| 层 Layer | 内容 Content |
| --- | --- |
| **主程序 UI** `src/Wpe.App` | WPF + MVVM 白主题外壳：概览 / 封包编辑器 / 进程注入 / SOCKS 代理 / 端口映射 / 代理账号 / 运行日志 / 设置 / 关于 |
| **原生内核** `src/Wpe.Core` | Winsock 钩子、EasyHook 注入（x86/x64）、SOCKS 代理、EF6 + SQLite 持久化、OWIN 自托管 WebAPI、多语言与日志；注入模式下的封包编辑器沿用 WinForms 实现 |
| **解决方案** `src/WPE.sln` | 统一编译入口，主程序引用原生内核，UI 数据绑定直连原生数据结构以保证 1:1 |

> UI 全部通过数据绑定直连原生 `Socket_Cache.*` 等内核结构，不复制状态，保证代理/注入两模式与原始功能严格一致。

## [🎖️] 软件特色

- [x] 支持 **进程注入** 与 **SOCKS 代理** 两种模式，确保各种情况下都能拦截 Winsock 封包。
- [x] 代理模式支持多种主流代理协议与 SSL 安全协议，具备端口映射（本地/远程映射）与断点调试。
- [x] 内置代理账号体系：启停、密码、有效期、最大链接数与最大设备数限制、在线状态。
- [x] 可编程自动化机器人，满足触发条件时执行预定义指令集。
- [x] 消息队列缓存模式，封包依次入队即时显示，无需等待缓存结束。
- [x] 可自定义拦截的封包类型，已涵盖 WinSock 1.1 与 2.0 的 API。
- [x] 注入器与封包编辑器相对独立，可一次注入多个程序并分别获取其网络封包。
- [x] 支持对尚未运行的程序进行注入，从启动阶段即开始捕获全部封包。
- [x] 直观的封包对比：原始 vs 修改逐字节差异高亮，支持多种数据格式快速切换。
- [x] 便捷的封包搜索，支持多种数据格式的快速定位。
- [x] 批量发送封包，可自定义发送顺序与循环次数，支持导入导出与备注。
- [x] 强大的滤镜功能，支持高级滤镜，可自定义修改封包长度与修改次数。
- [x] 可注入各类模拟器，直接获取模拟器及其运行程序的网络封包。
- [x] 配置实时保存，下次启动自动带出上一次的设置。
- [x] 运行期间实时记录日志并支持导出，便于定位与反馈问题。
- [x] 支持 64 位 Windows 与 64 位目标程序，按目标进程类型自动调用 32/64 位动态库注入。
- [x] 所用 .NET 程序集无需注册到全局程序集缓存（GAC），简化使用与二次开发。
- [x] 多线程处理封包，不影响程序正常操作；拦截结束自动摘钩并释放资源，无资源/内存泄漏风险。
- [x] 支持多语言版本，方便不同国家和地区的用户使用。
- [x] **全新白主题 WPF 界面**：无边框自绘标题栏、矢量图标、丝滑过渡，商业生产级排版观感。

## [🎖️] Features

- [x] Supports both **process injection** and **SOCKS proxy** modes so packets can be captured in any scenario.
- [x] Proxy mode supports mainstream proxy protocols and SSL, with port mapping (local/remote) and breakpoint debugging.
- [x] Built-in proxy account system: enable/disable, password, expiry, link/device limits and online status.
- [x] Programmable automation robot that runs predefined instruction sets when triggers are met.
- [x] Message-queue caching: packets are enqueued and shown immediately, no wait for the cache to finish.
- [x] Customizable capture types, already covering WinSock 1.1 and 2.0 APIs.
- [x] Independent injector and editor — inject multiple programs at once and read each one's packets separately.
- [x] Inject a not-yet-running program and capture all its packets from startup.
- [x] Intuitive packet comparison: byte-level diff highlighting of original vs modified, with quick format switching.
- [x] Convenient packet search with fast location across multiple data formats.
- [x] Batch packet sending with custom order and loop counts, plus import/export and remarks.
- [x] Powerful filters, including advanced filters and customizable packet length / modification counts.
- [x] Inject emulators directly and capture packets from the emulator and its running programs.
- [x] Settings are saved in real time and restored automatically on the next launch.
- [x] Real-time logging with export during operation for easy troubleshooting.
- [x] Supports 64-bit Windows and 64-bit targets, auto-selecting 32/64-bit DLLs by target process type.
- [x] .NET assemblies need no GAC registration, simplifying usage and secondary development.
- [x] Multithreaded packet handling; hooks are released automatically after capture with no leak risk.
- [x] Multiple language versions for users worldwide.
- [x] **Brand-new white-theme WPF UI**: borderless custom title bar, vector icons and silky transitions — a commercial-grade look and feel.

## [🛠️] 构建 · Build

> 需要 Windows + Visual Studio 2022（含 .NET 桌面开发工作负载，目标框架 .NET Framework 4.8）。

```bash
# 使用 Visual Studio 打开并生成
src/WPE.sln

# 或命令行（MSBuild / dotnet）
dotnet build src/WPE.sln -c Release
```

- 主程序：`src/Wpe.App`（WPF 白主题 UI）
- 原生内核：`src/Wpe.Core`（钩子/注入/代理/持久化/WebAPI）
- 依赖经 NuGet 还原（EasyHook、EntityFramework 6 + SQLite、OWIN 等）。
- 运行环境需已安装 .NET Framework 4.8 运行库。

## [🖼️] 软件界面 · Software UI

<img width="1200" height="750" alt="Home" src="https://github.com/user-attachments/assets/fbdaba92-edf8-486c-905a-a92ebb523ea2" />

<img width="1450" height="802" alt="PacketList" src="https://github.com/user-attachments/assets/59f26b9d-e6df-4ccd-b3a4-9807d7db5ba8" />

<img width="1450" height="802" alt="Statistical" src="https://github.com/user-attachments/assets/9e5f5330-ebe0-4b3f-92eb-c13ec6329a78" />

<img width="1450" height="802" alt="Robot" src="https://github.com/user-attachments/assets/b7eb16b7-fee1-4381-8b7f-0ab6e49287a4" />

![111](https://github.com/user-attachments/assets/e33412c1-3a9f-41f8-b23e-aada6a1bb104)

![222](https://github.com/user-attachments/assets/6c9f6fa8-94a9-4aea-8119-2ebe152ff7c2)

## [⚠️] 免责声明 · Disclaimer

本软件仅供网络协议学习、调试与安全研究等合法用途。请在获得授权的前提下使用，任何因滥用造成的后果由使用者自行承担。<br/>
This software is intended for lawful purposes only, such as protocol learning, debugging and security research. Use it only with proper authorization; the user bears all responsibility for any misuse.

## [👏] 特别说明 · Special Note

本项目已加入 [DotNetGuide](https://github.com/YSGStudyHards/DotNetGuide) 列表。<br/>
本项目已加入 [dotNET China](https://gitee.com/dotnetchina) 组织。<br/>

![dotnetchina](https://images.gitee.com/uploads/images/2021/0324/120117_2da9922c_416720.png "132645_21007ea0_974299.png")
