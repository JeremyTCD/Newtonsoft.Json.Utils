using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace JeremyTCD.PipelinesCE.PluginAndConfigTools.Tests.IntegrationTests
{
    public class PrivateFieldsJsonConverterIntegrationTests
    {
        [Fact]
        public void ReadJson_IfJsonIsMissingAFieldStoresItAndContinuesDeserializing()
        {
            // Arrange
            string testField2Value = "testField2Value";
            bool? testField3Value = true;

            string json = $"{{\"_stubField2\":\"{testField2Value}\",\"_stubField3\":{testField3Value.ToString().ToLowerInvariant()}}}";

            PrivateFieldsJsonConverter converter = new PrivateFieldsJsonConverter();

            // Act
            StubClass result = JsonConvert.DeserializeObject<StubClass>(json, converter);

            // Assert
            Assert.Equal(1, converter.MissingFields.Count);
            Assert.Equal("_stubField1", converter.MissingFields[0].Name);
            Assert.Equal(testField2Value, result.StubProperty2);
            Assert.Equal(testField3Value, result.StubProperty3);
        }

        [Fact]
        public void ReadJson_IfJsonContainsAnExtraFieldStoresItAndContinuesDeserializing()
        {
            // Arrange
            int testField1Value = 1;
            string testField2Value = "testField2Value";
            bool? testField3Value = true;
            string extraFieldKey = "extraFieldKey";
            string extraFieldValue = "extraFieldValue";

            string json = $"{{\"{extraFieldKey}\":\"{extraFieldValue}\",\"_stubField1\":{testField1Value},\"_stubField2\":\"{testField2Value}\",\"_stubField3\":{testField3Value.ToString().ToLowerInvariant()}}}";

            PrivateFieldsJsonConverter converter = new PrivateFieldsJsonConverter();

            // Act
            StubClass result = JsonConvert.DeserializeObject<StubClass>(json, converter);

            // Assert
            Assert.Equal(1, converter.ExtraFields.Count);
            Assert.Equal(extraFieldValue, (string)converter.ExtraFields[0].Value);
            Assert.Equal(testField1Value, result.StubProperty1);
            Assert.Equal(testField2Value, result.StubProperty2);
            Assert.Equal(testField3Value, result.StubProperty3);
        }

        [Fact]
        public void ReadJson_DeserializesPrivateFields()
        {
            // Arrange
            int testField1Value = 1;
            string testField2Value = "testField2Value";
            bool? testField3Value = true;

            string json = $"{{\"_stubField1\":{testField1Value},\"_stubField2\":\"{testField2Value}\",\"_stubField3\":{testField3Value.ToString().ToLowerInvariant()}}}";

            // Act
            StubClass result = JsonConvert.DeserializeObject<StubClass>(json, new PrivateFieldsJsonConverter());

            // Assert
            Assert.Equal(testField1Value, result.StubProperty1);
            Assert.Equal(testField2Value, result.StubProperty2);
            Assert.Equal(testField3Value, result.StubProperty3);
        }

        [Fact]
        public void WriteJson_SerializesPrivateFields()
        {
            // Arrange
            int testField1Value = 1;
            string testField2Value = "testField2Value";
            bool? testField3Value = true;

            StubClass stubClass = new StubClass
            {
                StubProperty1 = testField1Value,
                StubProperty2 = testField2Value,
                StubProperty3 = testField3Value
            };

            // Act
            string result = JsonConvert.SerializeObject(stubClass, new PrivateFieldsJsonConverter());

            // Assert
            Assert.Equal(3, Regex.Matches(Regex.Escape(result), "_stubField").Count);
            Assert.Contains($"\"_stubField1\":{testField1Value}", result);
            Assert.Contains($"\"_stubField2\":\"{testField2Value}\"", result);
            Assert.Contains($"\"_stubField3\":{testField3Value.ToString().ToLowerInvariant()}", result);
        }

        private class StubClass
        {
            private int _stubField1;
            private string _stubField2;
            private bool? _stubField3;
            public int _stubField4; // public field should not get serialized

            public int StubProperty1
            {
                get
                {
                    return _stubField1;
                }
                set
                {
                    _stubField1 = value;
                }
            }

            public string StubProperty2
            {
                get
                {
                    return _stubField2;
                }
                set
                {
                    _stubField2 = value;
                }
            }

            public bool? StubProperty3
            {
                get
                {
                    return _stubField3;
                }
                set
                {
                    _stubField3 = value;
                }
            }

            public int StubProperty4
            {
                get
                {
                    return _stubField4;
                }
                set
                {
                    _stubField4 = value;
                }
            }
        }
    }
}
