using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MetalPod.Rendering
{
    public class FrostOverlayFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public Material material;
            public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public bool runInSceneView = false;
        }

        private class FrostOverlayPass : ScriptableRenderPass
        {
            private readonly string _profilerTag;
            private Material _material;
            private RTHandle _temporaryColorTexture;

            public FrostOverlayPass(string profilerTag, RenderPassEvent passEvent)
            {
                _profilerTag = profilerTag;
                renderPassEvent = passEvent;
            }

            public void Setup(Material material)
            {
                _material = material;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                if (_material == null)
                {
                    return;
                }

                ConfigureInput(ScriptableRenderPassInput.Color);

                RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
                descriptor.depthBufferBits = 0;
                RenderingUtils.ReAllocateIfNeeded(
                    ref _temporaryColorTexture,
                    descriptor,
                    FilterMode.Bilinear,
                    TextureWrapMode.Clamp,
                    name: "_MetalPodFrostOverlayTemp");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (_material == null || _temporaryColorTexture == null)
                {
                    return;
                }

                CommandBuffer cmd = CommandBufferPool.Get(_profilerTag);
                RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

                Blitter.BlitCameraTexture(cmd, source, _temporaryColorTexture, _material, 0);
                Blitter.BlitCameraTexture(cmd, _temporaryColorTexture, source);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
                _temporaryColorTexture?.Release();
            }
        }

        public Settings settings = new Settings();

        private FrostOverlayPass _pass;

        public override void Create()
        {
            _pass = new FrostOverlayPass("MetalPod Frost Overlay", settings.renderPassEvent);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings.material == null)
            {
                return;
            }

            if (!settings.runInSceneView && renderingData.cameraData.isSceneViewCamera)
            {
                return;
            }

            _pass.renderPassEvent = settings.renderPassEvent;
            _pass.Setup(settings.material);
            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _pass?.Dispose();
        }
    }
}
