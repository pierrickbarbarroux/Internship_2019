namespace UnityEngine.PostProcessing
{
    public sealed class GetSetAttribute : PropertyAttribute
    {
        public readonly string name;
        public bool dirty;

        public GetSetAttribute(string name)
        {
            name = name;
        }
    }
}
