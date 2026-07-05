// IPC 通道名集中管理，避免主/渲染进程写错字符串

export const IPC = {
  // 渲染进程 -> 主进程（invoke）
  clientSend: 'client:send',
  serverStart: 'server:start',
  serverStop: 'server:stop',
  serverGetState: 'server:getState',

  // 窗口控制（无边框自定义标题栏）
  winMinimize: 'win:minimize',
  winToggleMaximize: 'win:toggle-maximize',
  winClose: 'win:close',

  // 主进程 -> 渲染进程（事件）
  logEvent: 'evt:log',
  serverStateEvent: 'evt:server-state',
  serverDataEvent: 'evt:server-data',
  winMaximizeEvent: 'evt:win-maximize'
} as const
