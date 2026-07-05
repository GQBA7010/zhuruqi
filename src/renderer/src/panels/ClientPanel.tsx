import React, { useMemo, useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { Segment, Switch } from '../components/controls'
import { SendIcon, LinkIcon } from '../components/icons'
import type {
  ClientSendRequest,
  ClientSendResult,
  PayloadEncoding,
  ProxyProtocol
} from '../../../shared/types'
import { estimateBytes, formatBytes, groupHex, validateHex } from '../lib/format'

const cardMotion = {
  initial: { opacity: 0, y: 16 },
  animate: { opacity: 1, y: 0 },
  transition: { duration: 0.35, ease: [0.22, 1, 0.36, 1] as [number, number, number, number] }
}

export default function ClientPanel(): React.JSX.Element {
  const [protocol, setProtocol] = useState<ProxyProtocol>('socks5')
  const [host, setHost] = useState('')
  const [port, setPort] = useState('1080')
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')

  const [targetHost, setTargetHost] = useState('')
  const [targetPort, setTargetPort] = useState('80')

  const [encoding, setEncoding] = useState<PayloadEncoding>('hex')
  const [payload, setPayload] = useState('')
  const [timeoutMs, setTimeoutMs] = useState('8000')
  const [keepAlive, setKeepAlive] = useState(false)

  const [sending, setSending] = useState(false)
  const [result, setResult] = useState<ClientSendResult | null>(null)

  const hexInfo = useMemo(() => validateHex(payload), [payload])
  const bytes = useMemo(() => estimateBytes(payload, encoding), [payload, encoding])

  const payloadInvalid = encoding === 'hex' && payload.length > 0 && !hexInfo.ok

  const canSend =
    !sending &&
    host.trim().length > 0 &&
    Number(port) > 0 &&
    targetHost.trim().length > 0 &&
    Number(targetPort) > 0 &&
    payload.trim().length > 0 &&
    !payloadInvalid

  const formatHex = (): void => {
    if (encoding === 'hex' && hexInfo.ok) setPayload(groupHex(hexInfo.normalized))
  }

  const send = async (): Promise<void> => {
    if (!canSend) return
    setSending(true)
    setResult(null)
    const req: ClientSendRequest = {
      proxy: {
        protocol,
        host: host.trim(),
        port: Number(port),
        username: username.trim() || undefined,
        password: password || undefined
      },
      targetHost: targetHost.trim(),
      targetPort: Number(targetPort),
      payload: encoding === 'hex' ? hexInfo.normalized : payload,
      encoding,
      timeoutMs: Number(timeoutMs) || 8000,
      keepAlive
    }
    try {
      const res = await window.api.clientSend(req)
      setResult(res)
    } finally {
      setSending(false)
    }
  }

  return (
    <div>
      <div className="page-head">
        <h1 className="page-title">伪客户端</h1>
        <p className="page-desc">
          经 ccproxy 上游代理（SOCKS5 / HTTP）连接目标，发送封包数据并接收目标回传的数据。
        </p>
      </div>

      <div className="grid grid-2">
        <motion.div className="card" {...cardMotion}>
          <h3 className="card-title">
            <span className="dot" /> ccproxy 上游代理
          </h3>

          <div className="field">
            <label>代理协议</label>
            <Segment
              idPrefix="proto"
              value={protocol}
              onChange={setProtocol}
              options={[
                { value: 'socks5', label: 'SOCKS5' },
                { value: 'http', label: 'HTTP' }
              ]}
            />
          </div>

          <div className="row">
            <div className="field" style={{ flex: 2 }}>
              <label>公网 IP / 域名</label>
              <input
                value={host}
                onChange={(e) => setHost(e.target.value)}
                placeholder="例如 203.0.113.10"
              />
            </div>
            <div className="field">
              <label>端口号</label>
              <input
                value={port}
                onChange={(e) => setPort(e.target.value.replace(/\D/g, ''))}
                placeholder="1080"
              />
            </div>
          </div>

          <div className="row">
            <div className="field">
              <label>账号（可选）</label>
              <input
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="username"
                autoComplete="off"
              />
            </div>
            <div className="field">
              <label>密码（可选）</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="password"
                autoComplete="off"
              />
            </div>
          </div>

          <div className="row">
            <div className="field" style={{ flex: 2 }}>
              <label>目标主机</label>
              <input
                value={targetHost}
                onChange={(e) => setTargetHost(e.target.value)}
                placeholder="经代理连接的最终目标 IP / 域名"
              />
            </div>
            <div className="field">
              <label>目标端口</label>
              <input
                value={targetPort}
                onChange={(e) => setTargetPort(e.target.value.replace(/\D/g, ''))}
                placeholder="80"
              />
            </div>
          </div>
        </motion.div>

        <motion.div
          className="card"
          {...cardMotion}
          transition={{ ...cardMotion.transition, delay: 0.06 }}
        >
          <h3 className="card-title">
            <span className="dot" /> 封包数据
          </h3>

          <div className="field">
            <label>数据编码</label>
            <Segment
              idPrefix="enc"
              value={encoding}
              onChange={setEncoding}
              options={[
                { value: 'hex', label: '十六进制' },
                { value: 'utf8', label: 'UTF-8' },
                { value: 'base64', label: 'Base64' }
              ]}
            />
          </div>

          <div className="field">
            <label>
              {encoding === 'hex' ? '十六进制封包（每字节两位，可含空格）' : '发送内容'}
            </label>
            <textarea
              className={encoding !== 'utf8' ? 'mono' : ''}
              value={payload}
              onChange={(e) => setPayload(e.target.value)}
              onBlur={formatHex}
              placeholder={
                encoding === 'hex'
                  ? '例如：47 45 54 20 2f 20 48 54 54 50 2f 31 2e 31 0d 0a'
                  : encoding === 'base64'
                    ? '粘贴 Base64 编码数据'
                    : '输入要发送的文本'
              }
              spellCheck={false}
            />
            <div className={`hint ${payloadInvalid ? 'err' : ''}`}>
              {payloadInvalid ? hexInfo.error : `约 ${formatBytes(bytes)}（${bytes} 字节）`}
            </div>
          </div>

          <div className="row" style={{ alignItems: 'flex-end' }}>
            <div className="field" style={{ marginBottom: 0 }}>
              <label>超时（毫秒）</label>
              <input
                value={timeoutMs}
                onChange={(e) => setTimeoutMs(e.target.value.replace(/\D/g, ''))}
              />
            </div>
            <div className="field" style={{ marginBottom: 0, display: 'flex', alignItems: 'flex-end' }}>
              <Switch checked={keepAlive} onChange={setKeepAlive} label="收到后保持连接" />
            </div>
          </div>

          <div style={{ marginTop: 18 }}>
            <motion.button
              className="btn"
              style={{ width: '100%' }}
              disabled={!canSend}
              onClick={send}
              whileTap={{ scale: 0.98 }}
            >
              <SendIcon size={18} />
              {sending ? '发送中…' : '发送封包'}
            </motion.button>
          </div>
        </motion.div>
      </div>

      <AnimatePresence>
        {result && (
          <motion.div
            className="card"
            style={{ marginTop: 18 }}
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -8 }}
            transition={{ duration: 0.3, ease: [0.22, 1, 0.36, 1] }}
          >
            <h3 className="card-title">
              <span
                className="dot"
                style={{ background: result.ok ? 'var(--ok)' : 'var(--err)', boxShadow: 'none' }}
              />
              收发结果 {result.ok ? '· 成功' : '· 失败'}
            </h3>

            {result.ok ? (
              <>
                <div className="result" style={{ marginBottom: 14 }}>
                  <div className="kv">
                    <span className="k">
                      <LinkIcon size={12} /> 隧道握手
                    </span>
                    <span className="v">{result.connectMs} ms</span>
                  </div>
                  <div className="kv">
                    <span className="k">首字节延迟</span>
                    <span className="v">
                      {result.firstByteMs != null ? `${result.firstByteMs} ms` : '—'}
                    </span>
                  </div>
                  <div className="kv">
                    <span className="k">总耗时</span>
                    <span className="v">{result.totalMs} ms</span>
                  </div>
                  <div className="kv">
                    <span className="k">发送 / 接收</span>
                    <span className="v">
                      {result.bytesSent} B / {result.bytesReceived} B
                    </span>
                  </div>
                </div>
                <label style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>
                  目标回传数据（解码）
                </label>
                <div className="pre" style={{ marginTop: 6 }}>
                  {result.response || '（无数据）'}
                </div>
                <label
                  style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600, display: 'block', marginTop: 12 }}
                >
                  十六进制预览
                </label>
                <div className="pre" style={{ marginTop: 6, color: 'var(--text-dim)' }}>
                  {result.responseHex || '（无数据）'}
                </div>
              </>
            ) : (
              <div className="hint err" style={{ fontSize: 13 }}>
                {result.error}
              </div>
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  )
}
