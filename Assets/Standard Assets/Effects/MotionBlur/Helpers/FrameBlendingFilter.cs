using UnityEngine;
using UnityEngine.Rendering;

namespace UnityStandardAssets.CinematicEffects
{
    public partial class MotionBlur
    {
        //
        // Multiple frame blending filter
        //
        // This filter acts like a finite impluse response filter; stores
        // succeeding four frames and calculate the weighted average of them.
        //
        // To save memory, it compresses frame images with the 4:2:2 chroma
        // subsampling scheme. This requires MRT support. If the current
        // environment doesn't support MRT, it tries to use one of the 16-bit
        // texture format instead. Unfortunately, some GPUs don't support
        // 16-bit color render targets. So, in the worst case, it ends up with
        // using 32-bit raw textures.
        //
        class FrameBlendingFilter
        {
            #region Public methods

            public FrameBlendingFilter()
            {
                _useCompression = CheckSupportCompression();
                _rawTextureFormat = GetPreferredRenderTextureFormat();

                _material = new Material(Shader.Find("Hidden/Image Effects/Cinematic/MotionBlur/FrameBlending"));
                _material.hideFlags = HideFlags.DontSave;

                _frameList = new Frame[200];

                FetchUniformLocations();
            }

            public void Release()
            {
                DestroyImmediate(_material);
                _material = null;

                foreach (var frame in _frameList) frame.Release();
                _frameList = null;
            }

            public void PushFrame(RenderTexture source)
            {
                // Push only when actual update (do nothing while pausing)
                var frameCount = Time.frameCount;
                if (frameCount == _lastFrameCount) return;

                // Update the frame record.
                var index = frameCount % _frameList.Length;
                if (_useCompression)
                    _frameList[index].MakeRecord(source, _material);
                else
                    _frameList[index].MakeRecordRaw(source, _rawTextureFormat);
                _lastFrameCount = frameCount;
            }

            public void BlendFrames(float strength, RenderTexture source, RenderTexture destination)
            {
                var t = Time.time;

                var f1 = GetFrameRelative(-4);
                var f2 = GetFrameRelative(-8);
                var f3 = GetFrameRelative(-15);
                var f4 = GetFrameRelative(-25);
				var f5 = GetFrameRelative(-40);
				var f6 = GetFrameRelative(-60);
				var f7 = GetFrameRelative(-80);
				var f8 = GetFrameRelative(-100);
				var f9 = GetFrameRelative(-150);
				var f10 = GetFrameRelative(-200);

				_material.SetTexture(_History1LumaTex, f1.lumaTexture);
                _material.SetTexture(_History2LumaTex, f2.lumaTexture);
                _material.SetTexture(_History3LumaTex, f3.lumaTexture);
                _material.SetTexture(_History4LumaTex, f4.lumaTexture);
				_material.SetTexture(_History5LumaTex, f5.lumaTexture);
				_material.SetTexture(_History6LumaTex, f6.lumaTexture);
				_material.SetTexture(_History7LumaTex, f7.lumaTexture);
				_material.SetTexture(_History8LumaTex, f8.lumaTexture);
				_material.SetTexture(_History9LumaTex, f9.lumaTexture);
				_material.SetTexture(_History10LumaTex, f10.lumaTexture);

				_material.SetTexture(_History1ChromaTex, f1.chromaTexture);
                _material.SetTexture(_History2ChromaTex, f2.chromaTexture);
                _material.SetTexture(_History3ChromaTex, f3.chromaTexture);
                _material.SetTexture(_History4ChromaTex, f4.chromaTexture);
				_material.SetTexture(_History5ChromaTex, f5.chromaTexture);
				_material.SetTexture(_History6ChromaTex, f6.chromaTexture);
				_material.SetTexture(_History7ChromaTex, f7.chromaTexture);
				_material.SetTexture(_History8ChromaTex, f8.chromaTexture);
				_material.SetTexture(_History9ChromaTex, f9.chromaTexture);
				_material.SetTexture(_History10ChromaTex, f10.chromaTexture);

				_material.SetFloat(_History1Weight, f1.CalculateWeight(strength, t));
                _material.SetFloat(_History2Weight, f2.CalculateWeight(strength, t));
                _material.SetFloat(_History3Weight, f3.CalculateWeight(strength, t));
                _material.SetFloat(_History4Weight, f4.CalculateWeight(strength, t));
				_material.SetFloat(_History5Weight, f5.CalculateWeight(strength, t));
				_material.SetFloat(_History6Weight, f6.CalculateWeight(strength, t));
				_material.SetFloat(_History7Weight, f7.CalculateWeight(strength, t));
				_material.SetFloat(_History8Weight, f8.CalculateWeight(strength, t));
				_material.SetFloat(_History9Weight, f9.CalculateWeight(strength, t));
				_material.SetFloat(_History10Weight, f10.CalculateWeight(strength, t));

				Graphics.Blit(source, destination, _material, _useCompression ? 1 : 2);
            }

            #endregion

            #region Frame record struct

            struct Frame
            {
                public RenderTexture lumaTexture;
                public RenderTexture chromaTexture;
                public float time;

                RenderBuffer[] _mrt;

                public float CalculateWeight(float strength, float currentTime)
                {
                    if (time == 0) return 0;
                    var coeff = Mathf.Lerp(80.0f, 16.0f, strength);
                    return Mathf.Exp((time - currentTime) * coeff);
                }

                public void Release()
                {
                    if (lumaTexture != null) RenderTexture.ReleaseTemporary(lumaTexture);
                    if (chromaTexture != null) RenderTexture.ReleaseTemporary(chromaTexture);

                    lumaTexture = null;
                    chromaTexture = null;
                }

                public void MakeRecord(RenderTexture source, Material material)
                {
                    Release();

                    lumaTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.R8);
                    chromaTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.R8);

                    lumaTexture.filterMode = FilterMode.Point;
                    chromaTexture.filterMode = FilterMode.Point;

                    if (_mrt == null) _mrt = new RenderBuffer[2];

                    _mrt[0] = lumaTexture.colorBuffer;
                    _mrt[1] = chromaTexture.colorBuffer;

                    Graphics.SetRenderTarget(_mrt, lumaTexture.depthBuffer);
                    Graphics.Blit(source, material, 0);

                    time = Time.time;
                }

                public void MakeRecordRaw(RenderTexture source, RenderTextureFormat format)
                {
                    Release();

                    lumaTexture = RenderTexture.GetTemporary(source.width, source.height, 0, format);
                    lumaTexture.filterMode = FilterMode.Point;

                    Graphics.Blit(source, lumaTexture);

                    time = Time.time;
                }
            }

            #endregion

            #region Private members

            bool _useCompression;
            RenderTextureFormat _rawTextureFormat;

            Material _material;

            Frame[] _frameList;
            int _lastFrameCount;

            int _History1LumaTex;
            int _History2LumaTex;
            int _History3LumaTex;
            int _History4LumaTex;
			int _History5LumaTex;
			int _History6LumaTex;
			int _History7LumaTex;
			int _History8LumaTex;
			int _History9LumaTex;
			int _History10LumaTex;

			int _History1ChromaTex;
            int _History2ChromaTex;
            int _History3ChromaTex;
            int _History4ChromaTex;
			int _History5ChromaTex;
			int _History6ChromaTex;
			int _History7ChromaTex;
			int _History8ChromaTex;
			int _History9ChromaTex;
			int _History10ChromaTex;

			int _History1Weight;
            int _History2Weight;
            int _History3Weight;
            int _History4Weight;
			int _History5Weight;
			int _History6Weight;
			int _History7Weight;
			int _History8Weight;
			int _History9Weight;
			int _History10Weight;

			// Check if the platform has the capability of compression.
			static bool CheckSupportCompression()
            {
                return
                    // Exclude OpenGL ES 2.0 because most of them don't support
                    // more than eight textures (we needs nine).
                    SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2 &&
                    SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) &&
                    SystemInfo.supportedRenderTargetCount > 1;
            }

            // Determine which 16-bit render texture format is available.
            static RenderTextureFormat GetPreferredRenderTextureFormat()
            {
                RenderTextureFormat[] formats = {
                    RenderTextureFormat.RGB565,
                    RenderTextureFormat.ARGB1555,
                    RenderTextureFormat.ARGB4444
                };

                foreach (var f in formats)
                    if (SystemInfo.SupportsRenderTextureFormat(f)) return f;

                return RenderTextureFormat.Default;
            }

            // Retrieve a frame record with relative indexing.
            // Use a negative index to refer to previous frames.
            Frame GetFrameRelative(int offset)
            {
                var index = (Time.frameCount + _frameList.Length + offset) % _frameList.Length;
                return _frameList[index];
            }

            void FetchUniformLocations()
            {
                _History1LumaTex = Shader.PropertyToID("_History1LumaTex");
                _History2LumaTex = Shader.PropertyToID("_History2LumaTex");
                _History3LumaTex = Shader.PropertyToID("_History3LumaTex");
                _History4LumaTex = Shader.PropertyToID("_History4LumaTex");
				_History5LumaTex = Shader.PropertyToID("_History5LumaTex");
				_History6LumaTex = Shader.PropertyToID("_History6LumaTex");
				_History7LumaTex = Shader.PropertyToID("_History7LumaTex");
				_History8LumaTex = Shader.PropertyToID("_History8LumaTex");
				_History9LumaTex = Shader.PropertyToID("_History9LumaTex");
				_History10LumaTex = Shader.PropertyToID("_History10LumaTex");

				_History1ChromaTex = Shader.PropertyToID("_History1ChromaTex");
                _History2ChromaTex = Shader.PropertyToID("_History2ChromaTex");
                _History3ChromaTex = Shader.PropertyToID("_History3ChromaTex");
                _History4ChromaTex = Shader.PropertyToID("_History4ChromaTex");
				_History5ChromaTex = Shader.PropertyToID("_History5ChromaTex");
				_History6ChromaTex = Shader.PropertyToID("_History6ChromaTex");
				_History7ChromaTex = Shader.PropertyToID("_History7ChromaTex");
				_History8ChromaTex = Shader.PropertyToID("_History8ChromaTex");
				_History9ChromaTex = Shader.PropertyToID("_History9ChromaTex");
				_History10ChromaTex = Shader.PropertyToID("_History10ChromaTex");

				_History1Weight = Shader.PropertyToID("_History1Weight");
                _History2Weight = Shader.PropertyToID("_History2Weight");
                _History3Weight = Shader.PropertyToID("_History3Weight");
                _History4Weight = Shader.PropertyToID("_History4Weight");
				_History5Weight = Shader.PropertyToID("_History5Weight");
				_History6Weight = Shader.PropertyToID("_History6Weight");
				_History7Weight = Shader.PropertyToID("_History7Weight");
				_History8Weight = Shader.PropertyToID("_History8Weight");
				_History9Weight = Shader.PropertyToID("_History9Weight");
				_History10Weight = Shader.PropertyToID("_History104Weight");
			}

            #endregion
        }
    }
}
