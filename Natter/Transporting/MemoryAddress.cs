namespace Natter.Transporting
{
    public class MemoryAddress : IAddress
    {
        public string Address
        {
            get;
            private set;
        }

        public MemoryAddress(string address)
        {
            Address = address;
        }

        public string Serialise()
        {
            return Address;
        }

        public IAddress Deserialise(string address)
        {
            return new MemoryAddress(address);
        }
    }
}
