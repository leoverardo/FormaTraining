import * as THREE from 'three';

export function createBrandSculpture(host, { onFailure }) {
  const renderer = new THREE.WebGLRenderer({ alpha: true, antialias: true, powerPreference: 'low-power' });
  renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.5));
  const scene = new THREE.Scene();
  const camera = new THREE.PerspectiveCamera(32, 1, 0.1, 30);
  camera.position.set(0, 0, 7);
  const sculpture = new THREE.Group();
  scene.add(sculpture);
  const colors = ['#6366f1', '#818cf8', '#06b6d4'];
  colors.forEach((color, index) => {
    const ring = new THREE.Mesh(new THREE.TorusGeometry(1.05 - index * 0.15, 0.07, 12, 64), new THREE.MeshStandardMaterial({ color, roughness: 0.32, metalness: 0.18 }));
    ring.rotation.set(0.6 + index * 0.75, index * 0.5, 0.2);
    sculpture.add(ring);
    const height = 0.35 + index * 0.25;
    const bar = new THREE.Mesh(new THREE.BoxGeometry(0.16, height, 0.16), new THREE.MeshStandardMaterial({ color, roughness: 0.5 }));
    bar.position.set((index - 1) * 0.3, height / 2 - 0.4, 0);
    sculpture.add(bar);
  });
  scene.add(new THREE.AmbientLight(0xffffff, 2));
  const light = new THREE.DirectionalLight(0xffffff, 4);
  light.position.set(3, 4, 5);
  scene.add(light);
  host.appendChild(renderer.domElement);
  host.dataset.renderer = 'webgl';
  let disposed = false;
  let visible = false;
  let frame = 0;
  let last = 0;
  let elapsed = 0;
  const render = timestamp => {
    frame = 0;
    if (disposed || !visible || document.hidden) return;
    if (timestamp - last >= 1000 / 30) {
      elapsed += Math.min(timestamp - last, 80) / 1000;
      last = timestamp;
      sculpture.rotation.set(0.12 + Math.sin(elapsed * 0.3) * 0.12, Math.sin(elapsed * 0.22) * 0.45, Math.sin(elapsed * 0.2) * 0.08);
      renderer.render(scene, camera);
    }
    frame = requestAnimationFrame(render);
  };
  const update = () => {
    if (frame) cancelAnimationFrame(frame);
    frame = 0;
    if (!disposed && visible && !document.hidden) { last = performance.now(); frame = requestAnimationFrame(render); }
  };
  const resize = new ResizeObserver(() => {
    if (disposed) return;
    const width = Math.max(1, host.clientWidth);
    const height = Math.max(1, host.clientHeight);
    renderer.setSize(width, height);
    camera.aspect = width / height;
    camera.updateProjectionMatrix();
  });
  resize.observe(host);
  const intersection = new IntersectionObserver(([entry]) => { visible = entry.isIntersecting; update(); });
  intersection.observe(host);
  document.addEventListener('visibilitychange', update);
  function dispose() {
    if (disposed) return;
    disposed = true;
    cancelAnimationFrame(frame);
    resize.disconnect(); intersection.disconnect();
    document.removeEventListener('visibilitychange', update);
    renderer.domElement.removeEventListener('webglcontextlost', onContextLost);
    sculpture.children.forEach(mesh => { mesh.geometry.dispose(); mesh.material.dispose(); });
    renderer.dispose(); renderer.forceContextLoss(); renderer.domElement.remove();
    host.dataset.renderer = 'fallback';
  }
  function onContextLost(event) { event.preventDefault(); dispose(); onFailure(); }
  renderer.domElement.addEventListener('webglcontextlost', onContextLost);
  return dispose;
}
