namespace Natter.Transporting
{
    public interface IAddress
    {
        string Serialise();
        IAddress Deserialise(string address);
    }
}
