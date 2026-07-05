# 入侵器 · SOCKS5 / HTTP 数据收发工具

一个生产级桌面应用（Electron + React + TypeScript），实现「伪客户端 + 伪服务端」的数据收发：

- **伪客户端**：经 ccproxy 上游代理（**SOCKS5** 或 **HTTP CONNECT**，支持公网 IP / 端口 / 账号密码）连接目标，发送封包数据并接收目标回传的数据。封包支持 **十六进制 / UTF-8 / Base64** 三种编码，十六进制模式带校验与格式化。
- **伪服务端**：监听端口，接收目标（经代理转发）发来的数据，可开启 **echo 回传** 模拟目标回应。
- **UI**：深色玻璃拟态风格，Framer Motion 丝滑过渡，定制品牌图标。

## 技术栈

| 层 | 技术 |
| --- | --- |
| 桌面框架 | Electron 31 |
| 前端 | React 18 + TypeScript + Vite + Framer Motion |
| 构建 | electron-vite + electron-builder |
| 网络内核 | Node.js `net`，SOCKS5 使用 [`socks`](https://www.npmjs.com/package/socks)，HTTP 走 CONNECT 隧道 |

## 目录结构

```
src/
  main/            Electron 主进程
    core/
      client.ts        伪客户端核心（经代理发送/接收）
      server.ts        伪服务端核心（接收/回传）
      httpConnect.ts   HTTP CONNECT 隧道（含 Basic 认证）
    index.ts       窗口 + IPC
  preload/         安全桥（contextBridge 暴露 window.api）
  renderer/        React UI
    src/panels/    ClientPanel / ServerPanel
    src/components/ 图标、控件、日志控制台
  shared/          主/渲染进程共享类型与 IPC 通道
build/             应用图标（svg/png/ico）
```

## 开发

```bash
npm install
npm run dev          # 启动开发模式（热重载）
npm run typecheck    # 类型检查
npm run lint         # 代码检查
npm run build        # 编译产物到 out/
```

## 打包

```bash
npm run build:win    # Windows 安装包 (nsis)
npm run build:linux  # Linux AppImage
npm run build:mac    # macOS dmg
```

## 使用流程

1. 打开「伪服务端」，设置监听端口（如 `9000`），开启「回传」，点击 **启动监听**。
2. 打开「伪客户端」，填写 ccproxy 代理的公网 IP、端口、账号密码，选择协议（SOCKS5 / HTTP）。
3. 填写目标主机与端口（可指向伪服务端），选择编码为「十六进制」，输入封包数据。
4. 点击 **发送封包**，即可在结果区看到目标回传的数据，服务端也会实时显示收到的封包。

> 数据流：伪客户端 → ccproxy → 目标；目标 → 伪服务端（回传）。
