﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using RazorSlices.Benchmarks.WebApp;

BenchmarkRunner.Run<RazorSlicesBenchmarks>();

[MemoryDiagnoser]
[AnyCategoriesFilter("Lorem25"), GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Config(typeof(Config))]
public class RazorSlicesBenchmarks
{
    private readonly HttpClient _slicesNuGetClient = new();
    private readonly HttpClient _slicesLocalClient = new();
    private readonly HttpClient _pagesClient = new();
    private readonly HttpClient _componentsClient = new();
    private readonly HttpClient _blazorClient = new();
    private readonly byte[] _buffer = new byte[1024 * 256]; // 256 KB buffer
    private readonly int _iterations = 100;

    public RazorSlicesBenchmarks()
    {
        _slicesNuGetClient = CreateHttpClient<BenchmarksWebAppRazorSlicesPreviousVersion>();
        _slicesLocalClient = CreateHttpClient<BenchmarksWebApp>();
        _pagesClient = CreateHttpClient<BenchmarksRazorPagesWebApp>();
        _componentsClient = CreateHttpClient<BenchmarksRazorComponentsWebApp>();
        _blazorClient = CreateHttpClient<BenchmarksBlazorWebApp>();
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Hello", "RazorSlices")]
    public Task<int> RazorSlicesHello_NuGet() => GetPath(_slicesNuGetClient, "/hello");

    [Benchmark, BenchmarkCategory("Hello", "RazorSlices", "Local")]
    public Task<int> RazorSlicesHello_Local() => GetPath(_slicesLocalClient, "/hello");

    [Benchmark, BenchmarkCategory("Hello", "RazorPages")]
    public Task<int> RazorPagesHello() => GetPath(_pagesClient, "/hello");

    [Benchmark, BenchmarkCategory("Hello", "RazorComponents")]
    public Task<int> RazorComponentsHello() => GetPath(_componentsClient, "/hello");

    [Benchmark, BenchmarkCategory("Hello", "BlazorSSR")]
    public Task<int> BlazorSSRHello() => GetPath(_blazorClient, "/hello");

    [Benchmark, BenchmarkCategory("Lorem", "Lorem25", "RazorSlices", "Local")]
    public Task<int> RazorSlicesLorem25_Local() => GetPath(_slicesLocalClient, "/lorem25");

    [Benchmark, BenchmarkCategory("Lorem", "RazorSlices", "Local")]
    public Task<int> RazorSlicesLorem50_Local() => GetPath(_slicesLocalClient, "/lorem50");

    [Benchmark, BenchmarkCategory("Lorem", "RazorSlices", "Local")]
    public Task<int> RazorSlicesLorem100_Local() => GetPath(_slicesLocalClient, "/lorem100");

    [Benchmark, BenchmarkCategory("Lorem", "RazorSlices", "Local")]
    public Task<int> RazorSlicesLorem200_Local() => GetPath(_slicesLocalClient, "/lorem200");

    [Benchmark(Baseline = true), BenchmarkCategory("Lorem", "Lorem25", "RazorSlices")]
    public Task<int> RazorSlicesLorem25_NuGet() => GetPath(_slicesNuGetClient, "/lorem25");

    [Benchmark, BenchmarkCategory("Lorem", "RazorSlices")]
    public Task<int> RazorSlicesLorem5_NuGet0() => GetPath(_slicesNuGetClient, "/lorem50");

    [Benchmark, BenchmarkCategory("Lorem", "RazorSlices")]
    public Task<int> RazorSlicesLorem100_NuGet() => GetPath(_slicesNuGetClient, "/lorem100");

    [Benchmark, BenchmarkCategory("Lorem", "RazorSlices")]
    public Task<int> RazorSlicesLorem200_NuGet() => GetPath(_slicesNuGetClient, "/lorem200");

    [Benchmark, BenchmarkCategory("Lorem", "Lorem25", "RazorPages")]
    public Task<int> RazorPagesLorem25() => GetPath(_pagesClient, "/lorem25");

    [Benchmark, BenchmarkCategory("Lorem", "RazorPages")]
    public Task<int> RazorPagesLorem50() => GetPath(_pagesClient, "/lorem50");

    [Benchmark, BenchmarkCategory("Lorem", "RazorPages")]
    public Task<int> RazorPagesLorem100() => GetPath(_pagesClient, "/lorem100");

    [Benchmark, BenchmarkCategory("Lorem", "RazorPages")]
    public Task<int> RazorPagesLorem200() => GetPath(_pagesClient, "/lorem200");

    [Benchmark, BenchmarkCategory("Lorem", "Lorem25", "RazorComponents")]
    public Task<int> RazoComponentsLorem25() => GetPath(_componentsClient, "/lorem25");

    [Benchmark, BenchmarkCategory("Lorem", "RazorComponents")]
    public Task<int> RazoComponentsLorem50() => GetPath(_componentsClient, "/lorem50");

    [Benchmark, BenchmarkCategory("Lorem", "RazorComponents")]
    public Task<int> RazoComponentsLorem100() => GetPath(_componentsClient, "/lorem100");

    [Benchmark, BenchmarkCategory("Lorem", "RazorComponents")]
    public Task<int> RazoComponentsLorem200() => GetPath(_componentsClient, "/lorem200");

    private async Task<int> GetPath(HttpClient httpClient, string path)
    {
        var bytesRead = 0;

        for (int i = 0; i < _iterations; i++)
        {
            var body = await httpClient.GetStreamAsync(path);
            bytesRead += await body.ReadAsync(_buffer);
        }

        return bytesRead;
    }

    private static HttpClient CreateHttpClient<TApp>() where TApp : class
    {
        var waf = new WebApplicationFactory<TApp>();
        waf.WithWebHostBuilder(webHost => webHost.ConfigureLogging(logging => logging.ClearProviders()));
        return waf.CreateClient();
    }

    private class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.ShortRun.WithCustomBuildConfiguration("Benchmarks").WithId("Benchmarks"));
        }
    }
}
