# Metal Pod Asset Sourcing Guide

This guide is the sourcing checklist for all external assets required by Metal Pod.

## Scope Coverage

- Music tracks: 5
- SFX clips: 35
- Fonts: 2
- Textures: 15
- 3D models: 5

## License Rules (Commercial iOS Release)

Use only assets that are compatible with a paid/commercial game release.

- Allowed by default: `CC0`, `CC-BY` (with attribution), `OFL` (fonts), `royalty-free` licenses that allow app/game use.
- Avoid for shipping build: `CC-BY-NC`, `CC-BY-ND` (if editing/looping needed), "personal use only" fonts, and any source that forbids game/app distribution.

## Source License References

- Freesound FAQ (CC0/CC-BY/CC-BY-NC breakdown): https://freesound.org/help/faq/
- OpenGameArt examples (licenses shown per asset page): https://opengameart.org/
- Pixabay Content License summary: https://pixabay.com/service/license-summary/
- Incompetech licensing (CC attribution or paid standard): https://incompetech.com/music/royalty-free/licenses/
- Mixamo FAQ (commercial game use allowed): https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html
- Kenney support/license (CC0/public domain): https://kenney.nl/support
- ambientCG license (CC0): https://docs.ambientcg.com/license/
- Poly Haven license (CC0): https://polyhaven.com/license
- Textures.com game-use FAQ and terms: https://www.textures.com/support/faq-license and https://www.textures.com/about/terms-of-use
- Font Squirrel FAQ (commercial use guidance): https://www.fontsquirrel.com/faq
- DaFont homepage note (license differs per font, verify per zip readme): https://www.dafont.com/

---

## 1. Audio - Music (5 Tracks)

| ID | Name | Specs | Where Used | Search Terms | Source Options (2-3) | License Compatibility Notes |
|---|---|---|---|---|---|---|
| 1.1 | Main Menu Theme | MP3/OGG, 60-120s, loopable, 44.1kHz stereo, 100-120 BPM | `Assets/Scripts/UI/MainMenuUI.cs`, `Assets/Scripts/Core/AudioManager.cs` | heavy metal intro riff, menu loop, medium tempo | OpenGameArt metal tracks: https://opengameart.org/content/megasong ; OpenGameArt recent metal: https://opengameart.org/content/unholy-surge ; Incompetech catalog: https://incompetech.com/music/royalty-free/ | Prefer CC0 from OGA for zero attribution risk. Incompetech requires attribution under free CC option (or paid standard license). |
| 1.2 | Workshop Ambient Music | MP3/OGG, 120-180s, loopable, 44.1kHz stereo, 80-100 BPM | `Assets/Scripts/Workshop/WorkshopManager.cs`, `Assets/Scripts/Core/AudioManager.cs` | chill rock metal ambient, workshop loop, low intensity | OpenGameArt collection example: https://opengameart.org/content/music-for-videos ; Pixabay music search: https://pixabay.com/music/search/rock/ ; Incompetech catalog: https://incompetech.com/music/royalty-free/ | Use CC0/CC-BY tracks only. For Pixabay, embed in game binary only (no standalone redistribution). |
| 1.3 | Racing Music - Lava | MP3/OGG, 180-240s, loopable, 44.1kHz stereo, 140-160 BPM | `Assets/Scripts/Core/AudioManager.cs`, `Assets/Scripts/Course/CourseManager.cs` | aggressive metal racing, fast drums, lava battle | OpenGameArt track: https://opengameart.org/content/shadows-awaken-within ; OpenGameArt track: https://opengameart.org/content/heavy-battle-1 ; Incompetech catalog: https://incompetech.com/music/royalty-free/ | Prefer CC0 or CC-BY with attribution in credits. Validate loop points before import. |
| 1.4 | Racing Music - Ice | MP3/OGG, 180-240s, loopable, 44.1kHz stereo, 130-150 BPM | `Assets/Scripts/Core/AudioManager.cs`, `Assets/Scripts/Course/CourseManager.cs` | melodic metal, ethereal synth, cold intense | OpenGameArt track: https://opengameart.org/content/realm-of-torment ; OpenGameArt track: https://opengameart.org/content/achilles ; Pixabay music search: https://pixabay.com/music/search/epic%20rock/ | Keep only licenses compatible with commercial app distribution (no NC tracks). |
| 1.5 | Racing Music - Toxic | MP3/OGG, 180-240s, loopable, 44.1kHz stereo, 120-140 BPM | `Assets/Scripts/Core/AudioManager.cs`, `Assets/Scripts/Course/CourseManager.cs` | industrial metal, mechanical grind, toxic factory | OpenGameArt track: https://opengameart.org/content/shadows-awaken-within ; OpenGameArt track: https://opengameart.org/content/unholy-surge ; Incompetech catalog: https://incompetech.com/music/royalty-free/ | Prioritize CC0 from OGA; if using CC-BY, add artist/title/license in in-game credits and App Store support page. |

Notes:
- Avoid Mixkit stock music for in-game BGM. Their FAQ states music is not permitted for video games: https://mixkit.co/free-stock-music/
- Keep master music stems in `WAV`, import gameplay copies as OGG/Vorbis.

---

## 2. Audio - Sound Effects (35 Clips)

### 2.1 Hovercraft SFX

| ID | Name | Specs | Where Used | Search Terms | Source Options (2-3) | License Compatibility Notes |
|---|---|---|---|---|---|---|
| 2.1 | Engine Loop | OGG, 2-5s seamless, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (`engineLoop`) | sci-fi engine loop, hover hum, turbine idle | Freesound query: https://freesound.org/search/?q=sci-fi+engine+loop ; Mixkit engine SFX: https://mixkit.co/free-sound-effects/engine/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/engine/ | On Freesound, keep to CC0 or CC-BY only. |
| 2.2 | Boost Activate | OGG, 0.5-1s, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (`boostClip`) | boost whoosh, power surge, thruster ignite | Freesound query: https://freesound.org/search/?q=boost+whoosh ; Mixkit whoosh tags: https://mixkit.co/free-sound-effects/technology/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/whoosh/ | Trim transient to avoid click and normalize to -1 dB. |
| 2.3 | Boost Sustain | OGG, 1-2s loop, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (layer with boost state) | rocket flame loop, thrust sustain, burner | Freesound query: https://freesound.org/search/?q=rocket+flame+loop ; Mixkit engine SFX: https://mixkit.co/free-sound-effects/engine/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/rocket/ | Prefer clips with stable noise floor to support seamless looping. |
| 2.4 | Brake | OGG, 0.5-1s, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (`brakeClip`) | brake screech, metal friction, hover brake | Freesound query: https://freesound.org/search/?q=brake+screech ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack ; Pixabay SFX: https://pixabay.com/sound-effects/search/brake/ | If CC-BY, track attribution per clip in credits sheet. |
| 2.5 | Collision Impact | OGG, 0.3-0.5s, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (`collisionClip`) | metal impact, crash hit, thud | Freesound query: https://freesound.org/search/?q=metal+impact ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack ; Pixabay SFX: https://pixabay.com/sound-effects/search/metal%20impact/ | Choose short tails to prevent overlap mud during frequent impacts. |
| 2.6 | Shield Hit | OGG, 0.3-0.5s, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (`shieldHitClip`) | energy shield hit, force field impact | Freesound query: https://freesound.org/search/?q=energy+shield+hit ; Mixkit game SFX: https://mixkit.co/free-sound-effects/game/ ; OpenGameArt sci-fi SFX: https://opengameart.org/content/sound-effects-pack | Keep high-frequency content controlled for mobile speakers. |
| 2.7 | Health Hit | OGG, 0.3-0.5s, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (`healthHitClip`) | damage hit, metallic crunch, armor hit | Freesound query: https://freesound.org/search/?q=metal+damage+hit ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack ; Pixabay SFX: https://pixabay.com/sound-effects/search/hit/ | Distinguish from shield hit by lower-pitched transient. |
| 2.8 | Shield Break | OGG, 0.5-1s, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (`shieldBreakClip`) | shield break, energy shatter, glassy burst | Freesound query: https://freesound.org/search/?q=energy+shatter ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack ; Pixabay SFX: https://pixabay.com/sound-effects/search/shatter/ | Consider layered tonal drop for clear gameplay feedback. |
| 2.9 | Explosion | OGG, 1-2s, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (`explosionClip`) | vehicle explosion, large blast, fireball | Freesound query: https://freesound.org/search/?q=vehicle+explosion ; Mixkit boom SFX: https://mixkit.co/free-sound-effects/boom/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/explosion/ | Keep one dry and one distant variant for mix control. |
| 2.10 | Respawn | OGG, 1-1.5s, mono | `Assets/Scripts/Hovercraft/HovercraftAudio.cs` (`respawnClip`) | respawn materialize, power up appear | Freesound query: https://freesound.org/search/?q=respawn+sound ; Mixkit game SFX: https://mixkit.co/free-sound-effects/game/ ; OpenGameArt UI library: https://opengameart.org/content/ui-sound-effects-library | Ensure this has unique tonal identity versus collectible pickup. |

### 2.2 Hazard SFX

| ID | Name | Specs | Where Used | Search Terms | Source Options (2-3) | License Compatibility Notes |
|---|---|---|---|---|---|---|
| 2.11 | Lava Bubbling | OGG, 3-5s loop, mono | `Assets/Scripts/Core/AudioManager.cs` (lava ambient bus) | lava bubbling loop, magma surface | Freesound query: https://freesound.org/search/?q=lava+bubbling+loop ; Pixabay SFX: https://pixabay.com/sound-effects/search/lava/ ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack | Prefer low-mid focused clips to avoid masking HUD SFX. |
| 2.12 | Eruption Rumble | OGG, 2-3s, mono | `Assets/Scripts/Hazards/Lava/VolcanicEruption.cs` (`warningRumble`) | volcanic rumble warning, quake rumble | Freesound query: https://freesound.org/search/?q=volcanic+rumble ; Mixkit boom SFX: https://mixkit.co/free-sound-effects/boom/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/rumble/ | Needs clear pre-eruption telegraph before damage window. |
| 2.13 | Eruption Blast | OGG, 1-2s, mono | `Assets/Scripts/Hazards/Lava/VolcanicEruption.cs` (`eruptionSound`) | volcanic eruption blast, magma burst | Freesound query: https://freesound.org/search/?q=volcanic+eruption ; Pixabay SFX: https://pixabay.com/sound-effects/search/eruption/ ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack | Layer with low-frequency one-shot for impact weight. |
| 2.14 | Geyser Burst | OGG, 1-2s, mono | `Assets/Scripts/Hazards/Lava/LavaGeyser.cs` (`eruptSound`) | geyser burst, steam vent blast | Freesound query: https://freesound.org/search/?q=geyser+burst ; Pixabay SFX: https://pixabay.com/sound-effects/search/steam/ ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack | Clip should have quick attack and short decay for repeated hazards. |
| 2.15 | Ice Crack | OGG, 0.5-1s, mono | `Assets/Scripts/Hazards/Ice/FallingIcicle.cs` (`crackSound`) | ice cracking, frozen crack | Freesound query: https://freesound.org/search/?q=ice+crack ; Pixabay SFX: https://pixabay.com/sound-effects/search/ice/ ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack | Keep brittle high-end; avoid long reverbs in gameplay mix. |
| 2.16 | Ice Shatter | OGG, 0.5-1s, mono | `Assets/Scripts/Hazards/Ice/FallingIcicle.cs` (`shatterSound`) / `Assets/Scripts/Hazards/Ice/IceWall.cs` | ice shatter, frozen break, glassy ice | Freesound query: https://freesound.org/search/?q=ice+shatter ; Pixabay SFX: https://pixabay.com/sound-effects/search/shatter/ ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack | Distinguish from regular collision by brighter transient. |
| 2.17 | Wind/Blizzard | OGG, 5-10s loop, mono | `Assets/Scripts/Hazards/Ice/BlizzardZone.cs` (`windLoop`) | blizzard wind loop, howling wind | Freesound query: https://freesound.org/search/?q=blizzard+wind+loop ; Pixabay SFX: https://pixabay.com/sound-effects/search/wind/ ; Mixkit wind SFX: https://mixkit.co/free-sound-effects/technology/ | Use loop with no directional bias for 3D point playback. |
| 2.18 | Avalanche Rumble | OGG, 3-5s loop, mono | `Assets/Scripts/Hazards/Ice/Avalanche.cs` (`rumbleLoop`) | avalanche rumble, snow slide | Freesound query: https://freesound.org/search/?q=avalanche+rumble ; Pixabay SFX: https://pixabay.com/sound-effects/search/avalanche/ ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack | Keep dynamic range moderate so rumble remains audible on phone speakers. |
| 2.19 | Toxic Hiss | OGG, 1-2s, mono | `Assets/Scripts/Hazards/Toxic/ToxicGas.cs` (`hissSound`) | toxic gas hiss, vent release | Freesound query: https://freesound.org/search/?q=gas+hiss ; Pixabay SFX: https://pixabay.com/sound-effects/search/gas/ ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack | Use band-limited hiss to avoid listener fatigue. |
| 2.20 | Acid Sizzle | OGG, 0.5-1s, mono | `Assets/Scripts/Hazards/Toxic/AcidPool.cs` (`sizzleSound`) | acid sizzle, corrosive burn | Freesound query: https://freesound.org/search/?q=acid+sizzle ; Pixabay SFX: https://pixabay.com/sound-effects/search/sizzle/ ; Mixkit misc SFX: https://mixkit.co/free-sound-effects/game/ | Add subtle pitch randomization to reduce repetition. |
| 2.21 | Industrial Press | OGG, 0.5-1s, mono | `Assets/Scripts/Hazards/Toxic/IndustrialPress.cs` (`slamSound`, `hydraulicSound`) | hydraulic press slam, metal ram | Freesound query: https://freesound.org/search/?q=hydraulic+press ; Pixabay SFX: https://pixabay.com/sound-effects/search/hydraulic/ ; Mixkit industrial tags: https://mixkit.co/free-sound-effects/technology/ | Pull two clips: one pre-motion hydraulic, one slam impact. |
| 2.22 | Electric Zap | OGG, 0.3-0.5s, mono | `Assets/Scripts/Hazards/Toxic/ElectricFence.cs` (`zapSound`) | electric zap, electric shock | Freesound query: https://freesound.org/search/?q=electric+zap ; Pixabay SFX: https://pixabay.com/sound-effects/search/electric/ ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack | Keep transient short so repeated fence hits stay clear. |
| 2.23 | Electric Hum | OGG, 2-3s loop, mono | `Assets/Scripts/Hazards/Toxic/ElectricFence.cs` (`electricHum`) | powerline hum, electric fence buzz | Freesound query: https://freesound.org/search/?q=electric+hum+loop ; Pixabay SFX: https://pixabay.com/sound-effects/search/hum/ ; Mixkit tech SFX: https://mixkit.co/free-sound-effects/technology/ | Validate seamless loop and no phasing on loop boundary. |
| 2.24 | Barrel Explosion | OGG, 0.5-1s, mono | `Assets/Scripts/Hazards/Toxic/BarrelExplosion.cs` (`explosionSound`) | barrel explosion, small blast | Freesound query: https://freesound.org/search/?q=barrel+explosion ; Mixkit boom SFX: https://mixkit.co/free-sound-effects/boom/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/explosion/ | Keep shorter and less bass-heavy than player destruction explosion. |

### 2.3 Ambient Loops

| ID | Name | Specs | Where Used | Search Terms | Source Options (2-3) | License Compatibility Notes |
|---|---|---|---|---|---|---|
| 2.25 | Lava Ambient | OGG, 10-30s loop, stereo | `Assets/Scripts/Core/AudioManager.cs` + lava environment setup | volcanic ambience, fire crackle, magma | Freesound query: https://freesound.org/search/?q=volcanic+ambient+loop ; Pixabay SFX: https://pixabay.com/sound-effects/search/volcano/ ; OpenGameArt SFX pack: https://opengameart.org/content/sound-effects-pack | Keep stereo width moderate for mobile speakers/headphones. |
| 2.26 | Ice Ambient | OGG, 10-30s loop, stereo | `Assets/Scripts/Core/AudioManager.cs` + ice environment setup | arctic ambience, icy wind loop | Freesound query: https://freesound.org/search/?q=arctic+ambient+loop ; Pixabay SFX: https://pixabay.com/sound-effects/search/arctic/ ; Mixkit wind SFX: https://mixkit.co/free-sound-effects/run/ | Prefer low-noise loops with no obvious one-shot events. |
| 2.27 | Toxic Ambient | OGG, 10-30s loop, stereo | `Assets/Scripts/Core/AudioManager.cs` + toxic environment setup | industrial ambient loop, machinery hum | Freesound query: https://freesound.org/search/?q=industrial+ambient+loop ; Pixabay SFX: https://pixabay.com/sound-effects/search/industrial/ ; Mixkit tech SFX: https://mixkit.co/free-sound-effects/technology/ | Avoid tonal drones that conflict with music key center. |
| 2.28 | Workshop Ambient | OGG, 10-30s loop, stereo | `Assets/Scripts/Workshop/WorkshopManager.cs`, `Assets/Scripts/Core/AudioManager.cs` | garage ambience, workshop tools, room tone | Freesound query: https://freesound.org/search/?q=garage+ambient+loop ; Pixabay SFX: https://pixabay.com/sound-effects/search/workshop/ ; Mixkit ambience SFX: https://mixkit.co/free-sound-effects/lifestyle/ | Keep this subtle to avoid masking menu/UI interaction sounds. |

### 2.4 UI SFX

| ID | Name | Specs | Where Used | Search Terms | Source Options (2-3) | License Compatibility Notes |
|---|---|---|---|---|---|---|
| 2.29 | Button Click | OGG, 0.1-0.2s, mono | `Assets/Scripts/UI/UIManager.cs`, menu and workshop UI scripts | metal UI click, menu click | OpenGameArt UI SFX: https://opengameart.org/content/ui-sound-effects-library ; Mixkit arcade SFX: https://mixkit.co/free-sound-effects/arcade/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/click/ | Keep one default click across all buttons for consistency. |
| 2.30 | Purchase Confirm | OGG, 0.5-1s, mono | `Assets/Scripts/Workshop/UpgradeUI.cs`, `Assets/Scripts/Workshop/CustomizationUI.cs` | purchase success, coin confirm, cash register | OpenGameArt basic SFX: https://opengameart.org/content/basic-sound-effects ; Mixkit game SFX: https://mixkit.co/free-sound-effects/game/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/coin/ | Use positive but short cue; avoid casino-like long stingers. |
| 2.31 | Medal Earned | OGG, 1-3s, mono | `Assets/Scripts/UI/ResultsScreenUI.cs` | medal fanfare, achievement unlock | OpenGameArt UI SFX: https://opengameart.org/content/ui-sound-effects-library ; Mixkit ding SFX: https://mixkit.co/free-sound-effects/ding/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/fanfare/ | Prepare 3 variants (bronze/silver/gold) from one base source. |
| 2.32 | Countdown Tick | OGG, 0.2-0.3s, mono | `Assets/Scripts/UI/CountdownUI.cs` | countdown beep, race tick | OpenGameArt UI SFX: https://opengameart.org/content/ui-sound-effects-library ; Mixkit arcade sport SFX: https://mixkit.co/free-sound-effects/arcade-sport/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/countdown/ | Keep transient sharp and high-frequency for race-start clarity. |
| 2.33 | Countdown Go | OGG, 0.5-1s, mono | `Assets/Scripts/UI/CountdownUI.cs`, `Assets/Scripts/Course/CourseManager.cs` | race start horn, go signal | Mixkit race SFX: https://mixkit.co/free-sound-effects/race-car/ ; OpenGameArt basic SFX: https://opengameart.org/content/basic-sound-effects ; Pixabay SFX: https://pixabay.com/sound-effects/search/race/ | Should clearly differ from countdown ticks and medal stingers. |
| 2.34 | Collectible Pickup | OGG, 0.3-0.5s, mono | `Assets/Scripts/Course/Collectible.cs`, `Assets/Scripts/UI/CurrencyDisplay.cs` | pickup chime, sparkle coin | OpenGameArt basic SFX: https://opengameart.org/content/basic-sound-effects ; Mixkit ding SFX: https://mixkit.co/free-sound-effects/ding/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/pickup/ | Choose clean, bright cue that stays audible at speed. |
| 2.35 | Course Unlock | OGG, 1-1.5s, mono | `Assets/Scripts/UI/CourseUnlockedPopup.cs`, `Assets/Scripts/Workshop/CourseSelectionUI.cs` | unlock sound, reveal stinger | OpenGameArt random SFX: https://opengameart.org/content/random-sfx-sound-effects ; Mixkit arcade SFX: https://mixkit.co/free-sound-effects/arcade/ ; Pixabay SFX: https://pixabay.com/sound-effects/search/unlock/ | Keep this longer than button click but shorter than medal fanfare. |

SFX implementation notes:
- Store raw master in `WAV`, then import compressed OGG into Unity.
- For Freesound and OpenGameArt, maintain `Docs/ASSET_ATTRIBUTION.csv` with `asset_name, author, url, license`.
- Filter Freesound to `CC0`/`CC-BY` only. Skip `CC-BY-NC` for App Store release.

---

## 3. Fonts (2 Fonts)

| ID | Role | Specs and Requirements | Where Used | Search Terms | Source Options (2-3) | License Compatibility Notes |
|---|---|---|---|---|---|---|
| 3.1 | Display Font (titles/headers) | Heavy/industrial style, all caps friendly, numbers 0-9, punctuation, bold weight | `Assets/Scripts/UI/MainMenuUI.cs`, `Assets/Scripts/UI/CountdownUI.cs`, `Assets/Scripts/UI/ResultsScreenUI.cs`, TMP title assets | heavy display, metal title, industrial headline | Black Ops One: https://fonts.google.com/specimen/Black+Ops+One ; Bungee: https://fonts.google.com/specimen/Bungee ; Rubik Mono One: https://fonts.google.com/specimen/Rubik+Mono+One | Google Fonts families ship with OFL-compatible licensing; safe for commercial games with redistribution in built app. |
| 3.2 | Body Font (UI text/stats) | Highly readable at small sizes, regular+bold weights, full numeric set and punctuation | HUD, settings, upgrade stats, descriptions in UI/workshop scripts | tech sans, futuristic UI font, readable game UI | Rajdhani: https://fonts.google.com/specimen/Rajdhani ; Orbitron: https://fonts.google.com/specimen/Orbitron ; Exo 2: https://fonts.google.com/specimen/Exo+2 | OFL-compatible families are preferred. For DaFont alternatives, verify each font's included license/readme before use. |

Font backup sources:
- Font Squirrel catalog: https://www.fontsquirrel.com/
- DaFont metal category exploration: https://www.dafont.com/

---

## 4. Textures (15 Textures)

| ID | Name | Specs | Where Used | Search Terms | Source Options (2-3) | License Compatibility Notes |
|---|---|---|---|---|---|---|
| 4.1 | Lava Noise Texture | 256x256 PNG, seamless grayscale noise | `Assets/Shaders/Environment/Lava/LavaSurface.shader` (`_NoiseTex`), `Assets/Shaders/Environment/Toxic/ToxicSludge.shader` | seamless noise texture, flow noise | ambientCG search: https://ambientcg.com/list?search=noise ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=noise | Prefer CC0 from ambientCG/Poly Haven. |
| 4.2 | Lava Surface Texture | 512x512 PNG, seamless lava albedo | `Assets/Shaders/Environment/Lava/LavaSurface.shader` (`_MainTex`) | lava seamless texture, molten surface | ambientCG search: https://ambientcg.com/list?search=lava ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=lava | Textures.com is commercial-use allowed but redistribution restrictions apply. |
| 4.3 | Volcanic Rock + Normal | 512x512 PNG set, seamless dark rock | `Assets/Shaders/Environment/Lava/VolcanicRock.shader` (`_MainTex`, `_BumpMap`) | basalt rock seamless, volcanic rock pbr | ambientCG search: https://ambientcg.com/list?search=basalt ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=volcanic%20rock | Ensure normal map tangent space matches Unity import settings. |
| 4.4 | Volcanic Vein Emissive Mask | 512x512 PNG, B/W crack mask | `Assets/Shaders/Environment/Lava/VolcanicRock.shader` (`_EmissiveMask`) | crack mask texture, emissive vein map | ambientCG cracked surfaces: https://ambientcg.com/list?search=crack ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com crack search: https://www.textures.com/search?q=crack | May be hand-authored from CC0 crack sources; keep source attribution for derivatives if not CC0. |
| 4.5 | Ice Texture + Normal | 512x512 PNG set, seamless cracked ice | `Assets/Shaders/Environment/Ice/IceSurface.shader` (`_MainTex`, `_BumpMap`) | cracked ice seamless, ice pbr | ambientCG search: https://ambientcg.com/list?search=ice ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=ice | Use high smoothness, preserve detail with ASTC 6x6 quality checks. |
| 4.6 | Frost Pattern Texture | 512x512 PNG, seamless frost crystals | `Assets/Shaders/Environment/Ice/FrostOverlay.shader` (`_FrostTex`) | frost overlay texture, ice crystal pattern | ambientCG search: https://ambientcg.com/list?search=frost ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=frost | Keep contrast moderate to avoid over-obscuring gameplay view. |
| 4.7 | Toxic Sludge Texture | 256x256 PNG, seamless bubbling liquid | `Assets/Shaders/Environment/Toxic/ToxicSludge.shader` (`_MainTex`) | toxic slime texture, green sludge seamless | ambientCG search: https://ambientcg.com/list?search=slime ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=sludge | Can derive by color-grading lava source if license allows derivatives. |
| 4.8 | Rusted Metal + Normal + Rust Mask | 512x512 PNG set, seamless rust metal | `Assets/Shaders/Environment/Toxic/RustedMetal.shader` (`_MainTex`, `_BumpMap`, `_RustMask`) | rusted metal pbr, corrosion mask | ambientCG search: https://ambientcg.com/list?search=rust ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=rusted%20metal | Prefer CC0 packs with included roughness/normal maps. |
| 4.9 | Hex Pattern Texture | 256x256 PNG, seamless hex lines | `Assets/Shaders/Hovercraft/ShieldBubble.shader` (`_HexPattern`) | hex grid texture, hologram hex pattern | ambientCG search: https://ambientcg.com/list?search=hexagon ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=hexagon | If generated manually, keep source file in project art source folder. |
| 4.10 | Hovercraft Color Mask | 512x512 PNG, RGB mask channels | `Assets/Shaders/Hovercraft/HovercraftBody.shader` (`_MaskTex`) | color mask uv template, rgb material mask | Kenney vehicle references: https://kenney.nl/assets/car-kit ; Sketchfab hovercraft refs: https://sketchfab.com/3d-models/underpoly-free-generic-hovercraft-46805762e91049b6ab20a737728ede5f ; Poly Pizza vehicle refs: https://poly.pizza/ | This is normally custom-authored for final UVs; source refs are for layout guidance only. |
| 4.11 | Metal Panel Texture | 256x256 PNG, seamless brushed/scratched metal | `Assets/Shaders/UI/MetalPanel.shader` (`_MainTex`) | brushed metal seamless, ui metal panel | ambientCG search: https://ambientcg.com/list?search=brushed%20metal ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=scratched%20metal | UI texture can disable mipmaps for sharper text overlays. |
| 4.12 | Rivet Overlay Texture | 256x256 PNG, transparent rivet edges | `Assets/Shaders/UI/MetalPanel.shader` (`_RivetTex`) | rivet overlay, metal rivets transparent | ambientCG search: https://ambientcg.com/list?search=rivet ; Poly Haven textures: https://polyhaven.com/textures ; Textures.com search: https://www.textures.com/search?q=rivet | Often hand-authored from CC0 rivet references for clean alpha edges. |
| 4.13 | Particle Texture (soft circle) | 64x64 PNG, white radial falloff | Base sprite for many particle prefabs generated by particle tools | soft particle circle, radial gradient sprite | Kenney particles/effects: https://kenney.nl/assets ; OpenGameArt particle refs: https://opengameart.org/ ; Poly Haven textures: https://polyhaven.com/textures | Generated textures are preferred for consistency and zero licensing ambiguity. |
| 4.14 | Spark Particle Texture | 32x32 PNG, white streak shape | Spark systems (`Damage`, `Electric`, `Metal hit`) | spark streak sprite, particle streak | Kenney effects assets: https://kenney.nl/assets ; OpenGameArt effects packs: https://opengameart.org/ ; Pixabay texture refs: https://pixabay.com/images/search/spark/ | Use alpha-friendly format and disable compression artifacts for tiny sprites. |
| 4.15 | Smoke Particle Texture | 128x128 PNG, soft cloud alpha | Smoke/gas/explosion particle systems | smoke puff particle, cloud alpha sprite | Kenney effects assets: https://kenney.nl/assets ; OpenGameArt effects packs: https://opengameart.org/ ; Pixabay image search: https://pixabay.com/images/search/smoke/ | Ensure source allows derivative processing into sprite atlas. |

Texture sourcing note:
- Prioritize `ambientCG` and `Poly Haven` first (both CC0).
- Use `Textures.com` only when needed and keep usage aligned with their redistribution restrictions.

---

## 5. 3D Models (5 Models)

| ID | Name | Specs | Where Used | Search Terms | Source Options (2-3) | License Compatibility Notes |
|---|---|---|---|---|---|---|
| 5.1 | Hovercraft Model | FBX/OBJ, target 3k-5k tris, UVs for color mask | `Assets/Prefabs/Vehicles/Hovercraft_Player.prefab`, `Assets/Scripts/Hovercraft/HovercraftController.cs`, `Assets/Scripts/Hovercraft/HovercraftCustomization.cs` | low poly hovercraft, sci-fi hover vehicle | Sketchfab (CC-BY): https://sketchfab.com/3d-models/underpoly-free-generic-hovercraft-46805762e91049b6ab20a737728ede5f ; Sketchfab alternative: https://sketchfab.com/3d-models/hovercraft-01d848ea3e83466abf6282e35ee49b8b ; Kenney fallback vehicle kit (CC0): https://kenney.nl/assets/car-kit | Prefer CC0/CC-BY downloadable models; decimate high-poly assets to mobile budget. |
| 5.2 | Protagonist Character | FBX, 1k-2k tris target, humanoid rig; animations: idle/work/celebrate | `Assets/Scripts/Workshop/ProtagonistController.cs`, workshop scene | rigged low poly character, mixamo character | Mixamo home: https://www.mixamo.com/ ; Mixamo commercial FAQ: https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html ; Sketchfab character search: https://sketchfab.com/search?type=models&q=low%20poly%20character%20cc0&features=downloadable | Mixamo allows commercial game use; do not redistribute raw character/animation files as standalone assets. |
| 5.3 | Workshop Environment Props | FBX/OBJ low-poly set (bench, racks, amp, crates, shelves) | Workshop scene setup and decoration | industrial workshop props, low poly factory props | Kenney City Kit Industrial (CC0): https://kenney.nl/assets/city-kit-industrial ; Kenney Conveyor Kit (CC0): https://kenney.nl/assets/conveyor-kit ; Poly Pizza industrial examples: https://poly.pizza/m/h5RUr3vlcS | Kenney is safest default (CC0). On Poly Pizza, verify per-model license (CC0 or CC-BY shown on model page). |
| 5.4 | Checkpoint Marker | FBX/OBJ, <500 tris, emissive gate/ring style | `Assets/Scripts/Course/Checkpoint.cs` | checkpoint gate sci-fi, race checkpoint marker | Kenney Racing Kit (CC0): https://kenney.nl/assets/racing-kit ; Poly Pizza search root: https://poly.pizza/ ; OpenGameArt 3D assets: https://opengameart.org/ | If no fit is found quickly, build from Unity primitives and custom emissive material. |
| 5.5 | Collectible Model | FBX/OBJ, <200 tris, crystal/coin/orb style | `Assets/Scripts/Course/Collectible.cs` | low poly collectible gem, pickup crystal | Kenney assets root (CC0): https://kenney.nl/assets ; Poly Pizza model library: https://poly.pizza/ ; Sketchfab downloadable CC search: https://sketchfab.com/search?type=models&q=low%20poly%20collectible%20cc0&features=downloadable | Keep silhouette simple and readable at race speed; use emissive shader for visibility. |

---

## 6. Quick-Start Asset Pack Recommendations

Use these first for a 1-2 hour project bootstrap.

| Priority | Pack/Source | Why It Is Fast | Link | License Note |
|---|---|---|---|---|
| 1 | Kenney CC0 Asset Packs | Immediate drop-in props/models/UI elements with uniform style | https://kenney.nl/assets | CC0/public domain (commercial safe, no attribution required). |
| 2 | ambientCG | PBR textures with maps (albedo/normal/roughness) ready for shaders | https://ambientcg.com/ | CC0; free for commercial use and redistribution in project. |
| 3 | Poly Haven | High quality CC0 textures/models for environment polish | https://polyhaven.com/ | CC0; commercial-safe and attribution optional. |
| 4 | Freesound (filtered) | Fast SFX coverage for hazard/UI/vehicle sounds | https://freesound.org/ | Only pull CC0 or CC-BY clips for this project. |
| 5 | Mixamo | Quick rigged humanoids + animation clips for protagonist | https://www.mixamo.com/ | Adobe FAQ states personal/commercial use allowed; avoid raw-asset redistribution. |

Music quick-start:
- For immediate placeholder gameplay music, use OpenGameArt CC0 metal tracks first.
- Add Incompetech track(s) when you can support CC attribution text cleanly.

---

## 7. Unity Import Settings (iOS Optimization)

### Audio

| Asset Type | Format | Load Type | Quality | Sample Rate | Notes |
|---|---|---|---|---|---|
| Music (BGM) | Vorbis/OGG | Streaming | 0.70 | 44.1 kHz | Long files, minimize memory footprint. |
| SFX short one-shots | Vorbis/OGG | Decompress On Load | 1.00 | 22.05 kHz | Low latency for responsive gameplay. |
| Ambient loops | Vorbis/OGG | Compressed In Memory | 0.80 | 44.1 kHz | Loop-check in Unity import preview. |

### Textures

| Texture Class | Max Size | Compression | Mipmaps | Filter | Notes |
|---|---|---|---|---|---|
| Environment shader textures | 512 | ASTC 6x6 | On | Bilinear | Keep normal maps readable. |
| Hovercraft body + masks | 1024 (body), 512 (masks) | ASTC 6x6 | On | Bilinear | Preserve mask channel fidelity. |
| UI textures | 256-512 | ASTC 6x6 or RGBA32 if artifacts | Off | Bilinear | Disable mipmaps for crisp UI. |
| Particle textures | 64-256 | ASTC 6x6 | Off | Bilinear | Avoid over-compressing small alpha sprites. |

### Models

| Model Class | Scale Factor | Mesh Compression | Read/Write | Animation Type | Notes |
|---|---|---|---|---|---|
| Hovercraft + props | 1.0 | Medium | Off | None | Keep pivot centered for gameplay scripts. |
| Protagonist | 1.0 | Medium | Off | Humanoid | Retarget Mixamo clips through Avatar setup. |

### Fonts (TextMeshPro)

| Font Role | TMP Atlas | Character Set | Fallback |
|---|---|---|---|
| Display | 1024x1024 | ASCII + digits + punctuation | Add body font as fallback for unsupported glyphs. |
| Body | 512x512 | Full UI character set | Include symbols used in settings and timers. |

---

## 8. Execution Checklist

- [ ] Download first-pass assets for all 62 rows (5 + 35 + 2 + 15 + 5).
- [ ] Verify every selected file license before import (CC0/CC-BY/OFL/royalty-free app-safe).
- [ ] Record attribution-required assets in a single tracking sheet.
- [ ] Import with iOS settings above and test on target device.
- [ ] Replace placeholders with higher quality alternatives only when gameplay stability is done.

## 9. Attribution Tracking Template

Use this CSV shape when any `CC-BY` item is selected:

```csv
asset_id,asset_name,author,source_url,license,credit_text,used_in
2.29,Button Click,Little Robot Sound Factory,https://opengameart.org/content/ui-sound-effects-library,CC-BY 3.0,"UI Sound Effects Library by Little Robot Sound Factory (CC-BY 3.0)",MainMenuUI
```

