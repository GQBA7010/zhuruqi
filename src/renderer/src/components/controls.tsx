import React from 'react'
import { motion } from 'framer-motion'

interface SegmentOption<T extends string> {
  value: T
  label: string
}

export function Segment<T extends string>({
  options,
  value,
  onChange,
  idPrefix
}: {
  options: SegmentOption<T>[]
  value: T
  onChange: (v: T) => void
  idPrefix: string
}): React.JSX.Element {
  return (
    <div className="segment">
      {options.map((opt) => {
        const active = opt.value === value
        return (
          <button
            key={opt.value}
            className={active ? 'active' : ''}
            onClick={() => onChange(opt.value)}
            type="button"
          >
            {active && (
              <motion.span
                layoutId={`${idPrefix}-seg`}
                className="seg-pill"
                transition={{ type: 'spring', stiffness: 380, damping: 30 }}
              />
            )}
            <span>{opt.label}</span>
          </button>
        )
      })}
    </div>
  )
}

export function Switch({
  checked,
  onChange,
  label
}: {
  checked: boolean
  onChange: (v: boolean) => void
  label: string
}): React.JSX.Element {
  return (
    <div
      className={`switch ${checked ? 'on' : ''}`}
      onClick={() => onChange(!checked)}
      role="switch"
      aria-checked={checked}
    >
      <div className="track">
        <motion.div
          className="knob"
          layout
          transition={{ type: 'spring', stiffness: 500, damping: 34 }}
          style={{ marginLeft: checked ? 18 : 0 }}
        />
      </div>
      <span>{label}</span>
    </div>
  )
}
