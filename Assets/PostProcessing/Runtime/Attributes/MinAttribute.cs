namespace UnityEngine.PostProcessing
{
    public sealed class MinAttribute : PropertyAttribute
    {
        public readonly float min;

        public MinAttribute(float min)
        {
            min = min;
        }
    }
}
