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
    <button type="button" @click="copyAsMarkdown">
      {{ copied ? 'Copied' : 'Copy page as Markdown' }}
    </button>
    <button type="button" @click="viewAsMarkdown">
      View as Markdown
    </button>
    <button type="button" @click="downloadMarkdown">
      {{ downloaded ? 'Downloaded' : 'Download Markdown' }}
    </button>
  </div>
</template>

<style scoped>
.markdown-page-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin: 16px 0 32px;
}

button {
  padding: 8px 16px;
  color: var(--vp-c-text-1);
  font-size: 14px;
  background: transparent;
  border: 1px solid var(--vp-c-divider);
  border-radius: 6px;
  cursor: pointer;
}

button:hover {
  color: var(--vp-c-brand-1);
  border-color: var(--vp-c-brand-1);
}
</style>
