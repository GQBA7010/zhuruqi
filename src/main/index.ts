import { app, shell, BrowserWindow, ipcMain } from 'electron'
import { join } from 'node:path'
import { IPC } from '../shared/ipc'
import type {
  ClientSendRequest,
  ServerConfig,
  LogLevel,
  ServerState,
  ServerDataEvent
} from '../shared/types'
import { runClientSend } from './core/client'
import { ReceiverServer } from './core/server'

let mainWindow: BrowserWindow | null = null
const receiver = new ReceiverServer()

function sendToRenderer(channel: string, ...args: unknown[]): void {
  if (mainWindow && !mainWindow.isDestroyed()) {
    mainWindow.webContents.send(channel, ...args)
  }
}

function pushLog(scope: 'client' | 'server', level: LogLevel, message: string): void {
  sendToRenderer(IPC.logEvent, {
    id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
    ts: Date.now(),
    scope,
    level,
    message
  })
}

function createWindow(): void {
  mainWindow = new BrowserWindow({
    width: 1180,
    height: 780,
    minWidth: 940,
    minHeight: 640,
    show: false,
    frame: false,
    autoHideMenuBar: true,
    title: 'Cac',
    icon: join(__dirname, '../../build/icon.png'),
    backgroundColor: '#eef1f8',
    titleBarStyle: process.platform === 'darwin' ? 'hidden' : 'hidden',
    trafficLightPosition: { x: 14, y: 14 },
    webPreferences: {
      preload: join(__dirname, '../preload/index.js'),
      sandbox: false,
      contextIsolation: true,
      nodeIntegration: false
    }
  })

  mainWindow.on('ready-to-show', () => mainWindow?.show())

  const emitMaximize = (): void =>
    sendToRenderer(IPC.winMaximizeEvent, mainWindow?.isMaximized() ?? false)
  mainWindow.on('maximize', emitMaximize)
  mainWindow.on('unmaximize', emitMaximize)

  mainWindow.webContents.setWindowOpenHandler((details) => {
    shell.openExternal(details.url)
    return { action: 'deny' }
  })

  if (process.env['ELECTRON_RENDERER_URL']) {
    mainWindow.loadURL(process.env['ELECTRON_RENDERER_URL'])
  } else {
    mainWindow.loadFile(join(__dirname, '../renderer/index.html'))
  }
}

// 服务端事件转发到渲染进程
receiver.on('log', (level: LogLevel, msg: string) => pushLog('server', level, msg))
receiver.on('state', (state: ServerState) => sendToRenderer(IPC.serverStateEvent, state))
receiver.on('data', (evt: ServerDataEvent) => sendToRenderer(IPC.serverDataEvent, evt))

function registerIpc(): void {
  ipcMain.handle(IPC.clientSend, async (_e, req: ClientSendRequest) => {
    return runClientSend(req, (level, msg) => pushLog('client', level, msg))
  })

  ipcMain.handle(IPC.serverStart, async (_e, config: ServerConfig) => {
    try {
      const state = await receiver.start(config)
      return { ok: true, state }
    } catch (err) {
      const message = err instanceof Error ? err.message : String(err)
      pushLog('server', 'error', `启动失败：${message}`)
      return { ok: false, error: message }
    }
  })

  ipcMain.handle(IPC.serverStop, async () => {
    const state = await receiver.stop()
    return { ok: true, state }
  })

  ipcMain.handle(IPC.serverGetState, async () => receiver.getState())

  ipcMain.on(IPC.winMinimize, () => mainWindow?.minimize())
  ipcMain.on(IPC.winToggleMaximize, () => {
    if (!mainWindow) return
    if (mainWindow.isMaximized()) mainWindow.unmaximize()
    else mainWindow.maximize()
  })
  ipcMain.on(IPC.winClose, () => mainWindow?.close())
}

app.whenReady().then(() => {
  registerIpc()
  createWindow()

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow()
  })
})

app.on('window-all-closed', () => {
  receiver.stop().finally(() => {
    if (process.platform !== 'darwin') app.quit()
  })
})
