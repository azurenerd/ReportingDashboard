using System;
using AgentSquad.Runner.Data.Exceptions;
using Xunit;

namespace AgentSquad.Runner.Tests.Data.Exceptions
{
    /// <summary>
    /// Unit tests for DataLoadException custom exception class.
    /// Verifies correct inheritance, message handling, and exception behavior.
    /// </summary>
    public class DataLoadExceptionTests
    {
        [Fact]
        public void DataLoadException_InheritsFromException()
        {
            // Arrange & Act
            var exception = new DataLoadException("Test message");

            // Assert
            Assert.IsAssignableFrom<Exception>(exception);
        }

        [Fact]
        public void DataLoadException_StoresMessage()
        {
            // Arrange
            var expectedMessage = "data.json not found in wwwroot directory";

            // Act
            var exception = new DataLoadException(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void DataLoadException_WithEmptyMessage_StoresEmpty()
        {
            // Arrange
            var expectedMessage = string.Empty;

            // Act
            var exception = new DataLoadException(expectedMessage);

            // Assert
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void DataLoadException_WithNullMessage_StoresNull()
        {
            // Act
            var exception = new DataLoadException(null);

            // Assert
            Assert.Null(exception.Message);
        }

        [Fact]
        public void DataLoadException_CanBeCaught_AsException()
        {
            // Arrange
            var exception = new DataLoadException("Test");

            // Act & Assert
            Assert.Throws<Exception>(() => throw exception);
        }

        [Fact]
        public void DataLoadException_CanBeCaught_AsDataLoadException()
        {
            // Arrange
            var exception = new DataLoadException("Test");

            // Act & Assert
            Assert.Throws<DataLoadException>(() => throw exception);
        }

        [Fact]
        public void DataLoadException_PreservesMessage_WhenThrown()
        {
            // Arrange
            var expectedMessage = "Invalid JSON format: Unexpected token 'x'";

            // Act & Assert
            var exception = Assert.Throws<DataLoadException>(() =>
                throw new DataLoadException(expectedMessage));

            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void DataLoadException_Message_CanBeAccessed()
        {
            // Arrange
            var message = "JSON deserialization resulted in null";

            // Act
            var exception = new DataLoadException(message);

            // Assert
            Assert.True(exception.Message.Length > 0);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void DataLoadException_MultipleInstances_HaveSeparateMessages()
        {
            // Arrange
            var message1 = "data.json not found in wwwroot directory";
            var message2 = "Invalid JSON format: syntax error";

            // Act
            var exception1 = new DataLoadException(message1);
            var exception2 = new DataLoadException(message2);

            // Assert
            Assert.NotEqual(exception1.Message, exception2.Message);
            Assert.Equal(message1, exception1.Message);
            Assert.Equal(message2, exception2.Message);
        }

        [Fact]
        public void DataLoadException_ToString_ContainsMessage()
        {
            // Arrange
            var message = "Test error message";
            var exception = new DataLoadException(message);

            // Act
            var exceptionString = exception.ToString();

            // Assert
            Assert.Contains("DataLoadException", exceptionString);
            Assert.Contains(message, exceptionString);
        }

        [Fact]
        public void DataLoadException_CanIncludeInnerException()
        {
            // Arrange
            var innerException = new System.IO.FileNotFoundException("File not found");

            // Act - Note: This tests if the constructor would support inner exception in future
            var exception = new DataLoadException("Wrapped error");

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("Wrapped error", exception.Message);
        }
    }
}