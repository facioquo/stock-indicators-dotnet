<script setup lang="ts">
import { ref } from 'vue'
import { useCopyOrDownloadAsMarkdownButtons } from 'vitepress-plugin-llms/vitepress-components'

const {
  copied,
  copyAsMarkdown,
  downloadMarkdown,
  downloaded,
  viewAsMarkdown
} = useCopyOrDownloadAsMarkdownButtons()

const menu = ref<HTMLDetailsElement>()

function closeMenu(): void {
  menu.value?.removeAttribute('open')
}
</script>

<template>
  <div class="markdown-page-actions" role="group" aria-label="Markdown page actions">
    <button class="primary-action" type="button" @click="copyAsMarkdown">
      <svg v-if="copied" aria-hidden="true" viewBox="0 0 20 20">
        <path d="m4.5 10.5 3.25 3.25L15.5 6" />
      </svg>
      <svg v-else aria-hidden="true" viewBox="0 0 20 20">
        <rect x="6.5" y="6.5" width="9" height="9" rx="1.5" />
        <path d="M13.5 6.5v-2a1 1 0 0 0-1-1h-8a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1h2" />
      </svg>
      {{ copied ? 'Copied' : 'Copy Markdown' }}
    </button>
    <details ref="menu">
      <summary aria-label="More Markdown actions">
        <svg aria-hidden="true" viewBox="0 0 20 20">
          <path d="m6 8 4 4 4-4" />
        </svg>
      </summary>
      <div class="action-menu">
        <button type="button" @click="viewAsMarkdown(); closeMenu()">
          <svg aria-hidden="true" viewBox="0 0 20 20">
            <path d="M3 10s2.5-4.5 7-4.5S17 10 17 10s-2.5 4.5-7 4.5S3 10 3 10Z" />
            <circle cx="10" cy="10" r="1.75" />
          </svg>
          <span>View as Markdown</span>
        </button>
        <button type="button" @click="downloadMarkdown(); closeMenu()">
          <svg aria-hidden="true" viewBox="0 0 20 20">
            <path d="M10 3.5v9m-3.5-3L10 13l3.5-3.5M4 16.5h12" />
          </svg>
          <span>
            {{ downloaded ? 'Downloaded' : 'Download Markdown' }}
          </span>
        </button>
      </div>
    </details>
  </div>
</template>

<style scoped>
.markdown-page-actions {
  position: relative;
  z-index: 2;
  float: right;
  display: flex;
  margin: -48px 0 16px 16px;
}

button,
summary {
  height: 34px;
  padding: 0 11px;
  color: var(--vp-c-text-1);
  font-family: inherit;
  font-size: 13px;
  font-weight: 600;
  line-height: 1;
  background: var(--vp-c-bg);
  border: 1px solid var(--vp-c-divider);
  cursor: pointer;
}

button:hover,
summary:hover {
  color: var(--vp-c-brand-2);
  border-color: var(--vp-c-brand-1);
}

button:focus-visible,
summary:focus-visible {
  position: relative;
  z-index: 1;
  outline: 2px solid var(--vp-c-brand-1);
  outline-offset: 2px;
}

svg {
  width: 16px;
  height: 16px;
  fill: none;
  stroke: currentcolor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 1.5;
}

.primary-action {
  display: flex;
  gap: 6px;
  align-items: center;
  border-radius: 6px 0 0 6px;
}

details {
  position: relative;
}

summary {
  display: flex;
  width: 34px;
  padding: 0;
  align-items: center;
  justify-content: center;
  border-left: 0;
  border-radius: 0 6px 6px 0;
  list-style: none;
}

details[open] summary {
  color: var(--vp-c-brand-1);
  background: var(--vp-c-bg-soft);
  border-color: var(--vp-c-brand-1);
}

details[open] summary svg {
  transform: rotate(180deg);
}

summary::-webkit-details-marker {
  display: none;
}

.action-menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  display: grid;
  width: max-content;
  min-width: 208px;
  padding: 5px;
  background: var(--vp-c-bg-elv);
  border-radius: 10px;
  box-shadow: 0 12px 32px rgb(0 0 0 / 18%);
}

.action-menu button {
  display: flex;
  gap: 10px;
  width: 100%;
  height: 38px;
  padding: 0 10px;
  align-items: center;
  color: var(--vp-c-text-1);
  text-align: left;
  white-space: nowrap;
  background: transparent;
  border: 0;
  border-radius: 6px;
}

.action-menu button:hover {
  color: var(--vp-c-text-1);
  background: var(--vp-c-bg-soft);
}

.action-menu button svg {
  color: var(--vp-c-text-2);
}

@media (max-width: 640px) {
  .markdown-page-actions {
    float: none;
    width: fit-content;
    margin: 16px 0 24px;
  }
}
</style>
