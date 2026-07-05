import React from 'react'

type IconProps = React.SVGProps<SVGSVGElement> & { size?: number }

const base = (size = 20): React.SVGProps<SVGSVGElement> => ({
  width: size,
  height: size,
  viewBox: '0 0 24 24',
  fill: 'none',
  stroke: 'currentColor',
  strokeWidth: 1.8,
  strokeLinecap: 'round' as const,
  strokeLinejoin: 'round' as const
})

/** 定制品牌图标：盾牌 + 数据流箭头，象征“经代理隧道收发数据”。 */
export const BrandLogo: React.FC<{ size?: number }> = ({ size = 40 }) => (
  <svg width={size} height={size} viewBox="0 0 48 48" fill="none">
    <defs>
      <linearGradient id="brandg" x1="0" y1="0" x2="48" y2="48">
        <stop offset="0" stopColor="#6ee7ff" />
        <stop offset="0.5" stopColor="#7c8cff" />
        <stop offset="1" stopColor="#b06bff" />
      </linearGradient>
    </defs>
    <path
      d="M24 3 41 9v11c0 11-7.4 20-17 25C14.4 40 7 31 7 20V9L24 3Z"
      fill="url(#brandg)"
      opacity="0.16"
    />
    <path
      d="M24 3 41 9v11c0 11-7.4 20-17 25C14.4 40 7 31 7 20V9L24 3Z"
      stroke="url(#brandg)"
      strokeWidth="2"
    />
    <path d="M15 20h13m0 0-4-4m4 4-4 4" stroke="url(#brandg)" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
    <circle cx="17" cy="28" r="1.6" fill="url(#brandg)" />
    <circle cx="24" cy="28" r="1.6" fill="url(#brandg)" />
    <circle cx="31" cy="28" r="1.6" fill="url(#brandg)" />
  </svg>
)

export const SendIcon: React.FC<IconProps> = ({ size, ...p }) => (
  <svg {...base(size)} {...p}>
    <path d="M22 2 11 13" />
    <path d="M22 2 15 22l-4-9-9-4 20-7Z" />
  </svg>
)

export const ClientIcon: React.FC<IconProps> = ({ size, ...p }) => (
  <svg {...base(size)} {...p}>
    <rect x="3" y="4" width="18" height="12" rx="2" />
    <path d="M8 20h8M12 16v4" />
  </svg>
)

export const ServerIcon: React.FC<IconProps> = ({ size, ...p }) => (
  <svg {...base(size)} {...p}>
    <rect x="3" y="4" width="18" height="7" rx="2" />
    <rect x="3" y="13" width="18" height="7" rx="2" />
    <path d="M7 7.5h.01M7 16.5h.01" />
  </svg>
)

export const PlayIcon: React.FC<IconProps> = ({ size, ...p }) => (
  <svg {...base(size)} {...p}>
    <path d="M6 4l14 8-14 8V4Z" fill="currentColor" stroke="none" />
  </svg>
)

export const StopIcon: React.FC<IconProps> = ({ size, ...p }) => (
  <svg {...base(size)} {...p}>
    <rect x="6" y="6" width="12" height="12" rx="2" fill="currentColor" stroke="none" />
  </svg>
)

export const TrashIcon: React.FC<IconProps> = ({ size, ...p }) => (
  <svg {...base(size)} {...p}>
    <path d="M3 6h18M8 6V4h8v2M6 6l1 14h10l1-14" />
  </svg>
)

export const LinkIcon: React.FC<IconProps> = ({ size, ...p }) => (
  <svg {...base(size)} {...p}>
    <path d="M10 13a5 5 0 0 0 7 0l3-3a5 5 0 0 0-7-7l-1 1" />
    <path d="M14 11a5 5 0 0 0-7 0l-3 3a5 5 0 0 0 7 7l1-1" />
  </svg>
)

export const MinimizeIcon: React.FC<IconProps> = ({ size = 14, ...p }) => (
  <svg {...base(size)} strokeWidth={1.6} {...p}>
    <path d="M5 12h14" />
  </svg>
)

export const MaximizeIcon: React.FC<IconProps> = ({ size = 14, ...p }) => (
  <svg {...base(size)} strokeWidth={1.6} {...p}>
    <rect x="5" y="5" width="14" height="14" rx="2.5" />
  </svg>
)

export const RestoreIcon: React.FC<IconProps> = ({ size = 14, ...p }) => (
  <svg {...base(size)} strokeWidth={1.6} {...p}>
    <rect x="7" y="7" width="12" height="12" rx="2.5" />
    <path d="M5 15V6a2 2 0 0 1 2-2h9" />
  </svg>
)

export const CloseIcon: React.FC<IconProps> = ({ size = 14, ...p }) => (
  <svg {...base(size)} strokeWidth={1.6} {...p}>
    <path d="M6 6l12 12M18 6 6 18" />
  </svg>
)
