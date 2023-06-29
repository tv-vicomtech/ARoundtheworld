interface Scripts {
  name: string;
  src: string;
  type: string;
}
export const ScriptStore: Scripts[] = [
  { name: 'main', src: 'assets/client/public/js/main.js', type: 'module' },
  {
    name: 'mainReceiver',
    src: 'assets/client/public/receiver/js/main.js',
    type: 'module',
  },
  {
    name: 'min',
    src: 'https://unpkg.com/event-target@latest/min.js',
    type: 'text/javascript',
  },
  {
    name: 'ResizeObserver',
    src:
      'https://unpkg.com/resize-observer-polyfill@1.5.0/dist/ResizeObserver.global.js',
    type: 'text/javascript',
  },
  {
    name: 'intersectionObserver',
    src:
      'https://cdn.polyfill.io/v2/polyfill.min.js?features=IntersectionObserver',
    type: 'text/javascript',
  },
  {
    name: 'adapter',
    src: 'https://webrtc.github.io/adapter/adapter-latest.js',
    type: 'text/javascript',
  },
];
