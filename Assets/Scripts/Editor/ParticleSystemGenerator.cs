#if UNITY_EDITOR
using System;
using System.IO;
using MetalPod.Hovercraft;
using UnityEditor;
using UnityEngine;

namespace MetalPod.Editor
{
    public static class ParticleSystemGenerator
    {
        private const string HovercraftPrefabPath = "Assets/Prefabs/Hovercraft/Hovercraft.prefab";

        private const string EffectsRoot = "Assets/Prefabs/Effects";
        private const string HovercraftEffectsRoot = EffectsRoot + "/Hovercraft";
        private const string LavaEffectsRoot = EffectsRoot + "/Hazards/Lava";
        private const string IceEffectsRoot = EffectsRoot + "/Hazards/Ice";
        private const string ToxicEffectsRoot = EffectsRoot + "/Hazards/Toxic";
        private const string CommonEffectsRoot = EffectsRoot + "/Common";
        private const string UIEffectsRoot = EffectsRoot + "/UI";
        private const string MaterialRoot = "Assets/Materials/Particles";

        [MenuItem("Metal Pod/Effects/Generate All Particle Prefabs")]
        public static void GenerateAll()
        {
            EditorUtility.DisplayProgressBar("Generating Particles", "Configuring hovercraft particles...", 0.05f);
            try
            {
                GenerateHovercraftParticles();

                EditorUtility.DisplayProgressBar("Generating Particles", "Generating lava effects...", 0.2f);
                GenerateLavaEffects();

                EditorUtility.DisplayProgressBar("Generating Particles", "Generating ice effects...", 0.38f);
                GenerateIceEffects();

                EditorUtility.DisplayProgressBar("Generating Particles", "Generating toxic effects...", 0.56f);
                GenerateToxicEffects();

                EditorUtility.DisplayProgressBar("Generating Particles", "Generating common effects...", 0.75f);
                GenerateCommonEffects();

                EditorUtility.DisplayProgressBar("Generating Particles", "Generating UI effects...", 0.88f);
                GenerateUIEffects();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[ParticleSystemGenerator] All particle prefabs generated and configured.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Metal Pod/Effects/Configure Hovercraft Particles")]
        public static void GenerateHovercraftParticles()
        {
            EnsureFolder(HovercraftEffectsRoot);
            EnsureFolder(MaterialRoot);

            Material thrusterMat = GetOrCreateMaterial(
                MaterialRoot + "/ThrusterGlow_Additive.mat",
                new[]
                {
                    "MetalPod/Hovercraft/ThrusterGlow",
                    "Particles/Standard Unlit",
                    "Legacy Shaders/Particles/Additive"
                },
                new Color(0f, 0.86f, 1f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            Material fireMat = GetOrCreateMaterial(
                MaterialRoot + "/Fire_Additive.mat",
                new[]
                {
                    "MetalPod/Hovercraft/ThrusterGlow",
                    "Particles/Standard Unlit",
                    "Legacy Shaders/Particles/Additive"
                },
                new Color(1f, 0.45f, 0f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            Material smokeMat = GetOrCreateMaterial(
                MaterialRoot + "/Smoke_AlphaBlended.mat",
                new[]
                {
                    "Particles/Standard Unlit",
                    "Legacy Shaders/Particles/Alpha Blended"
                },
                new Color(0.2f, 0.2f, 0.2f, 0.65f),
                enableSoftParticles: true,
                additiveFallback: false);

            Material sparkMat = GetOrCreateMaterial(
                MaterialRoot + "/Spark_Additive.mat",
                new[]
                {
                    "MetalPod/Hovercraft/ThrusterGlow",
                    "Particles/Standard Unlit",
                    "Legacy Shaders/Particles/Additive"
                },
                new Color(1f, 0.8f, 0.25f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            string explosionPath = HovercraftEffectsRoot + "/HovercraftExplosion.prefab";
            string respawnPath = CommonEffectsRoot + "/RespawnEffect.prefab";

            GameObject explosionPrefab = CreateOrUpdateParticlePrefab(explosionPath, root =>
            {
                ParticleSystem main = EnsureParticleSystem(root);
                ParticlePresets.ConfigureExplosionFire(main, fireMat, 80, 120);

                GameObject sparksChild = EnsureChild(root.transform, "ExplosionSparks");
                ParticleSystem sparksPs = EnsureParticleSystem(sparksChild);
                ParticlePresets.ConfigureDamageSparks(sparksPs, sparkMat);
                SetAsSubEmitter(main, sparksPs, ParticleSystemSubEmitterType.Birth);

                GameObject smokeChild = EnsureChild(root.transform, "ExplosionSmoke");
                ParticleSystem smokePs = EnsureParticleSystem(smokeChild);
                ParticlePresets.ConfigureExplosionSmoke(smokePs, smokeMat);
                SetAsSubEmitter(main, smokePs, ParticleSystemSubEmitterType.Birth);
            });

            CreateOrUpdateParticlePrefab(respawnPath, root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureRespawnShimmer(ps, thrusterMat);
            });

            EnsureFolder(Path.GetDirectoryName(HovercraftPrefabPath).Replace("\\", "/"));

            GameObject prefabRoot = null;
            bool loadedFromPrefab = File.Exists(HovercraftPrefabPath);
            try
            {
                prefabRoot = loadedFromPrefab
                    ? PrefabUtility.LoadPrefabContents(HovercraftPrefabPath)
                    : CreateHovercraftPrefabShell();

                ConfigureHovercraftParticlesOnObject(prefabRoot, thrusterMat, fireMat, smokeMat, sparkMat, explosionPrefab);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, HovercraftPrefabPath);
            }
            finally
            {
                if (prefabRoot != null)
                {
                    if (loadedFromPrefab)
                    {
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(prefabRoot);
                    }
                }
            }
        }

        [MenuItem("Metal Pod/Effects/Generate Lava Effects")]
        public static void GenerateLavaEffects()
        {
            EnsureFolder(LavaEffectsRoot);

            Material lavaAdd = GetOrCreateMaterial(
                MaterialRoot + "/Lava_Additive.mat",
                new[]
                {
                    "MetalPod/Hovercraft/ThrusterGlow",
                    "MetalPod/Environment/Lava/LavaGeyserSpray",
                    "Particles/Standard Unlit",
                    "Legacy Shaders/Particles/Additive"
                },
                new Color(1f, 0.45f, 0.05f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            Material lavaSmoke = GetOrCreateMaterial(
                MaterialRoot + "/Lava_Smoke.mat",
                new[] { "Particles/Standard Unlit", "Legacy Shaders/Particles/Alpha Blended" },
                new Color(0.3f, 0.25f, 0.2f, 0.45f),
                enableSoftParticles: true,
                additiveFallback: false);

            CreateOrUpdateParticlePrefab(LavaEffectsRoot + "/LavaBubbles.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureLavaBubbles(ps, lavaAdd);
            });

            CreateOrUpdateParticlePrefab(LavaEffectsRoot + "/LavaSplash.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureLavaSplash(ps, lavaAdd);
            });

            CreateOrUpdateParticlePrefab(LavaEffectsRoot + "/EruptionDebris.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureEruptionDebris(ps, lavaAdd);
            });

            CreateOrUpdateParticlePrefab(LavaEffectsRoot + "/GeyserSpray.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureGeyserSpray(ps, lavaAdd);
            });

            CreateOrUpdateParticlePrefab(LavaEffectsRoot + "/HeatShimmer.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureHeatShimmer(ps, lavaSmoke);
            });

            CreateOrUpdateParticlePrefab(LavaEffectsRoot + "/EmberFloat.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureEmberFloat(ps, lavaAdd);
            });

            CreateOrUpdateParticlePrefab(LavaEffectsRoot + "/AshFall.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureAshFall(ps, lavaSmoke);
            });
        }

        [MenuItem("Metal Pod/Effects/Generate Ice Effects")]
        public static void GenerateIceEffects()
        {
            EnsureFolder(IceEffectsRoot);

            Material snowMat = GetOrCreateMaterial(
                MaterialRoot + "/Ice_Snow.mat",
                new[] { "MetalPod/Environment/Ice/SnowParticle", "Particles/Standard Unlit", "Legacy Shaders/Particles/Alpha Blended" },
                new Color(0.9f, 0.95f, 1f, 0.8f),
                enableSoftParticles: true,
                additiveFallback: false);

            Material iceSparkle = GetOrCreateMaterial(
                MaterialRoot + "/Ice_Sparkle_Additive.mat",
                new[] { "Particles/Standard Unlit", "Legacy Shaders/Particles/Additive" },
                new Color(0.8f, 0.95f, 1f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            Material avalancheMat = GetOrCreateMaterial(
                MaterialRoot + "/Ice_Avalanche.mat",
                new[] { "Particles/Standard Unlit", "Legacy Shaders/Particles/Alpha Blended" },
                new Color(0.9f, 0.95f, 1f, 0.65f),
                enableSoftParticles: true,
                additiveFallback: false);

            CreateOrUpdateParticlePrefab(IceEffectsRoot + "/SnowFall.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureSnowFall(ps, snowMat);
            });

            CreateOrUpdateParticlePrefab(IceEffectsRoot + "/BlizzardHeavy.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureBlizzardHeavy(ps, snowMat);
            });

            CreateOrUpdateParticlePrefab(IceEffectsRoot + "/IceShatter.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureIceShatter(ps, iceSparkle);
            });

            CreateOrUpdateParticlePrefab(IceEffectsRoot + "/IceDust.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureIceDust(ps, iceSparkle);
            });

            CreateOrUpdateParticlePrefab(IceEffectsRoot + "/FrostBreath.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureFrostBreath(ps, snowMat);
            });

            CreateOrUpdateParticlePrefab(IceEffectsRoot + "/AvalancheCloud.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureAvalancheCloud(ps, avalancheMat);
            });
        }

        [MenuItem("Metal Pod/Effects/Generate Toxic Effects")]
        public static void GenerateToxicEffects()
        {
            EnsureFolder(ToxicEffectsRoot);

            Material toxicGasMat = GetOrCreateMaterial(
                MaterialRoot + "/Toxic_Gas.mat",
                new[] { "Particles/Standard Unlit", "Legacy Shaders/Particles/Alpha Blended" },
                new Color(0.25f, 1f, 0.1f, 0.35f),
                enableSoftParticles: true,
                additiveFallback: false);

            Material toxicAdd = GetOrCreateMaterial(
                MaterialRoot + "/Toxic_Additive.mat",
                new[]
                {
                    "MetalPod/Environment/Toxic/ElectricArc",
                    "Particles/Standard Unlit",
                    "Legacy Shaders/Particles/Additive"
                },
                new Color(0.55f, 1f, 0.35f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            Material smokeMat = GetOrCreateMaterial(
                MaterialRoot + "/ExplosionSmoke.mat",
                new[] { "Particles/Standard Unlit", "Legacy Shaders/Particles/Alpha Blended" },
                new Color(0.25f, 0.25f, 0.25f, 0.65f),
                enableSoftParticles: true,
                additiveFallback: false);

            CreateOrUpdateParticlePrefab(ToxicEffectsRoot + "/ToxicCloud.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureToxicCloud(ps, toxicGasMat);
            });

            CreateOrUpdateParticlePrefab(ToxicEffectsRoot + "/AcidBubble.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureAcidBubble(ps, toxicAdd);
            });

            CreateOrUpdateParticlePrefab(ToxicEffectsRoot + "/AcidSplash.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureAcidSplash(ps, toxicAdd);
            });

            CreateOrUpdateParticlePrefab(ToxicEffectsRoot + "/ElectricSpark.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureElectricSpark(ps, toxicAdd);
            });

            CreateOrUpdateParticlePrefab(ToxicEffectsRoot + "/SteamVent.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureSteamVent(ps, smokeMat);
            });

            CreateOrUpdateParticlePrefab(ToxicEffectsRoot + "/ExplosionSmoke.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureExplosionSmoke(ps, smokeMat);
            });

            CreateOrUpdateParticlePrefab(ToxicEffectsRoot + "/ExplosionFire.prefab", root =>
            {
                ParticleSystem fire = EnsureParticleSystem(root);
                ParticlePresets.ConfigureExplosionFire(fire, toxicAdd, 50, 80);

                GameObject smokeChild = EnsureChild(root.transform, "ExplosionSmoke_Sub");
                ParticleSystem smoke = EnsureParticleSystem(smokeChild);
                ParticlePresets.ConfigureExplosionSmoke(smoke, smokeMat);
                SetAsSubEmitter(fire, smoke, ParticleSystemSubEmitterType.Birth);
            });
        }

        [MenuItem("Metal Pod/Effects/Generate Common Effects")]
        public static void GenerateCommonEffects()
        {
            EnsureFolder(CommonEffectsRoot);

            Material glowMat = GetOrCreateMaterial(
                MaterialRoot + "/CollectibleGlow_Additive.mat",
                new[] { "Particles/Standard Unlit", "Legacy Shaders/Particles/Additive" },
                new Color(1f, 0.9f, 0.3f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            Material burstMat = GetOrCreateMaterial(
                MaterialRoot + "/CollectibleBurst_Additive.mat",
                new[] { "Particles/Standard Unlit", "Legacy Shaders/Particles/Additive" },
                new Color(1f, 0.85f, 0.2f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            Material checkpointMat = GetOrCreateMaterial(
                MaterialRoot + "/CheckpointActivate_Additive.mat",
                new[] { "Particles/Standard Unlit", "Legacy Shaders/Particles/Additive" },
                new Color(0.2f, 1f, 0.4f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            Material respawnMat = GetOrCreateMaterial(
                MaterialRoot + "/RespawnShimmer_Additive.mat",
                new[] { "MetalPod/Hovercraft/ThrusterGlow", "Particles/Standard Unlit", "Legacy Shaders/Particles/Additive" },
                new Color(0.2f, 0.9f, 1f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            CreateOrUpdateParticlePrefab(CommonEffectsRoot + "/CollectibleGlow.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureCollectibleGlow(ps, glowMat);
            });

            CreateOrUpdateParticlePrefab(CommonEffectsRoot + "/CollectibleBurst.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureCollectibleBurst(ps, burstMat);
            });

            CreateOrUpdateParticlePrefab(CommonEffectsRoot + "/CheckpointActivate.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureCheckpointActivate(ps, checkpointMat);
            });

            CreateOrUpdateParticlePrefab(CommonEffectsRoot + "/RespawnEffect.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureRespawnShimmer(ps, respawnMat);
            });
        }

        [MenuItem("Metal Pod/Effects/Generate UI Effects")]
        public static void GenerateUIEffects()
        {
            EnsureFolder(UIEffectsRoot);

            Material uiAdd = GetOrCreateMaterial(
                MaterialRoot + "/UI_Additive.mat",
                new[] { "Particles/Standard Unlit", "Legacy Shaders/Particles/Additive" },
                new Color(1f, 0.9f, 0.45f, 1f),
                enableSoftParticles: false,
                additiveFallback: true);

            CreateOrUpdateParticlePrefab(UIEffectsRoot + "/MedalBurst.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureMedalBurst(ps, uiAdd);
            });

            CreateOrUpdateParticlePrefab(UIEffectsRoot + "/CurrencyPickup.prefab", root =>
            {
                ParticleSystem ps = EnsureParticleSystem(root);
                ParticlePresets.ConfigureCurrencyPickup(ps, uiAdd);
            });
        }

        private static void ConfigureHovercraftParticlesOnObject(
            GameObject hovercraftRoot,
            Material thrusterMat,
            Material fireMat,
            Material smokeMat,
            Material sparkMat,
            GameObject explosionPrefab)
        {
            GameObject visualsRoot = EnsureNamedChild(hovercraftRoot.transform, "Visuals");

            ParticleSystem mainL = ConfigureChildParticle(visualsRoot.transform, "Thruster_Main_L", ps => ParticlePresets.ConfigureMainThruster(ps, thrusterMat));
            ParticleSystem mainR = ConfigureChildParticle(visualsRoot.transform, "Thruster_Main_R", ps => ParticlePresets.ConfigureMainThruster(ps, thrusterMat));
            ParticleSystem sideL = ConfigureChildParticle(visualsRoot.transform, "Thruster_Side_L", ps => ParticlePresets.ConfigureSideThruster(ps, fireMat));
            ParticleSystem sideR = ConfigureChildParticle(visualsRoot.transform, "Thruster_Side_R", ps => ParticlePresets.ConfigureSideThruster(ps, fireMat));
            ParticleSystem boost = ConfigureChildParticle(visualsRoot.transform, "Boost_Effect", ps => ParticlePresets.ConfigureBoostThruster(ps, fireMat));
            ParticleSystem sparks = ConfigureChildParticle(visualsRoot.transform, "Sparks", ps => ParticlePresets.ConfigureDamageSparks(ps, sparkMat));
            ParticleSystem smoke = ConfigureChildParticle(visualsRoot.transform, "Smoke", ps => ParticlePresets.ConfigureDamageSmoke(ps, smokeMat));
            ParticleSystem fire = ConfigureChildParticle(visualsRoot.transform, "Fire", ps => ParticlePresets.ConfigureDamageFire(ps, fireMat));

            if (mainL != null)
            {
                mainL.transform.localPosition = new Vector3(-0.6f, 0f, -1.5f);
                mainL.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            }

            if (mainR != null)
            {
                mainR.transform.localPosition = new Vector3(0.6f, 0f, -1.5f);
                mainR.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            }

            if (sideL != null)
            {
                sideL.transform.localPosition = new Vector3(-1.0f, 0f, 0.15f);
                sideL.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            }

            if (sideR != null)
            {
                sideR.transform.localPosition = new Vector3(1.0f, 0f, 0.15f);
                sideR.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            }

            if (boost != null)
            {
                boost.transform.localPosition = new Vector3(0f, 0f, -2f);
                boost.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            if (sparks != null)
            {
                sparks.transform.localPosition = Vector3.zero;
            }

            if (smoke != null)
            {
                smoke.transform.localPosition = new Vector3(0f, 0.65f, -0.25f);
                smoke.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            }

            if (fire != null)
            {
                fire.transform.localPosition = new Vector3(0f, 0.65f, 0.1f);
                fire.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            }

            HovercraftVisuals visuals = hovercraftRoot.GetComponent<HovercraftVisuals>();
            if (visuals == null)
            {
                visuals = hovercraftRoot.GetComponentInChildren<HovercraftVisuals>(true);
            }

            if (visuals != null)
            {
                SerializedObject so = new SerializedObject(visuals);
                AssignArrayProperty(so, "mainThrusters", new[] { mainL, mainR });
                AssignArrayProperty(so, "sideThrusters", new[] { sideL, sideR });
                AssignObjectProperty(so, "boostThruster", boost);
                AssignObjectProperty(so, "sparksEffect", sparks);
                AssignObjectProperty(so, "smokeEffect", smoke);
                AssignObjectProperty(so, "fireEffect", fire);
                AssignObjectProperty(so, "explosionPrefab", explosionPrefab);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(visuals);
            }

            EditorUtility.SetDirty(hovercraftRoot);
        }

        private static ParticleSystem ConfigureChildParticle(Transform parent, string childName, Action<ParticleSystem> configure)
        {
            GameObject child = EnsureNamedChild(parent, childName);
            ParticleSystem ps = EnsureParticleSystem(child);
            configure?.Invoke(ps);
            return ps;
        }

        private static GameObject CreateOrUpdateParticlePrefab(string prefabPath, Action<GameObject> configure)
        {
            EnsureFolder(Path.GetDirectoryName(prefabPath).Replace("\\", "/"));

            bool exists = File.Exists(prefabPath);
            GameObject root = exists
                ? PrefabUtility.LoadPrefabContents(prefabPath)
                : new GameObject(Path.GetFileNameWithoutExtension(prefabPath));

            try
            {
                configure?.Invoke(root);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                if (exists)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static Material GetOrCreateMaterial(
            string materialPath,
            string[] shaderCandidates,
            Color tint,
            bool enableSoftParticles,
            bool additiveFallback)
        {
            EnsureFolder(Path.GetDirectoryName(materialPath).Replace("\\", "/"));

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Shader shader = FindShader(shaderCandidates);
                if (shader == null)
                {
                    shader = Shader.Find(additiveFallback ? "Legacy Shaders/Particles/Additive" : "Legacy Shaders/Particles/Alpha Blended");
                }

                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", tint);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
            }

            if (material.HasProperty("_Intensity"))
            {
                material.SetFloat("_Intensity", additiveFallback ? 2f : 1f);
            }

            if (material.HasProperty("_SoftParticleFade"))
            {
                material.SetFloat("_SoftParticleFade", enableSoftParticles ? 1.25f : 0.01f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader FindShader(string[] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                Shader shader = Shader.Find(candidates[i]);
                if (shader != null)
                {
                    return shader;
                }
            }

            return null;
        }

        private static void SetAsSubEmitter(ParticleSystem main, ParticleSystem sub, ParticleSystemSubEmitterType type)
        {
            if (main == null || sub == null)
            {
                return;
            }

            ParticleSystem.SubEmittersModule subEmitters = main.subEmitters;
            subEmitters.enabled = true;

            for (int i = subEmitters.subEmittersCount - 1; i >= 0; i--)
            {
                ParticleSystem candidate = subEmitters.GetSubEmitterSystem(i);
                if (candidate == sub)
                {
                    subEmitters.RemoveSubEmitter(i);
                }
            }

            subEmitters.AddSubEmitter(sub, type, ParticleSystemSubEmitterProperties.InheritNothing);
        }

        private static void AssignObjectProperty(SerializedObject so, string propertyName, UnityEngine.Object value)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
            }
        }

        private static void AssignArrayProperty(SerializedObject so, string propertyName, ParticleSystem[] values)
        {
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop == null || !prop.isArray)
            {
                return;
            }

            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static ParticleSystem EnsureParticleSystem(GameObject go)
        {
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = go.AddComponent<ParticleSystem>();
            }

            if (go.GetComponent<ParticleSystemRenderer>() == null)
            {
                go.AddComponent<ParticleSystemRenderer>();
            }

            return ps;
        }

        private static GameObject EnsureNamedChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child.gameObject;
            }

            GameObject go = new GameObject(childName);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject EnsureChild(Transform parent, string childName)
        {
            return EnsureNamedChild(parent, childName);
        }

        private static GameObject CreateHovercraftPrefabShell()
        {
            GameObject root = new GameObject("Hovercraft");
            GameObject visuals = new GameObject("Visuals");
            visuals.transform.SetParent(root.transform, false);
            root.AddComponent<HovercraftVisuals>();
            return root;
        }

        private static void EnsureFolder(string assetFolderPath)
        {
            if (string.IsNullOrWhiteSpace(assetFolderPath))
            {
                return;
            }

            string normalized = assetFolderPath.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string[] parts = normalized.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new InvalidOperationException($"Path must be under Assets: {assetFolderPath}");
            }

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
