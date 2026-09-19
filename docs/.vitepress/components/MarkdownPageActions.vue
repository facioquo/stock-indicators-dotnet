<script setup lang="ts">
import { useCopyOrDownloadAsMarkdownButtons } from 'vitepress-plugin-llms/vitepress-components'

const {
  copied,
  copyAsMarkdown,
  downloadMarkdown,
  downloaded,
  viewAsMarkdown
} = useCopyOrDownloadAsMarkdownButtons()
</script>

<template>
  <div class="markdown-page-actions" role="group" aria-label="Markdown page actions">
    <button class="primary-action" type="button" @click="copyAsMarkdown">
      <span aria-hidden="true">⧉</span>
      {{ copied ? 'Copied' : 'Copy Markdown' }}
    </button>
    <details>
      <summary aria-label="More Markdown actions">⌄</summary>
      <div class="action-menu">
        <button type="button" @click="viewAsMarkdown">View as Markdown</button>
        <button type="button" @click="downloadMarkdown">
          {{ downloaded ? 'Downloaded' : 'Download Markdown' }}
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
  min-height: 32px;
  padding: 4px 10px;
  color: var(--vp-c-text-1);
  font-size: 14px;
  font-weight: 500;
  background: var(--vp-c-bg-soft);
  border: 1px solid var(--vp-c-divider);
  cursor: pointer;
}

button:hover,
summary:hover {
  color: var(--vp-c-brand-1);
  border-color: var(--vp-c-brand-1);
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
  display: grid;
  min-width: 32px;
  padding: 4px;
  place-items: center;
  border-left: 0;
  border-radius: 0 6px 6px 0;
  list-style: none;
}

summary::-webkit-details-marker {
  display: none;
}

.action-menu {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  display: grid;
  min-width: 170px;
  padding: 6px;
  background: var(--vp-c-bg-elv);
  border: 1px solid var(--vp-c-divider);
  border-radius: 8px;
  box-shadow: var(--vp-shadow-3);
}

.action-menu button {
  text-align: left;
  background: transparent;
  border: 0;
  border-radius: 4px;
}

@media (max-width: 640px) {
  .markdown-page-actions {
    float: none;
    width: fit-content;
    margin: 16px 0 24px;
  }
}
</style>
