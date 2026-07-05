import React, { useEffect, useRef } from 'react'
import { TrashIcon } from './icons'
import type { LogEntry } from '../../../shared/types'
import { fmtTime } from '../lib/format'

export default function Console({
  logs,
  onClear
}: {
  logs: LogEntry[]
  onClear: () => void
}): React.JSX.Element {
  const bodyRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const el = bodyRef.current
    if (el) el.scrollTop = el.scrollHeight
  }, [logs])

  return (
    <div className="console">
      <div className="console-head">
        <span className="t">运行日志 · CONSOLE</span>
        <button
          className="btn ghost"
          onClick={onClear}
          style={{ padding: '6px 12px', fontSize: 12 }}
        >
          <TrashIcon size={13} /> 清空
        </button>
      </div>
      <div className="console-body" ref={bodyRef}>
        {logs.length === 0 ? (
          <div style={{ color: 'var(--text-faint)' }}>等待操作…</div>
        ) : (
          logs.map((l) => (
            <div className="log-line" key={l.id}>
              <span className="lt">{fmtTime(l.ts)}</span>
              <span className={`ls ${l.scope}`}>
                {l.scope === 'client' ? '客户端' : '服务端'}
              </span>
              <span className={`log-${l.level}`}>{l.message}</span>
            </div>
          ))
        )}
      </div>
    </div>
  )
}
