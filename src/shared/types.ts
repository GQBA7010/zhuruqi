// 共享类型定义：主进程与渲染进程通过 IPC 交换的数据结构

export type ProxyProtocol = 'socks5' | 'http'

export type PayloadEncoding = 'utf8' | 'hex' | 'base64'

/** ccproxy 上游代理配置 */
export interface ProxyConfig {
  protocol: ProxyProtocol
  host: string // 公网 IP / 域名
  port: number // 端口号
  username?: string // 账号（可选）
  password?: string // 密码（可选）
}

/** 客户端一次发送请求 */
export interface ClientSendRequest {
  proxy: ProxyConfig
  /** 目标主机（经代理连接的最终目标） */
  targetHost: string
  targetPort: number
  /** 发送的数据 */
  payload: string
  encoding: PayloadEncoding
  /** 连接与等待响应的超时（毫秒） */
  timeoutMs: number
  /** 收到响应后是否保持连接（false = 收到即断开） */
  keepAlive: boolean
}

/** 客户端一次发送的结果 */
export interface ClientSendResult {
  ok: boolean
  error?: string
  /** 连接握手耗时（毫秒） */
  connectMs?: number
  /** 从发送到收到首字节的耗时（毫秒） */
  firstByteMs?: number
  /** 总耗时（毫秒） */
  totalMs?: number
  /** 已发送字节数 */
  bytesSent?: number
  /** 已接收字节数 */
  bytesReceived?: number
  /** 收到的响应（按请求编码解码） */
  response?: string
  /** 响应的十六进制预览 */
  responseHex?: string
}

export type ServerStatus = 'stopped' | 'listening'

export interface ServerConfig {
  host: string
  port: number
  /** 是否把收到的数据原样回传（echo），用于模拟“目标回传数据” */
  echo: boolean
  /** 自定义回包内容（非空时，收到数据后回复此内容） */
  replyPayload?: string
  /** 自定义回包的编码 */
  replyEncoding?: PayloadEncoding
}

export interface ServerState {
  status: ServerStatus
  host: string
  port: number
  connections: number
  totalConnections: number
  bytesReceived: number
  bytesSent: number
}

export type LogLevel = 'info' | 'success' | 'warn' | 'error' | 'data'

export interface LogEntry {
  id: string
  ts: number
  scope: 'client' | 'server'
  level: LogLevel
  message: string
}

/** 服务端收到的一条数据记录 */
export interface ServerDataEvent {
  ts: number
  remote: string // 来源地址 ip:port
  encoding: PayloadEncoding
  preview: string
  hexPreview: string
  bytes: number
}
