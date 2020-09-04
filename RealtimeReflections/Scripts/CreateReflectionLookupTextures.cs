//RealtimeReflections for Daggerfall-Unity
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: Michael Rauter (a.k.a. Nystul)
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)

using System;
using UnityEngine;
using UnityEngine.Rendering;

using RealtimeReflections;

using DaggerfallWorkshop.Game;

namespace RealtimeReflections
{
    public class CreateReflectionLookupTextures : MonoBehaviour
    {
        [SerializeField]
        private Shader m_ShaderCreateReflectionTextureCoordinates;
        public Shader shaderCreateReflectionTextureCoordinates
        {
            get
            {
                if (m_ShaderCreateReflectionTextureCoordinates == null)
                    m_ShaderCreateReflectionTextureCoordinates = componentUpdateReflectionTextures.ShaderCreateLookupReflectionTextureCoordinates;

                return m_ShaderCreateReflectionTextureCoordinates;
            }
        }

        [SerializeField]
        private Material m_MaterialCreateReflectionTextureCoordinates;
        public Material materialCreateReflectionTextureCoordinates
        {
            get
            {
                if (m_MaterialCreateReflectionTextureCoordinates == null)
                    m_MaterialCreateReflectionTextureCoordinates = new Material(shaderCreateReflectionTextureCoordinates);

                return m_MaterialCreateReflectionTextureCoordinates;
            }
        }

        [SerializeField]
        private Shader m_ShaderCreateReflectionTextureIndex;
        public Shader shaderCreateReflectionTextureIndex
        {
            get
            {
                if (m_ShaderCreateReflectionTextureIndex == null)
                    m_ShaderCreateReflectionTextureIndex = componentUpdateReflectionTextures.ShaderCreateLookupReflectionTextureIndex;

                return m_ShaderCreateReflectionTextureIndex;
            }
        }

        [SerializeField]
        private Material m_MaterialCreateReflectionTextureIndex;
        public Material materialCreateReflectionTextureIndex
        {
            get
            {
                if (m_MaterialCreateReflectionTextureIndex == null)
                    m_MaterialCreateReflectionTextureIndex = new Material(shaderCreateReflectionTextureIndex);

                return m_MaterialCreateReflectionTextureIndex;
            }
        }

        private Camera m_Camera;
        public Camera camera_
        {
            get
            {
                return m_Camera;
            }
        }

        public RenderTexture m_RenderTextureReflectionTextureCoordinates;
        public RenderTexture renderTextureReflectionTextureCoordinates
        {
            get
            {
                if (m_RenderTextureReflectionTextureCoordinates == null)
                    m_RenderTextureReflectionTextureCoordinates = new RenderTexture(camera_.pixelWidth, camera_.pixelHeight, 16, RenderTextureFormat.RGFloat); // 2-channel 16-bit floating-point per channel texture

                return m_RenderTextureReflectionTextureCoordinates;
            }
        }

        public RenderTexture m_RenderTextureReflectionTextureIndex;
        public RenderTexture renderTextureReflectionTextureIndex
        {
            get
            {
                if (m_RenderTextureReflectionTextureIndex == null)
                    m_RenderTextureReflectionTextureIndex = new RenderTexture(camera_.pixelWidth, camera_.pixelHeight, 16, RenderTextureFormat.ARGB32); // 4-channel 8-bit fixed point texture (1st channel: index of reflection texture to sample from, 2nd channel: metallic amount, 3rd channel: smoothness amount)

                return m_RenderTextureReflectionTextureIndex;
            }
        }

        private UpdateReflectionTextures componentUpdateReflectionTextures = null;

        private void Awake()
        {
            componentUpdateReflectionTextures = GameObject.Find("RealtimeReflections").GetComponent<UpdateReflectionTextures>();

            if (m_Camera == null)
            {
                m_Camera = this.gameObject.AddComponent<Camera>();
                m_Camera.CopyFrom(Camera.main);
                m_Camera.RemoveAllCommandBuffers();
                m_Camera.renderingPath = RenderingPath.Forward; // important to make fragment shader work with function Camera.RenderWithShader() - it won't work in deferred, this is no problem since we only want to render a custom index rendertexture anyway
                m_Camera.depthTextureMode = DepthTextureMode.None;
                m_Camera.clearFlags = CameraClearFlags.SolidColor;
                m_Camera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
                m_Camera.enabled = false; // important to disable camera so we can invoke Camera.RenderWithShader() manually later
            }
            //Shader.WarmupAllShaders();
        }

        void Start()
        {            
        }

        private void OnEnable()
        {
        }

        void OnDisable()
        {
            if (m_MaterialCreateReflectionTextureCoordinates)
                DestroyImmediate(m_MaterialCreateReflectionTextureCoordinates);

            m_MaterialCreateReflectionTextureCoordinates = null;

            if (m_MaterialCreateReflectionTextureIndex)
                DestroyImmediate(m_MaterialCreateReflectionTextureIndex);

            m_MaterialCreateReflectionTextureIndex = null;
        }

        public void createReflectionTextureCoordinatesAndIndexTextures()
        {
            m_Camera.transform.position = Camera.main.transform.position;
            m_Camera.transform.rotation = Camera.main.transform.rotation;
            float tmp = componentUpdateReflectionTextures.ReflectionPlaneGroundLevelY;
            Shader.SetGlobalFloat("_GroundLevelHeight", componentUpdateReflectionTextures.ReflectionPlaneGroundLevelY); // + GameManager.Instance.MainCamera.transform.localPosition.y);
            Shader.SetGlobalFloat("_LowerLevelHeight", componentUpdateReflectionTextures.ReflectionPlaneLowerLevelY); // + GameManager.Instance.MainCamera.transform.localPosition.y);
            //Shader.SetGlobalFloat("_CameraHeightFromGround", GameManager.Instance.MainCamera.transform.localPosition.y);

            m_Camera.targetTexture = renderTextureReflectionTextureCoordinates;
            m_Camera.RenderWithShader(shaderCreateReflectionTextureCoordinates, null); // apply custom fragment shader and write into renderTextureReflectionTextureCoordinates
            m_Camera.targetTexture = renderTextureReflectionTextureIndex;
            m_Camera.RenderWithShader(shaderCreateReflectionTextureIndex, null); // apply custom fragment shader and write into renderTextureReflectionTextureIndex            
        }
    }
}