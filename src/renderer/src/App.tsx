import React, { useEffect, useState } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import { BrandLogo, ClientIcon, ServerIcon } from './components/icons'
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
    <div className="app">
      <aside className="sidebar">
        <div className="brand">
          <BrandLogo size={40} />
          <div>
            <div className="brand-title">入侵器</div>
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
          <AnimatePresence mode="wait">
            <motion.div
              key={tab}
              initial={{ opacity: 0, x: 18 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -18 }}
              transition={{ duration: 0.28, ease: [0.22, 1, 0.36, 1] }}
            >
              {tab === 'client' ? <ClientPanel /> : <ServerPanel />}
            </motion.div>
          </AnimatePresence>
        </div>

        <Console logs={logs} onClear={() => setLogs([])} />
      </main>
    </div>
  )
}
