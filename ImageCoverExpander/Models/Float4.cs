using UnityEngine;

namespace ImageCoverExpander.Models
{
    public class Float4
    {
        public float w;
        public float x;
        public float y;
        public float z;

        public Float4() { }

        public Float4(Float4 float4) : this(float4.w, float4.x, float4.y, float4.z) { }

        public Float4(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Converts the PluginConfig.StoreableFloatVector4 to a UnityEngine.Vector4 format
        /// </summary>
        public static Color ToColor(Float4 float4) => new Color(float4.w, float4.x, float4.y, float4.z);
    }
}
