import net from 'node:net'
import type { ProxyConfig } from '../../shared/types'

/**
 * 通过 HTTP 代理的 CONNECT 方法建立到目标的 TCP 隧道，返回一个已就绪的 socket。
 * 支持 Proxy-Authorization: Basic（账号密码）。
 */
export function httpConnectTunnel(
  proxy: ProxyConfig,
  targetHost: string,
  targetPort: number,
  timeoutMs: number
): Promise<net.Socket> {
  return new Promise((resolve, reject) => {
    const socket = net.connect({ host: proxy.host, port: proxy.port })
    let settled = false

    const timer = setTimeout(() => {
      if (settled) return
      settled = true
      socket.destroy()
      reject(new Error(`HTTP 代理连接超时（${timeoutMs}ms）`))
    }, timeoutMs)

    const fail = (err: Error): void => {
      if (settled) return
      settled = true
      clearTimeout(timer)
      socket.destroy()
      reject(err)
    }

    socket.once('error', (err) => fail(err))

    socket.once('connect', () => {
      const hostHeader = `${targetHost}:${targetPort}`
      let req =
        `CONNECT ${hostHeader} HTTP/1.1\r\n` + `Host: ${hostHeader}\r\n`
      if (proxy.username) {
        const token = Buffer.from(
          `${proxy.username}:${proxy.password ?? ''}`
        ).toString('base64')
        req += `Proxy-Authorization: Basic ${token}\r\n`
      }
      req += 'Proxy-Connection: Keep-Alive\r\n\r\n'
      socket.write(req)
    })

    let buffer = Buffer.alloc(0)
    const onData = (chunk: Buffer): void => {
      buffer = Buffer.concat([buffer, chunk])
      const headerEnd = buffer.indexOf('\r\n\r\n')
      if (headerEnd === -1) return // 响应头未接收完整

      const header = buffer.subarray(0, headerEnd).toString('utf8')
      const statusLine = header.split('\r\n')[0] ?? ''
      const match = statusLine.match(/^HTTP\/\d\.\d\s+(\d{3})/)
      const status = match ? parseInt(match[1], 10) : 0

      socket.removeListener('data', onData)

      if (status !== 200) {
        fail(new Error(`HTTP 代理拒绝 CONNECT：${statusLine.trim() || '无响应'}`))
        return
      }

      // 隧道建立成功。若代理在响应头后附带了目标数据，回填到 socket。
      const leftover = buffer.subarray(headerEnd + 4)
      if (leftover.length > 0) socket.unshift(leftover)

      settled = true
      clearTimeout(timer)
      resolve(socket)
    }

    socket.on('data', onData)
  })
}
