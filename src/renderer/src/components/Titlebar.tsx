import React, { useEffect, useState } from 'react'
import { BrandLogo, MinimizeIcon, MaximizeIcon, RestoreIcon, CloseIcon } from './icons'

export default function Titlebar(): React.JSX.Element {
  const [maximized, setMaximized] = useState(false)

  useEffect(() => window.api.onWinMaximize(setMaximized), [])

  return (
    <div className="titlebar">
      <div className="titlebar-brand">
        <BrandLogo size={22} />
        <span className="titlebar-name">Cac</span>
        <span className="titlebar-sub">SOCKS5 / HTTP 数据收发</span>
      </div>

      <div className="titlebar-controls">
        <button
          className="win-btn"
          title="最小化"
          onClick={() => window.api.winMinimize()}
        >
          <MinimizeIcon />
        </button>
        <button
          className="win-btn"
          title={maximized ? '还原' : '最大化'}
          onClick={() => window.api.winToggleMaximize()}
        >
          {maximized ? <RestoreIcon /> : <MaximizeIcon />}
        </button>
        <button
          className="win-btn danger"
          title="关闭"
          onClick={() => window.api.winClose()}
        >
          <CloseIcon />
        </button>
      </div>
    </div>
  )
}
