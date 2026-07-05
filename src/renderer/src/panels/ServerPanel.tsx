import React, { useEffect, useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { Switch } from '../components/controls'
import { PlayIcon, StopIcon, TrashIcon } from '../components/icons'
import type { ServerDataEvent, ServerState } from '../../../shared/types'
import { formatBytes, fmtTime } from '../lib/format'

const cardMotion = {
  initial: { opacity: 0, y: 16 },
  animate: { opacity: 1, y: 0 },
  transition: { duration: 0.35, ease: [0.22, 1, 0.36, 1] as [number, number, number, number] }
}

export default function ServerPanel(): React.JSX.Element {
  const [host, setHost] = useState('0.0.0.0')
  const [port, setPort] = useState('9000')
  const [echo, setEcho] = useState(true)
  const [busy, setBusy] = useState(false)
  const [state, setState] = useState<ServerState | null>(null)
  const [data, setData] = useState<ServerDataEvent[]>([])
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    window.api.serverGetState().then(setState)
    const offState = window.api.onServerState(setState)
    const offData = window.api.onServerData((evt) =>
      setData((prev) => [evt, ...prev].slice(0, 200))
    )
    return () => {
      offState()
      offData()
    }
  }, [])

  const listening = state?.status === 'listening'

  const toggle = async (): Promise<void> => {
    setBusy(true)
    setError(null)
    try {
      if (listening) {
        const res = await window.api.serverStop()
        if (res.state) setState(res.state)
      } else {
        const res = await window.api.serverStart({
          host: host.trim() || '0.0.0.0',
          port: Number(port),
          echo
        })
        if (!res.ok) setError(res.error ?? '启动失败')
        else if (res.state) setState(res.state)
      }
    } finally {
      setBusy(false)
    }
  }

  return (
    <div>
      <div className="page-head">
        <h1 className="page-title">伪服务端</h1>
        <p className="page-desc">
          监听端口，接收目标（经代理转发）发来的数据；开启回传后原样发回，模拟目标回应。
        </p>
      </div>

      <div className="grid grid-2">
        <motion.div className="card" {...cardMotion}>
          <h3 className="card-title">
            <span className="dot" /> 监听配置
          </h3>

          <div className="row">
            <div className="field" style={{ flex: 2 }}>
              <label>绑定地址</label>
              <input
                value={host}
                onChange={(e) => setHost(e.target.value)}
                placeholder="0.0.0.0"
                disabled={listening}
              />
            </div>
            <div className="field">
              <label>端口</label>
              <input
                value={port}
                onChange={(e) => setPort(e.target.value.replace(/\D/g, ''))}
                placeholder="9000"
                disabled={listening}
              />
            </div>
          </div>

          <div className="field">
            <Switch checked={echo} onChange={setEcho} label="回传收到的数据（echo）" />
          </div>

          <div style={{ display: 'flex', gap: 12, alignItems: 'center', marginTop: 6 }}>
            <motion.button
              className={`btn ${listening ? 'danger' : ''}`}
              disabled={busy || (!listening && Number(port) <= 0)}
              onClick={toggle}
              whileTap={{ scale: 0.98 }}
            >
              {listening ? <StopIcon size={16} /> : <PlayIcon size={16} />}
              {listening ? '停止监听' : '启动监听'}
            </motion.button>
            <span className={`badge ${listening ? 'listening' : 'stopped'}`}>
              <span className="bdot" />
              {listening ? `监听中 ${state?.host}:${state?.port}` : '已停止'}
            </span>
          </div>
          {error && (
            <div className="hint err" style={{ marginTop: 10 }}>
              {error}
            </div>
          )}
        </motion.div>

        <motion.div
          className="card"
          {...cardMotion}
          transition={{ ...cardMotion.transition, delay: 0.06 }}
        >
          <h3 className="card-title">
            <span className="dot" /> 实时指标
          </h3>
          <div className="stats">
            <div className="stat">
              <div className="k">当前连接</div>
              <div className="v">{state?.connections ?? 0}</div>
            </div>
            <div className="stat">
              <div className="k">累计连接</div>
              <div className="v">{state?.totalConnections ?? 0}</div>
            </div>
            <div className="stat">
              <div className="k">已接收</div>
              <div className="v" style={{ fontSize: 16 }}>
                {formatBytes(state?.bytesReceived ?? 0)}
              </div>
            </div>
            <div className="stat">
              <div className="k">已回传</div>
              <div className="v" style={{ fontSize: 16 }}>
                {formatBytes(state?.bytesSent ?? 0)}
              </div>
            </div>
          </div>
        </motion.div>
      </div>

      <motion.div
        className="card"
        style={{ marginTop: 18 }}
        {...cardMotion}
        transition={{ ...cardMotion.transition, delay: 0.1 }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h3 className="card-title" style={{ margin: 0 }}>
            <span className="dot" /> 接收到的数据（{data.length}）
          </h3>
          <button className="btn ghost" onClick={() => setData([])} style={{ padding: '8px 14px' }}>
            <TrashIcon size={14} /> 清空
          </button>
        </div>

        <div style={{ marginTop: 14 }}>
          {data.length === 0 ? (
            <div className="empty">暂无数据。启动监听后，目标发来的封包将实时显示在这里。</div>
          ) : (
            <AnimatePresence initial={false}>
              {data.map((d, i) => (
                <motion.div
                  key={`${d.ts}-${i}`}
                  className="data-item"
                  initial={{ opacity: 0, x: -12 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ duration: 0.25 }}
                  layout
                >
                  <div className="meta">
                    <span>来自 {d.remote}</span>
                    <span>
                      {fmtTime(d.ts)} · {formatBytes(d.bytes)}
                    </span>
                  </div>
                  <div className="pre-label">文本预览</div>
                  <div className="pre" style={{ maxHeight: 100 }}>
                    {d.preview}
                  </div>
                  <div className="pre-label" style={{ marginTop: 8 }}>
                    十六进制
                  </div>
                  <div className="pre" style={{ maxHeight: 80, marginTop: 4, color: 'var(--text-dim)' }}>
                    {d.hexPreview}
                  </div>
                </motion.div>
              ))}
            </AnimatePresence>
          )}
        </div>
      </motion.div>
    </div>
  )
}
