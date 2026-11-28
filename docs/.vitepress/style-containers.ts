import type MarkdownIt from 'markdown-it'
import type StateBlock from 'markdown-it/lib/rules_block/state_block.mjs'
import { INDICATOR_STYLES } from './theme/indicator-style-keys.ts'

// Indicator-style containers: `::: series`, `::: buffer`, and `::: stream`
// wrap the content that belongs to one indicator style. The reader's chosen
// style (see theme/indicator-style.ts) hides the other two with CSS, so one
// source page serves all three styles at one URL.

const OPEN = new RegExp(`^:{3,}\\s*(${INDICATOR_STYLES.join('|')})\\s*$`)
const ANY_OPEN = /^:{3,}\s*\S/
const CLOSE = /^:{3,}\s*$/
const FENCE = /^(`{3,}|~{3,})/

function lineText(state: StateBlock, line: number): string {
  const start = state.bMarks[line] + state.tShift[line]
  return state.src.slice(start, state.eMarks[line])
}

function styleContainer(state: StateBlock, startLine: number, endLine: number, silent: boolean): boolean {
  if (state.sCount[startLine] - state.blkIndent >= 4) return false

  const open = lineText(state, startLine).match(OPEN)
  if (!open) return false
  if (silent) return true

  // Find the matching close, counting nested `:::` containers and skipping
  // fenced code so a `:::` inside a code sample never closes the container.
  let depth = 1
  let fence: string | undefined
  let closed = false
  let line = startLine + 1
  for (; line < endLine; line++) {
    if (state.sCount[line] < state.blkIndent) break
    const text = lineText(state, line)
    const marker = text.match(FENCE)?.[1]
    if (marker && (!fence || marker.startsWith(fence))) {
      fence = fence ? undefined : marker
      continue
    }
    if (fence) continue
    if (CLOSE.test(text)) {
      if (--depth === 0) { closed = true; break }
    } else if (ANY_OPEN.test(text)) {
      depth++
    }
  }

  // An unclosed container would swallow the rest of the page into one style,
  // hiding it from readers of the other two: fail the build instead.
  if (!closed) {
    const page = (state.env as { relativePath?: string } | undefined)?.relativePath ?? 'unknown page'
    throw new Error(`Unclosed \`::: ${open[1]}\` container at line ${startLine + 1} of ${page}`)
  }

  const style = open[1]
  const openToken = state.push('style_container_open', 'div', 1)
  openToken.block = true
  openToken.map = [startLine, line]
  openToken.attrSet('class', `style-block style-${style}`)
  openToken.attrSet('data-style', style)

  const oldParent = state.parentType
  const oldLineMax = state.lineMax
  state.parentType = 'container' as typeof state.parentType
  state.lineMax = line
  state.md.block.tokenize(state, startLine + 1, line)
  state.parentType = oldParent
  state.lineMax = oldLineMax

  state.push('style_container_close', 'div', -1).block = true
  state.line = line + 1
  return true
}

export function styleContainers(md: MarkdownIt): void {
  md.block.ruler.before('fence', 'style_container', styleContainer, {
    alt: ['paragraph', 'reference', 'blockquote', 'list']
  })
}
