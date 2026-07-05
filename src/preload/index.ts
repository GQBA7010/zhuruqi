import { contextBridge, ipcRenderer } from 'electron'
import { IPC } from '../shared/ipc'
import type {
  ClientSendRequest,
  ClientSendResult,
  ServerConfig,
  ServerState,
  ServerDataEvent,
  LogEntry
} from '../shared/types'

export interface Api {
  clientSend: (req: ClientSendRequest) => Promise<ClientSendResult>
  serverStart: (
    config: ServerConfig
  ) => Promise<{ ok: boolean; state?: ServerState; error?: string }>
  serverStop: () => Promise<{ ok: boolean; state?: ServerState }>
  serverGetState: () => Promise<ServerState>
  onLog: (cb: (entry: LogEntry) => void) => () => void
  onServerState: (cb: (state: ServerState) => void) => () => void
  onServerData: (cb: (evt: ServerDataEvent) => void) => () => void
}

const api: Api = {
  clientSend: (req) => ipcRenderer.invoke(IPC.clientSend, req),
  serverStart: (config) => ipcRenderer.invoke(IPC.serverStart, config),
  serverStop: () => ipcRenderer.invoke(IPC.serverStop),
  serverGetState: () => ipcRenderer.invoke(IPC.serverGetState),
  onLog: (cb) => {
    const listener = (_e: unknown, entry: LogEntry): void => cb(entry)
    ipcRenderer.on(IPC.logEvent, listener)
    return () => ipcRenderer.removeListener(IPC.logEvent, listener)
  },
  onServerState: (cb) => {
    const listener = (_e: unknown, state: ServerState): void => cb(state)
    ipcRenderer.on(IPC.serverStateEvent, listener)
    return () => ipcRenderer.removeListener(IPC.serverStateEvent, listener)
  },
  onServerData: (cb) => {
    const listener = (_e: unknown, evt: ServerDataEvent): void => cb(evt)
    ipcRenderer.on(IPC.serverDataEvent, listener)
    return () => ipcRenderer.removeListener(IPC.serverDataEvent, listener)
  }
}

contextBridge.exposeInMainWorld('api', api)
