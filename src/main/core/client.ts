import net from 'node:net'
import { SocksClient } from 'socks'
import type {
  ClientSendRequest,
  ClientSendResult,
  PayloadEncoding
} from '../../shared/types'
import { httpConnectTunnel } from './httpConnect'

function decodePayload(payload: string, encoding: PayloadEncoding): Buffer {
  switch (encoding) {
    case 'hex':
      return Buffer.from(payload.replace(/\s+/g, ''), 'hex')
    case 'base64':
      return Buffer.from(payload, 'base64')
    default:
      return Buffer.from(payload, 'utf8')
  }
}

function encodeResponse(buf: Buffer, encoding: PayloadEncoding): string {
  switch (encoding) {
    case 'hex':
      return buf.toString('hex')
    case 'base64':
      return buf.toString('base64')
    default:
      return buf.toString('utf8')
  }
}

function hexPreview(buf: Buffer, max = 512): string {
  const slice = buf.subarray(0, max)
  const hex = slice.toString('hex').replace(/(.{2})/g, '$1 ').trim()
  return buf.length > max ? `${hex} …(+${buf.length - max}B)` : hex
}

/** 建立经上游代理到目标的 socket（SOCKS5 或 HTTP CONNECT）。 */
async function connectViaProxy(
  req: ClientSendRequest
): Promise<net.Socket> {
  if (req.proxy.protocol === 'socks5') {
    const info = await SocksClient.createConnection({
      proxy: {
        host: req.proxy.host,
        port: req.proxy.port,
        type: 5,
        userId: req.proxy.username || undefined,
        password: req.proxy.password || undefined
      },
      command: 'connect',
      destination: { host: req.targetHost, port: req.targetPort },
      timeout: req.timeoutMs
    })
    return info.socket
  }
  return httpConnectTunnel(
    req.proxy,
    req.targetHost,
    req.targetPort,
    req.timeoutMs
  )
}

/**
 * 客户端核心：经 ccproxy（SOCKS5/HTTP）连接目标，发送数据并接收目标回传的数据。
 * onLog 用于把过程实时推给 UI。
 */
export async function runClientSend(
  req: ClientSendRequest,
  onLog: (level: 'info' | 'success' | 'warn' | 'error' | 'data', msg: string) => void
): Promise<ClientSendResult> {
  const t0 = Date.now()
  const data = decodePayload(req.payload, req.encoding)

  onLog(
    'info',
    `经 ${req.proxy.protocol.toUpperCase()} 代理 ${req.proxy.host}:${req.proxy.port} 连接目标 ${req.targetHost}:${req.targetPort} …`
  )

  let socket: net.Socket
  try {
    socket = await connectViaProxy(req)
  } catch (err) {
    const msg = err instanceof Error ? err.message : String(err)
    onLog('error', `代理连接失败：${msg}`)
    return { ok: false, error: msg }
  }

  const connectMs = Date.now() - t0
  onLog('success', `隧道已建立（${connectMs}ms），开始发送 ${data.length} 字节`)

  return await new Promise<ClientSendResult>((resolve) => {
    const chunks: Buffer[] = []
    let firstByteMs: number | undefined
    let settled = false
    const sendAt = Date.now()

    const timer = setTimeout(() => finish(), req.timeoutMs)

    const finish = (error?: string): void => {
      if (settled) return
      settled = true
      clearTimeout(timer)
      socket.destroy()

      const received = Buffer.concat(chunks)
      if (error) {
        onLog('error', `发送失败：${error}`)
        resolve({ ok: false, error, connectMs, bytesSent: data.length })
        return
      }
      onLog(
        'success',
        `收到目标回传 ${received.length} 字节，本次完成`
      )
      resolve({
        ok: true,
        connectMs,
        firstByteMs,
        totalMs: Date.now() - t0,
        bytesSent: data.length,
        bytesReceived: received.length,
        response: encodeResponse(received, req.encoding),
        responseHex: hexPreview(received)
      })
    }

    socket.on('data', (chunk: Buffer) => {
      if (firstByteMs === undefined) {
        firstByteMs = Date.now() - sendAt
        onLog('info', `收到首字节（${firstByteMs}ms）`)
      }
      chunks.push(chunk)
      onLog('data', `← 收到 ${chunk.length} 字节`)
      if (!req.keepAlive) {
        // 非保持连接：给对端一点时间把剩余数据发完
        clearTimeout(timer)
        setTimeout(() => finish(), 150)
      }
    })

    socket.on('error', (err) => finish(err.message))
    socket.on('end', () => finish())
    socket.on('close', () => finish())

    socket.write(data, (err) => {
      if (err) finish(err.message)
      else onLog('data', `→ 已发送 ${data.length} 字节`)
    })
  })
}
