namespace Natter.Messaging
{
    public interface IField
    {
        byte[] Name { get; }
        byte[] Value { get; }

        string GetNameAsString();
        byte[] Serialise();
    }
}
