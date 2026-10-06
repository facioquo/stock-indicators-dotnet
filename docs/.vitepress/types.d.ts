declare module '*.vue' {
  import type { DefineComponent } from 'vue'

  const component: DefineComponent<{}, {}, any>
  export default component
}

declare module 'vitepress-plugin-llms/vitepress-components/CopyOrDownloadAsMarkdownButtons.vue' {
  import type { DefineComponent } from 'vue'

  const CopyOrDownloadAsMarkdownButtons: DefineComponent<{}, {}, any>
  export default CopyOrDownloadAsMarkdownButtons
}
