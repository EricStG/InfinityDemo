using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AspNetEnumerable.Server.Controllers
{
    public class Model
    {
        public int Index { get; set; }
        public int A { get; set; }
        public float B { get; set; }
        public string C { get; set; }
    }

    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.mvcoptions.maxiasyncenumerablebufferlimit?view=aspnetcore-3.1
    // https://docs.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-3.1

    // http://localhost:5000/infinity/safe/1000
    // http://localhost:5000/infinity/unsafe/1000

    [ApiController]
    [Route("[controller]")]
    public class InfinityController : ControllerBase
    {
        private readonly Fixture _fixture = new Fixture();

        // System.InvalidOperationException: 'AsyncEnumerableReader' reached the configured maximum size of the buffer when enumerating a value of type 'AspNetEnumerable.Server.Controllers.InfinityController+<GenerateModelsAsync>d__3'. This limit is in place to prevent infinite streams of 'IAsyncEnumerable<>' from continuing indefinitely. If this is not a programming mistake, consider ways to reduce the collection size, or consider manually converting 'AspNetEnumerable.Server.Controllers.InfinityController+<GenerateModelsAsync>d__3' into a list rather than increasing the limit.

        [HttpGet("unsafe/{count}")]
        public IAsyncEnumerable<Model> GetUnSafeInfinityAsync(long count)
        {
            return GenerateModelsAsync(count);
        }

        [HttpGet("safe/{count}")]
        public async Task GetSafeInfinityAsync(long count)
        {
            Response.Headers.Add("Content-Type", "application/json");
            Response.Headers.Add("Charset", "utf-8");
            var writer = new Utf8JsonWriter(Response.Body);
            try
            {
                writer.WriteStartArray();
                //await writer.FlushAsync();
                await foreach (var model in GenerateModelsAsync(count))
                {
                    writer.WriteStartObject();
                    writer.WriteString("index", model.Index.ToString());
                    writer.WriteString("a", model.A.ToString());
                    writer.WriteString("b", model.B.ToString());
                    writer.WriteString("c", model.C);
                    writer.WriteEndObject();
                    //JsonSerializer.Serialize(writer, model);
                }
                writer.WriteEndArray();
                //await writer.FlushAsync();
            }
            finally
            {
                await writer.DisposeAsync();
            }
        }

        [HttpGet("safe2/{count}")]
        public async Task GetSafe2InfinityAsync(long count)
        {
            Response.Headers.Add("Content-Type", "application/json");
            Response.Headers.Add("Charset", "utf-8");
            var writer = new Utf8JsonWriter(Response.Body);
            try
            {
                var i = 1;
                writer.WriteStartArray();
                //await writer.FlushAsync();
                await foreach (var model in GenerateModelsAsync(count))
                {
                    writer.WriteStartObject();
                    writer.WriteString("index", model.Index.ToString());
                    writer.WriteString("a", model.A.ToString());
                    writer.WriteString("b", model.B.ToString());
                    writer.WriteString("c", model.C);
                    writer.WriteEndObject();
                    i++;
                    if (i % 10000 == 0)
                    {
                        await writer.FlushAsync();
                    }
                }
                writer.WriteEndArray();
                //await writer.FlushAsync();
            }
            finally
            {
                await writer.DisposeAsync();
            }
        }

        private async IAsyncEnumerable<Model> GenerateModelsAsync(long count)
        {
            for (var i = 0; i < count; i++)
            {
                await Task.CompletedTask; // Faking async
                yield return _fixture.Build<Model>().With(m => m.Index, i).Create();
            }
        }
    }
}
