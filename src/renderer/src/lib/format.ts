import type { PayloadEncoding } from '../../../shared/types'

/** 归一化十六进制封包字符串：去掉空格/0x 前缀/常见分隔符，转小写。 */
export function normalizeHex(input: string): string {
  return input
    .replace(/0x/gi, '')
    .replace(/[^0-9a-fA-F]/g, '')
    .toLowerCase()
}

/** 把归一化后的 hex 按字节以空格分组，便于阅读。 */
export function groupHex(hex: string): string {
  return hex.replace(/(.{2})/g, '$1 ').trim()
}

export interface HexValidation {
  ok: boolean
  bytes: number
  error?: string
  normalized: string
}

/** 校验十六进制封包是否合法（偶数个 hex 字符）。 */
export function validateHex(input: string): HexValidation {
  const normalized = normalizeHex(input)
  if (normalized.length === 0) {
    return { ok: false, bytes: 0, error: '请输入十六进制封包数据', normalized }
  }
  if (normalized.length % 2 !== 0) {
    return {
      ok: false,
      bytes: Math.floor(normalized.length / 2),
      error: '十六进制字符数必须为偶数（每字节 2 位）',
      normalized
    }
  }
  return { ok: true, bytes: normalized.length / 2, normalized }
}

/** 估算某编码下 payload 的字节数。 */
export function estimateBytes(payload: string, encoding: PayloadEncoding): number {
  try {
    switch (encoding) {
      case 'hex':
        return validateHex(payload).bytes
      case 'base64':
        return payload ? Math.floor((payload.replace(/=+$/, '').length * 3) / 4) : 0
      default:
        return new TextEncoder().encode(payload).length
    }
  } catch {
    return 0
  }
}

export function formatBytes(n: number): string {
  if (n < 1024) return `${n} B`
  if (n < 1024 * 1024) return `${(n / 1024).toFixed(1)} KB`
  return `${(n / 1024 / 1024).toFixed(2)} MB`
}

export function fmtTime(ts: number): string {
  const d = new Date(ts)
  const p = (n: number): string => String(n).padStart(2, '0')
  return `${p(d.getHours())}:${p(d.getMinutes())}:${p(d.getSeconds())}`
}
