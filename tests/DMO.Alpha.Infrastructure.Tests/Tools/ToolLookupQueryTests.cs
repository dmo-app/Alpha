using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Data;
using DMO.Alpha.Infrastructure.Tools;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Tests.Tools;

/// <summary>
/// Prova o surface nativo de leitura IToolLookupQuery: a resolução é feita
/// exclusivamente pelo tool_id canónico exato, nunca deriva identidade de
/// referência/lote/tipo, é estritamente só de leitura (AsNoTracking, sem
/// SaveChanges) e trata o Tool.Type como facto descritivo — nunca como gate.
/// </summary>
public sealed class ToolLookupQueryTests : IDisposable
{
    private readonly DmoDbContext _db;
    private readonly ToolLookupQuery _query;

    public ToolLookupQueryTests()
    {
        var options = new DbContextOptionsBuilder<DmoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new DmoDbContext(options);
        _db.Database.EnsureCreated();
        _query = new ToolLookupQuery(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task FindByToolIdAsync_ExactCanonicalToolId_ResolvesPersistedTool()
    {
        _db.Tools.Add(new Tool
        {
            ToolId = "BQ-TOOL-9",
            Type = ToolType.Bq,
            Reference = "9389T194",
            Lot = "12"
        });
        await _db.SaveChangesAsync();

        var result = await _query.FindByToolIdAsync("BQ-TOOL-9");

        Assert.NotNull(result);
        Assert.Equal("BQ-TOOL-9", result.ToolId);
        Assert.Equal(ToolType.Bq, result.Type);
        Assert.Equal("9389T194", result.Reference);
        Assert.Equal("12", result.Lot);
    }

    [Fact]
    public async Task FindByToolIdAsync_UnknownToolId_ReturnsNull()
    {
        _db.Tools.Add(new Tool
        {
            ToolId = "BQ-TOOL-KNOWN",
            Type = ToolType.Bq,
            Reference = "9389T194",
            Lot = "12"
        });
        await _db.SaveChangesAsync();

        var result = await _query.FindByToolIdAsync("BQ-TOOL-MISSING");

        Assert.Null(result);
    }

    [Fact]
    public async Task FindByToolIdAsync_IdenticalDescriptiveFacts_RemainDistinctByToolId()
    {
        // Duas Tools com factos descritivos idênticos (mesma referência, lote
        // e tipo) continuam distintas pelo tool_id: cada uma resolve de forma
        // independente e a referência comum nunca cria unicidade nem colisão.
        _db.Tools.Add(new Tool
        {
            ToolId = "BQ-TOOL-A",
            Type = ToolType.Bq,
            Reference = "9389T194",
            Lot = "12"
        });
        _db.Tools.Add(new Tool
        {
            ToolId = "BQ-TOOL-B",
            Type = ToolType.Bq,
            Reference = "9389T194",
            Lot = "12"
        });
        await _db.SaveChangesAsync();

        var a = await _query.FindByToolIdAsync("BQ-TOOL-A");
        var b = await _query.FindByToolIdAsync("BQ-TOOL-B");

        Assert.NotNull(a);
        Assert.NotNull(b);
        Assert.Equal("BQ-TOOL-A", a.ToolId);
        Assert.Equal("BQ-TOOL-B", b.ToolId);
        Assert.NotEqual(a.ToolId, b.ToolId);
        Assert.Equal(a.Reference, b.Reference);
        Assert.Equal(a.Lot, b.Lot);
        Assert.Equal(a.Type, b.Type);
    }

    [Fact]
    public async Task FindByToolIdAsync_IsReadOnly_DoesNotTrackOrMutate()
    {
        _db.Tools.Add(new Tool
        {
            ToolId = "BQ-TOOL-RO",
            Type = ToolType.Bq,
            Reference = "9389T194",
            Lot = "12"
        });
        await _db.SaveChangesAsync();

        // Destaca as entidades semeadas para que qualquer estado de tracking
        // observado adiante provenha apenas da query em teste.
        _db.ChangeTracker.Clear();

        var result = await _query.FindByToolIdAsync("BQ-TOOL-RO");

        Assert.NotNull(result);

        // AsNoTracking: nenhuma Tool fica rastreada e não há entradas
        // Added/Modified/Deleted — a query nunca muta estado.
        Assert.Empty(_db.ChangeTracker.Entries<Tool>());
        Assert.DoesNotContain(
            _db.ChangeTracker.Entries(),
            entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        // A linha persistida mantém-se inalterada.
        var persisted = await _db.Tools.AsNoTracking().SingleAsync(t => t.ToolId == "BQ-TOOL-RO");
        Assert.Equal(ToolType.Bq, persisted.Type);
        Assert.Equal("9389T194", persisted.Reference);
        Assert.Equal("12", persisted.Lot);
    }

    [Theory]
    [InlineData("BQ-TOOL-T", ToolType.Bq)]
    [InlineData("CM-TOOL-T", ToolType.Cm)]
    [InlineData("MF-TOOL-T", ToolType.Mf)]
    public async Task FindByToolIdAsync_ResolvesToolOfAnyType_TypeIsNeverAGate(string toolId, ToolType expectedType)
    {
        _db.Tools.Add(new Tool { ToolId = "BQ-TOOL-T", Type = ToolType.Bq, Reference = "REF-BQ", Lot = "L1" });
        _db.Tools.Add(new Tool { ToolId = "CM-TOOL-T", Type = ToolType.Cm, Reference = "REF-CM", Lot = "L2" });
        _db.Tools.Add(new Tool { ToolId = "MF-TOOL-T", Type = ToolType.Mf, Reference = "REF-MF", Lot = "L3" });
        await _db.SaveChangesAsync();

        var result = await _query.FindByToolIdAsync(toolId);

        Assert.NotNull(result);
        Assert.Equal(toolId, result.ToolId);
        Assert.Equal(expectedType, result.Type);
    }

    [Theory]
    [InlineData("9389T194")]      // a referência não é identidade
    [InlineData("12")]            // o lote não é identidade
    [InlineData("BQ")]            // o token de tipo não é identidade
    [InlineData("bq-tool-a")]     // a correspondência exata é sensível a maiúsculas
    [InlineData(" BQ-TOOL-A ")]   // sem trimming nem correspondência difusa
    public async Task FindByToolIdAsync_DescriptiveFactOrInexactValue_DoesNotResolve(string inexactToolId)
    {
        _db.Tools.Add(new Tool
        {
            ToolId = "BQ-TOOL-A",
            Type = ToolType.Bq,
            Reference = "9389T194",
            Lot = "12"
        });
        await _db.SaveChangesAsync();

        // A identidade é apenas o tool_id canónico exato: nenhum facto
        // descritivo (referência/lote/tipo) nem valor inexato (diferença de
        // maiúsculas ou whitespace) resolve a Tool.
        var result = await _query.FindByToolIdAsync(inexactToolId);

        Assert.Null(result);
    }
}
