#if UNITY_EDITOR
using UnityEngine;

namespace MetalPod.Editor
{
    public static class ParticlePresets
    {
        public static void ConfigureMainThruster(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);

            ParticleSystem.MainModule main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startColor = GradientMinMax(
                "#00DDFF", "#0066FF",
                "#00B8FF", "#0049D9",
                1f, 0.9f,
                1f, 0.85f);
            main.gravityModifier = -0.1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 100;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 80f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.15f;

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = GradientMinMax(
                "#00DDFF", "#0066FF",
                "#00DDFF", "#005BCC",
                1f, 0f,
                0.95f, 0f);

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[] { Key(0f, 1f), Key(1f, 0.3f) });

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureSideThruster(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);

            ParticleSystem.MainModule main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.startColor = GradientMinMax(
                "#FF9900", "#FF2200",
                "#FF7F00", "#CC2200",
                1f, 0.9f,
                0.95f, 0.8f);
            main.gravityModifier = 0f;
            main.maxParticles = 60;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 40f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.1f;

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = GradientMinMax(
                "#FF8800", "#5A0000",
                "#FF6600", "#440000",
                1f, 0f,
                0.95f, 0f);

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[] { Key(0f, 1f), Key(1f, 0.2f) });

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureBoostThruster(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);

            ParticleSystem.MainModule main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startColor = GradientMinMax(
                "#FFFFFF", "#FF3D00",
                "#FFE9B0", "#CC1D00",
                1f, 0f,
                1f, 0f);
            main.maxParticles = 200;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 150f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 20f;
            shape.radius = 0.2f;

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = GradientMinMax(new[]
            {
                ColorKey("#FFFFFF", 0f),
                ColorKey("#FF8800", 0.4f),
                ColorKey("#551100", 1f)
            }, new[]
            {
                AlphaKey(1f, 0f),
                AlphaKey(0.8f, 0.5f),
                AlphaKey(0f, 1f)
            });

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[]
            {
                Key(0f, 1.2f),
                Key(1f, 0.1f)
            });

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 3f;
            noise.scrollSpeed = 0.5f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureDamageSparks(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);

            ParticleSystem.MainModule main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main.startColor = GradientMinMax("#FFAA00", "#FFDD00", "#FF8A00", "#FFC000", 1f, 1f, 1f, 1f);
            main.gravityModifier = 2f;
            main.maxParticles = 50;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20, 40) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = GradientMinMax("#FFAA00", "#220A00", "#FFCC33", "#170700", 1f, 0f, 1f, 0f);

            ParticleSystemRenderer renderer = ApplyRenderer(ps, material, ParticleSystemRenderMode.Stretch, ParticleSystemSortMode.Distance, false);
            renderer.velocityScale = 0.1f;
            renderer.lengthScale = 2f;
        }

        public static void ConfigureDamageSmoke(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);

            ParticleSystem.MainModule main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startColor = ColorHex("#333333", 0.6f);
            main.gravityModifier = -0.3f;
            main.maxParticles = 30;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 10f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = GradientMinMax("#333333", "#0F0F0F", "#444444", "#111111", 0.6f, 0f, 0.55f, 0f);

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[] { Key(0f, 0.5f), Key(1f, 2f) });

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.OldestInFront, true);
        }

        public static void ConfigureDamageFire(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);

            ParticleSystem.MainModule main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startColor = GradientMinMax("#FFC040", "#A21900", "#FF9B2D", "#8F1300", 1f, 0.8f, 1f, 0.7f);
            main.gravityModifier = -1f;
            main.maxParticles = 40;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 30f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.15f;

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = GradientMinMax(new[]
            {
                ColorKey("#FFF48A", 0f),
                ColorKey("#FF7700", 0.45f),
                ColorKey("#4A1200", 1f)
            }, new[]
            {
                AlphaKey(1f, 0f),
                AlphaKey(0.8f, 0.5f),
                AlphaKey(0f, 1f)
            });

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[]
            {
                Key(0f, 0.8f),
                Key(0.6f, 1.5f),
                Key(1f, 0f)
            });

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1f;
            noise.frequency = 2f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureExplosionFire(ParticleSystem ps, Material material, int burstMin = 80, int burstMax = 120)
        {
            ResetForReuse(ps);

            ParticleSystem.MainModule main = ps.main;
            main.duration = 1.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 20f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            main.startColor = GradientMinMax("#FFFFFF", "#AA2200", "#FFC070", "#8A1400", 1f, 0.7f, 1f, 0.7f);
            main.gravityModifier = 0.5f;
            main.maxParticles = Mathf.Max(150, burstMax + 10);

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstMin, (short)burstMax) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = GradientMinMax(new[]
            {
                ColorKey("#FFFFFF", 0f),
                ColorKey("#FF7A00", 0.35f),
                ColorKey("#353535", 0.75f),
                ColorKey("#202020", 1f)
            }, new[]
            {
                AlphaKey(1f, 0f),
                AlphaKey(1f, 0.25f),
                AlphaKey(0.55f, 0.7f),
                AlphaKey(0f, 1f)
            });

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[]
            {
                Key(0f, 1f),
                Key(0.55f, 2.5f),
                Key(1f, 0f)
            });

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1f;
            noise.frequency = 2.4f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureRespawnShimmer(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);

            ParticleSystem.MainModule main = ps.main;
            main.duration = 2f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            main.startColor = ColorHex("#00CCFF", 1f);
            main.maxParticles = 60;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(40f);
            emission.rateOverDistance = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(1f, 0) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 2f;

            ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
            color.enabled = true;
            color.color = GradientMinMax(new[]
            {
                ColorKey("#00CCFF", 0f),
                ColorKey("#FFFFFF", 0.5f),
                ColorKey("#D9FFFF", 1f)
            }, new[]
            {
                AlphaKey(1f, 0f),
                AlphaKey(0.5f, 0.5f),
                AlphaKey(0f, 1f)
            });

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[]
            {
                Key(0f, 0.5f),
                Key(0.65f, 1.5f),
                Key(1f, 0f)
            });

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureLavaBubbles(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startColor = GradientMinMax("#FF7A00", "#611600", "#FF9B33", "#4A1000", 0.9f, 0f, 0.9f, 0f);
            main.maxParticles = 50;

            ps.emission.rateOverTime = 8f;
            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(8f, 0.2f, 8f);

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[]
            {
                Key(0f, 0.4f),
                Key(0.75f, 1.3f),
                Key(1f, 0.1f)
            });

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureLavaSplash(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 0.55f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.25f);
            main.startColor = GradientMinMax("#FF7A00", "#A01500", "#FF9A28", "#7A1100", 1f, 0f, 1f, 0f);
            main.gravityModifier = 3f;
            main.maxParticles = 30;

            ps.emission.rateOverTime = 0f;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15, 25) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.4f;

            ParticleSystemRenderer renderer = ApplyRenderer(ps, material, ParticleSystemRenderMode.Stretch, ParticleSystemSortMode.Distance, false);
            renderer.velocityScale = 0.25f;
            renderer.lengthScale = 1.6f;
        }

        public static void ConfigureEruptionDebris(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 2.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 25f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startColor = GradientMinMax("#3F2A20", "#FF6A00", "#5A3A2B", "#CC4A00", 1f, 0.4f, 1f, 0.45f);
            main.gravityModifier = 1.5f;
            main.maxParticles = 60;

            ps.emission.rateOverTime = 0f;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30, 50) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 40f;
            shape.radius = 0.5f;

            ParticleSystem.TrailModule trail = ps.trails;
            trail.enabled = true;
            trail.mode = ParticleSystemTrailMode.PerParticle;
            trail.lifetime = 0.35f;
            trail.dieWithParticles = true;
            trail.widthOverTrail = new ParticleSystem.MinMaxCurve(0.12f, Curve(new[] { Key(0f, 1f), Key(1f, 0f) }));

            ParticleSystem.CollisionModule collision = ps.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.bounce = 0.25f;
            collision.dampen = 0.4f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureGeyserSpray(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(15f, 30f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startColor = GradientMinMax("#FF7A00", "#FFFFFF", "#FFA53A", "#FFD7A6", 0.85f, 0f, 0.85f, 0f);
            main.maxParticles = 300;

            ps.emission.rateOverTime = 200f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 5f;
            shape.radius = 0.25f;

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1f;
            noise.frequency = 1.5f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureHeatShimmer(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
            main.startColor = ColorHex("#FFAA55", 0.2f);
            main.gravityModifier = -0.1f;
            main.maxParticles = 60;

            ps.emission.rateOverTime = 12f;
            ps.shape.shapeType = ParticleSystemShapeType.Box;
            ps.shape.scale = new Vector3(6f, 0.2f, 6f);

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, true);
        }

        public static void ConfigureEmberFloat(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 6f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startColor = ColorHex("#FF8A33", 0.9f);
            main.gravityModifier = -0.05f;
            main.maxParticles = 70;

            ps.emission.rateOverTime = 6f;
            ps.shape.shapeType = ParticleSystemShapeType.Box;
            ps.shape.scale = new Vector3(20f, 5f, 20f);

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.2f;
            noise.frequency = 0.5f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureAshFall(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 8f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
            main.startColor = ColorHex("#808080", 0.7f);
            main.gravityModifier = 0.1f;
            main.maxParticles = 150;

            ps.emission.rateOverTime = 16f;
            ps.shape.shapeType = ParticleSystemShapeType.Box;
            ps.shape.scale = new Vector3(30f, 1f, 30f);
            ps.shape.position = new Vector3(0f, 20f, 0f);

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.3f;
            noise.frequency = 0.35f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, true);
        }

        public static void ConfigureSnowFall(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 10f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main.startColor = ColorHex("#FFFFFF", 0.7f);
            main.gravityModifier = 0.2f;
            main.maxParticles = 220;

            ps.emission.rateOverTime = 30f;
            ps.shape.shapeType = ParticleSystemShapeType.Box;
            ps.shape.scale = new Vector3(30f, 1f, 30f);
            ps.shape.position = new Vector3(0f, 25f, 0f);

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.35f;
            noise.frequency = 0.45f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, true);
        }

        public static void ConfigureBlizzardHeavy(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 4f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = ColorHex("#FFFFFF", 0.5f);
            main.gravityModifier = 0.3f;
            main.maxParticles = 450;

            ps.emission.rateOverTime = 280f;
            ps.shape.shapeType = ParticleSystemShapeType.Box;
            ps.shape.scale = new Vector3(28f, 8f, 28f);

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1f;
            noise.frequency = 2f;

            ParticleSystemRenderer renderer = ApplyRenderer(ps, material, ParticleSystemRenderMode.Stretch, ParticleSystemSortMode.Distance, true);
            renderer.velocityScale = 0.2f;
            renderer.lengthScale = 1.3f;
        }

        public static void ConfigureIceShatter(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.3f);
            main.startColor = GradientMinMax("#D5F5FF", "#FFFFFF", "#B9EAFF", "#EAFBFF", 1f, 0.6f, 1f, 0.65f);
            main.gravityModifier = 3f;
            main.maxParticles = 80;

            ps.emission.rateOverTime = 0f;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30, 60) });

            ps.shape.shapeType = ParticleSystemShapeType.Sphere;
            ps.shape.radius = 0.35f;

            ParticleSystem.RotationOverLifetimeModule rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-4f, 4f);

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureIceDust(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 4f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.03f);
            main.startColor = GradientMinMax("#FFFFFF", "#9BE3FF", "#F8FFFF", "#84D0FF", 0.8f, 0f, 0.75f, 0f);
            main.maxParticles = 80;

            ps.emission.rateOverTime = 8f;
            ps.shape.shapeType = ParticleSystemShapeType.Sphere;
            ps.shape.radius = 10f;

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.05f;
            noise.frequency = 0.4f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureFrostBreath(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 2f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startColor = ColorHex("#D5F4FF", 0.55f);
            main.gravityModifier = -0.15f;
            main.maxParticles = 90;

            ps.emission.rateOverTime = 22f;
            ps.shape.shapeType = ParticleSystemShapeType.Cone;
            ps.shape.angle = 18f;
            ps.shape.radius = 0.15f;

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[] { Key(0f, 0.45f), Key(1f, 1.5f) });

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, true);
        }

        public static void ConfigureAvalancheCloud(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 3f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 5f);
            main.startColor = GradientMinMax("#FFFFFF", "#808080", "#F2FAFF", "#7A7A7A", 0.9f, 0f, 0.85f, 0f);
            main.gravityModifier = 0.5f;
            main.maxParticles = 500;

            ps.emission.rateOverTime = 350f;
            ps.shape.shapeType = ParticleSystemShapeType.Box;
            ps.shape.scale = new Vector3(18f, 8f, 6f);

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1.4f;
            noise.frequency = 1.8f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, true);
        }

        public static void ConfigureToxicCloud(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 4f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startColor = GradientMinMax("#44FF00", "#0E2F00", "#66FF33", "#153D08", 0.3f, 0f, 0.3f, 0f);
            main.maxParticles = 180;

            ps.emission.rateOverTime = 22f;
            ps.shape.shapeType = ParticleSystemShapeType.Box;
            ps.shape.scale = new Vector3(10f, 2f, 10f);

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.5f;
            noise.frequency = 0.6f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, true);
        }

        public static void ConfigureAcidBubble(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 2f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startColor = ColorHex("#55FF00", 0.9f);
            main.maxParticles = 30;

            ps.emission.rateOverTime = 6f;
            ps.shape.shapeType = ParticleSystemShapeType.Box;
            ps.shape.scale = new Vector3(6f, 0.2f, 6f);

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[]
            {
                Key(0f, 0.4f),
                Key(0.75f, 1.1f),
                Key(1f, 0.05f)
            });

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureAcidSplash(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = ColorHex("#66FF00", 0.9f);
            main.gravityModifier = 4f;
            main.maxParticles = 30;

            ps.emission.rateOverTime = 0f;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10, 20) });
            ps.shape.shapeType = ParticleSystemShapeType.Hemisphere;
            ps.shape.radius = 0.25f;

            ParticleSystemRenderer renderer = ApplyRenderer(ps, material, ParticleSystemRenderMode.Stretch, ParticleSystemSortMode.Distance, false);
            renderer.velocityScale = 0.25f;
            renderer.lengthScale = 1.45f;
        }

        public static void ConfigureElectricSpark(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 30f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
            main.startColor = GradientMinMax("#FFFFFF", "#B7E7FF", "#FFFFFF", "#A8D8FF", 1f, 0.1f, 1f, 0.05f);
            main.maxParticles = 80;

            ps.emission.rateOverTime = 42f;
            ps.shape.shapeType = ParticleSystemShapeType.Box;
            ps.shape.scale = new Vector3(2f, 0.1f, 0.1f);

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 1.5f;
            noise.frequency = 4f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureSteamVent(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 3.5f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 1f);
            main.startColor = ColorHex("#FFFFFF", 0.4f);
            main.maxParticles = 120;

            ps.emission.rateOverTime = 50f;
            ps.shape.shapeType = ParticleSystemShapeType.Cone;
            ps.shape.angle = 15f;
            ps.shape.radius = 0.35f;

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.4f;
            noise.frequency = 0.7f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, true);
        }

        public static void ConfigureExplosionSmoke(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 4f;
            main.loop = false;
            main.startDelay = 0.3f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 2f);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 4f);
            main.startColor = GradientMinMax("#3A3A3A", "#111111", "#444444", "#1A1A1A", 0.7f, 0f, 0.7f, 0f);
            main.gravityModifier = -0.2f;
            main.maxParticles = 120;

            ps.emission.rateOverTime = 20f;

            ps.shape.shapeType = ParticleSystemShapeType.Sphere;
            ps.shape.radius = 0.5f;

            ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = CurveMinMax(new[] { Key(0f, 0.45f), Key(1f, 1.45f) });

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.OldestInFront, true);
        }

        public static void ConfigureCollectibleGlow(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 2f;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            main.startColor = ColorHex("#FFD700", 0.9f);
            main.maxParticles = 35;

            ps.emission.rateOverTime = 5f;
            ps.shape.shapeType = ParticleSystemShapeType.Sphere;
            ps.shape.radius = 0.3f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureCollectibleBurst(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startColor = ColorHex("#FFD700", 1f);
            main.gravityModifier = -0.5f;
            main.maxParticles = 50;

            ps.emission.rateOverTime = 0f;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20, 30) });
            ps.shape.shapeType = ParticleSystemShapeType.Sphere;
            ps.shape.radius = 0.25f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureCheckpointActivate(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startColor = GradientMinMax("#33FF66", "#FFFFFF", "#2EEA5B", "#FFFFFF", 1f, 0f, 1f, 0f);
            main.maxParticles = 55;

            ps.emission.rateOverTime = 0f;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40, 40) });

            ps.shape.shapeType = ParticleSystemShapeType.ConeVolume;
            ps.shape.radius = 0.8f;
            ps.shape.angle = 0f;
            ps.shape.scale = new Vector3(1f, 0.1f, 1f);

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureMedalBurst(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 1.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startColor = GradientMinMax("#FFD700", "#C0C0C0", "#CD7F32", "#FFD700", 1f, 1f, 1f, 1f);
            main.gravityModifier = 2f;
            main.maxParticles = 110;

            ps.emission.rateOverTime = 0f;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60, 100) });

            ps.shape.shapeType = ParticleSystemShapeType.Cone;
            ps.shape.angle = 60f;
            ps.shape.radius = 0.2f;

            ParticleSystem.RotationOverLifetimeModule rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-8f, 8f);

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ConfigureCurrencyPickup(ParticleSystem ps, Material material)
        {
            ResetForReuse(ps);
            ParticleSystem.MainModule main = ps.main;
            main.duration = 0.8f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
            main.startColor = ColorHex("#FFD700", 1f);
            main.gravityModifier = -0.1f;
            main.maxParticles = 40;

            ps.emission.rateOverTime = 0f;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18, 30) });

            ps.shape.shapeType = ParticleSystemShapeType.Sphere;
            ps.shape.radius = 0.2f;

            ParticleSystem.NoiseModule noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.15f;
            noise.frequency = 0.8f;

            ApplyRenderer(ps, material, ParticleSystemRenderMode.Billboard, ParticleSystemSortMode.Distance, false);
        }

        public static void ResetForReuse(ParticleSystem ps)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);
            ps.useAutoRandomSeed = true;

            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.scalingMode = ParticleSystemScalingMode.Shape;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[0]);

            ps.shape.enabled = true;

            DisableAndResetNoise(ps.noise);
            DisableAndResetTrails(ps.trails);
            DisableAndResetRotation(ps.rotationOverLifetime);
            DisableAndResetColor(ps.colorOverLifetime);
            DisableAndResetSize(ps.sizeOverLifetime);
            DisableAndResetCollision(ps.collision);

            ParticleSystem.SubEmittersModule subEmitters = ps.subEmitters;
            for (int i = subEmitters.subEmittersCount - 1; i >= 0; i--)
            {
                subEmitters.RemoveSubEmitter(i);
            }
        }

        public static ParticleSystemRenderer ApplyRenderer(
            ParticleSystem ps,
            Material material,
            ParticleSystemRenderMode renderMode,
            ParticleSystemSortMode sortMode,
            bool enableSoftParticles)
        {
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = renderMode;
            renderer.sortMode = sortMode;
            renderer.material = material;

            if (material != null && material.HasProperty("_SoftParticleFade"))
            {
                material.SetFloat("_SoftParticleFade", enableSoftParticles ? 1.5f : 0.01f);
            }

            return renderer;
        }

        public static ParticleSystem.MinMaxGradient GradientMinMax(
            string colorA,
            string colorB,
            string colorC,
            string colorD,
            float alphaA,
            float alphaB,
            float alphaC,
            float alphaD)
        {
            Gradient gradientA = new Gradient();
            gradientA.SetKeys(
                new[] { ColorKey(colorA, 0f), ColorKey(colorB, 1f) },
                new[] { AlphaKey(alphaA, 0f), AlphaKey(alphaB, 1f) });

            Gradient gradientB = new Gradient();
            gradientB.SetKeys(
                new[] { ColorKey(colorC, 0f), ColorKey(colorD, 1f) },
                new[] { AlphaKey(alphaC, 0f), AlphaKey(alphaD, 1f) });

            return new ParticleSystem.MinMaxGradient(gradientA, gradientB);
        }

        public static ParticleSystem.MinMaxGradient GradientMinMax(GradientColorKey[] colors, GradientAlphaKey[] alphas)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(colors, alphas);
            return new ParticleSystem.MinMaxGradient(gradient);
        }

        public static ParticleSystem.MinMaxCurve CurveMinMax(Keyframe[] keys)
        {
            return new ParticleSystem.MinMaxCurve(1f, Curve(keys));
        }

        public static AnimationCurve Curve(Keyframe[] keys)
        {
            AnimationCurve curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++)
            {
                curve.SmoothTangents(i, 0f);
            }

            return curve;
        }

        public static Color ColorHex(string hex, float alpha = 1f)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                color = Color.white;
            }

            color.a = alpha;
            return color;
        }

        public static GradientColorKey ColorKey(string hex, float time)
        {
            return new GradientColorKey(ColorHex(hex, 1f), time);
        }

        public static GradientAlphaKey AlphaKey(float alpha, float time)
        {
            return new GradientAlphaKey(alpha, time);
        }

        public static Keyframe Key(float time, float value)
        {
            return new Keyframe(time, value);
        }

        private static void DisableAndResetNoise(ParticleSystem.NoiseModule noise)
        {
            noise.enabled = false;
            noise.strength = 0f;
            noise.frequency = 0.5f;
            noise.scrollSpeed = 0f;
        }

        private static void DisableAndResetTrails(ParticleSystem.TrailModule trails)
        {
            trails.enabled = false;
            trails.mode = ParticleSystemTrailMode.PerParticle;
            trails.dieWithParticles = true;
            trails.lifetime = 0.2f;
        }

        private static void DisableAndResetRotation(ParticleSystem.RotationOverLifetimeModule rotation)
        {
            rotation.enabled = false;
        }

        private static void DisableAndResetColor(ParticleSystem.ColorOverLifetimeModule color)
        {
            color.enabled = false;
        }

        private static void DisableAndResetSize(ParticleSystem.SizeOverLifetimeModule size)
        {
            size.enabled = false;
        }

        private static void DisableAndResetCollision(ParticleSystem.CollisionModule collision)
        {
            collision.enabled = false;
        }
    }
}
#endif
