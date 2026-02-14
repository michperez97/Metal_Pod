using System;

namespace MetalPod.Transitions
{
    // Static collection of loading screen tips and flavor text.
    // Heavy metal themed to match the game aesthetic.
    public static class LoadingTips
    {
        private static readonly string[] Tips = new[]
        {
            "Tilt your device to steer. Lean into it!",
            "Tap the right side of the screen to activate boost.",
            "Tap the left side to brake - useful for tight corners.",
            "Upgrade Speed to increase your top velocity.",
            "Armor upgrades help you survive more hazard hits.",
            "Handling upgrades make steering more responsive.",
            "Boost upgrades give you longer, more powerful bursts.",
            "Gold medals require near-perfect runs. Practice makes perfect.",
            "Checkpoints save your position. Hit every one!",
            "Collect bolts during races to fund your upgrades.",
            "Replaying courses still earns bolts (at 50% rate).",
            "Shield absorbs damage before your health takes hits.",
            "Below 50% health, your speed is reduced.",
            "Below 25% health, handling also suffers.",
            "Lava geysers erupt on a timer. Learn the pattern.",
            "Ice patches reduce your traction. Keep your speed steady.",
            "Toxic gas clouds deal damage over time. Don't linger.",
            "Exploding barrels have a warning flash before detonation.",
            "Electric fences cycle on and off. Time your pass.",
            "The Workshop protagonist celebrates when you buy upgrades!",
            "Each environment has unique hazards. Adapt your strategy.",
            "Metal never dies. Neither should your hovercraft.",
            "No ads. No microtransactions. Just pure racing.",
        };

        private static readonly Random Random = new Random();

        public static string GetRandomTip()
        {
            return Tips[Random.Next(Tips.Length)];
        }
    }
}
