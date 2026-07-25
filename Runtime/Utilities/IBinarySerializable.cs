namespace VeryFS.Framework.Runtime.Utilities
{
    public interface IBinarySerializable
    {
        void Deserialize(BytesReader reader);

        void Serialize(BytesWriter writer);
    }
}