# Codex Task 9: Asset Sourcing Guide

> **Goal**: Create a comprehensive document listing every audio, font, texture, and 3D model asset needed with exact specs, plus links to free/royalty-free sources. This is a DOCUMENTATION task — no code to write.

---

## Context

Metal Pod needs external assets that can't be generated with code: audio clips, fonts, textures, and 3D models. When the developer opens Unity, they need a shopping list with direct download links so they can quickly populate the project.

**Read these files for requirements**:
- `IMPLEMENTATION_PLAN.md` — Phase 5 audio/visual specs
- `AGENT_2_HOVERCRAFT_VEHICLE.md` — Vehicle audio specs
- `AGENT_3_COURSES_AND_HAZARDS.md` — Environment audio and visuals
- `AGENT_5_UI_AND_WORKSHOP.md` — UI fonts and theme
- `CODEX_TASK_2_SHADERS.md` — Texture requirements for shaders
- `CODEX_TASK_6_PARTICLE_SYSTEMS.md` — Particle texture needs
- `Hovercraft_Game_SRS.docx` content (summarized in IMPLEMENTATION_PLAN.md)

---

## File to Create

```
Docs/ASSET_SOURCING_GUIDE.md
```

---

## Document Structure

The document should be organized into these sections. For EACH asset, provide:
- **Name**: Descriptive name
- **Specs**: Format, duration/size, technical requirements
- **Where used**: Which script/system uses it
- **Source options**: 2-3 links to free/royalty-free sources (prioritize these sites):
  - **Audio**: freesound.org, opengameart.org, kenney.nl, mixkit.co, pixabay.com/sound-effects
  - **Fonts**: Google Fonts, dafont.com, fontsquirrel.com
  - **Textures**: ambientcg.com, polyhaven.com, textures.com (free tier), kenney.nl
  - **3D Models**: kenney.nl, opengameart.org, poly.pizza, sketchfab (CC0 licensed)
  - **Music**: opengameart.org, freemusicarchive.org, incompetech.com (Kevin MacLeod)

IMPORTANT: Only recommend assets with licenses compatible with commercial iOS game release (CC0, CC-BY, MIT, public domain, or royalty-free). Note the license for each.

---

## SECTION 1: AUDIO — MUSIC (5 tracks)

### 1.1 Main Menu Theme
```
Specs: MP3/OGG, 60-120s, loopable, 44.1kHz stereo
Style: Heavy metal intro riff, medium energy, sets the mood
Tempo: 100-120 BPM
Where used: MainMenuUI, AudioManager
```

### 1.2 Workshop Ambient Music
```
Specs: MP3/OGG, 120-180s, loopable, 44.1kHz stereo
Style: Low-key metal/rock, chill but edgy, not distracting
  Think: muted rhythm guitar + light drums
Tempo: 80-100 BPM
Where used: WorkshopManager, AudioManager
```

### 1.3 Racing Music — Lava Environment
```
Specs: MP3/OGG, 180-240s, loopable, 44.1kHz stereo
Style: Fast, aggressive heavy metal. Driving double bass drums, heavy riffs
Tempo: 140-160 BPM
Where used: AudioManager (plays during lava courses)
```

### 1.4 Racing Music — Ice Environment
```
Specs: MP3/OGG, 180-240s, loopable, 44.1kHz stereo
Style: Intense but slightly ethereal. Metal with melodic elements, maybe synth layer
Tempo: 130-150 BPM
Where used: AudioManager (plays during ice courses)
```

### 1.5 Racing Music — Toxic Environment
```
Specs: MP3/OGG, 180-240s, loopable, 44.1kHz stereo
Style: Industrial metal. Mechanical, grinding, distorted. Think: NIN/Ministry style
Tempo: 120-140 BPM
Where used: AudioManager (plays during toxic courses)
```

Search for free heavy metal game music tracks. Sites like opengameart.org and incompetech.com have rock/metal tracks. Also search for "royalty free metal game music" to find options.

---

## SECTION 2: AUDIO — SOUND EFFECTS (30+ clips)

### Hovercraft SFX

```
2.1  Engine Loop         — continuous thruster hum, sci-fi vehicle, pitch-shiftable
                           Specs: OGG, 2-5s seamless loop, mono
                           Search: "sci-fi engine loop" "hover vehicle hum"

2.2  Boost Activate      — whoosh + power surge, brief
                           Specs: OGG, 0.5-1s, mono
                           Search: "boost whoosh" "power surge"

2.3  Boost Sustain       — sustained flame/thrust, loopable
                           Specs: OGG, 1-2s loop, mono
                           Search: "rocket flame sustained"

2.4  Brake               — screech/resistance sound
                           Specs: OGG, 0.5-1s, mono
                           Search: "brake screech sci-fi"

2.5  Collision Impact     — metal thud/crash
                           Specs: OGG, 0.3-0.5s, mono
                           Search: "metal impact" "vehicle crash"

2.6  Shield Hit          — energy zap/deflection
                           Specs: OGG, 0.3-0.5s, mono
                           Search: "energy shield hit" "force field impact"

2.7  Health Hit          — metallic damage, crunch
                           Specs: OGG, 0.3-0.5s, mono
                           Search: "metal damage crunch"

2.8  Shield Break        — glass/energy shatter
                           Specs: OGG, 0.5-1s, mono
                           Search: "shield break" "energy shatter"

2.9  Explosion           — vehicle destruction, large
                           Specs: OGG, 1-2s, mono
                           Search: "vehicle explosion" "large explosion"

2.10 Respawn             — power-up/materialization
                           Specs: OGG, 1-1.5s, mono
                           Search: "respawn sound" "power up materialize"
```

### Hazard SFX

```
2.11 Lava Bubbling       — ambient lava surface
                           Specs: OGG, 3-5s loop, mono
                           Search: "lava bubbling" "magma"

2.12 Eruption Rumble     — volcanic warning
                           Specs: OGG, 2-3s, mono
                           Search: "volcanic rumble" "earthquake"

2.13 Eruption Blast      — eruption moment
                           Specs: OGG, 1-2s, mono
                           Search: "volcanic eruption" "explosion blast"

2.14 Geyser Burst        — steam/lava geyser
                           Specs: OGG, 1-2s, mono
                           Search: "geyser burst" "steam vent"

2.15 Ice Crack           — icicle/wall cracking
                           Specs: OGG, 0.5-1s, mono
                           Search: "ice cracking" "ice break"

2.16 Ice Shatter         — icicle/wall breaking
                           Specs: OGG, 0.5-1s, mono
                           Search: "ice shatter" "glass break ice"

2.17 Wind/Blizzard       — howling wind loop
                           Specs: OGG, 5-10s loop, mono
                           Search: "blizzard wind loop" "howling wind"

2.18 Avalanche Rumble    — approaching avalanche
                           Specs: OGG, 3-5s loop, mono
                           Search: "avalanche rumble" "landslide"

2.19 Toxic Hiss          — gas vent release
                           Specs: OGG, 1-2s, mono
                           Search: "gas hiss" "steam release"

2.20 Acid Sizzle         — acid contact
                           Specs: OGG, 0.5-1s, mono
                           Search: "acid sizzle" "frying"

2.21 Industrial Press    — hydraulic slam
                           Specs: OGG, 0.5-1s, mono
                           Search: "hydraulic press" "metal slam"

2.22 Electric Zap        — fence shock
                           Specs: OGG, 0.3-0.5s, mono
                           Search: "electric zap" "electricity shock"

2.23 Electric Hum        — fence ambient (when on)
                           Specs: OGG, 2-3s loop, mono
                           Search: "electric hum" "power line buzz"

2.24 Barrel Explosion    — smaller explosion
                           Specs: OGG, 0.5-1s, mono
                           Search: "barrel explosion" "small explosion"
```

### Ambient Loops

```
2.25 Lava Ambient        — fire crackling, distant rumbling
                           Specs: OGG, 10-30s loop, stereo
                           Search: "fire ambient" "volcanic ambient"

2.26 Ice Ambient         — wind, ice creaking, cold atmosphere
                           Specs: OGG, 10-30s loop, stereo
                           Search: "arctic ambient" "ice cave ambient"

2.27 Toxic Ambient       — industrial hum, dripping, machinery
                           Specs: OGG, 10-30s loop, stereo
                           Search: "industrial ambient" "factory ambient"

2.28 Workshop Ambient    — garage atmosphere, tools, radio static
                           Specs: OGG, 10-30s loop, stereo
                           Search: "workshop ambient" "garage atmosphere"
```

### UI SFX

```
2.29 Button Click        — metallic click
                           Specs: OGG, 0.1-0.2s, mono
                           Search: "metal button click" "UI click"

2.30 Purchase Confirm    — cash register / success chime
                           Specs: OGG, 0.5-1s, mono
                           Search: "purchase sound" "coin collect"

2.31 Medal Earned        — fanfare (scale by medal: bronze=short, gold=full)
                           Specs: OGG, 1-3s, mono
                           Search: "medal fanfare" "achievement unlock"

2.32 Countdown Tick      — countdown beep (3, 2, 1)
                           Specs: OGG, 0.2-0.3s, mono
                           Search: "countdown beep" "timer tick"

2.33 Countdown Go        — race start horn/burst
                           Specs: OGG, 0.5-1s, mono
                           Search: "race start" "horn blast"

2.34 Collectible Pickup  — sparkle/chime
                           Specs: OGG, 0.3-0.5s, mono
                           Search: "pickup chime" "collectible sound"

2.35 Course Unlock       — unlock/reveal sound
                           Specs: OGG, 1-1.5s, mono
                           Search: "unlock sound" "level up"
```

---

## SECTION 3: FONTS (2 fonts)

```
3.1  Display Font (Titles/Headers)
     Style: Heavy metal, aggressive, angular. Like band logos.
     Used for: Game title "METAL POD", section headers, medal text, countdown numbers
     Requirements: All caps, numbers 0-9, basic punctuation
     Weight: Bold/Black only needed
     License: OFL (Open Font License) or similar for commercial use

     Search Google Fonts for: "display", "heavy", "industrial"
     Recommended searches on dafont.com: "heavy metal", "rock", "industrial"
     Specific suggestions to evaluate:
       - "Black Ops One" (Google Fonts) — military/industrial
       - "Bungee" (Google Fonts) — bold, blocky
       - "Rubik Mono One" (Google Fonts) — heavy, geometric
       - Search dafont.com for "metal" category fonts with free commercial license

3.2  Body Font (UI text, stats, descriptions)
     Style: Clean, readable, slightly techy/futuristic
     Used for: Stats numbers, descriptions, settings labels, timer display
     Requirements: Full character set, numbers, punctuation
     Weights: Regular + Bold
     License: OFL or similar

     Search Google Fonts for: "monospace", "tech", "sans-serif"
     Specific suggestions:
       - "Rajdhani" (Google Fonts) — techy, very readable
       - "Orbitron" (Google Fonts) — sci-fi, geometric
       - "Share Tech Mono" (Google Fonts) — monospace, clean
       - "Exo 2" (Google Fonts) — modern, multiple weights
```

---

## SECTION 4: TEXTURES (15+ textures)

### Shader Textures

```
4.1  Lava Noise Texture
     Specs: 256x256 PNG, seamless tileable, grayscale noise
     Used by: LavaSurface.shader (_NoiseTex), ToxicSludge.shader
     Search: "seamless noise texture" on ambientcg.com or generate with Photoshop/GIMP

4.2  Lava Surface Texture
     Specs: 512x512 PNG, seamless, orange/red lava look
     Used by: LavaSurface.shader (_MainTex)
     Search: ambientcg.com "lava", polyhaven.com textures

4.3  Volcanic Rock Texture + Normal Map
     Specs: 512x512 PNG, seamless, dark igneous rock
     Used by: VolcanicRock.shader (_MainTex, _BumpMap)
     Search: ambientcg.com "basalt" or "volcanic rock"

4.4  Volcanic Vein Emissive Mask
     Specs: 512x512 PNG, seamless, B&W (white=veins)
     Used by: VolcanicRock.shader (_EmissiveMask)
     Create: Can be generated — cracks pattern in white on black

4.5  Ice Texture + Normal Map
     Specs: 512x512 PNG, seamless, cracked ice
     Used by: IceSurface.shader (_MainTex, _BumpMap)
     Search: ambientcg.com "ice", polyhaven.com "ice"

4.6  Frost Pattern Texture
     Specs: 512x512 PNG, seamless, ice crystal frost pattern
     Used by: FrostOverlay.shader (_FrostTex)
     Search: "frost texture overlay" "ice crystal pattern"

4.7  Toxic Sludge Texture
     Specs: 256x256 PNG, seamless, bubbling liquid
     Used by: ToxicSludge.shader (_MainTex)
     Search: Similar to lava but green-tinted, or recolor lava texture

4.8  Rusted Metal Texture + Normal + Rust Mask
     Specs: 512x512 PNG, seamless
     Used by: RustedMetal.shader (_MainTex, _BumpMap, _RustMask)
     Search: ambientcg.com "rusted metal", polyhaven.com "rust"

4.9  Hex Pattern Texture
     Specs: 256x256 PNG, seamless hexagonal grid (white lines on transparent)
     Used by: ShieldBubble.shader (_HexPattern)
     Search: "hexagonal grid pattern" or generate

4.10 Hovercraft Color Mask
     Specs: 512x512 PNG, R=primary areas, G=secondary, B=accent
     Used by: HovercraftBody.shader (_MaskTex)
     Create: Must be hand-painted to match hovercraft model UV layout
     Note: Use placeholder (solid colors per channel) until real model exists
```

### UI Textures

```
4.11 Metal Panel Texture
     Specs: 256x256 PNG, seamless, brushed/scratched metal
     Used by: MetalPanel.shader (_MainTex)
     Search: ambientcg.com "brushed metal", "scratched metal"

4.12 Rivet Overlay Texture
     Specs: 256x256 PNG, transparent background, rivets at edges
     Used by: MetalPanel.shader (_RivetTex)
     Create: 4 corner rivets + edge detail on transparent

4.13 Particle Texture (soft circle)
     Specs: 64x64 PNG, white soft circle (radial gradient, transparent edges)
     Used by: All particle systems as base texture
     Search: "soft particle texture" or create (radial gradient circle)

4.14 Spark Particle Texture
     Specs: 32x32 PNG, white elongated dot/streak
     Used by: Spark particle effects
     Create: Small bright streak on transparent

4.15 Smoke Particle Texture
     Specs: 128x128 PNG, soft cloud/puff shape, white on transparent
     Used by: Smoke, gas, cloud particle effects
     Search: "smoke particle texture" "cloud puff"
```

---

## SECTION 5: 3D MODELS (5 models)

```
5.1  Hovercraft Model
     Specs: FBX/OBJ, 3000-5000 triangles (mobile budget)
     Style: Industrial, chunky, yellow/black (match concept art)
     Features: Clear areas for primary/secondary/accent color zones
     UV mapped for color mask texture
     Reference: See Art/ folder for concept art
     Note: This is the most important model. Consider commissioning if free isn't good enough.
     Search: sketchfab.com "hovercraft" (CC0 license filter)
             poly.pizza "vehicle" "hover"
     Alternative: Use Unity primitives (cube approximation) until proper model is ready

5.2  Protagonist Character
     Specs: FBX, 1000-2000 triangles, humanoid rig
     Style: 18-year-old, band t-shirt, jeans, boots
     Animations needed: Idle, Working (wrench), Celebrating (fist pump)
     Search: mixamo.com (free characters + animations)
             sketchfab.com "low poly character" CC0
     Note: Mixamo provides free rigged characters AND animations

5.3  Workshop Environment Props
     Specs: FBX/OBJ, low-poly, various
     Items needed: Workbench, tool rack, amplifier stack, crates/barrels, metal shelves
     Search: kenney.nl (free game assets, furniture/industrial packs)
             poly.pizza "workshop" "garage" "industrial"

5.4  Checkpoint Marker
     Specs: FBX/OBJ, < 500 triangles
     Style: Glowing ring/gate, sci-fi
     Search: kenney.nl, opengameart "checkpoint" "gate"
     Alternative: Create from Unity primitives (torus + emissive material)

5.5  Collectible Model
     Specs: FBX/OBJ, < 200 triangles
     Style: Floating geometric shape (crystal, coin, orb)
     Search: kenney.nl (has gem/coin packs)
     Alternative: Unity primitive sphere/diamond with emissive material
```

---

## SECTION 6: QUICK-START ASSET PACK RECOMMENDATIONS

For fastest setup, these asset packs cover many needs at once:

```
6.1  Kenney Game Assets (kenney.nl)
     Free, CC0 license, includes:
     - Particle textures pack
     - UI elements pack
     - Basic 3D models
     - Sound effects

6.2  Mixamo (mixamo.com)
     Free with Adobe account:
     - Rigged character models
     - Animation library (idle, celebrate, work)
     Perfect for protagonist character

6.3  AmbientCG (ambientcg.com)
     Free, CC0 license:
     - PBR material textures (metal, rock, ice, etc.)
     - Normal maps included
     Perfect for environment shaders

6.4  Freesound.org
     Free (various CC licenses):
     - Massive library of sound effects
     - Filter by CC0 for safest licensing
     Good for all SFX needs

6.5  Kevin MacLeod / Incompetech (incompetech.com)
     Free with attribution (CC-BY):
     - Music tracks in many styles including rock/metal
     Good for placeholder or final music
```

---

## SECTION 7: ASSET IMPORT SETTINGS

When importing into Unity, use these settings for iOS optimization:

```
Audio:
  - Format: Vorbis (OGG)
  - Quality: 70% for music, 100% for short SFX
  - Load Type: Streaming for music, Decompress On Load for SFX
  - Sample Rate: Override to 22050 Hz for SFX, 44100 Hz for music

Textures:
  - Format: ASTC 6x6 (iOS default, good quality/size balance)
  - Max Size: 512 for environment, 256 for particles/UI, 1024 for hovercraft
  - Generate Mipmaps: Yes for 3D, No for UI
  - Filter Mode: Bilinear

Models:
  - Scale Factor: 1 (ensure modeling in meters)
  - Mesh Compression: Medium
  - Read/Write Enabled: false (saves memory)
  - Animation Type: Humanoid (for protagonist), None (for props)

Fonts:
  - Import as TMP Font Asset (TextMeshPro)
  - Atlas Resolution: 512x512 for body, 1024x1024 for display
  - Character Set: ASCII + extended for display, full Unicode for body
```

---

## Acceptance Criteria

- [ ] Document covers ALL asset categories: Music (5), SFX (35), Fonts (2), Textures (15), 3D Models (5)
- [ ] Each asset has: name, specs, where used, 2-3 source links
- [ ] All recommended sources are free or royalty-free with commercial-compatible licenses
- [ ] License type noted for each source/recommendation
- [ ] Import settings section for iOS optimization
- [ ] Quick-start pack recommendations for fastest setup
- [ ] Search terms provided for each asset (so developer can find alternatives)
- [ ] Document is well-organized with clear sections and tables
- [ ] Practical and actionable — a developer can follow this and populate the project in 1-2 hours
