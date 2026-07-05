using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using Transformations.Mapping;

BenchmarkRunner.Run<SingleObjectMappingBenchmarks>(
    DefaultConfig.Instance.AddJob(Job.ShortRun));

BenchmarkRunner.Run<CollectionMappingBenchmarks>(
    DefaultConfig.Instance.AddJob(Job.ShortRun));

BenchmarkRunner.Run<ProjectionBenchmarks>(
    DefaultConfig.Instance.AddJob(Job.ShortRun));

BenchmarkRunner.Run<ConfigurationBenchmarks>(
    DefaultConfig.Instance.AddJob(Job.ShortRun));

[MemoryDiagnoser]
public class SingleObjectMappingBenchmarks
{
    private static readonly IMapper AutoMapper = CreateAutoMapper();
    private static readonly TypeAdapterConfig MapsterConfig = CreateMapsterConfig();
    private readonly BenchmarkUser source = BenchmarkData.CreateUser(1);

    [Benchmark(Baseline = true)]
    public BenchmarkUserDto Manual()
    {
        return BenchmarkMapper.Manual(source);
    }

    [Benchmark]
    public BenchmarkUserDto Transformations()
    {
        return source.ToBenchmarkUserDto();
    }

    [Benchmark]
    public BenchmarkUserDto AutoMapperMap()
    {
        return AutoMapper.Map<BenchmarkUserDto>(source);
    }

    [Benchmark]
    public BenchmarkUserDto MapsterAdapt()
    {
        return source.Adapt<BenchmarkUserDto>(MapsterConfig);
    }

    private static IMapper CreateAutoMapper()
    {
        var config = new MapperConfiguration(
            cfg => cfg.CreateMap<BenchmarkUser, BenchmarkUserDto>(),
            NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    private static TypeAdapterConfig CreateMapsterConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<BenchmarkUser, BenchmarkUserDto>();
        config.Compile();
        return config;
    }
}

[MemoryDiagnoser]
public class CollectionMappingBenchmarks
{
    private static readonly IMapper AutoMapper = CreateAutoMapper();
    private static readonly TypeAdapterConfig MapsterConfig = CreateMapsterConfig();
    private readonly List<BenchmarkUser> source = BenchmarkData.CreateUsers(1_000);

    [Benchmark(Baseline = true)]
    public List<BenchmarkUserDto> ManualList()
    {
        var result = new List<BenchmarkUserDto>(source.Count);
        foreach (BenchmarkUser item in source)
        {
            result.Add(BenchmarkMapper.Manual(item));
        }

        return result;
    }

    [Benchmark]
    public List<BenchmarkUserDto> TransformationsList()
    {
        return source.ToBenchmarkUserDtoList();
    }

    [Benchmark]
    public List<BenchmarkUserDto> AutoMapperList()
    {
        return AutoMapper.Map<List<BenchmarkUserDto>>(source);
    }

    [Benchmark]
    public List<BenchmarkUserDto> MapsterList()
    {
        return source.Adapt<List<BenchmarkUserDto>>(MapsterConfig);
    }

    private static IMapper CreateAutoMapper()
    {
        var config = new MapperConfiguration(
            cfg => cfg.CreateMap<BenchmarkUser, BenchmarkUserDto>(),
            NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    private static TypeAdapterConfig CreateMapsterConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<BenchmarkUser, BenchmarkUserDto>();
        config.Compile();
        return config;
    }
}

[MemoryDiagnoser]
public class ProjectionBenchmarks
{
    private static readonly IMapper AutoMapper = CreateAutoMapper();
    private static readonly TypeAdapterConfig MapsterConfig = CreateMapsterConfig();
    private readonly IQueryable<BenchmarkUser> source = BenchmarkData.CreateUsers(1_000).AsQueryable();

    [Benchmark(Baseline = true)]
    public List<BenchmarkUserDto> ManualSelect()
    {
        return source.Select(BenchmarkMapper.ManualExpression).ToList();
    }

    [Benchmark]
    public List<BenchmarkUserDto> TransformationsProjectTo()
    {
        return source.ProjectToBenchmarkUserDto().ToList();
    }

    [Benchmark]
    public List<BenchmarkUserDto> AutoMapperProjectTo()
    {
        return AutoMapper.ProjectTo<BenchmarkUserDto>(source).ToList();
    }

    [Benchmark]
    public List<BenchmarkUserDto> MapsterProjectTo()
    {
        return source.ProjectToType<BenchmarkUserDto>(MapsterConfig).ToList();
    }

    private static IMapper CreateAutoMapper()
    {
        var config = new MapperConfiguration(
            cfg => cfg.CreateMap<BenchmarkUser, BenchmarkUserDto>(),
            NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    private static TypeAdapterConfig CreateMapsterConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<BenchmarkUser, BenchmarkUserDto>();
        config.Compile();
        return config;
    }
}

[MemoryDiagnoser]
public class ConfigurationBenchmarks
{
    [Benchmark(Baseline = true)]
    public object TransformationsConfiguration()
    {
        return typeof(BenchmarkUser);
    }

    [Benchmark]
    public IMapper AutoMapperConfiguration()
    {
        var config = new MapperConfiguration(
            cfg => cfg.CreateMap<BenchmarkUser, BenchmarkUserDto>(),
            NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    [Benchmark]
    public TypeAdapterConfig MapsterConfiguration()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<BenchmarkUser, BenchmarkUserDto>();
        config.Compile();
        return config;
    }
}

public static class BenchmarkMapper
{
    public static readonly System.Linq.Expressions.Expression<Func<BenchmarkUser, BenchmarkUserDto>> ManualExpression =
        source => new BenchmarkUserDto
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            Department = source.Department,
            IsActive = source.IsActive,
            LoginCount = source.LoginCount,
            Score = source.Score
        };

    public static BenchmarkUserDto Manual(BenchmarkUser source)
    {
        return new BenchmarkUserDto
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            Department = source.Department,
            IsActive = source.IsActive,
            LoginCount = source.LoginCount,
            Score = source.Score
        };
    }
}

public static class BenchmarkData
{
    public static List<BenchmarkUser> CreateUsers(int count)
    {
        var users = new List<BenchmarkUser>(count);
        for (int i = 0; i < count; i++)
        {
            users.Add(CreateUser(i));
        }

        return users;
    }

    public static BenchmarkUser CreateUser(int id)
    {
        return new BenchmarkUser
        {
            Id = id,
            FirstName = "First" + id,
            LastName = "Last" + id,
            Email = "user" + id + "@example.com",
            Department = id % 2 == 0 ? "Engineering" : "Operations",
            IsActive = id % 3 != 0,
            LoginCount = id * 7,
            Score = id / 10m
        };
    }
}

public sealed class BenchmarkUserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int LoginCount { get; set; }
    public decimal Score { get; set; }
}

[MapTo<BenchmarkUserDto>]
public partial class BenchmarkUser
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int LoginCount { get; set; }
    public decimal Score { get; set; }
}
