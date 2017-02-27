namespace LostTech.Storage
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NameEncodingTests
    {

        [TestMethod]
        public void PropertyNameRoundtrip()
        {
            const string disallowedCharacters = "\\/#?а";
            string encoded = AzureTableEntity.EncodeKey(disallowedCharacters);
            string decoded = AzureTableEntity.DecodeKey(encoded);
            Assert.AreEqual(disallowedCharacters, decoded);
        }

        [TestMethod]
        public void KeyRoundtrip()
        {
            const string disallowedCharacters = "\\/#?а";
            string encoded = AzureTable.KeyEncode(disallowedCharacters);
            string decoded = AzureTable.KeyDecode(encoded);
            Assert.AreEqual(disallowedCharacters, decoded);
        }
    }
}
