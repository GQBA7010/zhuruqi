import React, { useEffect, useState } from 'react'
import { motion } from 'framer-motion'
import { BrandLogo, ClientIcon, ServerIcon } from './components/icons'
import Titlebar from './components/Titlebar'
import Console from './components/Console'
import ClientPanel from './panels/ClientPanel'
import ServerPanel from './panels/ServerPanel'
import type { LogEntry } from '../../shared/types'

type Tab = 'client' | 'server'

const NAV: { key: Tab; label: string; icon: React.FC<{ size?: number }> }[] = [
  { key: 'client', label: '伪客户端', icon: ClientIcon },
  { key: 'server', label: '伪服务端', icon: ServerIcon }
]

export default function App(): React.JSX.Element {
  const [tab, setTab] = useState<Tab>('client')
  const [logs, setLogs] = useState<LogEntry[]>([])

  useEffect(() => {
    return window.api.onLog((entry) =>
      setLogs((prev) => [...prev, entry].slice(-500))
    )
  }, [])

  return (
    <div className="shell">
      <Titlebar />
      <div className="app">
        <aside className="sidebar">
          <div className="brand">
            <BrandLogo size={38} />
            <div>
              <div className="brand-title">Cac</div>
              <div className="brand-sub">SOCKS5 / HTTP · ccproxy</div>
            </div>
          </div>

          {NAV.map((n) => {
            const Icon = n.icon
            const active = tab === n.key
            return (
              <div
                key={n.key}
                className={`nav-item ${active ? 'active' : ''}`}
                onClick={() => setTab(n.key)}
              >
                {active && (
                  <motion.div
                    layoutId="nav-pill"
                    className="nav-pill"
                    transition={{ type: 'spring', stiffness: 380, damping: 32 }}
                  />
                )}
                <Icon size={20} />
                <span>{n.label}</span>
              </div>
            )
          })}

          <div className="sidebar-foot">
            伪客户端 → ccproxy → 目标
            <br />
            目标 → 伪服务端（回传）
          </div>
        </aside>

        <main className="main">
          <div className="content">
            <div className={`page-layer ${tab === 'client' ? 'active' : ''}`}>
              <ClientPanel />
            </div>
            <div className={`page-layer ${tab === 'server' ? 'active' : ''}`}>
              <ServerPanel />
            </div>
          </div>

          <Console logs={logs} onClear={() => setLogs([])} />
        </main>
      </div>
    </div>
  )
}
