using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using KeyValueServer.Queries.Result;
using KeyValueServer.Queries;

namespace KeyValueServer.IntegrationTests
{
    public class KeyValueServerTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private const string _basePath = "/api/keyvalue";

        public KeyValueServerTests()
        {
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _client = _server.CreateClient();
        }
        public async Task CheckForSuccess(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var result = JsonConvert.DeserializeObject<CommandResult>(await response.Content.ReadAsStringAsync());
            Assert.Equal(CommandResult.CommandStatus.Success, result.Status);
        }
        public async Task CheckForFailure(HttpResponseMessage response)
        {
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            var result = JsonConvert.DeserializeObject<CommandResult>(await response.Content.ReadAsStringAsync());
            Assert.Equal(CommandResult.CommandStatus.Failure, result.Status);
        }
        public async Task fillKeyValues(IEnumerable<string> collection)
        {
            foreach (var item in collection)
            {
                var command = new Command()
                {
                    Type = Command.CommandType.Create,
                    Key = item,
                    Value = item
                };
                var httpCommand = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync(_basePath, httpCommand);
                result.EnsureSuccessStatusCode();
            }
        }
        private async Task CheckGet(string path, Func<HttpResponseMessage, Task> checkFor)
        {
            var response = await _client.GetAsync(path);
            await checkFor(response);
        }
        private async Task CheckPost(string path, Command command, Func<HttpResponseMessage, Task> checkFor)
        {
            var httpCommand = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json"); ;
            var response = await _client.PostAsync(path, httpCommand);
            await checkFor(response);
        }
        private async Task CheckDelete(string path, Command command, Func<HttpResponseMessage, Task> checkFor)
        {
            var response = await _client.DeleteAsync(path);
            await checkFor(response);
        }
        private async Task CheckUpdate(string path, Command command, Func<HttpResponseMessage, Task> checkFor)
        {
            var httpCommand = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json"); ;
            var response = await _client.PutAsync(path, httpCommand);
            await checkFor(response);
        }

        private async Task CheckAggregate(string path, IEnumerable<Command> commands, CommandResult.CommandStatus checkFor)
        {
            var httpCommand = new StringContent(JsonConvert.SerializeObject(commands), Encoding.UTF8, "application/json"); ;
            var response = await _client.PostAsync(path, httpCommand);
            response.EnsureSuccessStatusCode();
            var result = JsonConvert.DeserializeObject<IEnumerable<CommandResult>>(await response.Content.ReadAsStringAsync());
            foreach (var item in result)
            {
                Assert.Equal(CommandResult.CommandStatus.Success, item.Status);
            }
        }
        [Fact]
        public async Task RootShouldSuccess()
        {
            await CheckGet(_basePath + "/", CheckForSuccess);
        }
        [Fact]
        public async Task CreateValueShouldSuccess()
        {
            var command = new Command()
            {
                Type = Command.CommandType.Create,
                Key = "something",
                Value = "somethingDifferent"
            };
            await CheckPost(_basePath + "/", command, CheckForSuccess);
        }
        [Fact]
        public async Task CreateValueShouldFail_Key()
        {
            var command = new Command()
            {
                Type = Command.CommandType.Create,
                Value = "somethingDifferent"
            };
            await CheckPost(_basePath + "/", command, CheckForFailure);
        }
        [Fact]
        public async Task CreateValueShouldFail_Value()
        {
            var command = new Command()
            {
                Type = Command.CommandType.Create,
                Key = "something"
            };
            await CheckPost(_basePath + "/", command, CheckForFailure);
        }
        [Fact]
        public async Task DeleteValueShouldSuccess()
        {
            await fillKeyValues(new List<string> { "something" });
            var command = new Command()
            {
                Type = Command.CommandType.Delete
            };
            await CheckDelete(_basePath + "/something", command, CheckForSuccess);
        }
        [Fact]
        public async Task DeleteValueShouldFaile_NotFound()
        {
            await fillKeyValues(new List<string> { "something" });
            var command = new Command()
            {
                Type = Command.CommandType.Delete
            };
            await CheckDelete(_basePath + "/notfound", command, CheckForFailure);
        }
        [Fact]
        public async Task UpdateValueShouldSuccess()
        {
            await fillKeyValues(new List<string> { "something" });
            var command = new Command()
            {
                Type = Command.CommandType.Update,
                Value = "somethingNew"
            };
            await CheckUpdate(_basePath + "/something", command, CheckForSuccess);
        }
        [Fact]
        public async Task UpdateValueShouldFail_Value()
        {
            await fillKeyValues(new List<string> { "something" });
            var command = new Command()
            {
                Type = Command.CommandType.Update
            };
            await CheckUpdate(_basePath + "/something", command, CheckForFailure);
        }
        [Fact]
        public async Task UpdateValueShouldFail_NotFound()
        {
            await fillKeyValues(new List<string> { "something" });
            var command = new Command()
            {
                Type = Command.CommandType.Update,
                Value = "somethingNew"
            };
            await CheckUpdate(_basePath + "/notFound", command, CheckForFailure);
        }
        [Fact]
        public async Task AggregateJsonShouldSuccess()
        {
            var commandList = new List<Command>
            {
                new Command() { Type = Command.CommandType.Create, Key = "1", Value = "1" },
                new Command() { Type = Command.CommandType.Create, Key = "2", Value = "2" },
                new Command() { Type = Command.CommandType.ReadAll },
                new Command() { Type = Command.CommandType.Read, Key = "2" },
                new Command() { Type = Command.CommandType.Update, Key = "1", Value = "2" },
                new Command() { Type = Command.CommandType.Delete, Key = "1" }
            };
            await CheckAggregate("/api/keyvalue-actions", commandList, CommandResult.CommandStatus.Success);
        }
    }
}
