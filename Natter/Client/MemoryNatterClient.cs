using Natter.Transporting;

namespace Natter.Client
{
    public class MemoryNatterClient : NatterClient
    {
        public MemoryNatterClient(string me)
            : base(new MemoryTransport(me))
        {
        }

        public void Call(string them)
        {
            Call(new MemoryAddress(them));
        }
    }
}
