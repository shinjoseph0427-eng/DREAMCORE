// DREAM CORE — Web look-dev preview of the Unity prototype.
// Same corridor dimensions, materials and lighting as DreamcoreWorldBuilder.cs.

import * as THREE from 'three';
import { PointerLockControls } from 'three/addons/controls/PointerLockControls.js';
import { Water } from 'three/addons/objects/Water.js';
import { RoomEnvironment } from 'three/addons/environments/RoomEnvironment.js';
import { EffectComposer } from 'three/addons/postprocessing/EffectComposer.js';
import { RenderPass } from 'three/addons/postprocessing/RenderPass.js';
import { UnrealBloomPass } from 'three/addons/postprocessing/UnrealBloomPass.js';
import { ShaderPass } from 'three/addons/postprocessing/ShaderPass.js';
import { OutputPass } from 'three/addons/postprocessing/OutputPass.js';

// ----------------------------------------------------------------- dimensions
const ENTRY = 2.4;              // entry landing depth
const CORRIDOR = 19.6;          // compressed water section length
const ALCOVE = 2.25;            // pink landing depth at the far end
const TOTAL = ENTRY + CORRIDOR + ALCOVE;
const Z_WATER0 = ENTRY, Z_WATER1 = ENTRY + CORRIDOR;
const HALF_WATER = 1.25;        // water channel half width
const WALKWAY = 0.88;
const HALF_INNER = HALF_WATER + WALKWAY;
const WALL_H = 3.25;
const WALK_H = 0.48;            // walkway / landing height
const WATER_Y = 0.35;
const STEPS = 3, STEP_D = 0.34, RISE = WALK_H / (STEPS + 1);
const DOOR_W = 1.42, DOOR_H = 2.05;
const EYE = 1.48;
// second area: giant vaulted waterpark hall (local coords centered at SECOND_Z)
const HALL_W = 34;                    // ~6x the first corridor width
const HALL_D = 58;
const HALL_WALL = 6.2;                // vertical wall height before the vault springs
const HALL_ARCH = 8.4;                // arch rise above the spring -> apex ~14.6 m
const HALL_GAP = 5.0;                 // gap behind the corridor's far wall
const SECOND_Z = TOTAL + HALL_GAP + HALL_D / 2;
const SECOND_FLOOR = 0.0;             // hall deck height
const RIVER_HALF = 1.65;              // water half width
const RIVER_SURF = -0.12;             // water surface below the deck
const RIVER_BED = -1.15;              // channel bed (much deeper than the corridor)
const CURB_OUT = 2.42;                // curb outer offset from channel centerline
const SPAWN_LOCAL = { x: 12.4, z: -25.2 };

// ----------------------------------------------------------------- setup
const isTouchLike = matchMedia('(hover: none), (pointer: coarse)').matches || navigator.maxTouchPoints > 0;
const pixelRatioCap = isTouchLike ? 1.5 : 1.18;

const scene = new THREE.Scene();
scene.fog = new THREE.FogExp2(0xcbd8d0, 0.041);
scene.background = new THREE.Color(0xcbd8d0);

const camera = new THREE.PerspectiveCamera(53, innerWidth / innerHeight, 0.05, 150);
camera.position.set(0, WALK_H + EYE, 0.95);

const renderer = new THREE.WebGLRenderer({ antialias: true });
renderer.setSize(innerWidth, innerHeight);
renderer.setPixelRatio(Math.min(devicePixelRatio * (isTouchLike ? 0.82 : 0.72), pixelRatioCap));
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 0.82;
document.getElementById('app').appendChild(renderer.domElement);

const pmrem = new THREE.PMREMGenerator(renderer);
scene.environment = pmrem.fromScene(new RoomEnvironment(), 0.04).texture;
scene.environmentIntensity = 0.32;

const composer = new EffectComposer(renderer);
composer.addPass(new RenderPass(scene, camera));
const bloom = new UnrealBloomPass(new THREE.Vector2(innerWidth, innerHeight), 0.40, 0.88, 0.78);
composer.addPass(bloom);
const retroPass = new ShaderPass({
  uniforms: {
    tDiffuse: { value: null },
    time: { value: 0 },
    resolution: { value: new THREE.Vector2(innerWidth, innerHeight) },
  },
  vertexShader: `
    varying vec2 vUv;
    void main() {
      vUv = uv;
      gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
    }
  `,
  fragmentShader: `
    uniform sampler2D tDiffuse;
    uniform float time;
    uniform vec2 resolution;
    varying vec2 vUv;

    float hash(vec2 p) {
      return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
    }

    vec3 sampleSoft(vec2 uv) {
      vec2 px = 1.0 / resolution;
      vec3 c = texture2D(tDiffuse, uv).rgb * 0.42;
      c += texture2D(tDiffuse, uv + vec2(px.x, 0.0)).rgb * 0.10;
      c += texture2D(tDiffuse, uv - vec2(px.x, 0.0)).rgb * 0.10;
      c += texture2D(tDiffuse, uv + vec2(0.0, px.y)).rgb * 0.10;
      c += texture2D(tDiffuse, uv - vec2(0.0, px.y)).rgb * 0.10;
      c += texture2D(tDiffuse, uv + vec2(px.x, px.y) * 1.6).rgb * 0.09;
      c += texture2D(tDiffuse, uv - vec2(px.x, px.y) * 1.6).rgb * 0.09;
      return c;
    }

    void main() {
      vec2 uv = vUv;
      vec2 cc = uv - 0.5;
      float r2 = dot(cc, cc);

      // slight vintage barrel distortion with soft chromatic offsets
      vec2 warped = 0.5 + cc * (1.0 + r2 * 0.105);
      vec2 ca = cc * (0.0028 + r2 * 0.006);

      vec3 col;
      col.r = sampleSoft(warped + ca).r;
      col.g = sampleSoft(warped).g;
      col.b = sampleSoft(warped - ca).b;

      float luma = dot(col, vec3(0.299, 0.587, 0.114));
      vec3 bleed = sampleSoft(warped + vec2(0.003, -0.002));
      col = mix(col, bleed, smoothstep(0.58, 0.98, luma) * 0.24);

      col = pow(max(col, 0.0), vec3(0.96));
      col = mix(vec3(luma), col, 0.82);
      col *= vec3(1.05, 0.99, 0.91);
      col += vec3(0.030, 0.016, 0.026);

      float vignette = smoothstep(0.78, 0.20, length(cc * vec2(0.86, 1.12)));
      col *= mix(0.58, 1.06, vignette);

      float scan = sin((uv.y + time * 0.012) * resolution.y * 1.8) * 0.008;
      float grain = hash(floor(uv * resolution * 0.74) + time * 24.0) - 0.5;
      col += grain * 0.046 + scan;

      gl_FragColor = vec4(col, 1.0);
    }
  `,
});
composer.addPass(retroPass);
composer.addPass(new OutputPass());

function viewportSize() {
  return {
    width: Math.max(1, Math.round(innerWidth)),
    height: Math.max(1, Math.round(innerHeight)),
  };
}

function resizeViewport() {
  const { width, height } = viewportSize();
  camera.aspect = width / height;
  camera.updateProjectionMatrix();
  renderer.setPixelRatio(Math.min(devicePixelRatio * (isTouchLike ? 0.82 : 0.72), pixelRatioCap));
  renderer.setSize(width, height);
  composer.setSize(width, height);
  retroPass.uniforms.resolution.value.set(width, height);
}

addEventListener('resize', resizeViewport);
addEventListener('orientationchange', () => setTimeout(resizeViewport, 150));
if (window.visualViewport) window.visualViewport.addEventListener('resize', resizeViewport);

// ----------------------------------------------------------------- textures
function mulberry32(a) {
  return () => {
    a |= 0; a = a + 0x6D2B79F5 | 0;
    let t = Math.imul(a ^ a >>> 15, 1 | a);
    t = t + Math.imul(t ^ t >>> 7, 61 | t) ^ t;
    return ((t ^ t >>> 14) >>> 0) / 4294967296;
  };
}

function tileTexture(base, grout, variation, seed) {
  const S = 256, N = 8, T = S / N, G = Math.max(3, T / 9);
  const cv = document.createElement('canvas'); cv.width = cv.height = S;
  const ctx = cv.getContext('2d');
  const rnd = mulberry32(seed);
  ctx.fillStyle = grout; ctx.fillRect(0, 0, S, S);
  const b = new THREE.Color(base);
  for (let y = 0; y < N; y++) for (let x = 0; x < N; x++) {
    const v = 1 + (rnd() * 2 - 1) * variation;
    const h = (rnd() * 2 - 1) * variation * 0.5;
    const c = new THREE.Color(
      Math.min(1, Math.max(0, b.r * v - h)),
      Math.min(1, Math.max(0, b.g * v)),
      Math.min(1, Math.max(0, b.b * v + h)));
    ctx.fillStyle = '#' + c.getHexString();
    const tx = x * T + G, ty = y * T + G, tw = T - G, th = T - G;
    ctx.fillRect(tx, ty, tw, th);

    const gloss = ctx.createLinearGradient(tx, ty, tx + tw, ty + th);
    gloss.addColorStop(0, 'rgba(255,255,255,0.18)');
    gloss.addColorStop(0.48, 'rgba(255,255,255,0.035)');
    gloss.addColorStop(1, 'rgba(0,0,0,0.10)');
    ctx.fillStyle = gloss;
    ctx.fillRect(tx, ty, tw, th);

    // beveled edges so the aqua wall tiles read as slightly raised blocks
    ctx.strokeStyle = 'rgba(255,255,255,0.28)';
    ctx.lineWidth = 1.35;
    ctx.beginPath();
    ctx.moveTo(tx + 1, ty + th - 1);
    ctx.lineTo(tx + 1, ty + 1);
    ctx.lineTo(tx + tw - 1, ty + 1);
    ctx.stroke();

    ctx.strokeStyle = 'rgba(40,80,85,0.22)';
    ctx.beginPath();
    ctx.moveTo(tx + tw - 1, ty + 1);
    ctx.lineTo(tx + tw - 1, ty + th - 1);
    ctx.lineTo(tx + 1, ty + th - 1);
    ctx.stroke();
  }
  const tex = new THREE.CanvasTexture(cv);
  tex.wrapS = tex.wrapT = THREE.RepeatWrapping;
  tex.colorSpace = THREE.SRGBColorSpace;
  tex.anisotropy = 8;
  return tex;
}

function waterNormalTexture() {
  const S = 256;
  const cv = document.createElement('canvas'); cv.width = cv.height = S;
  const ctx = cv.getContext('2d');
  const rnd = mulberry32(99);
  ctx.fillStyle = '#808080'; ctx.fillRect(0, 0, S, S);
  // layered soft blobs -> height field (drawn wrapped for tileability)
  for (let i = 0; i < 240; i++) {
    const x = rnd() * S, y = rnd() * S, r = 8 + rnd() * 26;
    const bright = rnd() > 0.5;
    for (const ox of [-S, 0, S]) for (const oy of [-S, 0, S]) {
      const g = ctx.createRadialGradient(x + ox, y + oy, 0, x + ox, y + oy, r);
      g.addColorStop(0, bright ? 'rgba(255,255,255,0.10)' : 'rgba(0,0,0,0.10)');
      g.addColorStop(1, 'rgba(128,128,128,0)');
      ctx.fillStyle = g;
      ctx.fillRect(x + ox - r, y + oy - r, r * 2, r * 2);
    }
  }
  // height -> normal (sobel)
  const img = ctx.getImageData(0, 0, S, S);
  const out = ctx.createImageData(S, S);
  const hgt = (x, y) => img.data[(((y + S) % S) * S + ((x + S) % S)) * 4] / 255;
  for (let y = 0; y < S; y++) for (let x = 0; x < S; x++) {
    const dx = (hgt(x + 1, y) - hgt(x - 1, y)) * 2.2;
    const dy = (hgt(x, y + 1) - hgt(x, y - 1)) * 2.2;
    const inv = 1 / Math.hypot(dx, dy, 1);
    const o = (y * S + x) * 4;
    out.data[o] = (-dx * inv * 0.5 + 0.5) * 255;
    out.data[o + 1] = (-dy * inv * 0.5 + 0.5) * 255;
    out.data[o + 2] = (inv * 0.5 + 0.5) * 255;
    out.data[o + 3] = 255;
  }
  ctx.putImageData(out, 0, 0);
  const tex = new THREE.CanvasTexture(cv);
  tex.wrapS = tex.wrapT = THREE.RepeatWrapping;
  return tex;
}

const aquaTex = tileTexture('#45b8c5', '#6fa0a1', 0.17, 101);
const beigeTex = tileTexture('#d4c9a7', '#9f9477', 0.10, 202);
const ceilTex = tileTexture('#cfc4a7', '#938c73', 0.11, 303);

// tiles are 10 cm, texture holds 8 -> repeat = meters * 1.25
function tiledMat(tex, roughness, uMeters, vMeters, bumpScale = 0.018) {
  const map = tex.clone();
  map.needsUpdate = true;
  map.repeat.set(uMeters * 1.25, vMeters * 1.25);
  const bumpMap = tex.clone();
  bumpMap.needsUpdate = true;
  bumpMap.repeat.copy(map.repeat);
  return new THREE.MeshStandardMaterial({
    map,
    bumpMap,
    bumpScale,
    roughness,
    metalness: 0.0,
    envMapIntensity: 0.54,
  });
}

const geoBox = new THREE.BoxGeometry(1, 1, 1);
function box(mat, w, h, d, x, y, z, parent = scene) {
  const m = new THREE.Mesh(geoBox, mat);
  m.scale.set(w, h, d);
  m.position.set(x, y, z);
  parent.add(m);
  return m;
}
// tiled boxes: repeat sized to the dominant visible face
const wallBox = (w, h, d, x, y, z) => box(tiledMat(aquaTex, 0.31, Math.max(w, d), h, 0.038), w, h, d, x, y, z);
const beigeBox = (w, h, d, x, y, z) => box(tiledMat(beigeTex, 0.48, w, Math.max(d, h), 0.020), w, h, d, x, y, z);
const ceilBox = (w, h, d, x, y, z) => box(tiledMat(ceilTex, 0.54, w, d, 0.026), w, h, d, x, y, z);

// ----------------------------------------------------------------- structure
// floors
beigeBox(HALF_WATER * 2 + 0.4, 0.2, CORRIDOR, 0, -0.1, (Z_WATER0 + Z_WATER1) / 2); // channel floor
beigeBox(WALKWAY, WALK_H, TOTAL, -(HALF_WATER + WALKWAY / 2), WALK_H / 2, TOTAL / 2);
beigeBox(WALKWAY, WALK_H, TOTAL, HALF_WATER + WALKWAY / 2, WALK_H / 2, TOTAL / 2);
beigeBox(HALF_WATER * 2, WALK_H, ENTRY, 0, WALK_H / 2, ENTRY / 2);                  // entry landing
beigeBox(HALF_WATER * 2, WALK_H, ALCOVE, 0, WALK_H / 2, Z_WATER1 + ALCOVE / 2);     // alcove landing

// steps (entry down / exit up)
for (let i = 0; i < STEPS; i++) {
  const topDown = WALK_H - RISE * (i + 1);
  beigeBox(HALF_WATER * 2, topDown, STEP_D, 0, topDown / 2, ENTRY + (i + 0.5) * STEP_D);
  const topUp = RISE * (i + 1);
  beigeBox(HALF_WATER * 2, topUp, STEP_D, 0, topUp / 2, Z_WATER1 - STEPS * STEP_D + (i + 0.5) * STEP_D);
}

// walls (segmented like the Unity build)
const WALL_X = HALF_INNER + 0.15;
const SEGS = Math.ceil(TOTAL / 6), SEG_L = TOTAL / SEGS;
for (let i = 0; i < SEGS; i++) {
  const z = (i + 0.5) * SEG_L;
  wallBox(0.3, WALL_H, SEG_L, -WALL_X, WALL_H / 2, z);
  wallBox(0.3, WALL_H, SEG_L, WALL_X, WALL_H / 2, z);
  ceilBox(WALL_X * 2 + 0.3, 0.3, SEG_L, 0, WALL_H + 0.15, z);
}
wallBox(WALL_X * 2 + 0.3, WALL_H, 0.3, 0, WALL_H / 2, -0.15); // back wall

// front wall around the door
const sideW = WALL_X + 0.15 - DOOR_W / 2;
const openTop = WALK_H + DOOR_H;
wallBox(sideW, WALL_H, 0.3, -(DOOR_W / 2 + sideW / 2), WALL_H / 2, TOTAL + 0.15);
wallBox(sideW, WALL_H, 0.3, DOOR_W / 2 + sideW / 2, WALL_H / 2, TOTAL + 0.15);
wallBox(DOOR_W, WALL_H - openTop, 0.3, 0, openTop + (WALL_H - openTop) / 2, TOTAL + 0.15);
wallBox(DOOR_W, WALK_H, 0.3, 0, WALK_H / 2, TOTAL + 0.15);

// ----------------------------------------------------------------- door alcove
// stepped ziggurat frame (largest outline nearest the viewer)
for (let k = 0; k < 3; k++) {
  const w = DOOR_W + 0.32 * (k + 1);
  const hTop = openTop + 0.128 * (k + 1);
  const z = TOTAL - 0.13 * (k + 0.5);
  const jamb = 0.22;
  beigeBox(jamb, hTop - WALK_H, 0.13, -(w / 2 - jamb / 2), WALK_H + (hTop - WALK_H) / 2, z);
  beigeBox(jamb, hTop - WALK_H, 0.13, w / 2 - jamb / 2, WALK_H + (hTop - WALK_H) / 2, z);
  beigeBox(w, jamb, 0.13, 0, hTop - jamb / 2, z);
}

const doorMat = new THREE.MeshStandardMaterial({
  color: 0xf184ad, roughness: 0.62, metalness: 0,
  emissive: 0xff4fa0, emissiveIntensity: 0.40,
});
const door = box(doorMat, DOOR_W - 0.14, DOOR_H - 0.1, 0.09, 0, WALK_H + (DOOR_H - 0.1) / 2, TOTAL - 0.05);
const doorHalo = new THREE.Mesh(
  new THREE.PlaneGeometry(DOOR_W + 0.9, DOOR_H + 0.85),
  new THREE.MeshBasicMaterial({
    color: 0xff4aa4,
    transparent: true,
    opacity: 0.16,
    depthWrite: false,
    blending: THREE.AdditiveBlending,
  }));
doorHalo.position.set(0, WALK_H + DOOR_H / 2, TOTAL - 0.11);
doorHalo.rotation.y = Math.PI;
scene.add(doorHalo);

// CLOUD 09 label
{
  const cv = document.createElement('canvas'); cv.width = 512; cv.height = 128;
  const ctx = cv.getContext('2d');
  ctx.fillStyle = 'rgba(0,0,0,0)'; ctx.clearRect(0, 0, 512, 128);
  ctx.font = '700 64px "Segoe UI", sans-serif';
  ctx.textAlign = 'center'; ctx.textBaseline = 'middle';
  ctx.fillStyle = '#ff9a3d';
  ctx.fillText('C L O U D  0 9', 256, 66);
  const tex = new THREE.CanvasTexture(cv);
  tex.colorSpace = THREE.SRGBColorSpace;
  const label = new THREE.Mesh(
    new THREE.PlaneGeometry(1.15, 0.29),
    new THREE.MeshBasicMaterial({ map: tex, transparent: true }));
  label.position.set(0, WALK_H + DOOR_H * 0.60, TOTAL - 0.101);
  label.rotation.y = Math.PI;
  scene.add(label);
}

// pink tube above the door
const tubeMat = new THREE.MeshStandardMaterial({
  color: 0xff9ecb, emissive: 0xff5faf, emissiveIntensity: 2.45, roughness: 0.62,
});
const tube = new THREE.Mesh(new THREE.CylinderGeometry(0.045, 0.045, DOOR_W * 1.05, 16), tubeMat);
tube.rotation.z = Math.PI / 2;
tube.position.set(0, openTop + 0.22, TOTAL - 0.28);
scene.add(tube);

// ----------------------------------------------------------------- water
const waterNormals = waterNormalTexture();
const water = new Water(new THREE.PlaneGeometry(HALF_WATER * 2 + 0.35, CORRIDOR - 1.2), {
  textureWidth: 512, textureHeight: 512,
  waterNormals,
  sunDirection: new THREE.Vector3(0.1, 1.0, -0.35).normalize(),
  sunColor: 0x626960,
  waterColor: 0x238f86,
  distortionScale: 0.58,
  fog: true,
});
water.rotation.x = -Math.PI / 2;
water.position.set(0, WATER_Y, (Z_WATER0 + Z_WATER1) / 2 + 0.1);
water.material.uniforms.size.value = 6.8;
scene.add(water);

// ----------------------------------------------------------------- railings
const chromeMat = new THREE.MeshStandardMaterial({
  color: 0xc9c0aa, metalness: 0.82, roughness: 0.42, envMapIntensity: 0.62,
});
const railGeo = new THREE.CylinderGeometry(0.026, 0.026, 1, 14);
function railings(x) {
  const z0 = Z_WATER0 + 0.1, z1 = Z_WATER1 - 0.1, len = z1 - z0, zm = (z0 + z1) / 2;
  for (const h of [0.92, 0.52]) {
    const r = new THREE.Mesh(railGeo, chromeMat);
    r.scale.y = len; r.rotation.x = Math.PI / 2;
    r.position.set(x, WALK_H + h, zm);
    scene.add(r);
  }
  const posts = Math.round(len / 1.8) + 1;
  for (let i = 0; i < posts; i++) {
    const p = new THREE.Mesh(railGeo, chromeMat);
    p.scale.set(0.85, 0.92, 0.85);
    p.position.set(x, WALK_H + 0.46, z0 + (len * i) / (posts - 1));
    scene.add(p);
  }
}
railings(-(HALF_WATER + 0.12));
railings(HALF_WATER + 0.12);

// ----------------------------------------------------------------- clouds
const cloudMat = new THREE.MeshStandardMaterial({
  color: 0xe8e2d3, roughness: 1.0, emissive: 0xfffbf0, emissiveIntensity: 0.006,
});
const cloudShadowMat = new THREE.MeshStandardMaterial({
  color: 0xaeb7ad, roughness: 1.0, emissive: 0x879b93, emissiveIntensity: 0.004,
});
function cloudPatchTexture(seed) {
  const S = 256;
  const cv = document.createElement('canvas'); cv.width = cv.height = S;
  const ctx = cv.getContext('2d');
  const rnd = mulberry32(seed);
  ctx.clearRect(0, 0, S, S);
  for (let i = 0; i < 36; i++) {
    const x = S * (0.12 + rnd() * 0.76);
    const y = S * (0.26 + rnd() * 0.48);
    const rx = S * (0.08 + rnd() * 0.18);
    const ry = S * (0.07 + rnd() * 0.16);
    const g = ctx.createRadialGradient(x, y, 0, x, y, Math.max(rx, ry));
    g.addColorStop(0, 'rgba(245,241,229,0.55)');
    g.addColorStop(0.55, 'rgba(235,231,219,0.22)');
    g.addColorStop(1, 'rgba(235,231,219,0)');
    ctx.fillStyle = g;
    ctx.beginPath();
    ctx.ellipse(x, y, rx, ry, rnd() * Math.PI, 0, Math.PI * 2);
    ctx.fill();
  }
  const tex = new THREE.CanvasTexture(cv);
  tex.colorSpace = THREE.SRGBColorSpace;
  return tex;
}
const cloudBackMat = new THREE.MeshBasicMaterial({
  map: cloudPatchTexture(404),
  color: 0xf0eadc,
  transparent: true,
  opacity: 0.74,
  depthWrite: false,
});
const puffGeo = new THREE.SphereGeometry(1, 18, 12);
const cloudPlaneGeo = new THREE.PlaneGeometry(1, 1, 1, 1);
function cloud(x, y, z, scale, seed, sign) {
  const rnd = mulberry32(seed);
  const g = new THREE.Group();
  const lobes = 8 + Math.floor(rnd() * 5);
  const span = scale * (0.82 + lobes * 0.105);

  const backing = new THREE.Mesh(cloudPlaneGeo, cloudBackMat);
  backing.scale.set(span * 1.15, scale * (0.95 + rnd() * 0.35), 1);
  backing.position.set(sign * -0.025, -scale * 0.08, 0);
  backing.rotation.y = sign > 0 ? -Math.PI / 2 : Math.PI / 2;
  g.add(backing);

  for (let i = 0; i < lobes; i++) {
    const t = lobes === 1 ? 0.5 : i / (lobes - 1);
    const prof = 0.50 + 0.48 * Math.sin(t * Math.PI);
    const r = scale * prof * (0.20 + rnd() * 0.18);
    const zz = -span + 2 * span * t;
    addPuff(zz, (rnd() - 0.5) * scale * 0.28, r, 0.85 + rnd() * 0.35);
    const tops = 1 + Math.floor(rnd() * 2);
    for (let k = 0; k < tops; k++) {
      addPuff(
        zz + (rnd() - 0.5) * scale * 0.55,
        r * (0.35 + rnd() * 0.8),
        r * (0.55 + rnd() * 0.45),
        0.65 + rnd() * 0.5);
    }
  }
  function addPuff(zz, yy, r, squash) {
    const p = new THREE.Mesh(puffGeo, cloudMat);
    p.scale.set(r * 0.34, r * (0.92 + squash * 0.32), r * (1.05 + rnd() * 0.45));
    p.rotation.z = (rnd() - 0.5) * 0.55;
    p.position.set(sign * r * 0.10, yy, zz);
    g.add(p);

    const s = new THREE.Mesh(puffGeo, cloudShadowMat);
    s.scale.set(r * 0.28, r * 0.24, r * 0.92);
    s.position.set(sign * r * 0.08, yy - r * 0.38, zz + r * 0.03);
    g.add(s);
  }
  g.position.set(x, y, z);
  scene.add(g);
}
{
  const rnd = mulberry32(7);
  for (let side = 0; side < 2; side++) {
    const sign = side === 0 ? -1 : 1;
    for (let i = 0; i < 5; i++) {
      const f = (i + 0.5) / 5;
      const z = Z_WATER0 + 0.8 + f * (CORRIDOR - 1.6) + (rnd() - 0.5) * 1.3;
      cloud(sign * (HALF_INNER - 0.20), 1.58 + rnd() * 0.44, z, 0.78 + rnd() * 0.38, 700 + side * 50 + i, sign);
    }
  }
}

// ----------------------------------------------------------------- lighting
const ambient = new THREE.AmbientLight(0x9bbcb4, 0.30);
scene.add(ambient);
const hemi = new THREE.HemisphereLight(0xffe2e9, 0x789d96, 0.27);
scene.add(hemi);

// per-area atmosphere (corridor stays exactly as before; the hall goes near-black)
const ATMOS = {
  corridor: { fog: 0xcbd8d0, density: 0.041, bg: 0xcbd8d0, ambient: 0.30, hemi: 0.27 },
  mushroom: { fog: 0x030a0b, density: 0.038, bg: 0x020607, ambient: 0.052, hemi: 0.05 },
};
function applyAtmosphere(name) {
  const a = ATMOS[name];
  scene.fog.color.setHex(a.fog);
  scene.fog.density = a.density;
  scene.background.setHex(a.bg);
  ambient.intensity = a.ambient;
  hemi.intensity = a.hemi;
}

function point(color, intensity, dist, x, y, z) {
  const l = new THREE.PointLight(color, intensity, dist, 2);
  l.position.set(x, y, z);
  scene.add(l);
  return l;
}

// fluorescent ceiling panels
const panelMat = new THREE.MeshStandardMaterial({
  color: 0xffd19b, emissive: 0xffc27b, emissiveIntensity: 0.84, roughness: 0.75,
});
const flickerLights = [];
for (let i = 0; i < 4; i++) {
  const z = 1.6 + ((Z_WATER1 - 1.5) - 1.6) * ((i + 0.5) / 4);
  box(panelMat, 1.7, 0.06, 1.0, 0, WALL_H - 0.03, z);
  flickerLights.push(point(0xffd59a, 5.0, 6.6, 0, WALL_H - 0.42, z));
}

// neon strips: warm left, pink right
const NEON_L = [0xffe066, 0x7dff8e, 0x66f0e0];
const NEON_R = [0xff6ec7, 0xff6ec7, 0xc77dff];
const housingMat = new THREE.MeshStandardMaterial({ color: 0x28282c, roughness: 0.5, metalness: 0.4 });
function neonRow(x, sign, colors) {
  const z0 = 1.0, z1 = Z_WATER1, stride = 2.15, segL = 1.6;
  const count = Math.floor((z1 - z0) / stride);
  const zs = z0 + ((z1 - z0) - (count * stride - 0.55)) / 2 + segL / 2;
  for (let i = 0; i < count; i++) {
    const z = zs + i * stride;
    const c = colors[Math.min(colors.length - 1, Math.floor((i / (count - 1)) * colors.length))];
    const g = new THREE.Group();
    g.position.set(x, 2.52, z);
    g.rotation.z = sign * 0.32;
    const neonMat = new THREE.MeshStandardMaterial({
      color: c, emissive: c, emissiveIntensity: 2.1, roughness: 0.68,
    });
    box(housingMat, 0.16, 0.20, segL + 0.10, 0, 0, 0, g);
    box(neonMat, 0.07, 0.14, segL, -sign * 0.10, -0.02, 0, g);
    scene.add(g);
    if (i % 4 === 1) flickerLights.push(point(c, 4.4, 5.3, x - sign * 0.25, 2.35, z));
  }
}
neonRow(-(HALF_INNER - 0.10), -1, NEON_L);
neonRow(HALF_INNER - 0.10, 1, NEON_R);

// pink door glow
const doorLight = point(0xff5faa, 36, 8.6, 0, 1.62, TOTAL - 0.85);
point(0xff8fc0, 11, 4.4, 0, 0.9, Z_WATER1 + ALCOVE * 0.6);

// ----------------------------------------------------------------- second area: giant abandoned mushroom waterpark
const secondArea = new THREE.Group();
secondArea.position.set(0, 0, SECOND_Z);
secondArea.visible = false; // hidden (and unlit) until the portal is used — saves corridor perf
scene.add(secondArea);

function localPoint(parent, color, intensity, dist, x, y, z) {
  const l = new THREE.PointLight(color, intensity, dist, 2);
  l.position.set(x, y, z);
  parent.add(l);
  return l;
}

// --- lazy-river centerline: a closed winding loop through the hall (local xz)
const riverCurve = new THREE.CatmullRomCurve3([
  new THREE.Vector3(12, 0, -18),
  new THREE.Vector3(2, 0, -21),
  new THREE.Vector3(-8, 0, -18),
  new THREE.Vector3(-13, 0, -9),
  new THREE.Vector3(-11, 0, 1),
  new THREE.Vector3(-13, 0, 10),
  new THREE.Vector3(-7, 0, 18),
  new THREE.Vector3(2, 0, 22),
  new THREE.Vector3(10, 0, 18),
  new THREE.Vector3(13, 0, 9),
  new THREE.Vector3(8, 0, 2),
  new THREE.Vector3(10, 0, -8),
], true, 'catmullrom', 0.6);
const riverLen = riverCurve.getLength();
const riverPts = riverCurve.getSpacedPoints(200); // for distance queries (last == first)

function distToRiver(lx, lz) {
  let best = 1e9;
  for (let i = 0; i < riverPts.length - 1; i++) {
    const a = riverPts[i], b = riverPts[i + 1];
    const abx = b.x - a.x, abz = b.z - a.z;
    const t = THREE.MathUtils.clamp(
      ((lx - a.x) * abx + (lz - a.z) * abz) / (abx * abx + abz * abz), 0, 1);
    const dx = lx - (a.x + abx * t), dz = lz - (a.z + abz * t);
    const d2 = dx * dx + dz * dz;
    if (d2 < best) best = d2;
  }
  return Math.sqrt(best);
}

// loft a [offset, height] profile along the closed river curve (mirror = -1 flips sides)
function riverLoft(profile, material, mirror = 1, samples = 220, uTiles = 90) {
  const rows = profile.length;
  const pos = [], uv = [], idx = [];
  for (let i = 0; i <= samples; i++) {
    const f = (i % samples) / samples;
    const p = riverCurve.getPointAt(f);
    const tan = riverCurve.getTangentAt(f);
    const nx = -tan.z, nz = tan.x; // left normal
    for (let j = 0; j < rows; j++) {
      const [off, y] = profile[j];
      pos.push(p.x + nx * off * mirror, y, p.z + nz * off * mirror);
      uv.push((i / samples) * uTiles, j / (rows - 1));
    }
  }
  for (let i = 0; i < samples; i++) {
    for (let j = 0; j < rows - 1; j++) {
      const a = i * rows + j, b = a + rows;
      idx.push(a, b, a + 1, b, b + 1, a + 1);
    }
  }
  const g = new THREE.BufferGeometry();
  g.setAttribute('position', new THREE.Float32BufferAttribute(pos, 3));
  g.setAttribute('uv', new THREE.Float32BufferAttribute(uv, 2));
  g.setIndex(idx);
  g.computeVertexNormals();
  const m = new THREE.Mesh(g, material);
  m.material.side = THREE.DoubleSide;
  secondArea.add(m);
  return m;
}

// --- materials
const hallCeilTex = tileTexture('#0e2629', '#1d4247', 0.18, 505);
const hallVaultMat = new THREE.MeshStandardMaterial({
  map: hallCeilTex.clone(), roughness: 0.66, metalness: 0.08, envMapIntensity: 0.14,
});
hallVaultMat.map.needsUpdate = true;
hallVaultMat.map.repeat.set(9, 12);
hallVaultMat.side = THREE.BackSide;

const hallWallMat = tiledMat(tileTexture('#173a3e', '#0d2426', 0.22, 606), 0.6, 24, HALL_WALL, 0.03);
hallWallMat.color.multiplyScalar(0.72);
hallWallMat.envMapIntensity = 0.14;

const deckTex = tileTexture('#cfe0d6', '#8fa89f', 0.10, 707);
const deckMat = new THREE.MeshStandardMaterial({
  map: deckTex, roughness: 0.26, metalness: 0.02, envMapIntensity: 0.42,
});
deckMat.color.multiplyScalar(0.6);
deckMat.map.repeat.set(1.25, 1.25); // ShapeGeometry UVs are in meters

const copingMat = new THREE.MeshStandardMaterial({
  map: deckTex.clone(), roughness: 0.3, metalness: 0.02, envMapIntensity: 0.5,
  emissive: 0x1d332e, emissiveIntensity: 0.22,
});
copingMat.map.needsUpdate = true;
copingMat.map.repeat.set(1, 1.4);

const mossMat = new THREE.MeshStandardMaterial({
  // grassy hills: dark, but clearly green (self-lit so it reads in the near-black hall)
  color: 0x1c3d22, roughness: 1.0, emissive: 0x123a1c, emissiveIntensity: 0.30,
});
const bedMat = new THREE.MeshStandardMaterial({
  color: 0x06231f, roughness: 0.85, emissive: 0x06322c, emissiveIntensity: 0.25,
});

const riverNormals = waterNormalTexture();
riverNormals.repeat.set(1, 1);
const riverMat = new THREE.MeshStandardMaterial({
  color: 0x0d4a44,
  emissive: 0x25e8cd,
  emissiveIntensity: 0.42,
  roughness: 0.14,
  metalness: 0.25,
  normalMap: riverNormals,
  normalScale: new THREE.Vector2(0.5, 0.5),
  envMapIntensity: 0.55,
  transparent: true,
  opacity: 0.93,
});

const grassMat = new THREE.MeshStandardMaterial({
  // clearly green blades that self-illuminate a touch so the grass is unmistakably
  // green in the dark hall without lighting (and thus brightening) the whole room
  color: 0x3c8a3f, roughness: 1.0, emissive: 0x1f5a27, emissiveIntensity: 0.62,
});
const flowerMat = new THREE.MeshStandardMaterial({
  color: 0xf2f4ea, roughness: 0.8, emissive: 0xd8e3cf, emissiveIntensity: 0.34,
});
const hallStemMat = new THREE.MeshStandardMaterial({
  color: 0xd9e9e0, roughness: 0.3, metalness: 0.03, envMapIntensity: 0.5,
  emissive: 0x1c2a26, emissiveIntensity: 0.24,
});
const gillMat = new THREE.MeshStandardMaterial({
  color: 0xc4d4c9, roughness: 0.55, emissive: 0x16211d, emissiveIntensity: 0.28,
});
const hallFloatMat = new THREE.MeshPhysicalMaterial({
  color: 0xdd7ecb, roughness: 0.24, metalness: 0.02, clearcoat: 0.7, clearcoatRoughness: 0.35,
  emissive: 0x6e1c58, emissiveIntensity: 0.34, envMapIntensity: 0.55,
});
const skyPanelMat = new THREE.MeshStandardMaterial({
  color: 0xbfe9ff, emissive: 0x9fdcff, emissiveIntensity: 2.3, roughness: 0.6,
});
const redDoorMat = new THREE.MeshStandardMaterial({
  color: 0x4a0d08, emissive: 0xa02014, emissiveIntensity: 1.15, roughness: 0.7,
});

// --- amanita cap texture: red with soft irregular white blobs
function capTexture(seed) {
  const S = 256;
  const cv = document.createElement('canvas'); cv.width = cv.height = S;
  const ctx = cv.getContext('2d');
  const rnd = mulberry32(seed);
  const g = ctx.createLinearGradient(0, 0, 0, S);
  g.addColorStop(0, '#c1402a');
  g.addColorStop(0.55, '#a92e20');
  g.addColorStop(1, '#7e1f15');
  ctx.fillStyle = g; ctx.fillRect(0, 0, S, S);
  for (let i = 0; i < 26; i++) {
    const x = rnd() * S, y = S * (0.12 + rnd() * 0.72);
    const rx = S * (0.028 + rnd() * 0.05), ry = rx * (0.55 + rnd() * 0.5);
    for (const ox of [-S, 0, S]) { // wrap horizontally (sphere seam)
      const rg = ctx.createRadialGradient(x + ox, y, 0, x + ox, y, Math.max(rx, ry));
      rg.addColorStop(0, 'rgba(240,236,220,0.96)');
      rg.addColorStop(0.75, 'rgba(232,226,206,0.85)');
      rg.addColorStop(1, 'rgba(220,214,192,0)');
      ctx.fillStyle = rg;
      ctx.beginPath();
      ctx.ellipse(x + ox, y, rx, ry, rnd() * Math.PI, 0, Math.PI * 2);
      ctx.fill();
    }
  }
  const tex = new THREE.CanvasTexture(cv);
  tex.wrapS = THREE.RepeatWrapping;
  tex.colorSpace = THREE.SRGBColorSpace;
  return tex;
}
const capMats = [111, 222, 333].map(seed => new THREE.MeshStandardMaterial({
  map: capTexture(seed),
  roughness: 0.3, metalness: 0.03,
  emissive: 0x54100a, emissiveIntensity: 0.30,
  envMapIntensity: 0.5,
}));

// --- hall shell: deck (with river cut out), island, walls, vaulted ceiling
const berms = [
  // [x, z, radius, height] — kept clear of the river strip so mounds never swallow the water
  [-9, -25.3, 3.0, 2.2],
  [-17, -4, 3.6, 2.6],
  [-1, 2, 7.5, 2.5],
  [-16, 15.5, 4.2, 2.0],
  [1, 27.6, 4.5, 2.0],
  [15, 23, 4.2, 1.8],
  [16.5, -2, 4.0, 1.6],
  [5, -6, 3.0, 1.1],
  [7.5, -26, 3.0, 1.0],
  [0, 10, 3.4, 1.3],
];
function bermHeightAt(lx, lz) {
  let g = 0;
  for (const [bx, bz, br, bh] of berms) {
    const d2 = (lx - bx) * (lx - bx) + (lz - bz) * (lz - bz);
    if (d2 < br * br) {
      const f = 1 - d2 / (br * br);
      g = Math.max(g, bh * f * f);
    }
  }
  return g;
}
function hallGroundY(lx, lz) {
  const d = distToRiver(lx, lz);
  if (d < CURB_OUT) {
    const inner = RIVER_HALF - 0.1;
    if (d <= inner) return RIVER_BED;
    // rise from the bed, over the raised curb lip, then back down to the deck
    const lip = CURB_OUT - 0.5;
    if (d < lip) return THREE.MathUtils.lerp(RIVER_BED, 0.17, THREE.MathUtils.smoothstep(d, inner, lip));
    return THREE.MathUtils.lerp(0.17, 0, THREE.MathUtils.smoothstep(d, lip, CURB_OUT));
  }
  return bermHeightAt(lx, lz);
}

function addHallShell() {
  // deck: hall rectangle with the river loop punched out (shape y = -z)
  const outer2D = [], inner2D = [];
  const N = 160;
  for (let i = 0; i < N; i++) {
    const f = i / N;
    const p = riverCurve.getPointAt(f);
    const tan = riverCurve.getTangentAt(f);
    const nx = -tan.z, nz = tan.x;
    outer2D.push(new THREE.Vector2(p.x + nx * CURB_OUT, -(p.z + nz * CURB_OUT)));
    inner2D.push(new THREE.Vector2(p.x - nx * CURB_OUT, -(p.z - nz * CURB_OUT)));
  }
  const deckShape = new THREE.Shape([
    new THREE.Vector2(-HALL_W / 2, -HALL_D / 2),
    new THREE.Vector2(HALL_W / 2, -HALL_D / 2),
    new THREE.Vector2(HALL_W / 2, HALL_D / 2),
    new THREE.Vector2(-HALL_W / 2, HALL_D / 2),
  ]);
  deckShape.holes.push(new THREE.Path(outer2D));
  const deck = new THREE.Mesh(new THREE.ShapeGeometry(deckShape, 2), deckMat);
  deck.rotation.x = -Math.PI / 2;
  secondArea.add(deck);

  // island inside the loop (dark planted ground)
  const island = new THREE.Mesh(new THREE.ShapeGeometry(new THREE.Shape(inner2D), 2), mossMat);
  island.rotation.x = -Math.PI / 2;
  island.position.y = 0.005;
  secondArea.add(island);

  // vertical walls up to the vault spring line
  const wall = (w, h, d, x, y, z) => box(hallWallMat, w, h, d, x, y, z, secondArea);
  wall(0.4, HALL_WALL, HALL_D, -HALL_W / 2 - 0.2, HALL_WALL / 2, 0);
  wall(0.4, HALL_WALL, HALL_D, HALL_W / 2 + 0.2, HALL_WALL / 2, 0);
  wall(HALL_W + 1, HALL_WALL + HALL_ARCH + 1.5, 0.4, 0, (HALL_WALL + HALL_ARCH + 1.5) / 2, -HALL_D / 2 - 0.2);
  wall(HALL_W + 1, HALL_WALL + HALL_ARCH + 1.5, 0.4, 0, (HALL_WALL + HALL_ARCH + 1.5) / 2, HALL_D / 2 + 0.2);

  // huge flattened barrel vault fading into darkness
  const vaultGeo = new THREE.CylinderGeometry(HALL_W / 2, HALL_W / 2, HALL_D, 56, 1, true, Math.PI / 2, Math.PI);
  const vault = new THREE.Mesh(vaultGeo, hallVaultMat);
  vault.rotation.x = Math.PI / 2;
  vault.scale.set(1, 1, HALL_ARCH / (HALL_W / 2)); // local z maps to world y after the rotation
  vault.position.y = HALL_WALL;
  secondArea.add(vault);
}
addHallShell();

// --- faded painted wall murals: white waterpark clouds + worn flowers
// Painted look, not real sky: soft brushed shapes on a transparent ground so they
// read as pigment worn onto the tiled wall. Kept dim/faded to stay abandoned & moody.
function cloudMuralTexture(seed) {
  const W = 512, H = 256;
  const cv = document.createElement('canvas'); cv.width = W; cv.height = H;
  const ctx = cv.getContext('2d');
  const rnd = mulberry32(seed);
  ctx.clearRect(0, 0, W, H);
  const clusters = 3 + Math.floor(rnd() * 3);
  for (let c = 0; c < clusters; c++) {
    const cx = W * (0.14 + rnd() * 0.72);
    const cy = H * (0.36 + rnd() * 0.34);
    const cs = W * (0.10 + rnd() * 0.09);
    // flat painted base line (indoor-pool mural clouds sit on a soft shelf)
    const baseG = ctx.createLinearGradient(0, cy + cs * 0.15, 0, cy + cs * 0.85);
    baseG.addColorStop(0, 'rgba(238,235,224,0.55)');
    baseG.addColorStop(1, 'rgba(238,235,224,0)');
    ctx.fillStyle = baseG;
    ctx.fillRect(cx - cs * 1.7, cy, cs * 3.4, cs * 0.8);
    const puffs = 7 + Math.floor(rnd() * 6);
    for (let i = 0; i < puffs; i++) {
      const a = (i / puffs) * Math.PI * 2;
      const rr = cs * (0.5 + rnd() * 0.7);
      const x = cx + Math.cos(a) * rr * (0.9 + rnd() * 0.5);
      const y = cy + Math.sin(a) * rr * 0.5 - cs * 0.12;
      const r = cs * (0.34 + rnd() * 0.4);
      const g = ctx.createRadialGradient(x, y, 0, x, y, r);
      g.addColorStop(0, 'rgba(246,243,233,0.9)');
      g.addColorStop(0.6, 'rgba(230,226,212,0.5)');
      g.addColorStop(1, 'rgba(230,226,212,0)');
      ctx.fillStyle = g;
      ctx.beginPath(); ctx.ellipse(x, y, r, r * 0.78, 0, 0, Math.PI * 2); ctx.fill();
    }
  }
  // worn / chipped patches so the paint looks decades old
  ctx.globalCompositeOperation = 'destination-out';
  for (let i = 0; i < 70; i++) {
    const x = rnd() * W, y = rnd() * H, r = 5 + rnd() * 42;
    const g = ctx.createRadialGradient(x, y, 0, x, y, r);
    g.addColorStop(0, `rgba(0,0,0,${0.12 + rnd() * 0.5})`);
    g.addColorStop(1, 'rgba(0,0,0,0)');
    ctx.fillStyle = g; ctx.fillRect(x - r, y - r, r * 2, r * 2);
  }
  ctx.globalCompositeOperation = 'source-over';
  const tex = new THREE.CanvasTexture(cv);
  tex.colorSpace = THREE.SRGBColorSpace;
  return tex;
}

// faded amusement-park flower palettes (dusty, nostalgic)
const MURAL_FLOWER_PALETTES = [
  ['#c96a86', '#e3a4bb', '#ecd4a6'], // dusty rose
  ['#c99a3e', '#e6c874', '#a7bd97'], // faded marigold
  ['#7fa3c2', '#b2cfe2', '#e6ecdc'], // washed cornflower
  ['#a97fb4', '#d0b2da', '#ecdfc4'], // worn lavender
  ['#cf835a', '#ecb992', '#c4cf9c'], // muted coral
  ['#d0637a', '#e79fac', '#ebd9b0'], // chalky pink
];
function flowerMuralTexture(seed, palette) {
  const S = 256;
  const cv = document.createElement('canvas'); cv.width = cv.height = S;
  const ctx = cv.getContext('2d');
  const rnd = mulberry32(seed);
  ctx.clearRect(0, 0, S, S);
  const cx = S / 2, cy = S / 2;
  const petals = 6 + Math.floor(rnd() * 5);
  const R = S * (0.24 + rnd() * 0.09);
  const [petal, mid, center] = palette;
  // a couple of stray leaves behind the bloom
  for (let i = 0; i < 2; i++) {
    const a = rnd() * Math.PI * 2;
    const lx = cx + Math.cos(a) * R * 0.7, ly = cy + Math.sin(a) * R * 0.7;
    const g = ctx.createRadialGradient(lx, ly, 0, lx, ly, R * 0.5);
    g.addColorStop(0, 'rgba(140,168,132,0.7)');
    g.addColorStop(1, 'rgba(140,168,132,0)');
    ctx.fillStyle = g;
    ctx.save(); ctx.translate(lx, ly); ctx.rotate(a);
    ctx.beginPath(); ctx.ellipse(0, 0, R * 0.5, R * 0.2, 0, 0, Math.PI * 2); ctx.fill();
    ctx.restore();
  }
  // painted petals
  for (let i = 0; i < petals; i++) {
    const a = (i / petals) * Math.PI * 2 + (rnd() - 0.5) * 0.14;
    const px = cx + Math.cos(a) * R * 0.52, py = cy + Math.sin(a) * R * 0.52;
    const g = ctx.createRadialGradient(px, py, 0, px, py, R * 0.62);
    g.addColorStop(0, petal);
    g.addColorStop(0.7, mid);
    g.addColorStop(1, 'rgba(255,255,255,0)');
    ctx.fillStyle = g;
    ctx.save(); ctx.translate(px, py); ctx.rotate(a);
    ctx.globalAlpha = 0.9;
    ctx.beginPath(); ctx.ellipse(0, 0, R * 0.6, R * 0.32, 0, 0, Math.PI * 2); ctx.fill();
    ctx.restore();
  }
  ctx.globalAlpha = 1;
  const cg = ctx.createRadialGradient(cx, cy, 0, cx, cy, R * 0.42);
  cg.addColorStop(0, center);
  cg.addColorStop(1, 'rgba(255,255,255,0)');
  ctx.fillStyle = cg;
  ctx.beginPath(); ctx.arc(cx, cy, R * 0.42, 0, Math.PI * 2); ctx.fill();
  // worn patches
  ctx.globalCompositeOperation = 'destination-out';
  for (let i = 0; i < 34; i++) {
    const x = rnd() * S, y = rnd() * S, r = 4 + rnd() * 26;
    const g = ctx.createRadialGradient(x, y, 0, x, y, r);
    g.addColorStop(0, `rgba(0,0,0,${0.1 + rnd() * 0.45})`);
    g.addColorStop(1, 'rgba(0,0,0,0)');
    ctx.fillStyle = g; ctx.fillRect(x - r, y - r, r * 2, r * 2);
  }
  ctx.globalCompositeOperation = 'source-over';
  const tex = new THREE.CanvasTexture(cv);
  tex.colorSpace = THREE.SRGBColorSpace;
  return tex;
}

function addWallMurals() {
  const rnd = mulberry32(838);
  const cloudTexs = [841, 842, 843, 844].map(cloudMuralTexture);
  const flowerTexs = [];
  for (let i = 0; i < 10; i++) {
    flowerTexs.push(flowerMuralTexture(
      861 + i, MURAL_FLOWER_PALETTES[i % MURAL_FLOWER_PALETTES.length]));
  }
  const muralGeo = new THREE.PlaneGeometry(1, 1);
  function mural(tex, w, h, x, y, z, ry, opacity, tint) {
    const m = new THREE.Mesh(muralGeo, new THREE.MeshBasicMaterial({
      map: tex, color: tint, transparent: true, opacity,
      depthWrite: false, side: THREE.DoubleSide, fog: true,
    }));
    m.scale.set(w, h, 1);
    m.position.set(x, y, z);
    m.rotation.y = ry;
    secondArea.add(m);
  }
  // cloud tint reads as faded paint, not glowing sky
  const CLOUD_TINT = 0xb9c2bb;

  // --- long side walls: a high cloud band, worn flowers scattered below
  for (const side of [-1, 1]) {
    const x = side * (HALL_W / 2 - 0.24);
    const ry = side < 0 ? Math.PI / 2 : -Math.PI / 2;
    for (let i = 0; i < 4; i++) {
      const z = -HALL_D / 2 + HALL_D * (i + 0.5) / 4 + (rnd() - 0.5) * 3.5;
      const w = 7.5 + rnd() * 3.5, h = w * (0.38 + rnd() * 0.08);
      mural(cloudTexs[(i + (side < 0 ? 0 : 2)) % cloudTexs.length],
        w, h, x, 4.5 + (rnd() - 0.5) * 0.5, z, ry, 0.34 + rnd() * 0.1, CLOUD_TINT);
    }
    for (let i = 0; i < 8; i++) {
      const z = -HALL_D / 2 + HALL_D * (i + 0.5) / 8 + (rnd() - 0.5) * 2.2;
      const s = 0.8 + rnd() * 1.7;
      mural(flowerTexs[Math.floor(rnd() * flowerTexs.length)],
        s, s, x, 1.5 + rnd() * 2.0, z, ry, 0.42 + rnd() * 0.16, 0xffffff);
    }
  }

  // --- short end walls: a couple of clouds high, a scatter of flowers
  for (const [z, ry] of [[-HALL_D / 2 + 0.24, 0], [HALL_D / 2 - 0.24, Math.PI]]) {
    for (let i = 0; i < 3; i++) {
      const x = -HALL_W / 2 + HALL_W * (i + 0.5) / 3 + (rnd() - 0.5) * 3;
      const w = 7 + rnd() * 3, h = w * 0.4;
      mural(cloudTexs[Math.floor(rnd() * cloudTexs.length)],
        w, h, x, 4.6 + (rnd() - 0.5) * 0.4, z, ry, 0.32 + rnd() * 0.1, CLOUD_TINT);
    }
    for (let i = 0; i < 5; i++) {
      const x = -HALL_W / 2 + HALL_W * (i + 0.5) / 5 + (rnd() - 0.5) * 2;
      const s = 0.7 + rnd() * 1.4;
      mural(flowerTexs[Math.floor(rnd() * flowerTexs.length)],
        s, s, x, 1.6 + rnd() * 1.8, z, ry, 0.4 + rnd() * 0.14, 0xffffff);
    }
  }
}
addWallMurals();

// --- river: bed, glowing water, curbs
riverLoft([[-RIVER_HALF - 0.2, RIVER_BED], [RIVER_HALF + 0.2, RIVER_BED]], bedMat, 1, 200, 40);
const riverWater = riverLoft(
  [[-RIVER_HALF, RIVER_SURF], [RIVER_HALF, RIVER_SURF]], riverMat, 1, 200, Math.round(riverLen / 3));
for (const mirror of [1, -1]) {
  riverLoft([
    [RIVER_HALF - 0.12, RIVER_BED],
    [RIVER_HALF - 0.04, -0.10],
    [RIVER_HALF, 0.10],
    [RIVER_HALF + 0.42, 0.19],
    [CURB_OUT - 0.14, 0.15],
    [CURB_OUT, 0.0],
  ], copingMat, mirror, 220, Math.round(riverLen / 0.8));
}

// --- grassy berms with instanced grass + white flowers
const bermGeo = new THREE.SphereGeometry(1, 24, 16, 0, Math.PI * 2, 0, Math.PI / 2);
for (const [bx, bz, br, bh] of berms) {
  const m = new THREE.Mesh(bermGeo, mossMat);
  m.scale.set(br, bh * 1.04, br);
  m.position.set(bx, 0, bz);
  secondArea.add(m);
}
{
  const rnd = mulberry32(2026);
  const grassGeo = new THREE.ConeGeometry(0.045, 1, 5);
  grassGeo.translate(0, 0.5, 0);
  const GRASS_N = 2600, FLOWER_N = 720;
  const grass = new THREE.InstancedMesh(grassGeo, grassMat, GRASS_N);
  const flowerGeo = new THREE.SphereGeometry(0.055, 8, 6);
  const flowers = new THREE.InstancedMesh(flowerGeo, flowerMat, FLOWER_N);
  const dummy = new THREE.Object3D();
  let gi = 0, fi = 0;

  function scatterOnBerm(bx, bz, br, bh, nGrass, nFlower) {
    for (let k = 0; k < nGrass && gi < GRASS_N; k++) {
      const a = rnd() * Math.PI * 2, rr = Math.sqrt(rnd()) * br * 0.94;
      const x = bx + Math.cos(a) * rr, z = bz + Math.sin(a) * rr;
      const f = 1 - (rr * rr) / (br * br);
      dummy.position.set(x, bh * f * f - 0.03, z);
      dummy.scale.set(0.8 + rnd() * 1.4, 0.35 + rnd() * 0.75, 0.8 + rnd() * 1.4);
      dummy.rotation.set((rnd() - 0.5) * 0.7, rnd() * Math.PI, (rnd() - 0.5) * 0.7);
      dummy.updateMatrix();
      grass.setMatrixAt(gi++, dummy.matrix);
    }
    for (let k = 0; k < nFlower && fi < FLOWER_N; k++) {
      // bias flowers to the hill rims where they catch the water glow
      const a = rnd() * Math.PI * 2, rr = br * (0.55 + rnd() * 0.4);
      const x = bx + Math.cos(a) * rr, z = bz + Math.sin(a) * rr;
      const f = Math.max(0, 1 - (rr * rr) / (br * br));
      dummy.position.set(x, bh * f * f + 0.05 + rnd() * 0.06, z);
      const s = 0.6 + rnd() * 1.0;
      dummy.scale.set(s, s * 0.7, s);
      dummy.rotation.set(0, rnd() * Math.PI, 0);
      dummy.updateMatrix();
      flowers.setMatrixAt(fi++, dummy.matrix);
    }
  }
  for (const [bx, bz, br, bh] of berms) {
    scatterOnBerm(bx, bz, br, bh, Math.round(br * br * 5.5), Math.round(br * 11));
  }
  // dark hedge fringes along outer bank stretches (image: black bushes over the coping)
  for (const [f0, f1] of [[0.06, 0.2], [0.3, 0.42], [0.55, 0.68], [0.8, 0.9]]) {
    const steps = Math.round((f1 - f0) * 60);
    for (let k = 0; k < steps && gi < GRASS_N - 4; k++) {
      const f = f0 + (f1 - f0) * (k / steps);
      const p = riverCurve.getPointAt(f);
      const tan = riverCurve.getTangentAt(f);
      for (let b = 0; b < 4; b++) {
        const off = CURB_OUT + 0.35 + rnd() * 1.3;
        dummy.position.set(p.x - tan.z * off, -0.02, p.z + tan.x * off);
        dummy.scale.set(1.6 + rnd() * 2.2, 0.9 + rnd() * 1.6, 1.6 + rnd() * 2.2);
        dummy.rotation.set((rnd() - 0.5) * 0.8, rnd() * Math.PI, (rnd() - 0.5) * 0.8);
        dummy.updateMatrix();
        grass.setMatrixAt(gi++, dummy.matrix);
      }
    }
  }
  while (gi < GRASS_N) { dummy.position.set(0, -50, 0); dummy.scale.set(0.001, 0.001, 0.001); dummy.updateMatrix(); grass.setMatrixAt(gi++, dummy.matrix); }
  while (fi < FLOWER_N) { dummy.position.set(0, -50, 0); dummy.scale.set(0.001, 0.001, 0.001); dummy.updateMatrix(); flowers.setMatrixAt(fi++, dummy.matrix); }
  secondArea.add(grass);
  secondArea.add(flowers);
}

// --- mushrooms: giants near the water, medium + small scattered
const stemColliders = [];
function addHallMushroom(x, z, s, seed) {
  const rnd = mulberry32(seed);
  const h = 2.15 * s;
  const stemPts = [];
  for (const [r, y] of [
    [0.40, 0], [0.34, 0.06], [0.26, 0.16], [0.21, 0.36],
    [0.19, 0.62], [0.20, 0.84], [0.24, 0.97], [0.27, 1.0],
  ]) stemPts.push(new THREE.Vector2(r * s, y * h));
  const stem = new THREE.Mesh(new THREE.LatheGeometry(stemPts, 20), hallStemMat);
  const gy = bermHeightAt(x, z);
  stem.position.set(x, gy - 0.06, z);
  stem.rotation.y = rnd() * Math.PI;
  secondArea.add(stem);
  stemColliders.push({ x, z, r: 0.34 * s });

  // drooping amanita cap (sphere past the equator), spotted texture
  const cap = new THREE.Mesh(
    new THREE.SphereGeometry(1, 30, 16, 0, Math.PI * 2, 0, Math.PI * 0.58),
    capMats[Math.floor(rnd() * capMats.length)]);
  cap.scale.set(1.28 * s, 0.72 * s * (0.9 + rnd() * 0.2), 1.28 * s);
  cap.position.set(x, gy + h - 0.30 * s, z);
  cap.rotation.y = rnd() * Math.PI * 2;
  secondArea.add(cap);

  const gills = new THREE.Mesh(
    new THREE.CylinderGeometry(1.02 * s, 0.42 * s, 0.10 * s, 30), gillMat);
  gills.position.set(x, gy + h - 0.34 * s, z);
  secondArea.add(gills);
}
// giants (caps up near the haze)
addHallMushroom(-5, 3.5, 3.1, 11);
addHallMushroom(4, 26.5, 2.7, 12);
addHallMushroom(-14.5, -13.5, 2.6, 13);
addHallMushroom(14.5, 13, 2.5, 14);
// medium
addHallMushroom(9.5, -22.5, 1.9, 21);
addHallMushroom(-15.5, 4.5, 1.8, 22);
addHallMushroom(1.5, 7.5, 1.6, 23);
addHallMushroom(15.8, -7.5, 1.7, 24);
addHallMushroom(-4, -13.5, 1.45, 25);
// small
addHallMushroom(-8.5, -24.5, 0.95, 31);
addHallMushroom(3.5, -24.8, 0.8, 32);
addHallMushroom(-2, 26, 1.1, 33);
addHallMushroom(12, 1.5, 0.75, 34);
addHallMushroom(-16.5, 17, 1.0, 35);
addHallMushroom(5.5, 13.5, 0.7, 36);

// --- pink float tubes drifting endlessly along the loop
const hallFloats = [];
{
  const ringGeo = new THREE.TorusGeometry(0.55, 0.26, 14, 32);
  const topGeo = new THREE.CylinderGeometry(0.56, 0.56, 0.05, 24);
  const handleGeo = new THREE.TorusGeometry(0.16, 0.035, 8, 12, Math.PI);
  const rnd = mulberry32(515);
  const COUNT = 10;
  for (let i = 0; i < COUNT; i++) {
    const g = new THREE.Group();
    const ring = new THREE.Mesh(ringGeo, hallFloatMat);
    ring.rotation.x = Math.PI / 2;
    g.add(ring);
    const top = new THREE.Mesh(topGeo, hallFloatMat);
    top.position.y = 0.10;
    g.add(top);
    const handle = new THREE.Mesh(handleGeo, hallFloatMat);
    handle.position.set(0.30, 0.16, 0);
    handle.rotation.y = Math.PI / 2;
    g.add(handle);
    const s = 0.88 + rnd() * 0.3;
    g.scale.set(s, s, s);
    secondArea.add(g);
    hallFloats.push({
      g,
      offset: i / COUNT + (rnd() - 0.5) * 0.04,
      bobPhase: rnd() * Math.PI * 2,
      spin: (rnd() - 0.5) * 0.3,
    });
  }
}

// --- mist / haze sheets over the water
const mists = [];
{
  const mistTex = cloudPatchTexture(660);
  const rnd = mulberry32(717);
  for (let i = 0; i < 9; i++) {
    const f = i / 9;
    const p = riverCurve.getPointAt(f);
    const mat = new THREE.MeshBasicMaterial({
      map: mistTex, color: 0x9fe8dd, transparent: true,
      opacity: 0.05 + rnd() * 0.05, depthWrite: false,
    });
    const m = new THREE.Mesh(new THREE.PlaneGeometry(10 + rnd() * 8, 8 + rnd() * 6), mat);
    m.rotation.x = -Math.PI / 2;
    m.rotation.z = rnd() * Math.PI;
    m.position.set(p.x, 0.5 + rnd() * 1.3, p.z);
    secondArea.add(m);
    mists.push({ m, baseX: p.x, baseZ: p.z, phase: rnd() * Math.PI * 2, baseOp: mat.opacity });
  }
  // two huge high veils that eat the vault into haze
  for (const [x, y, z, w, d] of [[-3, 7.5, -6, 34, 26], [4, 9.0, 14, 36, 28]]) {
    const mat = new THREE.MeshBasicMaterial({
      map: mistTex, color: 0x486e6a, transparent: true, opacity: 0.10, depthWrite: false,
    });
    const m = new THREE.Mesh(new THREE.PlaneGeometry(w, d), mat);
    m.rotation.x = -Math.PI / 2;
    m.position.set(x, y, z);
    secondArea.add(m);
    mists.push({ m, baseX: x, baseZ: z, phase: x, baseOp: 0.10 });
  }
}

// --- sparse lighting: one pale sky-blue ceiling light, mint water glow, far red door
const skyPanel = box(skyPanelMat, 3.0, 0.16, 1.8, -4, HALL_WALL + HALL_ARCH * 0.93, -16, secondArea);
const skyGlow = new THREE.Mesh(
  new THREE.PlaneGeometry(4.6, 3.1),
  new THREE.MeshBasicMaterial({
    color: 0x9fdcff, transparent: true, opacity: 0.15,
    depthWrite: false, blending: THREE.AdditiveBlending,
  }));
skyGlow.rotation.x = Math.PI / 2;
skyGlow.position.set(-4, HALL_WALL + HALL_ARCH * 0.93 - 0.25, -16);
secondArea.add(skyGlow);
const skySpot = new THREE.SpotLight(0xbfe9ff, 420, 46, 0.62, 0.65, 2);
skySpot.position.set(-4, HALL_WALL + HALL_ARCH * 0.9, -16);
skySpot.target.position.set(-6, 0, -14);
secondArea.add(skySpot);
secondArea.add(skySpot.target);
const skyPoint = localPoint(secondArea, 0x9fdcff, 30, 24, -4, HALL_WALL + HALL_ARCH * 0.8, -16);

// mint glow rising off the water at intervals along the loop (kept few — lights are the perf cost)
const riverLights = [];
for (let i = 0; i < 5; i++) {
  const p = riverCurve.getPointAt(i / 5 + 0.06);
  riverLights.push(localPoint(secondArea, 0x35f5da, 9, 13, p.x, 0.55, p.z));
}
// faint teal volume fill so silhouettes read against the dark
localPoint(secondArea, 0x123c3e, 14, 36, 0, 9, -4);

// distant red-lit doorway at the far north end (landmark from the reference)
const redDoor = box(redDoorMat, 1.5, 2.3, 0.12, -2.5, 1.15, HALL_D / 2 - 0.28, secondArea);
const redDoorLight = localPoint(secondArea, 0xff2814, 7, 10, -2.5, 1.4, HALL_D / 2 - 1.1);

// dim pink arrival door behind the spawn point (where the player "came from")
const arriveDoor = box(doorMat.clone(), 1.42, 2.05, 0.12, SPAWN_LOCAL.x, 1.05, -HALL_D / 2 + 0.28, secondArea);
arriveDoor.material.emissiveIntensity = 0.22;
localPoint(secondArea, 0xff77b4, 4.5, 6.5, SPAWN_LOCAL.x, 1.5, -HALL_D / 2 + 1.2);

// where a given float sits (local hall coords) at time t — shared by the drift
// animation and the ride logic so a ridden tube tracks its float exactly
const RIVER_FLOW = 0.55; // m/s along the current
function floatTransform(f, t) {
  const drift = (t * RIVER_FLOW) / riverLen;
  const k = ((f.offset + drift) % 1 + 1) % 1;
  const p = riverCurve.getPointAt(k);
  const tan = riverCurve.getTangentAt(k);
  const sway = Math.sin(t * 0.4 + f.bobPhase) * 0.55;
  return {
    x: p.x - tan.z * sway,
    y: RIVER_SURF + 0.10 + Math.sin(t * 0.9 + f.bobPhase) * 0.045,
    z: p.z + tan.x * sway,
    tan, k,
  };
}

// second-area per-frame animation
function updateSecondArea(t, dt) {
  riverNormals.offset.x -= dt * 0.018;
  riverNormals.offset.y += dt * 0.006;
  riverMat.emissiveIntensity = 0.42 + 0.06 * Math.sin(t * 0.7);

  for (const f of hallFloats) {
    const tr = floatTransform(f, t);
    f.g.position.set(tr.x, tr.y, tr.z);
    f.g.rotation.y += dt * f.spin;
    f.g.rotation.x = Math.sin(t * 0.8 + f.bobPhase) * 0.05;
    f.g.rotation.z = Math.cos(t * 0.7 + f.bobPhase * 1.7) * 0.05;
  }

  for (const mi of mists) {
    mi.m.position.x = mi.baseX + Math.sin(t * 0.05 + mi.phase) * 2.2;
    mi.m.position.z = mi.baseZ + Math.cos(t * 0.04 + mi.phase * 1.3) * 1.8;
    mi.m.material.opacity = mi.baseOp * (0.8 + 0.25 * Math.sin(t * 0.16 + mi.phase));
  }

  skyPanelMat.emissiveIntensity = 2.3 * (1 + 0.05 * Math.sin(t * 0.9) + 0.02 * Math.sin(t * 7.3));
  skyGlow.material.opacity = 0.13 + 0.03 * Math.sin(t * 0.9);
  redDoorMat.emissiveIntensity = 1.15 * (1 + 0.10 * Math.sin(t * 2.1) * Math.sin(t * 5.7));
  redDoorLight.intensity = 7 * (1 + 0.12 * Math.sin(t * 2.1));
}

// ----------------------------------------------------------------- controls
const controls = new PointerLockControls(camera, document.body);
const overlay = document.getElementById('overlay');
const loading = document.getElementById('loading');
const promptEl = document.getElementById('prompt');
const ambienceAudio = document.getElementById('ambienceAudio');
const musicStatus = document.getElementById('musicStatus');
const transitionFade = document.getElementById('transitionFade');

const MUSIC_VOLUME = 0.45;
const MUSIC_FADE_SECONDS = 4.0;
let musicStarted = false;
let musicMuted = false;
let musicFade = 0;
let currentArea = 'corridor';
let transitioning = false;
let mobileActive = false;
let mobileForwardHold = false;
let mobileForwardPulse = 0;

// pink-tube ride state
let ridingTube = null;
const RIDE_REACH = 2.4;  // how close you must be to climb into a tube
const RIDE_SEAT = 1.06;  // seated eye height above the tube's waterline

ambienceAudio.volume = 0;
ambienceAudio.loop = true;

if (isTouchLike) {
  overlay.querySelector('.hint').innerHTML = 'Swipe to look around<br>Click and hold to move';
}

function updateMusicStatus() {
  musicStatus.textContent = musicMuted ? 'Music: Muted' : 'Music: On';
}

function startAmbience() {
  if (musicStarted) return;
  musicStarted = true;
  musicMuted = false;
  musicFade = 0;
  ambienceAudio.currentTime = ambienceAudio.currentTime || 0;
  ambienceAudio.volume = 0;
  ambienceAudio.muted = false;
  updateMusicStatus();
  ambienceAudio.play().catch(() => {
    musicStarted = false;
    musicMuted = true;
    updateMusicStatus();
  });
}

function toggleMusicMute() {
  if (!musicStarted) return;

  musicMuted = !musicMuted;
  ambienceAudio.muted = musicMuted;
  updateMusicStatus();
}

function restartAmbience() {
  if (!musicStarted) {
    startAmbience();
    return;
  }

  ambienceAudio.currentTime = 0;
  musicFade = 0;
  ambienceAudio.volume = 0;
  musicMuted = false;
  ambienceAudio.muted = false;
  updateMusicStatus();
  ambienceAudio.play().catch(() => {});
}

updateMusicStatus();

function enterSecondArea() {
  if (transitioning || currentArea === 'mushroom') return;
  transitioning = true;
  flash('SINKING THROUGH THE DOOR', 1.2);
  transitionFade.classList.add('active');
  keys.KeyE = false;

  setTimeout(() => {
    currentArea = 'mushroom';
    applyAtmosphere('mushroom');
    secondArea.visible = true;
    water.visible = false; // corridor mirror pass no longer needed (portal is one-way)
    camera.position.set(SPAWN_LOCAL.x, SECOND_FLOOR + EYE, SECOND_Z + SPAWN_LOCAL.z);
    // face into the hall, angled toward the leftward water route
    camera.lookAt(2.0, 1.3, SECOND_Z - 16);
    syncMobileLookFromCamera();
    smoothY = camera.position.y;
  }, 650);

  setTimeout(() => {
    transitionFade.classList.remove('active');
    transitioning = false;
    flash('THE MUSHROOM WATERPARK', 1.8);
  }, 1500);
}

// climb onto a drifting tube — the current now carries the player
function mountTube(f) {
  if (ridingTube) return;
  ridingTube = f;
  flash('CLIMBING INTO THE TUBE', 0.9);
}

// step off onto the nearest bank
function dismountTube(t) {
  if (!ridingTube) return;
  const tr = floatTransform(ridingTube, t);
  ridingTube = null;
  // hop to the outer bank (left normal of the current), clamped inside the hall
  const nx = -tr.tan.z, nz = tr.tan.x;
  let bx = tr.x + nx * (CURB_OUT + 0.6);
  let bz = tr.z + nz * (CURB_OUT + 0.6);
  bx = THREE.MathUtils.clamp(bx, -HALL_W / 2 + 0.9, HALL_W / 2 - 0.9);
  bz = THREE.MathUtils.clamp(bz, -HALL_D / 2 + 0.9, HALL_D / 2 - 0.9);
  const gy = SECOND_FLOOR + hallGroundY(bx, bz) + EYE;
  camera.position.set(bx, gy, bz + SECOND_Z);
  smoothY = gy;
  vel.set(0, 0, 0);
  flash('YOU STEP OFF THE TUBE', 1.0);
}

const mobileLook = {
  pointerId: null,
  downX: 0,
  downY: 0,
  lastX: 0,
  lastY: 0,
  downTime: 0,
  moved: false,
  longPressTimer: 0,
  yaw: 0,
  pitch: 0,
};
const mobileLookEuler = new THREE.Euler(0, 0, 0, 'YXZ');
const MOBILE_LOOK_SENSITIVITY = 0.0026;
const MOBILE_TAP_MOVE_SECONDS = 0.34;
const MOBILE_LONG_PRESS_MS = 260;
const MOBILE_DRAG_DEADZONE = 7;

function syncMobileLookFromCamera() {
  mobileLookEuler.setFromQuaternion(camera.quaternion, 'YXZ');
  mobileLook.pitch = mobileLookEuler.x;
  mobileLook.yaw = mobileLookEuler.y;
}

function applyMobileLook(deltaX, deltaY) {
  mobileLook.yaw -= deltaX * MOBILE_LOOK_SENSITIVITY;
  mobileLook.pitch -= deltaY * MOBILE_LOOK_SENSITIVITY;
  mobileLook.pitch = THREE.MathUtils.clamp(mobileLook.pitch, -Math.PI / 2 + 0.08, Math.PI / 2 - 0.08);
  mobileLookEuler.set(mobileLook.pitch, mobileLook.yaw, 0);
  camera.quaternion.setFromEuler(mobileLookEuler);
}

function beginExperience() {
  startAmbience();
  if (isTouchLike) {
    mobileActive = true;
    syncMobileLookFromCamera();
    overlay.classList.add('hidden');
    document.body.classList.add('playing');
  } else {
    controls.lock();
  }
}

overlay.addEventListener('pointerup', e => {
  e.preventDefault();
  beginExperience();
}, { passive: false });

overlay.addEventListener('click', e => {
  e.preventDefault();
}, { passive: false });

controls.addEventListener('lock', () => {
  overlay.classList.add('hidden');
  document.body.classList.add('playing');
});
controls.addEventListener('unlock', () => {
  if (mobileActive) return;
  overlay.classList.remove('hidden');
  overlay.querySelector('.hint').textContent = 'Click to Resume';
  document.body.classList.remove('playing');
});

function cancelMobileForwardHold() {
  mobileForwardHold = false;
  if (mobileLook.longPressTimer) clearTimeout(mobileLook.longPressTimer);
  mobileLook.longPressTimer = 0;
}

function onMobilePointerDown(e) {
  if (!isTouchLike || !mobileActive || mobileLook.pointerId !== null || e.pointerType === 'mouse') return;
  e.preventDefault();
  try { renderer.domElement.setPointerCapture(e.pointerId); } catch (_) {}
  syncMobileLookFromCamera();
  mobileLook.pointerId = e.pointerId;
  mobileLook.downX = mobileLook.lastX = e.clientX;
  mobileLook.downY = mobileLook.lastY = e.clientY;
  mobileLook.downTime = performance.now();
  mobileLook.moved = false;
  cancelMobileForwardHold();
  mobileLook.longPressTimer = setTimeout(() => {
    if (mobileLook.pointerId === e.pointerId && !mobileLook.moved) mobileForwardHold = true;
  }, MOBILE_LONG_PRESS_MS);
}

function onMobilePointerMove(e) {
  if (!isTouchLike || e.pointerId !== mobileLook.pointerId) return;
  e.preventDefault();
  const dxTotal = e.clientX - mobileLook.downX;
  const dyTotal = e.clientY - mobileLook.downY;
  const dx = e.clientX - mobileLook.lastX;
  const dy = e.clientY - mobileLook.lastY;
  mobileLook.lastX = e.clientX;
  mobileLook.lastY = e.clientY;

  if (!mobileLook.moved && Math.hypot(dxTotal, dyTotal) > MOBILE_DRAG_DEADZONE) {
    mobileLook.moved = true;
    cancelMobileForwardHold();
  }
  if (mobileLook.moved) applyMobileLook(dx, dy);
}

function onMobilePointerUp(e) {
  if (!isTouchLike || e.pointerId !== mobileLook.pointerId) return;
  e.preventDefault();
  const wasTap = !mobileLook.moved && performance.now() - mobileLook.downTime < MOBILE_LONG_PRESS_MS + 80;
  try {
    if (renderer.domElement.hasPointerCapture(e.pointerId)) renderer.domElement.releasePointerCapture(e.pointerId);
  } catch (_) {}
  mobileLook.pointerId = null;
  cancelMobileForwardHold();
  if (wasTap) mobileForwardPulse = Math.max(mobileForwardPulse, MOBILE_TAP_MOVE_SECONDS);
}

renderer.domElement.addEventListener('pointerdown', onMobilePointerDown, { passive: false });
renderer.domElement.addEventListener('pointermove', onMobilePointerMove, { passive: false });
renderer.domElement.addEventListener('pointerup', onMobilePointerUp, { passive: false });
renderer.domElement.addEventListener('pointercancel', onMobilePointerUp, { passive: false });

document.addEventListener('touchmove', e => e.preventDefault(), { passive: false });
document.addEventListener('gesturestart', e => e.preventDefault(), { passive: false });

const keys = {};
addEventListener('keydown', e => {
  keys[e.code] = true;
  if (e.code === 'KeyM' && !e.repeat) toggleMusicMute();
  if (e.code === 'KeyR' && !e.repeat) restartAmbience();
});
addEventListener('keyup', e => { keys[e.code] = false; });

// piecewise ground height along the corridor
function groundY(z) {
  if (currentArea === 'mushroom') {
    return SECOND_FLOOR + hallGroundY(camera.position.x, z - SECOND_Z);
  }
  if (z < ENTRY) return WALK_H;
  if (z < ENTRY + STEPS * STEP_D) {
    const i = Math.floor((z - ENTRY) / STEP_D);
    return WALK_H - RISE * (i + 1);
  }
  const exit0 = Z_WATER1 - STEPS * STEP_D;
  if (z < exit0) return 0;
  if (z < Z_WATER1) {
    const i = Math.floor((z - exit0) / STEP_D);
    return RISE * (i + 1);
  }
  return WALK_H;
}

// interaction messaging
let msgTimer = 0;
function flash(text, secs) {
  promptEl.textContent = text;
  promptEl.classList.add('visible');
  msgTimer = secs;
}

// Look-dev screenshot mode: ?shot&z=6 (hides overlay, fixed camera)
const params = new URLSearchParams(location.search);
if (params.has('shot')) {
  overlay.classList.add('hidden');
  loading.classList.add('hidden');
  document.body.classList.add('playing');
  const z = parseFloat(params.get('z') || '5');
  if (params.get('area') === 'second') {
    currentArea = 'mushroom';
    applyAtmosphere('mushroom');
    secondArea.visible = true;
    water.visible = false;
    // local-space camera / look-target overrides for look-dev framing
    const lx = parseFloat(params.get('x') ?? SPAWN_LOCAL.x);
    const lz = params.has('z') ? z : SPAWN_LOCAL.z;
    const tx = parseFloat(params.get('tx') ?? '2.0');
    const tz = parseFloat(params.get('tz') ?? '-16');
    const ty = parseFloat(params.get('ty') ?? '1.3');
    camera.position.set(lx, SECOND_FLOOR + hallGroundY(lx, lz) + EYE, SECOND_Z + lz);
    camera.lookAt(tx, ty, SECOND_Z + tz);
  } else {
    camera.position.set(0, groundY(z) + EYE, z);
    camera.lookAt(0, 1.5, TOTAL - 0.5);
  }
}

const vel = new THREE.Vector3();
let bobCycle = 0, bobWeight = 0, smoothY = camera.position.y;
const clock = new THREE.Clock();

function animate() {
  requestAnimationFrame(animate);
  const dt = Math.min(clock.getDelta(), 0.05);
  const t = clock.elapsedTime;

  // movement
  const inputActive = controls.isLocked || mobileActive;
  if (inputActive) {
    const slow = keys.ShiftLeft || keys.ShiftRight || keys.ControlLeft || keys.ControlRight;
    const speed = slow ? 1.7 : 3.2;
    const mobileForward = mobileForwardHold || mobileForwardPulse > 0 ? 1 : 0;
    if (mobileForwardPulse > 0) mobileForwardPulse = Math.max(0, mobileForwardPulse - dt);
    const fwd = (keys.KeyW ? 1 : 0) - (keys.KeyS ? 1 : 0) + mobileForward;
    const strafe = (keys.KeyD ? 1 : 0) - (keys.KeyA ? 1 : 0);

    const dir = new THREE.Vector3();
    camera.getWorldDirection(dir); dir.y = 0; dir.normalize();
    const right = new THREE.Vector3().crossVectors(dir, new THREE.Vector3(0, 1, 0)).negate();
    const target = new THREE.Vector3()
      .addScaledVector(dir, fwd).addScaledVector(right, -strafe);
    if (target.lengthSq() > 0) target.normalize().multiplyScalar(speed);
    vel.lerp(target, 1 - Math.exp(-dt * 5.5));

    camera.position.addScaledVector(vel, dt);
    if (currentArea === 'mushroom') {
      camera.position.x = THREE.MathUtils.clamp(camera.position.x, -HALL_W / 2 + 0.8, HALL_W / 2 - 0.8);
      camera.position.z = THREE.MathUtils.clamp(camera.position.z, SECOND_Z - HALL_D / 2 + 0.8, SECOND_Z + HALL_D / 2 - 0.8);
      // mushroom stems push the player out (not while drifting in a tube)
      if (!ridingTube) for (const s of stemColliders) {
        const dx = camera.position.x - s.x;
        const dz = camera.position.z - SECOND_Z - s.z;
        const d = Math.hypot(dx, dz);
        const min = s.r + 0.45;
        if (d < min && d > 1e-4) {
          const push = (min - d) / d;
          camera.position.x += dx * push;
          camera.position.z += dz * push;
        }
      }
    } else {
      // stay in the first corridor water channel
      camera.position.x = THREE.MathUtils.clamp(camera.position.x, -(HALF_WATER - 0.35), HALF_WATER - 0.35);
      camera.position.z = THREE.MathUtils.clamp(camera.position.z, 0.6, TOTAL - 0.75);
    }

    // head bob
    const hSpeed = Math.hypot(vel.x, vel.z);
    bobWeight = THREE.MathUtils.lerp(bobWeight, hSpeed > 0.25 ? Math.min(hSpeed / 3.2, 1) : 0, dt * 4);
    bobCycle += dt * 1.7 * Math.max(hSpeed / 3.2, 0.2) * Math.PI * 2;
    const bob = Math.sin(bobCycle * 2) * 0.03 * bobWeight;

    const gy = groundY(camera.position.z) + EYE;
    smoothY = THREE.MathUtils.lerp(smoothY, gy, 1 - Math.exp(-dt * 10));
    camera.position.y = smoothY + bob;

    // --- pink-tube ride: mount near a tube, then drift with the current
    if (currentArea === 'mushroom') {
      if (ridingTube) {
        const tr = floatTransform(ridingTube, t);
        camera.position.x = tr.x;
        camera.position.z = tr.z + SECOND_Z;
        const seatY = SECOND_FLOOR + tr.y + RIDE_SEAT;
        smoothY = THREE.MathUtils.lerp(smoothY, seatY, 1 - Math.exp(-dt * 7));
        camera.position.y = smoothY;
        vel.set(0, 0, 0);
        if (msgTimer <= 0) {
          promptEl.textContent = 'E — Get Off Tube';
          promptEl.classList.add('visible');
        }
        if (keys.KeyE) { dismountTube(t); keys.KeyE = false; }
      } else {
        // nearest tube within reach
        let near = null, nd = RIDE_REACH;
        for (const f of hallFloats) {
          const tr = floatTransform(f, t);
          const d = Math.hypot(camera.position.x - tr.x, camera.position.z - (tr.z + SECOND_Z));
          if (d < nd) { nd = d; near = f; }
        }
        if (msgTimer <= 0) {
          if (near) {
            promptEl.textContent = 'E — Ride Tube';
            promptEl.classList.add('visible');
            if (keys.KeyE) { mountTube(near); keys.KeyE = false; }
          } else {
            promptEl.classList.remove('visible');
          }
        }
      }
    }

    if (currentArea === 'corridor') {
      const dDoor = camera.position.distanceTo(new THREE.Vector3(0, camera.position.y, TOTAL - 0.05));
      if (msgTimer <= 0) {
        if (dDoor < 3.0) {
          promptEl.textContent = 'E — Enter';
          promptEl.classList.add('visible');
          if (keys.KeyE || dDoor < 1.15) enterSecondArea();
        } else {
          promptEl.classList.remove('visible');
        }
      } else {
        keys.KeyE = false;
      }
    }
  }

  if (msgTimer > 0) {
    msgTimer -= dt;
    if (msgTimer <= 0) promptEl.classList.remove('visible');
  }

  // water + pulses
  if (musicStarted && !musicMuted && musicFade < MUSIC_FADE_SECONDS) {
    musicFade = Math.min(MUSIC_FADE_SECONDS, musicFade + dt);
    ambienceAudio.volume = MUSIC_VOLUME * (musicFade / MUSIC_FADE_SECONDS);
  }

  retroPass.uniforms.time.value = t;
  water.material.uniforms.time.value += dt * 0.55;
  if (currentArea === 'mushroom') updateSecondArea(t, dt);
  tubeMat.emissiveIntensity = 2.65 * (1 + 0.12 * Math.sin(t * 1.65));
  doorMat.emissiveIntensity = 0.46 * (1 + 0.08 * Math.sin(t * 1.25 + 0.8));
  doorHalo.material.opacity = 0.105 + 0.025 * Math.sin(t * 1.35 + 0.6);
  doorLight.intensity = 36 * (1 + 0.06 * Math.sin(t * 1.65));
  for (let i = 0; i < flickerLights.length; i++) {
    const l = flickerLights[i];
    if (!l.userData.base) l.userData.base = l.intensity;
    const n = Math.sin(t * 13 + i * 7.31) * Math.sin(t * 5.7 + i * 3.17);
    l.intensity = l.userData.base * (1 + n * 0.035);
  }

  composer.render();
}

loading.classList.add('hidden');
animate();
