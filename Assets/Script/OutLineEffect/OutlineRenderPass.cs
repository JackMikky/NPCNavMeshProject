using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class OutlineRenderPass : ScriptableRenderPass
{
    private const string ProfilerTag = "Render Outline";

    private Material m_OutlineMaterial;
    private Color m_OutlineColor;
    private float m_OutlineWidth;
    private int m_DownSampleScale;
    private int m_BlurIterations;
    private float m_BlurSpread;

    public Renderer[] OutlineObjects;

    private class PassData
    {
        public Material outlineMaterial;
        public Color outlineColor;
        public float outlineWidth;
        public int blurIterations;
        public float blurSpread;
        public List<Renderer> outlineObjects;
        public TextureHandle outlineColorTex;
        public TextureHandle blurTex;
        public TextureHandle blurTempTex;
        public TextureHandle cameraColorTex;
        public TextureHandle finalTex;
    }

    public OutlineRenderPass(Material outlineMaterial)
    {
        m_OutlineMaterial = outlineMaterial;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public void Setup(Color outlineColor, float outlineWidth, int downSampleScale, int blurIterations, float blurSpread)
    {
        m_OutlineColor = outlineColor;
        m_OutlineWidth = outlineWidth;
        m_DownSampleScale = downSampleScale;
        m_BlurIterations = blurIterations;
        m_BlurSpread = blurSpread;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (OutlineObjects == null || OutlineObjects.Length == 0 || m_OutlineMaterial == null)
        {
            return;
        }

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        var desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;

        var outlineColorDesc = new TextureDesc(desc.width, desc.height)
        {
            name = "_OutlineColorRT",
            colorFormat = desc.graphicsFormat,
            clearBuffer = true,
            clearColor = Color.clear
        };
        TextureHandle outlineColorTex = renderGraph.CreateTexture(outlineColorDesc);

        int blurWidth = Mathf.Max(1, desc.width >> m_DownSampleScale);
        int blurHeight = Mathf.Max(1, desc.height >> m_DownSampleScale);

        var blurDesc = new TextureDesc(blurWidth, blurHeight)
        {
            name = "_BlurRT",
            colorFormat = desc.graphicsFormat,
            filterMode = FilterMode.Bilinear
        };
        TextureHandle blurTex = renderGraph.CreateTexture(blurDesc);

        blurDesc.name = "_BlurTempRT";
        TextureHandle blurTempTex = renderGraph.CreateTexture(blurDesc);

        var finalDesc = new TextureDesc(desc.width, desc.height)
        {
            name = "_OutlineFinalRT",
            colorFormat = desc.graphicsFormat
        };
        TextureHandle finalTex = renderGraph.CreateTexture(finalDesc);

        using (var builder = renderGraph.AddUnsafePass<PassData>(ProfilerTag, out var passData))
        {
            passData.outlineMaterial = m_OutlineMaterial;
            passData.outlineColor = m_OutlineColor;
            passData.outlineWidth = m_OutlineWidth;
            passData.blurIterations = m_BlurIterations;
            passData.blurSpread = m_BlurSpread;
            passData.outlineObjects = new List<Renderer>(OutlineObjects);
            passData.outlineColorTex = outlineColorTex;
            passData.blurTex = blurTex;
            passData.blurTempTex = blurTempTex;
            passData.cameraColorTex = resourceData.cameraColor;
            passData.finalTex = finalTex;

            builder.UseTexture(outlineColorTex, AccessFlags.Write);
            builder.UseTexture(blurTex, AccessFlags.ReadWrite);
            builder.UseTexture(blurTempTex, AccessFlags.ReadWrite);
            builder.UseTexture(finalTex, AccessFlags.Write);
            builder.UseTexture(resourceData.cameraColor, AccessFlags.ReadWrite);

            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, UnsafeGraphContext ctx) =>
            {
                CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);

                data.outlineMaterial.SetColor("_OutlineColor", data.outlineColor);
                data.outlineMaterial.SetFloat("_OutlineWidth", data.outlineWidth);

                cmd.SetRenderTarget(data.outlineColorTex);
                cmd.ClearRenderTarget(true, true, Color.clear);
                for (int i = 0; i < data.outlineObjects.Count; ++i)
                {
                    if (data.outlineObjects[i] != null)
                    {
                        cmd.DrawRenderer(data.outlineObjects[i], data.outlineMaterial, 0, 0);
                    }
                }

                cmd.Blit(data.outlineColorTex, data.blurTex);

                for (int i = 0; i < data.blurIterations; ++i)
                {
                    data.outlineMaterial.SetFloat("_BlurSize", 1.0f + i * data.blurSpread);
                    cmd.Blit(data.blurTex, data.blurTempTex, data.outlineMaterial, 1);
                    cmd.Blit(data.blurTempTex, data.blurTex, data.outlineMaterial, 2);
                }

                data.outlineMaterial.SetTexture("_OutlineColorTex", data.outlineColorTex);
                data.outlineMaterial.SetTexture("_BlurTex", data.blurTex);

                cmd.Blit(data.cameraColorTex, data.finalTex, data.outlineMaterial, 3);
                cmd.Blit(data.finalTex, data.cameraColorTex);
            });
        }
    }
}