//RealtimeReflections for Daggerfall-Unity
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: Michael Rauter (a.k.a. Nystul)
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)

using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Utility;

namespace RealtimeReflections
{
    public class UpdateReflectionTextures : MonoBehaviour
    {
        private GameObject reflectionPlaneGround = null; // = floor
        private GameObject reflectionPlaneLowerLevel = null; // = lower level

        private MirrorReflection mirrorRefl = null; 
        private MirrorReflection mirrorReflSeaLevel = null;

        private bool useDeferredReflections = false;
        private DeferredPlanarReflections componentDeferredPlanarReflections = null;

        public enum PlayerEnvironment
        {
            Unknown,
            Outdoors,
            Building,
            DungeonOrCastle
        }        
        private PlayerEnvironment playerEnvironment = PlayerEnvironment.Unknown;

        // mod settings mapping
        private bool isEnabledOutdoorGroundReflections;
        private bool isEnabledOutdoorSeaReflections;
        private bool isEnabledIndoorBuildingFloorReflections;
        private bool isEnabledIndoorBuildingLowerLevelReflection;
        private bool isEnabledDungeonGroundReflections;
        private bool isEnabledDungeonWaterReflections;
        private bool isFeatureEnabledFakeParallaxReflections;
        private int floorReflectionTextureWidth;
        private int floorReflectionTextureHeight;
        private int lowerLevelReflectionTextureWidth;
        private int lowerLevelReflectionTextureHeight;
        private float roughnessMultiplier;

        public bool IsEnabledOutdoorGroundReflections
        {
            get { return isEnabledOutdoorGroundReflections; }
            set { isEnabledOutdoorGroundReflections = value; }
        }        
        public bool IsEnabledOutdoorSeaReflections
        {
            get { return isEnabledOutdoorSeaReflections; }
            set { isEnabledOutdoorSeaReflections = value; }
        }        
        public bool IsEnabledIndoorBuildingFloorReflections
        {
            get { return isEnabledIndoorBuildingFloorReflections; }
            set { isEnabledIndoorBuildingFloorReflections = value; }
        }        
        public bool IsEnabledIndoorBuildingLowerLevelReflection
        {
            get { return isEnabledIndoorBuildingLowerLevelReflection; }
            set { isEnabledIndoorBuildingLowerLevelReflection = value; }
        }        
        public bool IsEnabledDungeonGroundReflections
        {
            get { return isEnabledDungeonGroundReflections; }
            set { isEnabledDungeonGroundReflections = value; }
        }
        public bool IsEnabledDungeonWaterReflections
        {
            get { return isEnabledDungeonWaterReflections; }
            set { isEnabledDungeonWaterReflections = value; }
        }
        public bool IsFeatureEnabledFakeParallaxReflections
        {
            get { return isFeatureEnabledFakeParallaxReflections; }
            set { isFeatureEnabledFakeParallaxReflections = value; }
        }        
        public int FloorReflectionTextureWidth
        {
            get { return floorReflectionTextureWidth; }
            set {
                floorReflectionTextureWidth = value;
                if (mirrorRefl)
                {
                    mirrorRefl.m_TextureWidth = floorReflectionTextureWidth;                    
                }
            }
        }
        public int FloorReflectionTextureHeight
        {
            get { return floorReflectionTextureHeight; }
            set {
                floorReflectionTextureHeight = value;
                if (mirrorRefl)
                {                    
                    mirrorRefl.m_TextureHeight = floorReflectionTextureHeight;
                }
            }
        }
        public int LowerLevelReflectionTextureWidth
        {
            get { return lowerLevelReflectionTextureWidth; }
            set {
                lowerLevelReflectionTextureWidth = value;
                if (mirrorReflSeaLevel)
                {
                    mirrorReflSeaLevel.m_TextureWidth = lowerLevelReflectionTextureWidth;                    
                }
            }
        }
        public int LowerLevelReflectionTextureHeight
        {
            get { return lowerLevelReflectionTextureHeight; }
            set {
                lowerLevelReflectionTextureHeight = value;
                if (mirrorReflSeaLevel)
                {                    
                    mirrorReflSeaLevel.m_TextureHeight = lowerLevelReflectionTextureHeight;
                }
            }
        }
        public float RoughnessMultiplier
        {
            get { return roughnessMultiplier; }
            set { roughnessMultiplier = value; }
        }

        public PlayerEnvironment CurrentPlayerEnvironment
        {
            get { return playerEnvironment; }
        }

        public GameObject GameobjectReflectionPlaneGround
        {
            get { return reflectionPlaneGround; }
        }
        public GameObject GameobjectReflectionPlaneLowerLevel
        {
            get { return reflectionPlaneLowerLevel; }
        }

        public RenderTexture getSeaReflectionRenderTexture()
        {
            return mirrorReflSeaLevel.m_ReflectionTexture;
        }

        public RenderTexture getGroundReflectionRenderTexture()
        {
            return mirrorRefl.m_ReflectionTexture;
        }

        public float ReflectionPlaneGroundLevelY
        {
            get
            {
                if (reflectionPlaneGround)
                    return reflectionPlaneGround.transform.position.y;
                else
                    return 0.0f;
            }
        }

        public float ReflectionPlaneGroundLevelYinWorldCoords
        {
            get
            {
                if (reflectionPlaneGround)
                    return reflectionPlaneGround.transform.TransformPoint(reflectionPlaneGround.transform.position).y;
                else
                    return 0.0f;
            }
        }

        public float ReflectionPlaneLowerLevelY
        {
            get
            {
                if (reflectionPlaneLowerLevel)
                    return reflectionPlaneLowerLevel.transform.position.y;
                else
                    return 0.0f;
            }
        }

        public float ReflectionPlaneLowerLevelYinWorldCoords
        {
            get
            {
                if (reflectionPlaneLowerLevel)
                    return reflectionPlaneLowerLevel.transform.TransformPoint(reflectionPlaneLowerLevel.transform.position).y;
                else
                    return 0.0f;
            }
        }

        Texture2D textureTileatlasReflective = null;
        public Texture2D TextureTileatlasReflective
        {
            get { return textureTileatlasReflective; }
            set { textureTileatlasReflective = value; }
        }

        Texture2D textureTileatlasReflectiveRaining = null;
        public Texture2D TextureTileatlasReflectiveRaining
        {
            get { return textureTileatlasReflectiveRaining; }
            set { textureTileatlasReflectiveRaining = value; }
        }

        Shader shaderTilemapWithReflections = null;
        public Shader ShaderTilemapWithReflections
        {
            get { return shaderTilemapWithReflections; }
            set { shaderTilemapWithReflections = value; }
        }

        Shader shaderTilemapTextureArrayWithReflections = null;
        public Shader ShaderTilemapTextureArrayWithReflections
        {
            get { return shaderTilemapTextureArrayWithReflections; }
            set { shaderTilemapTextureArrayWithReflections = value; }
        }

        Shader shaderCreateLookupReflectionTextureCoordinates = null;
        public Shader ShaderCreateLookupReflectionTextureCoordinates
        {
            get { return shaderCreateLookupReflectionTextureCoordinates; }
            set { shaderCreateLookupReflectionTextureCoordinates = value; }
        }

        Shader shaderCreateLookupReflectionTextureIndex = null;
        public Shader ShaderCreateLookupReflectionTextureIndex
        {
            get { return shaderCreateLookupReflectionTextureIndex; }
            set { shaderCreateLookupReflectionTextureIndex = value; }
        }

        Shader shaderDeferredPlanarReflections = null;
        public Shader ShaderDeferredPlanarReflections
        {
            get { return shaderDeferredPlanarReflections; }
            set { shaderDeferredPlanarReflections = value; }
        }

        Shader shaderDungeonWaterWithReflections = null;
        public Shader ShaderDungeonWaterWithReflections
        {
            get { return shaderDungeonWaterWithReflections; }
            set { shaderDungeonWaterWithReflections = value; }
        }

        Shader shaderInvisible = null;
        public Shader ShaderInvisible
        {
            get { return shaderInvisible; }
            set { shaderInvisible = value; }
        }

        bool computeStepDownRaycast(Vector3 raycastStartPoint, Vector3 directionVec, float maxDiffMagnitude, out RaycastHit hit)
        {
            if (Physics.Raycast(raycastStartPoint, directionVec, out hit, 1000.0F))
            {
                Vector3 hitPoint = hit.point;
                hitPoint -= directionVec * 0.1f; // move away a bit from hitpoint in opposite direction of raycast - this is required since daggerfall interiors often have small gaps/holes where a walls meet the floor - so upcoming down-raycast will not miss floor...

                // and now raycast down
                if (Physics.Raycast(hitPoint, Vector3.down, out hit, 1000.0F))
                {
                    Vector3 diffVec = raycastStartPoint - hit.point;
                    if (diffVec.sqrMagnitude <= maxDiffMagnitude)
                    {
                        return (true);
                    }
                }
                else
                {
                    return (false);
                }
            }
            return (false);
        }

        float distanceToLowerLevel(Vector3 startPoint, Vector3 directionVec, out RaycastHit hit)
        {
            const int maxIterations = 20;
            const float offset = 0.01f;
            float maxDiffMagnitude = 3 * offset * offset * 1.1f; // ... once for every axis, *1.1f ... make it a bit bigger than the offset "boundingbox"
            
            // iterative raycast in forward direction
            Vector3 raycastStartPoint = startPoint; // +Camera.main.transform.position; // +new Vector3(0.0f, 0.1f, 0.0f);
            for (int i = 0; i < maxIterations; i++)
            {
                if (computeStepDownRaycast(raycastStartPoint, directionVec, maxDiffMagnitude, out hit))
                { 
                    return (startPoint.y - hit.point.y);
                }

                Vector3 offsetVec = -directionVec * offset;  // move away a bit from hitpoint in opposite direction of raycast - this is required since daggerfall interiors often have small gaps/holes where a walls meet the floor - so upcoming down-raycast will not miss floor...
                offsetVec.y = offset; // move a bit up as well - so that next raycast starts above ground a bit
                raycastStartPoint = hit.point + offsetVec;
            }
            hit = new RaycastHit();
            return(float.MinValue);
        }

        float majorityOf3FloatValues(float value1, float value2, float value3, float allowedDistance, float valueWhenTied)
        {
            if ((Mathf.Abs(value1 - value2) < allowedDistance) || (Mathf.Abs(value1 - value3) < allowedDistance))
                return (value1);
            else if (Mathf.Abs(value2 - value3) < allowedDistance)
            {
                return (value2);
            }
            else
                return valueWhenTied;
        }

        float distanceToLowerLevelStartingFromWall(Vector3 startPoint, Vector3 directionVectorToWall, Vector3 directionVectorRaycast1, Vector3 directionVectorRaycast2)
        {
            const float offset = 0.1f;
            RaycastHit hit, hit1, hit2, hit3;
            float biasAmount = 0.5f; // vector bias of the 2 additional parallel raycasts
            Vector3 biasVec = biasAmount * Vector3.Normalize(Vector3.Cross(directionVectorToWall, Vector3.up)); // get normalized normal vector to directionVectorToWall and up vector
            Physics.Raycast(startPoint, directionVectorToWall, out hit1, 1000.0F);
            // do 2 additional parallel raycast with small bias and do majority vote - workaround to reduce problems with holes in geometry...
            Physics.Raycast(startPoint + biasVec, directionVectorToWall, out hit2, 1000.0F);
            Physics.Raycast(startPoint - biasVec, directionVectorToWall, out hit3, 1000.0F);

            Vector3 wallPoint;
            float distance = majorityOf3FloatValues(hit1.distance, hit2.distance, hit3.distance, 0.5f, float.MinValue); // 0.5f must be near to each other but allows for slanted walls (e.g. mages guild's spiral stair)
            if (Mathf.Abs(hit1.distance - distance) < 0.5f)
            {
                wallPoint = hit1.point;
            }
            else if (Mathf.Abs(hit2.distance - distance) < 0.5f)
            {
                wallPoint = hit2.point;
            }
            else if (Mathf.Abs(hit3.distance - distance) < 0.5f)
            {
                wallPoint = hit3.point;
            }
            else
            {
                return (float.MinValue);
            }


            //Vector3 wallPoint = hit.point;
            //wallPoint.y += 0.05f;
            float distance1 = distanceToLowerLevel(wallPoint - directionVectorToWall * offset, directionVectorRaycast1, out hit);
            float distance2 = distanceToLowerLevel(wallPoint - directionVectorToWall * offset, directionVectorRaycast2, out hit);
            return Mathf.Max(distance1, distance2);
        }

        float getDistanceToLowerLevel(GameObject goPlayerAdvanced)
        {
            float distanceToLowerLevelWhenGoingForward = float.MinValue;
            float distanceToLowerLevelWhenGoingBack = float.MinValue;
            float distanceToLowerLevelWhenGoingLeft = float.MinValue;
            float distanceToLowerLevelWhenGoingRight = float.MinValue;
            float distanceToLowerLevelWhenGoingForwardLeft = float.MinValue;
            float distanceToLowerLevelWhenGoingForwardRight = float.MinValue;
            float distanceToLowerLevelWhenGoingBackLeft = float.MinValue;
            float distanceToLowerLevelWhenGoingBackRight = float.MinValue;

            float distanceToLowerLevelStartingFromLeftWall = float.MinValue;
            float distanceToLowerLevelStartingFromRightWall = float.MinValue;
            float distanceToLowerLevelStartingFromForwardWall = float.MinValue;
            float distanceToLowerLevelStartingFromBackWall = float.MinValue;

            RaycastHit hit;

            Vector3 startPoint = goPlayerAdvanced.transform.position;

            // 2 additional raycasts parallel to the main raycast just next to it are performed - majority vote of result of these 3 raycasts is then used as distance to lower level in the direction of interest
            float biasAmount = 0.5f; // vector bias of the 2 additional parallel raycasts

            distanceToLowerLevelWhenGoingForward = majorityOf3FloatValues(distanceToLowerLevel(startPoint, Vector3.forward, out hit),
                                                                          distanceToLowerLevel(startPoint + new Vector3(-biasAmount, 0.0f, 0.0f), Vector3.forward, out hit),
                                                                          distanceToLowerLevel(startPoint + new Vector3(+biasAmount, 0.0f, 0.0f), Vector3.forward, out hit),
                                                                          0.001f,
                                                                          float.MinValue);
            distanceToLowerLevelWhenGoingBack = majorityOf3FloatValues(distanceToLowerLevel(startPoint, Vector3.back, out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(-biasAmount, 0.0f, 0.0f), Vector3.back, out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(+biasAmount, 0.0f, 0.0f), Vector3.back, out hit),
                                                                       0.001f,
                                                                       float.MinValue);


            distanceToLowerLevelWhenGoingLeft = majorityOf3FloatValues(distanceToLowerLevel(startPoint, Vector3.left, out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(0.0f, 0.0f, -biasAmount), Vector3.left, out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(0.0f, 0.0f, +biasAmount), Vector3.left, out hit),
                                                                       0.001f,
                                                                       float.MinValue);

            distanceToLowerLevelWhenGoingRight = majorityOf3FloatValues(distanceToLowerLevel(startPoint, Vector3.right, out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(0.0f, 0.0f, -biasAmount), Vector3.right, out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(0.0f, 0.0f, +biasAmount), Vector3.right, out hit),
                                                                       0.001f,
                                                                       float.MinValue);


            distanceToLowerLevelWhenGoingForwardLeft = majorityOf3FloatValues(distanceToLowerLevel(startPoint, new Vector3(-1.0f, 0.0f, 1.0f), out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(-biasAmount, 0.0f, -biasAmount), new Vector3(-1.0f, 0.0f, 1.0f), out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(+biasAmount, 0.0f, +biasAmount), new Vector3(-1.0f, 0.0f, 1.0f), out hit),
                                                                       0.001f,
                                                                       float.MinValue);

            distanceToLowerLevelWhenGoingForwardRight = majorityOf3FloatValues(distanceToLowerLevel(startPoint, new Vector3(1.0f, 0.0f, 1.0f), out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(-biasAmount, 0.0f, +biasAmount), new Vector3(1.0f, 0.0f, 1.0f), out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(+biasAmount, 0.0f, -biasAmount), new Vector3(1.0f, 0.0f, 1.0f), out hit),
                                                                       0.001f,
                                                                       float.MinValue);

            distanceToLowerLevelWhenGoingBackLeft = majorityOf3FloatValues(distanceToLowerLevel(startPoint, new Vector3(-1.0f, 0.0f, -1.0f), out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(-biasAmount, 0.0f, +biasAmount), new Vector3(-1.0f, 0.0f, -1.0f), out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(+biasAmount, 0.0f, -biasAmount), new Vector3(-1.0f, 0.0f, -1.0f), out hit),
                                                                       0.001f,
                                                                       float.MinValue);

            distanceToLowerLevelWhenGoingBackRight = majorityOf3FloatValues(distanceToLowerLevel(startPoint, new Vector3(1.0f, 0.0f, -1.0f), out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(-biasAmount, 0.0f, -biasAmount), new Vector3(1.0f, 0.0f, -1.0f), out hit),
                                                                       distanceToLowerLevel(startPoint + new Vector3(+biasAmount, 0.0f, +biasAmount), new Vector3(1.0f, 0.0f, -1.0f), out hit),
                                                                       0.001f,
                                                                       float.MinValue);

            // now go to wall and start 2 perpendicular raycast (perpendicular to the wall) to determine distance to lower level (this is important when facing the edge of a wall but you can see to lower level beside the edge, otherwise you miss the lower level plane with the above raycast steps)
            float extraY = Camera.main.transform.localPosition.y; // start from eye position to reduce unintentional "around the corner sampling" - can still happen but less likely
            startPoint.y += extraY;
            distanceToLowerLevelStartingFromLeftWall = -extraY + distanceToLowerLevelStartingFromWall(startPoint, Vector3.left, Vector3.forward, Vector3.back);
            distanceToLowerLevelStartingFromRightWall = -extraY + distanceToLowerLevelStartingFromWall(startPoint, Vector3.right, Vector3.forward, Vector3.back);
            distanceToLowerLevelStartingFromForwardWall = -extraY + distanceToLowerLevelStartingFromWall(startPoint, Vector3.forward, Vector3.left, Vector3.right);
            distanceToLowerLevelStartingFromBackWall = -extraY + distanceToLowerLevelStartingFromWall(startPoint, Vector3.back, Vector3.left, Vector3.right);


            return (Mathf.Max(distanceToLowerLevelWhenGoingForward, distanceToLowerLevelWhenGoingBack, distanceToLowerLevelWhenGoingLeft, distanceToLowerLevelWhenGoingRight, 
                                distanceToLowerLevelWhenGoingForwardLeft, distanceToLowerLevelWhenGoingForwardRight, distanceToLowerLevelWhenGoingBackLeft, distanceToLowerLevelWhenGoingBackRight,
                                distanceToLowerLevelStartingFromLeftWall, distanceToLowerLevelStartingFromRightWall, distanceToLowerLevelStartingFromForwardWall, distanceToLowerLevelStartingFromBackWall));

        }

        Mesh CreateMesh(float width, float height)
        {
            Mesh m = new Mesh();
            m.name = "ScriptedMesh";
            m.vertices = new Vector3[] {
                new Vector3(-width, 0.01f, -height),
                new Vector3(width, 0.01f, -height),
                new Vector3(width, 0.01f, height),
                new Vector3(-width, 0.01f, height),
                new Vector3(-width, -10000.0f, -height), // create 2nd plane at level -10000, so that the stacked near camera (with near clip plane of around 1000) will trigger OnWillRenderObject() callback
                new Vector3(width, -10000.0f, -height), // create 2nd plane at level -10000, so that the stacked near camera (with near clip plane of around 1000) will trigger OnWillRenderObject() callback
                new Vector3(width, -10000.0f, height), // create 2nd plane at level -10000, so that the stacked near camera (with near clip plane of around 1000) will trigger OnWillRenderObject() callback
                new Vector3(-width, -10000.0f, height) // create 2nd plane at level -10000, so that the stacked near camera (with near clip plane of around 1000) will trigger OnWillRenderObject() callback
            };
            m.uv = new Vector2[] {
                new Vector2 (0, 0),
                new Vector2 (0, 1),
                new Vector2(1, 1),
                new Vector2 (1, 0),
                new Vector2 (0, 0), // from 2nd plane
                new Vector2 (0, 1), // from 2nd plane
                new Vector2(1, 1), // from 2nd plane
                new Vector2 (1, 0) // from 2nd plane
            };
            m.triangles = new int[] { 0, 1, 2, 0, 2, 3, /* here starts 2nd plane */ 4, 5, 6, 4, 6, 7 };
            m.RecalculateNormals();

            return m;
        }

        void Awake()
        {         
            PlayerEnterExit.OnTransitionInterior += OnTransitionToInterior;
            PlayerEnterExit.OnTransitionExterior += OnTransitionToExterior;
            PlayerEnterExit.OnTransitionDungeonInterior += OnTransitionToInterior;
            PlayerEnterExit.OnTransitionDungeonExterior += OnTransitionToExterior;
        }

        void OnDestroy()
        {
            PlayerEnterExit.OnTransitionInterior -= OnTransitionToInterior;
            PlayerEnterExit.OnTransitionExterior -= OnTransitionToExterior;
            PlayerEnterExit.OnTransitionDungeonInterior -= OnTransitionToInterior;
            PlayerEnterExit.OnTransitionDungeonExterior -= OnTransitionToExterior;
        }

        void Enable()
        {

        }

        void Disable()
        {

        }

        void Start()
        {
            InjectReflectiveMaterialProperty scriptInjectReflectiveMaterialProperty = this.gameObject.AddComponent<InjectReflectiveMaterialProperty>();

            reflectionPlaneGround = new GameObject("ReflectionPlaneGroundLevel");
            reflectionPlaneGround.layer = LayerMask.NameToLayer("Water");
            MeshFilter meshFilter = (MeshFilter)reflectionPlaneGround.AddComponent(typeof(MeshFilter));
            meshFilter.mesh = CreateMesh(100000.0f, 100000.0f); // create quad with normal facing into negative y-direction (so it is not visible but it will trigger OnWillRenderObject() in MirrorReflection.cs) - should be big enough to be "visible" even when looking parallel to the x/z-plane
            MeshRenderer renderer = reflectionPlaneGround.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

            renderer.material = new Material(ShaderInvisible);
            // switch to this material for reflection plane height debugging
            //renderer.material.shader = Shader.Find("Standard");
            //Texture2D tex = new Texture2D(1, 1);
            //tex.SetPixel(0, 0, Color.green);
            //tex.Apply();
            //renderer.material.mainTexture = tex;
            //renderer.material.color = Color.green;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.enabled = true; // if this is set to false OnWillRenderObject() in MirrorReflection.cs will not work (workaround would be to change OnWillRenderObject() to Update()

            mirrorRefl = reflectionPlaneGround.AddComponent<MirrorReflection>();
            mirrorRefl.m_TextureWidth = floorReflectionTextureWidth;
            mirrorRefl.m_TextureHeight = floorReflectionTextureHeight;

            reflectionPlaneGround.transform.SetParent(this.transform);            

            reflectionPlaneLowerLevel = new GameObject("ReflectionPlaneLowerLevel");
            reflectionPlaneLowerLevel.layer = LayerMask.NameToLayer("Water");
            MeshFilter meshFilterSeaLevel = (MeshFilter)reflectionPlaneLowerLevel.AddComponent(typeof(MeshFilter));
            meshFilterSeaLevel.mesh = CreateMesh(1000000.0f, 1000000.0f); // create quad facing into negative y-direction (so it is not visible but it will trigger OnWillRenderObject() in MirrorReflection.cs) - should be big enough to be "visible" even when looking parallel to the x/z-plane
            MeshRenderer rendererSeaLevel = reflectionPlaneLowerLevel.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

            rendererSeaLevel.material = new Material(ShaderInvisible);
            // switch to this material for reflection plane height debugging
            //rendererSeaLevel.material.shader = Shader.Find("Standard");
            //Texture2D texSeaLevel = new Texture2D(1, 1);
            //texSeaLevel.SetPixel(0, 0, Color.green);
            //texSeaLevel.Apply();
            //rendererSeaLevel.material.mainTexture = texSeaLevel;
            //rendererSeaLevel.material.color = Color.green;
            rendererSeaLevel.shadowCastingMode = ShadowCastingMode.Off;
            rendererSeaLevel.enabled = true; // if this is set to false OnWillRenderObject() in MirrorReflection.cs will not work (workaround would be to change OnWillRenderObject() to Update()

            mirrorReflSeaLevel = reflectionPlaneLowerLevel.AddComponent<MirrorReflection>();
            mirrorReflSeaLevel.m_TextureWidth = lowerLevelReflectionTextureWidth;
            mirrorReflSeaLevel.m_TextureHeight = lowerLevelReflectionTextureHeight;

            reflectionPlaneLowerLevel.transform.SetParent(this.transform);

            LayerMask layerIndexWorldTerrain = LayerMask.NameToLayer("WorldTerrain");
            if (layerIndexWorldTerrain != -1)
            {
                mirrorRefl.m_ReflectLayers.value = (1 << LayerMask.NameToLayer("Default")) + (1 << LayerMask.NameToLayer("WorldTerrain"));
                mirrorReflSeaLevel.m_ReflectLayers = (1 << LayerMask.NameToLayer("Default")) + (1 << LayerMask.NameToLayer("WorldTerrain"));

                //mirrorRefl.layerCullDistances[LayerMask.NameToLayer("Default")] = 15000;
                //mirrorRefl.layerCullDistances[LayerMask.NameToLayer("WorldTerrain")] = 200000;

                //mirrorReflSeaLevel.layerCullDistances[LayerMask.NameToLayer("Default")] = 0;
                //mirrorReflSeaLevel.layerCullDistances[LayerMask.NameToLayer("WorldTerrain")] = 200000;

            }
            else
            {
                mirrorRefl.m_ReflectLayers.value = 1 << LayerMask.NameToLayer("Default");
                mirrorReflSeaLevel.m_ReflectLayers = 1 << LayerMask.NameToLayer("Default");

                //mirrorRefl.layerCullDistances[LayerMask.NameToLayer("Default")] = 15000;
                //mirrorReflSeaLevel.layerCullDistances[LayerMask.NameToLayer("Default")] = 0;
            }

            useDeferredReflections = (GameManager.Instance.MainCamera.renderingPath == RenderingPath.DeferredShading);

            if (useDeferredReflections)
            {
                componentDeferredPlanarReflections = GameManager.Instance.MainCameraObject.AddComponent<RealtimeReflections.DeferredPlanarReflections>();
            }

            if (!GameManager.Instance.IsPlayerInside)
            {
                playerEnvironment = PlayerEnvironment.Outdoors;
                UpdateBackgroundSettingsOutdoor();
            }
            else if (GameManager.Instance.IsPlayerInsideBuilding)
            {
                playerEnvironment = PlayerEnvironment.Building;
                UpdateBackgroundSettingsIndoor();
            }
            else
            {
                playerEnvironment = PlayerEnvironment.DungeonOrCastle;
                UpdateBackgroundSettingsIndoor();
            }
        }

        void OnTransitionToInterior(PlayerEnterExit.TransitionEventArgs args)
        {
            mirrorRefl.m_ReflectLayers.value = 1 << LayerMask.NameToLayer("Default");
            mirrorReflSeaLevel.m_ReflectLayers = 1 << LayerMask.NameToLayer("Default");

            mirrorRefl.CurrentBackgroundSettings = MirrorReflection.EnvironmentSetting.IndoorSetting;
            mirrorReflSeaLevel.CurrentBackgroundSettings = MirrorReflection.EnvironmentSetting.IndoorSetting;
        }

        void OnTransitionToExterior(PlayerEnterExit.TransitionEventArgs args)
        {
            LayerMask layerIndexWorldTerrain = LayerMask.NameToLayer("WorldTerrain");
            if (layerIndexWorldTerrain != -1)
            {
                mirrorRefl.m_ReflectLayers.value = (1 << LayerMask.NameToLayer("Default")) + (1 << LayerMask.NameToLayer("WorldTerrain"));
                mirrorReflSeaLevel.m_ReflectLayers = (1 << LayerMask.NameToLayer("Default")) + (1 << LayerMask.NameToLayer("WorldTerrain"));
            }
            else
            {
                mirrorRefl.m_ReflectLayers.value = 1 << LayerMask.NameToLayer("Default");
                mirrorReflSeaLevel.m_ReflectLayers = 1 << LayerMask.NameToLayer("Default");
            }

            mirrorRefl.CurrentBackgroundSettings = MirrorReflection.EnvironmentSetting.OutdoorSetting;
            mirrorReflSeaLevel.CurrentBackgroundSettings = MirrorReflection.EnvironmentSetting.OutdoorSetting;
        }

        void UpdateBackgroundSettingsIndoor()
        {
            //playerInside = true; // player now inside

            if (GameManager.Instance.IsPlayerInsideBuilding)
                playerEnvironment = PlayerEnvironment.Building;
            else if (GameManager.Instance.IsPlayerInsideCastle || GameManager.Instance.IsPlayerInsideDungeon)
                playerEnvironment = PlayerEnvironment.DungeonOrCastle;

            mirrorRefl.CurrentBackgroundSettings = MirrorReflection.EnvironmentSetting.IndoorSetting;
            mirrorReflSeaLevel.CurrentBackgroundSettings = MirrorReflection.EnvironmentSetting.IndoorSetting;

            if (
                ((IsEnabledIndoorBuildingFloorReflections || isEnabledIndoorBuildingLowerLevelReflection) && GameManager.Instance.IsPlayerInsideBuilding) ||
                ((IsEnabledDungeonGroundReflections || IsEnabledDungeonWaterReflections) && (GameManager.Instance.IsPlayerInsideCastle || GameManager.Instance.IsPlayerInsideDungeon))
                )
            {
                if (useDeferredReflections)
                {
                    componentDeferredPlanarReflections.enabled = true;
                    componentDeferredPlanarReflections.AddCommandBuffer();
                }
            }
            else
            {
                componentDeferredPlanarReflections.RemoveCommandBuffer();
                componentDeferredPlanarReflections.enabled = false;
            }

            if ((IsEnabledIndoorBuildingFloorReflections && GameManager.Instance.IsPlayerInsideBuilding) ||
                (IsEnabledDungeonGroundReflections && (GameManager.Instance.IsPlayerInsideDungeon || GameManager.Instance.IsPlayerInsideCastle)))
                reflectionPlaneGround.SetActive(true);
            else
                reflectionPlaneGround.SetActive(false);

            if ((IsEnabledIndoorBuildingLowerLevelReflection && GameManager.Instance.IsPlayerInsideBuilding) ||
                (IsEnabledDungeonWaterReflections && (GameManager.Instance.IsPlayerInsideDungeon || GameManager.Instance.IsPlayerInsideCastle)))
                reflectionPlaneLowerLevel.SetActive(true);
            else
                reflectionPlaneLowerLevel.SetActive(false);
        }

        void UpdateBackgroundSettingsOutdoor()
        {
            //playerInside = false; // player now outside
            playerEnvironment = PlayerEnvironment.Outdoors;

            mirrorRefl.CurrentBackgroundSettings = MirrorReflection.EnvironmentSetting.OutdoorSetting;
            mirrorReflSeaLevel.CurrentBackgroundSettings = MirrorReflection.EnvironmentSetting.OutdoorSetting;

            if (
                ((isEnabledOutdoorGroundReflections || IsEnabledOutdoorSeaReflections) && !GameManager.Instance.IsPlayerInside)
               )
            {
                if (useDeferredReflections)
                {
                    componentDeferredPlanarReflections.enabled = true;
                    componentDeferredPlanarReflections.AddCommandBuffer();
                }
            }
            else
            {
                componentDeferredPlanarReflections.RemoveCommandBuffer();
                componentDeferredPlanarReflections.enabled = false;
            }

            if (isEnabledOutdoorGroundReflections)
                reflectionPlaneGround.SetActive(true);
            else
                reflectionPlaneGround.SetActive(false);

            if (IsEnabledOutdoorSeaReflections)
                reflectionPlaneLowerLevel.SetActive(true);
            else
                reflectionPlaneLowerLevel.SetActive(false);
        }

        void Update()
        {
            GameObject goPlayerAdvanced = GameObject.Find("PlayerAdvanced");

            PlayerGPS playerGPS = GameObject.Find("PlayerAdvanced").GetComponent<PlayerGPS>();
            if (!playerGPS)
                return;

            //if (GameManager.Instance.IsPlayerInside)
            if (GameManager.Instance.IsPlayerInsideBuilding)
            {
                RaycastHit hit;
                float distanceToGround = 0;
                if (Physics.Raycast(goPlayerAdvanced.transform.position, -Vector3.up, out hit, 100.0F))
                    distanceToGround = hit.distance;

                // additional checks in distance of player/character controller in all directions - important so that reflections don't disappear too early, e.g attics near ladder
                float radius = GameManager.Instance.PlayerController.radius;
                float distanceToGroundNorth = 0, distanceToGroundSouth = 0, distanceToGroundWest = 0, distanceToGroundEast = 0;
                if (Physics.Raycast(goPlayerAdvanced.transform.position + new Vector3(0.0f, 0.0f, -radius), -Vector3.up, out hit, 100.0F))
                    distanceToGroundNorth = hit.distance;
                if (Physics.Raycast(goPlayerAdvanced.transform.position + new Vector3(0.0f, 0.0f, radius), -Vector3.up, out hit, 100.0F))
                    distanceToGroundSouth = hit.distance;
                if (Physics.Raycast(goPlayerAdvanced.transform.position + new Vector3(-radius, 0.0f, 0.0f), -Vector3.up, out hit, 100.0F))
                    distanceToGroundWest = hit.distance;
                if (Physics.Raycast(goPlayerAdvanced.transform.position + new Vector3(radius, 0.0f, 0.0f), -Vector3.up, out hit, 100.0F))
                    distanceToGroundEast = hit.distance;
                distanceToGround = Mathf.Min(distanceToGround, Mathf.Min(distanceToGroundNorth, Mathf.Min(distanceToGroundSouth, Mathf.Min(distanceToGroundWest, distanceToGroundEast))));

                reflectionPlaneGround.transform.position = goPlayerAdvanced.transform.position - new Vector3(0.0f, distanceToGround, 0.0f); //new Vector3(0.0f, GameManager.Instance.PlayerController.height * 0.5f, 0.0f);

                float distanceLevelBelow = getDistanceToLowerLevel(goPlayerAdvanced);
                //Debug.Log(string.Format("distance to lower level: {0}", distanceLevelBelow));
                reflectionPlaneLowerLevel.transform.position = goPlayerAdvanced.transform.position - new Vector3(0.0f, distanceLevelBelow, 0.0f);
            }
            else if (GameManager.Instance.IsPlayerInsideDungeon || GameManager.Instance.IsPlayerInsideCastle)
            {
                RaycastHit hit;
                float distanceToGround = 0;

                if (Physics.Raycast(goPlayerAdvanced.transform.position, -Vector3.up, out hit, 100.0F))
                {
                    distanceToGround = hit.distance;
                }
                reflectionPlaneGround.transform.position = goPlayerAdvanced.transform.position - new Vector3(0.0f, distanceToGround, 0.0f); //new Vector3(0.0f, GameManager.Instance.PlayerController.height * 0.5f, 0.0f);

                //Debug.Log(string.Format("distance to lower level: {0}", distanceLevelBelow));
                Vector3 pos = goPlayerAdvanced.transform.position;
                reflectionPlaneLowerLevel.transform.position = new Vector3(pos.x, GameManager.Instance.PlayerEnterExit.blockWaterLevel * -1 * MeshReader.GlobalScale, pos.z);

                //// prevent underwater reflections below water level
                //if (reflectionPlaneGround.transform.position.y < reflectionPlaneLowerLevel.transform.position.y)
                //{
                //    reflectionPlaneGround.transform.position = reflectionPlaneLowerLevel.transform.position;
                //}
            }
            else //if (!GameManager.Instance.IsPlayerInside)
            {
                Terrain terrainInstancePlayerTerrain = null;

                int referenceLocationX = playerGPS.CurrentMapPixel.X;
                int referenceLocationY = playerGPS.CurrentMapPixel.Y;

                ContentReader.MapSummary mapSummary;
                // if there is no location at current player position...
                if (!DaggerfallUnity.Instance.ContentReader.HasLocation(referenceLocationX, referenceLocationY, out mapSummary))
                {
                    // search for largest location in local 8-neighborhood and take this as reference location for location reflection plane
                    int maxLocationArea = -1;
                    for (int y = -1; y <= +1; y++)
                    {
                        for (int x = -1; x <= +1; x++)
                        {
                            if (DaggerfallUnity.Instance.ContentReader.HasLocation(playerGPS.CurrentMapPixel.X + x, playerGPS.CurrentMapPixel.Y + y, out mapSummary))
                            {
                                DFLocation location = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetLocation(mapSummary.RegionIndex, mapSummary.MapIndex);
                                byte locationRangeX = location.Exterior.ExteriorData.Width;
                                byte locationRangeY = location.Exterior.ExteriorData.Height;
                                int locationArea = locationRangeX * locationRangeY;

                                if (locationArea > maxLocationArea)
                                {
                                    referenceLocationX = playerGPS.CurrentMapPixel.X + x;
                                    referenceLocationY = playerGPS.CurrentMapPixel.Y + y;
                                    maxLocationArea = locationArea;
                                }
                            }
                        }
                    }
                }

                GameObject go = GameObject.Find("StreamingTarget");
                if (go == null)
                    return;

                foreach (Transform child in go.transform)
                {
                    DaggerfallTerrain dfTerrain = child.GetComponent<DaggerfallTerrain>();
                    if (!dfTerrain)
                        continue;

                    if ((dfTerrain.MapPixelX != referenceLocationX) || (dfTerrain.MapPixelY != referenceLocationY))
                        continue;


                    Terrain terrainInstance = child.GetComponent<Terrain>();
                    terrainInstancePlayerTerrain = terrainInstance;

                    if ((terrainInstance) && (terrainInstance.terrainData))
                    {
                        float scale = terrainInstance.terrainData.heightmapScale.x;
                        float xSamplePos = DaggerfallUnity.Instance.TerrainSampler.HeightmapDimension * 0.55f;
                        float ySamplePos = DaggerfallUnity.Instance.TerrainSampler.HeightmapDimension * 0.55f;
                        Vector3 pos = new Vector3(xSamplePos * scale, 0, ySamplePos * scale);
                        float height = terrainInstance.SampleHeight(pos + terrainInstance.transform.position);

                        float positionY = height + terrainInstance.transform.position.y;
                        reflectionPlaneGround.transform.position = new Vector3(goPlayerAdvanced.transform.position.x + terrainInstance.transform.position.x, positionY, goPlayerAdvanced.transform.position.z + terrainInstance.transform.position.z);
                    }
                }

                if (!terrainInstancePlayerTerrain)
                    return;

                //Debug.Log(string.Format("playerGPS: {0}, plane: {1}", goPlayerAdvanced.transform.position.y, reflectionPlaneGround.transform.position.y));
                if (playerGPS.transform.position.y < reflectionPlaneGround.transform.position.y)
                {
                    RaycastHit hit;
                    float distanceToGround = 0;

                    if (Physics.Raycast(goPlayerAdvanced.transform.position, -Vector3.up, out hit, 100.0F))
                    {
                        distanceToGround = hit.distance;
                    }

                    //Debug.Log(string.Format("distance to ground: {0}", distanceToGround));
                    reflectionPlaneGround.transform.position = goPlayerAdvanced.transform.position - new Vector3(0.0f, distanceToGround, 0.0f);
                }

                StreamingWorld streamingWorld = GameObject.Find("StreamingWorld").GetComponent<StreamingWorld>();
                Vector3 vecWaterHeight = new Vector3(0.0f, (DaggerfallUnity.Instance.TerrainSampler.OceanElevation + 0.0f) * streamingWorld.TerrainScale, 0.0f); // water height level on y-axis (+1.0f some coastlines are incorrect otherwise)
                Vector3 vecWaterHeightTransformed = terrainInstancePlayerTerrain.transform.TransformPoint(vecWaterHeight); // transform to world coordinates
                //Debug.Log(string.Format("x,y,z: {0}, {1}, {2}", vecWaterHeight.x, vecWaterHeight.y, vecWaterHeight.z));
                //Debug.Log(string.Format("transformed x,y,z: {0}, {1}, {2}", vecWaterHeightTransformed.x, vecWaterHeightTransformed.y, vecWaterHeightTransformed.z));
                reflectionPlaneLowerLevel.transform.position = new Vector3(goPlayerAdvanced.transform.position.x, vecWaterHeightTransformed.y, goPlayerAdvanced.transform.position.z);
            }

            if (GameManager.Instance.IsPlayerInsideBuilding && playerEnvironment != PlayerEnvironment.Building)
            {
                UpdateBackgroundSettingsIndoor();
            }
            else if ((GameManager.Instance.IsPlayerInsideCastle || GameManager.Instance.IsPlayerInsideDungeon) && playerEnvironment != PlayerEnvironment.DungeonOrCastle)
            {
                UpdateBackgroundSettingsIndoor();
            }
            else if (!GameManager.Instance.IsPlayerInside && playerEnvironment != PlayerEnvironment.Outdoors)
            {
                UpdateBackgroundSettingsOutdoor();
            }
            
        }
	}
}