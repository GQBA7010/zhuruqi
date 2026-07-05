import net from 'node:net'
import { EventEmitter } from 'node:events'
import type {
  ServerConfig,
  ServerState,
  ServerDataEvent
} from '../../shared/types'

function hexPreview(buf: Buffer, max = 512): string {
  const slice = buf.subarray(0, max)
  const hex = slice.toString('hex').replace(/(.{2})/g, '$1 ').trim()
  return buf.length > max ? `${hex} …(+${buf.length - max}B)` : hex
}

/**
 * 伪服务端：监听端口，接收目标（经代理转发）发来的数据，
 * 可选择原样回传（echo）以模拟“目标把数据发回来”。
 */
export class ReceiverServer extends EventEmitter {
  private server: net.Server | null = null
  private state: ServerState = {
    status: 'stopped',
    host: '0.0.0.0',
    port: 0,
    connections: 0,
    totalConnections: 0,
    bytesReceived: 0,
    bytesSent: 0
  }

  getState(): ServerState {
    return { ...this.state }
  }

  private emitState(): void {
    this.emit('state', this.getState())
  }

  start(config: ServerConfig): Promise<ServerState> {
    return new Promise((resolve, reject) => {
      if (this.server) {
        reject(new Error('服务端已在运行'))
        return
      }

      const server = net.createServer((socket) => {
        this.state.connections += 1
        this.state.totalConnections += 1
        const remote = `${socket.remoteAddress}:${socket.remotePort}`
        this.emit('log', 'success', `新连接：${remote}`)
        this.emitState()

        socket.on('data', (chunk: Buffer) => {
          this.state.bytesReceived += chunk.length
          const event: ServerDataEvent = {
            ts: Date.now(),
            remote,
            encoding: 'utf8',
            preview: chunk.toString('utf8').slice(0, 1024),
            hexPreview: hexPreview(chunk),
            bytes: chunk.length
          }
          this.emit('data', event)
          this.emit('log', 'data', `← 收到 ${chunk.length} 字节（来自 ${remote}）`)

          if (config.echo) {
            socket.write(chunk, (err) => {
              if (!err) {
                this.state.bytesSent += chunk.length
                this.emit('log', 'data', `→ 已回传 ${chunk.length} 字节到 ${remote}`)
                this.emitState()
              }
            })
          }
          this.emitState()
        })

        socket.on('error', (err) => {
          this.emit('log', 'warn', `连接错误 ${remote}：${err.message}`)
        })

        socket.on('close', () => {
          this.state.connections = Math.max(0, this.state.connections - 1)
          this.emit('log', 'info', `连接关闭：${remote}`)
          this.emitState()
        })
      })

      server.once('error', (err) => {
        this.server = null
        this.state.status = 'stopped'
        this.emitState()
        reject(err)
      })

      server.listen(config.port, config.host, () => {
        this.server = server
        this.state = {
          ...this.state,
          status: 'listening',
          host: config.host,
          port: config.port
        }
        this.emit('log', 'success', `伪服务端已监听 ${config.host}:${config.port}`)
        this.emitState()
        resolve(this.getState())
      })
    })
  }

  stop(): Promise<ServerState> {
    return new Promise((resolve) => {
      if (!this.server) {
        resolve(this.getState())
        return
      }
      const server = this.server
      this.server = null
      server.close(() => {
        this.state = {
          ...this.state,
          status: 'stopped',
          connections: 0
        }
        this.emit('log', 'info', '伪服务端已停止')
        this.emitState()
        resolve(this.getState())
      })
      // 关闭时不主动杀掉活跃连接，让其自然结束
    })
  }
}
