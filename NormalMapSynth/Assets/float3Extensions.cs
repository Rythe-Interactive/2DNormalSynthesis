namespace Unity.Mathematics
{
    public static class float3Extensions
    {
        public static float3 Cross(this float3 a, float3 b)
        {
            float x, y, z;
            x = a.y * b.z - a.z * b.y;
            y = a.z * b.x - a.x * b.z;
            z = a.x * b.y - a.y * b.x;
            return new float3(x, y, z);
        }
    }
}