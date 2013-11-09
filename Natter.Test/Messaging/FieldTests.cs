using NUnit.Framework;
using Natter.Byte;
using Natter.Messaging;

namespace Natter.Test.Messaging
{
    [TestFixture]
    public class FieldTests
    {
        [Test]
        public void SerialiseField()
        {
            string name = "From";
            string value = "Someone";

            Field f = new Field(name.GetBytes(), value.GetBytes());
            var res = f.Serialise();

            Assert.AreEqual("<4,7>From=Someone ", res.GetString());
        }


        [Test]
        public void DeserialiseField()
        {
            var field = new Field("From".GetBytes(), "Someone".GetBytes());
            var res = Field.Deserialise(field.Serialise());

            Assert.AreEqual("From", res.Name.GetString());
            Assert.AreEqual("Someone", res.Value.GetString());
        }

        [Test]
        public void HowLong()
        {
            var field = new Field("From".GetBytes(), "Someone".GetBytes());
            for (int loop = 0; loop < 1000; loop++)
            {
                var res = Field.Deserialise(field.Serialise());
            }
        }
    }
}
