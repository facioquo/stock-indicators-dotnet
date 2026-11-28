<script setup lang="ts">
import { useId } from 'vue'
import { withBase } from 'vitepress'
import {
  INDICATOR_STYLES,
  INDICATOR_STYLE_LABELS,
  useIndicatorStyle
} from '../theme/indicator-style.ts'

// `nav` sits beside the appearance toggle; `screen` is the mobile menu copy;
// `page` is the in-page copy at the top of indicator pages.
const props = withDefaults(defineProps<{ placement?: 'nav' | 'screen' | 'page' }>(), {
  placement: 'page'
})

const { style, setStyle } = useIndicatorStyle()
const name = `indicator-style-${useId()}`
</script>

<template>
  <div class="style-switch" :class="`style-switch--${props.placement}`">
    <a
      v-if="props.placement !== 'nav'"
      class="style-switch__label"
      :id="`${name}-label`"
      :href="withBase('/guide/styles/')"
      title="Compare indicator styles"
    >Indicator style</a>
    <div
      class="style-switch__group"
      role="radiogroup"
      :aria-labelledby="props.placement !== 'nav' ? `${name}-label` : undefined"
      :aria-label="props.placement === 'nav' ? 'Indicator style' : undefined"
      :title="props.placement === 'nav' ? 'Indicator style shown on indicator pages' : undefined"
    >
      <label
        v-for="option in INDICATOR_STYLES"
        :key="option"
        class="style-switch__option"
        :data-option="option"
      >
        <input
          type="radio"
          :name="name"
          :value="option"
          :checked="style === option"
          @change="setStyle(option)"
        >
        <span>{{ INDICATOR_STYLE_LABELS[option] }}</span>
      </label>
    </div>
  </div>
</template>

<style scoped>
.style-switch {
  display: flex;
  align-items: center;
  gap: 12px;
}

.style-switch--nav {
  margin-left: 16px;
}

.style-switch--page {
  margin: 16px 0;
}

.style-switch--screen {
  justify-content: space-between;
  margin-top: 24px;
  gap: 8px;
  padding: 12px 8px 12px 16px;
  border-radius: 8px;
  background-color: var(--vp-c-bg-soft);
}

.style-switch__label {
  font-size: 14px;
  font-weight: 500;
  color: var(--vp-c-text-2);
  text-decoration: none;
}

.style-switch--screen .style-switch__option span {
  padding: 2px 8px;
}

/* matches the Appearance row above it */
.style-switch--screen .style-switch__label {
  font-size: 12px;
  line-height: 24px;
  white-space: nowrap;
}

.style-switch__label:hover {
  color: var(--vp-c-brand-1);
}

.style-switch__group {
  display: inline-flex;
  padding: 2px;
  border: 1px solid var(--vp-c-divider);
  border-radius: 8px;
  background-color: var(--vp-c-bg-alt);
}

.style-switch__option {
  position: relative;
  cursor: pointer;
}

.style-switch__option input {
  position: absolute;
  opacity: 0;
  width: 1px;
  height: 1px;
  pointer-events: none;
}

.style-switch__option span {
  display: block;
  padding: 2px 10px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  line-height: 24px;
  color: var(--vp-c-text-2);
  transition: color 0.25s, background-color 0.25s;
}

.style-switch__option:hover span {
  color: var(--vp-c-text-1);
}

/* The active segment is styled in custom.scss from <html data-indicator-style>,
   which the early head script sets before hydration, so it never flickers. */

.style-switch__option input:focus-visible + span {
  outline: 2px solid var(--vp-c-brand-1);
  outline-offset: 1px;
}

/* every segment carries a Canvas border, so the Highlight border on the
   active one (custom.scss) keeps the segment size */
@media (forced-colors: active) {
  .style-switch__option span {
    padding: 0 8px;
    border: 2px solid Canvas;
  }

  .style-switch--screen .style-switch__option span {
    padding: 0 6px;
  }
}

/* the mobile menu carries its own copy */
@media (max-width: 767px) {
  .style-switch--nav {
    display: none;
  }
}
</style>
