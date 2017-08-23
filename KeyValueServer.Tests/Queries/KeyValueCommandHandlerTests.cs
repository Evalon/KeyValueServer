using KeyValueServer.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using KeyValueServer.Queries;
using KeyValueServer.Queries.Result;
using KeyValueServer.Repositories.Exceptions;

namespace KeyValueServer.UnitTests.Queries
{
    public class KeyValueCommandHandlerTests
    {
        private static (Mock<IKeyValueRepository>, KeyValueCommandHandler) PrepareTestObjects(MockBehavior mockBehavior = MockBehavior.Default)
        {
            var storeMock = new Mock<IKeyValueRepository>(mockBehavior);

            storeMock.Setup(x => x.GetAllKeys()).Returns(new List<string>() { "1", "2", "verify" });
            storeMock.Setup(x => x.GetValue(It.IsAny<string>())).Throws(new KeyNotFoundInRepositoryException("key not found"));
            storeMock.Setup(x => x.DeleteValue(It.IsAny<string>())).Throws(new KeyNotFoundInRepositoryException("key not found"));
            storeMock.Setup(x => x.UpdateValue(It.IsAny<string>(), It.IsAny<string>())).Throws(new KeyNotFoundInRepositoryException("key not found"));
            storeMock.Setup(x => x.GetValue("1")).Returns("1");
            storeMock.Setup(x => x.UpdateValue("1", "1"));
            storeMock.Setup(x => x.DeleteValue("1"));

            return (storeMock, new KeyValueCommandHandler(storeMock.Object));
        }
        public class SuccessFailureTests
        {
            [Fact]
            public void SuccessReadAllKeysCommand()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                var sendedCommand = new Command { Type = Command.CommandType.ReadAll };
                var resultFromCommand = (SuccessResult<ICollection<string>>) commandHandler.Handle(sendedCommand);
                var expectedCommand = new SuccessResult<ICollection<string>>() { Result = new List<string>() { "1", "2", "verify" } };

                Assert.Equal(resultFromCommand.Result, expectedCommand.Result);
            }
            [Fact]
            public void SuccessReadCommand()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                var sendedCommand = new Command { Type = Command.CommandType.Read, Key = "1" };
                var resultFromCommand = (SuccessResult<string>) commandHandler.Handle(sendedCommand);
                var expectedCommand = new SuccessResult<string>() { Result = "1" };
                Assert.Equal(resultFromCommand.Result, expectedCommand.Result);
            }
            [Fact]
            public void SuccessCreateCommand()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                var sendedCommand = new Command { Type = Command.CommandType.Create, Key = "5", Value = "5"};
                var resultFromCommand = (SuccessResult) commandHandler.Handle(sendedCommand);
                var expectedCommand = new SuccessResult();

                Assert.Equal(resultFromCommand.Status, expectedCommand.Status);
            }
            [Fact]
            public void SuccessDeleteCommand()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                var sendedCommand = new Command { Type = Command.CommandType.Delete, Key = "1" };
                var resultFromCommand = (SuccessResult) commandHandler.Handle(sendedCommand);
                var expectedCommand = new SuccessResult();
                commandHandler.Handle(sendedCommand);

                Assert.Equal(resultFromCommand.Status, expectedCommand.Status);
            }
            [Fact]
            public void SuccessUpdateCommand()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                var sendedCommand = new Command { Type = Command.CommandType.Update, Key = "1", Value = "1"};
                var resultFromCommand = (SuccessResult)commandHandler.Handle(sendedCommand);
                var expectedCommand = new SuccessResult();
                commandHandler.Handle(sendedCommand);

                Assert.Equal(resultFromCommand.Status, expectedCommand.Status);
            }
            [Fact]
            public void FailureCreateCommand_KeyRequired()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();

                var sendedCommand = new Command { Type = Command.CommandType.Create};
                var resultFromCommand = (FailureResult)commandHandler.Handle(sendedCommand);
                var expectedCommand = new FailureResult() { ErrorMessage = "key is required" };

                Assert.Equal(resultFromCommand.ErrorMessage, expectedCommand.ErrorMessage);
            }
            [Fact]
            public void FailureCreateCommand_ValueRequired()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();

                var sendedCommand = new Command { Type = Command.CommandType.Create, Key = "only" };
                var resultFromCommand = (FailureResult)commandHandler.Handle(sendedCommand);
                var expectedCommand = new FailureResult() { ErrorMessage = "value is required" };

                Assert.Equal(resultFromCommand.ErrorMessage, expectedCommand.ErrorMessage);
            }
            [Fact]
            public void FailureReadCommand()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                
                var sendedCommand = new Command { Type = Command.CommandType.Read, Key = "wrong" };
                var resultFromCommand = (FailureResult) commandHandler.Handle(sendedCommand);
                var expectedCommand = new FailureResult() { ErrorMessage = "key not found" };

                Assert.Equal(resultFromCommand.ErrorMessage, expectedCommand.ErrorMessage);
            }
            [Fact]
            public void FailureDeleteCommand()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();

                var sendedCommand = new Command { Type = Command.CommandType.Delete, Key = "wrong" };
                var resultFromCommand = (FailureResult) commandHandler.Handle(sendedCommand);
                var expectedCommand = new FailureResult() { ErrorMessage = "key not found" };

                Assert.Equal(resultFromCommand.ErrorMessage, expectedCommand.ErrorMessage);
            }
            [Fact]
            public void FailureDeleteCommand_KeyRequired()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();

                var sendedCommand = new Command { Type = Command.CommandType.Delete };
                var resultFromCommand = (FailureResult)commandHandler.Handle(sendedCommand);
                var expectedCommand = new FailureResult() { ErrorMessage = "key is required" };

                Assert.Equal(resultFromCommand.ErrorMessage, expectedCommand.ErrorMessage);
            }
            [Fact]
            public void FailureUpdateCommand()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();

                var sendedCommand = new Command { Type = Command.CommandType.Update, Key = "wrong", Value = "wrong" };
                var resultFromCommand = (FailureResult)commandHandler.Handle(sendedCommand);
                var expectedCommand = new FailureResult() { ErrorMessage = "key not found" };

                Assert.Equal(resultFromCommand.ErrorMessage, expectedCommand.ErrorMessage);
            }
            [Fact]
            public void FailureUpdateCommand_KeyRequired()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();

                var sendedCommand = new Command { Type = Command.CommandType.Update };
                var resultFromCommand = (FailureResult)commandHandler.Handle(sendedCommand);
                var expectedCommand = new FailureResult() { ErrorMessage = "key is required" };

                Assert.Equal(resultFromCommand.ErrorMessage, expectedCommand.ErrorMessage);
            }
            [Fact]
            public void FailureUpdateCommand_ValueRequired()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();

                var sendedCommand = new Command { Type = Command.CommandType.Update, Key = "only" };
                var resultFromCommand = (FailureResult)commandHandler.Handle(sendedCommand);
                var expectedCommand = new FailureResult() { ErrorMessage = "value is required" };

                Assert.Equal(resultFromCommand.ErrorMessage, expectedCommand.ErrorMessage);
            }

        }
        public class InternalCallTests
        {
            [Fact]
            public void OnReadAllKeysCommandCall()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                commandHandler.Handle(new Command { Type = Command.CommandType.ReadAll });
                storeMock.Verify(x => x.GetAllKeys(), Times.AtLeastOnce);
            }
            [Fact]
            public void OnReadCommandCall()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                commandHandler.Handle(new Command { Type = Command.CommandType.Read, Key = "1" });

                storeMock.Verify(x => x.GetValue("1"), Times.AtLeastOnce);
            }
            [Fact]
            public void OnCreateCommandCall()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                commandHandler.Handle(new Command { Type = Command.CommandType.Create, Key = "5", Value = "5" });

                storeMock.Verify(x => x.SetValue("5", "5"), Times.AtLeastOnce);
            }
            [Fact]
            public void OnDeleteCommandCall()
            {
                var (storeMock, commandHandler) = PrepareTestObjects();
                commandHandler.Handle(new Command { Type = Command.CommandType.Delete, Key = "1" });

                storeMock.Verify(x => x.DeleteValue("1"), Times.AtLeastOnce);
            }
        }
    }
}
