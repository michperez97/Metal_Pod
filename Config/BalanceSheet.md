# Metal Pod Balance Sheet

## Currency Economy

### Base Rewards By Difficulty
| Difficulty | Base Reward |
| --- | ---: |
| Easy | 100 |
| Medium | 150 |
| Hard | 200 |
| Extreme | 300 |

### Reward Formula
- `total = round(baseReward * completionMultiplier * (1 + medalBonus)) + collectibleBonus`
- `completionMultiplier`: `1.0` first completion, `0.5` replay
- `collectibleBonus`: `10` per collectible

### Medal Bonuses
| Medal | Bonus |
| --- | ---: |
| None | +0% |
| Bronze | +25% |
| Silver | +50% |
| Gold | +100% |

## Upgrade Costs And Effects

### Speed (`Upgrade_Speed`)
| Level | Cost | Multiplier | Effect |
| --- | ---: | ---: | --- |
| 1 | 100 | 1.10 | +10% max speed |
| 2 | 250 | 1.20 | +20% max speed |
| 3 | 500 | 1.30 | +30% max speed |
| 4 | 1000 | 1.40 | +40% max speed |
| 5 | 2000 | 1.50 | +50% max speed |

### Handling (`Upgrade_Handling`)
| Level | Cost | Multiplier | Effect |
| --- | ---: | ---: | --- |
| 1 | 100 | 1.08 | +8% handling |
| 2 | 250 | 1.16 | +16% handling |
| 3 | 500 | 1.24 | +24% handling |
| 4 | 1000 | 1.32 | +32% handling |
| 5 | 2000 | 1.40 | +40% handling |

### Shield (`Upgrade_Shield`)
| Level | Cost | Multiplier | Effect |
| --- | ---: | ---: | --- |
| 1 | 150 | 1.12 | +12% shield capacity |
| 2 | 350 | 1.24 | +24% shield capacity |
| 3 | 600 | 1.36 | +36% shield capacity |
| 4 | 1200 | 1.48 | +48% shield capacity |
| 5 | 2500 | 1.60 | +60% shield capacity |

### Boost (`Upgrade_Boost`)
| Level | Cost | Multiplier | Effect |
| --- | ---: | ---: | --- |
| 1 | 100 | 1.10 | +10% boost power |
| 2 | 250 | 1.20 | +20% boost power |
| 3 | 500 | 1.30 | +30% boost power |
| 4 | 1000 | 1.40 | +40% boost power |
| 5 | 2000 | 1.50 | +50% boost power |

## Course Medal Targets

### Lava
| Course | Gold | Silver | Bronze |
| --- | ---: | ---: | ---: |
| Lava 1 - Inferno Gate | 50s | 65s | 80s |
| Lava 2 - Magma Run | 70s | 90s | 110s |
| Lava 3 - Eruption | 90s | 120s | 150s |

### Ice
| Course | Gold | Silver | Bronze |
| --- | ---: | ---: | ---: |
| Ice 1 - Frozen Lake | 60s | 80s | 100s |
| Ice 2 - Crystal Caverns | 80s | 105s | 130s |
| Ice 3 - Avalanche Pass | 100s | 130s | 160s |

### Toxic
| Course | Gold | Silver | Bronze |
| --- | ---: | ---: | ---: |
| Toxic 1 - Waste Disposal | 65s | 85s | 105s |
| Toxic 2 - The Foundry | 85s | 110s | 140s |
| Toxic 3 - Meltdown | 110s | 145s | 180s |

### Medal Time Rationale
- Gold targets require optimized lines, low damage, and high hazard familiarity.
- Silver targets are the expected pace for skilled non-speedrun play.
- Bronze targets keep first clears accessible while preserving medal progression pressure.

## Unlock Progression Chain
- Lava 1: default unlocked
- Lava 2: complete Lava 1
- Lava 3: complete Lava 2 and total medals >= 3
- Ice 1: complete Lava 3 and total medals >= 5
- Ice 2: complete Ice 1 and total medals >= 7
- Ice 3: complete Ice 2 and total medals >= 9
- Toxic 1: complete Ice 3 and total medals >= 12
- Toxic 2: complete Toxic 1 and total medals >= 15
- Toxic 3: complete Toxic 2 and total medals >= 18

## Progression Pacing

### Target Milestones
- First upgrade (any): ~2-3 course completions (200-300 currency)
- Full Tier 1 upgrades: ~8-10 completions
- Full Tier 2 upgrades: ~20-25 completions
- Full Tier 5 upgrades: ~100+ completions (endgame)
- All cosmetics: ~50-60 completions

### Estimated Full Playthrough
- Target total runtime to unlock all courses with mostly silver medals: **~4-6 hours**.
- Assumes moderate replay for medal improvement and selective collectible routing.

## Notes For Tuning Iteration
- If players unlock Ice/Toxic too slowly, reduce `requiredMedals` or increase medium/hard base rewards.
- If early upgrades are too fast, reduce first-clear multipliers only on Easy tracks.
- Keep replay multiplier at `0.5` to reward optimization without making grind dominant.
