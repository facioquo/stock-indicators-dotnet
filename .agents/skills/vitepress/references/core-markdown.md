---
name: vitepress-markdown
description: Markdown extensions including frontmatter, custom containers, tables, anchors, and file includes
---

# Markdown Extensions

VitePress extends standard markdown with additional features for documentation.

## Frontmatter

YAML metadata at the top of markdown files:

```md
---
title: Page Title
description: Page description for SEO
layout: doc
outline: [2, 3]
---

# Content starts here
```

Access frontmatter in templates:

```md
# {{ $frontmatter.title }}
```

Or in script:

```vue
<script setup>
import { useData } from 'vitepress'
const { frontmatter } = useData()
</script>
```

## Custom Containers

Styled callout blocks:

```md
::: info
This is an info box.
:::

::: tip
This is a tip.
:::

::: warning
This is a warning.
:::

::: danger
This is a dangerous warning.
:::

::: details Click to expand
Hidden content here.
:::
```

Custom titles:

```md
::: danger STOP
Do not proceed!
:::

::: details Click me {open}
Open by default with {open} attribute.
:::
```

## GitHub-flavored Alerts

Alternative syntax using blockquotes:

```md
> [!NOTE]
> Highlights information users should know.

> [!TIP]
> Optional information for success.

> [!WARNING]
> Critical content requiring attention.

> [!CAUTION]
> Negative potential consequences.
```

## Header Anchors

Headers get automatic anchor links. Custom anchors:

```md
# My Heading {#custom-anchor}

[Link to heading](#custom-anchor)
```

## Table of Contents

Generate a TOC with:

```md
[[toc]]
```

## GitHub-Style Tables

```md
| Feature | Status |
|---------|--------|
| SSR     | ✅     |
| HMR     | ✅     |
```

## Emoji

Use shortcodes:

```md
:tada: :rocket: :100:
```

## File Includes

Include content from other files:

```md
<!--@include: ./shared/header.md-->
```

With line ranges:

```md
<!--@include: ./code.md{3,10}-->  <!-- Lines 3-10 -->
<!--@include: ./code.md{3,}-->   <!-- From line 3 -->
<!--@include: ./code.md{,10}-->  <!-- Up to line 10 -->
```

With regions:

```md
<!-- In parts/basics.md -->
<!-- #region usage -->
Usage content here
<!-- #endregion usage -->

<!-- Include just that region -->
<!--@include: ./parts/basics.md#usage-->
```

## Code Snippet Import

Import code from files:

```md
<<< @/snippets/example.js
```

With line highlighting:

```md
<<< @/snippets/example.js{2,4-6}
```

With language override:

```md
<<< @/snippets/example.cs{1,2 c#:line-numbers}
```

Import specific region:

```md
<<< @/snippets/example.js#regionName{1,2}
```

## Code Groups

Tab groups for code variants:

````md
::: code-group

```js [config.js]
export default { /* ... */ }
```

```ts [config.ts]
export default defineConfig({ /* ... */ })
```

:::
````

Import files in code groups:

```md
::: code-group

<<< @/snippets/config.js
<<< @/snippets/config.ts

:::
```

## Math Equations

Requires setup:

```bash
npm add -D markdown-it-mathjax3@^4
```

```ts
// .vitepress/config.ts
export default {
  markdown: {
    math: true
  }
}
```

Then use LaTeX:

```md
Inline: $E = mc^2$

Block:
$$
\frac{-b \pm \sqrt{b^2-4ac}}{2a}
$$
```

## Image Lazy Loading

```ts
export default {
  markdown: {
    image: {
      lazyLoading: true
    }
  }
}
```

## Raw Container

Prevent VitePress style conflicts:

```md
::: raw
<CustomComponent />
:::
```

## Key Points

- Frontmatter supports YAML or JSON format
- Custom containers support info, tip, warning, danger, details
- `[[toc]]` generates table of contents
- `@` in imports refers to source root (or `srcDir` if configured)
- Code groups create tabbed code blocks
- Math support requires markdown-it-mathjax3 package

<!--
Source references:
- https://vitepress.dev/guide/markdown
- https://vitepress.dev/guide/frontmatter
-->
