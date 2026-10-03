using DMO.Alpha.Core.Tools;
using DMO.Alpha.Infrastructure.Data;
using DMO.Alpha.Infrastructure.Tools;
using Microsoft.EntityFrameworkCore;

namespace DMO.Alpha.Infrastructure.Tests.Tools;

/// <summary>
/// Prova o surface de descoberta de candidatos IToolCandidateQuery: o
/// ToolId devolvido é sempre a identidade canónica persistida (nunca
/// derivada de referência/lote/tipo); candidatos com a mesma referência e
/// lotes diferentes nunca colapsam; a filtragem apenas reduz o conjunto de
/// candidatos; a query nunca auto-seleciona uma Tool (um único candidato
/// que resta continua a ser uma lista por selecionar por ato humano
/// explícito); valores de filtro desconhecidos devolvem uma lista vazia sem
/// inferir identidade; e os factos projetados são exatamente os valores
/// persistidos. É estritamente só de leitura (AsNoTracking, sem
/// SaveChanges) e determinística (ordenação por tool_id).
/// </summary>
public sealed class ToolCandidateQueryTests : IDisposable
{
    private readonly DmoDbContext _db;
    private readonly ToolCandidateQuery _query;
    private readonly ToolLookupQuery _lookup;

    public ToolCandidateQueryTests()
    {
        var options = new DbContextOptionsBuilder<DmoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new DmoDbContext(options);
        _db.Database.EnsureCreated();
        _query = new ToolCandidateQuery(_db);
        _lookup = new ToolLookupQuery(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    /// <summary>
    /// Registo misto com os três tipos em scope, a mesma referência em
    /// lotes diferentes (identidades canónicas distintas) e referências
    /// distintas no mesmo tipo.
    /// </summary>
    private async Task SeedRegistryAsync()
    {
        _db.Tools.AddRange(
            new Tool { ToolId = "BQ-TOOL-9", Type = ToolType.Bq, Reference = "9389T194", Lot = "12" },
            new Tool { ToolId = "BQ-TOOL-10", Type = ToolType.Bq, Reference = "9389T194", Lot = "13" },
            new Tool { ToolId = "BQ-TOOL-11", Type = ToolType.Bq, Reference = "7430C092", Lot = "4" },
            new Tool { ToolId = "CM-TOOL-1", Type = ToolType.Cm, Reference = "5809", Lot = "L1" },
            new Tool { ToolId = "MF-TOOL-1", Type = ToolType.Mf, Reference = "5810", Lot = "L2" });
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task FindCandidatesAsync_NoFilterApplied_ReturnsEveryRegisteredToolWithCanonicalPersistedIdentity()
    {
        await SeedRegistryAsync();

        // Filtro null: nenhuma dimensão aplicada — o registo completo de
        // candidatos, sem exceção de argumento.
        var candidates = await _query.FindCandidatesAsync(filter: null);

        Assert.Equal(5, candidates.Count);

        // Cada ToolId devolvido é exatamente o tool_id persistido (a chave
        // canónica emitida pelo backend), reconciliado contra as linhas
        // persistidas: nunca um identificador sintetizado.
        var persistedIds = await _db.Tools.AsNoTracking().Select(tool => tool.ToolId).ToListAsync();
        Assert.Equal(persistedIds.OrderBy(id => id, StringComparer.Ordinal), candidates.Select(c => c.ToolId));

        foreach (var candidate in candidates)
        {
            var persisted = await _db.Tools.AsNoTracking().SingleAsync(tool => tool.ToolId == candidate.ToolId);
            Assert.Equal(persisted.ToolId, candidate.ToolId);
            Assert.Equal(persisted.Type, candidate.Type);
            Assert.Equal(persisted.Reference, candidate.Reference);
            Assert.Equal(persisted.Lot, candidate.Lot);
        }
    }

    [Fact]
    public async Task FindCandidatesAsync_ExactPersistedValuesAreProjectedForEveryType()
    {
        await SeedRegistryAsync();

        var candidates = await _query.FindCandidatesAsync(filter: null);

        // Projeção exata dos factos persistidos, incluindo o round-trip do
        // ToolType convertido em valor (exatamente CM, MF e BQ).
        Assert.Contains(candidates, c =>
            c.ToolId == "BQ-TOOL-9" && c.Type == ToolType.Bq && c.Reference == "9389T194" && c.Lot == "12");
        Assert.Contains(candidates, c =>
            c.ToolId == "CM-TOOL-1" && c.Type == ToolType.Cm && c.Reference == "5809" && c.Lot == "L1");
        Assert.Contains(candidates, c =>
            c.ToolId == "MF-TOOL-1" && c.Type == ToolType.Mf && c.Reference == "5810" && c.Lot == "L2");
    }

    [Fact]
    public async Task FindCandidatesAsync_SameReferenceWithDifferentLots_NeverCollapsesIntoOneCandidate()
    {
        await SeedRegistryAsync();

        // A mesma referência em lotes diferentes são Tools canónicas
        // distintas: a descoberta devolve ambas como candidatos — o humano
        // é quem distingue pelo lote e seleciona o tool_id exato.
        var byReference = await _query.FindCandidatesAsync(new ToolCandidateFilter(Reference: "9389T194"));

        Assert.Equal(2, byReference.Count);
        Assert.Equal(["BQ-TOOL-10", "BQ-TOOL-9"], byReference.Select(c => c.ToolId));
        Assert.All(byReference, c => Assert.Equal("9389T194", c.Reference));
        Assert.Equal(["13", "12"], byReference.Select(c => c.Lot));

        // A referência comum nunca vira unicidade nem chave derivada.
        Assert.NotEqual(byReference[0].ToolId, byReference[1].ToolId);
    }

    [Fact]
    public async Task FindCandidatesAsync_EachFilterDimensionOnlyReducesTheCandidateSet()
    {
        await SeedRegistryAsync();

        var all = await _query.FindCandidatesAsync(filter: null);
        var bqOnly = await _query.FindCandidatesAsync(new ToolCandidateFilter(Type: ToolType.Bq));
        var bqByReference = await _query.FindCandidatesAsync(
            new ToolCandidateFilter(Type: ToolType.Bq, Reference: "9389T194"));
        var bqByReferenceAndLot = await _query.FindCandidatesAsync(
            new ToolCandidateFilter(Type: ToolType.Bq, Reference: "9389T194", Lot: "12"));

        // Cada dimensão aplicada reduz (ou mantém) o conjunto anterior —
        // nunca o amplia e nunca devolve candidatos fora do filtro.
        Assert.Equal(5, all.Count);
        Assert.Equal(3, bqOnly.Count);
        Assert.Equal(2, bqByReference.Count);
        Assert.Single(bqByReferenceAndLot);

        Assert.All(bqOnly, c => Assert.Equal(ToolType.Bq, c.Type));
        Assert.All(bqByReference, c => Assert.Equal(ToolType.Bq, c.Type));
        Assert.All(bqByReference, c => Assert.Equal("9389T194", c.Reference));
        Assert.All(bqByReferenceAndLot, c => Assert.Equal(ToolType.Bq, c.Type));
        Assert.All(bqByReferenceAndLot, c => Assert.Equal("9389T194", c.Reference));
        Assert.All(bqByReferenceAndLot, c => Assert.Equal("12", c.Lot));

        // Relação de subconjunto: filtrar apenas remove candidatos.
        var allIds = all.Select(c => c.ToolId).ToHashSet();
        var bqIds = bqOnly.Select(c => c.ToolId).ToHashSet();
        var bqRefIds = bqByReference.Select(c => c.ToolId).ToHashSet();
        var bqRefLotIds = bqByReferenceAndLot.Select(c => c.ToolId).ToHashSet();
        Assert.Subset(allIds, bqIds);
        Assert.Subset(bqIds, bqRefIds);
        Assert.Subset(bqRefIds, bqRefLotIds);
    }

    [Fact]
    public async Task FindCandidatesAsync_SingleRemainingCandidate_StaysAnUnselectedCandidate_NeverAutoSelects()
    {
        await SeedRegistryAsync();

        // A filtragem deixa exatamente um candidato: o contrato continua a
        // devolver uma LISTA de candidatos — não um candidato resolvido, não
        // um marcador de seleção, não um tool_id inferido. A seleção é um
        // ato humano explícito posterior; a query nunca a antecipa.
        var candidates = await _query.FindCandidatesAsync(
            new ToolCandidateFilter(Type: ToolType.Bq, Reference: "9389T194", Lot: "12"));

        var single = Assert.Single(candidates);

        // O tool_id devolvido é o persistido, não uma derivação de
        // referência + lote + tipo.
        Assert.Equal("BQ-TOOL-9", single.ToolId);

        // O candidato resolve de volta à mesma Tool canónica pelo surface
        // de lookup exato — a identidade continua canónica e persistida.
        var resolved = await _lookup.FindByToolIdAsync(single.ToolId);
        Assert.NotNull(resolved);
        Assert.Equal(single.ToolId, resolved.ToolId);
        Assert.Equal(single.Reference, resolved.Reference);
        Assert.Equal(single.Lot, resolved.Lot);
    }

    [Fact]
    public async Task FindCandidatesAsync_UnknownFilterValues_ReturnEmptyList_NeverInferIdentity()
    {
        await SeedRegistryAsync();

        // Valores que não correspondem a nada persistido devolvem uma
        // lista vazia — nunca null, nunca exceção, nunca a "melhor
        // correspondência" nem um tool_id adivinhado.
        var unknownReference = await _query.FindCandidatesAsync(new ToolCandidateFilter(Reference: "UNKNOWN-REF"));
        var knownReferenceUnknownLot = await _query.FindCandidatesAsync(
            new ToolCandidateFilter(Reference: "9389T194", Lot: "99"));
        var knownReferenceDifferentType = await _query.FindCandidatesAsync(
            new ToolCandidateFilter(Type: ToolType.Cm, Reference: "9389T194"));

        Assert.Empty(unknownReference);
        Assert.Empty(knownReferenceUnknownLot);
        Assert.Empty(knownReferenceDifferentType);

        // Uma dimensão conhecida com valor sem correspondência reduz a
        // zero candidatos sem inferir nada: nunca há fallback parcial para
        // as outras dimensões nem escolha do "mais próximo".
        var bqUnknownLot = await _query.FindCandidatesAsync(
            new ToolCandidateFilter(Type: ToolType.Bq, Reference: "9389T194", Lot: "99"));
        Assert.Empty(bqUnknownLot);
    }

    [Theory]
    [InlineData("9389t194")]   // sem case-insensitivity
    [InlineData(" 9389T194")]  // sem trimming na correspondência
    [InlineData("9389T19")]    // sem prefixo/contains
    public async Task FindCandidatesAsync_InexactReferenceValues_DoNotMatch(string reference)
    {
        await SeedRegistryAsync();

        // Nenhuma semântica difusa: apenas a igualdade exata ao valor
        // persistido corresponde.
        var candidates = await _query.FindCandidatesAsync(new ToolCandidateFilter(Reference: reference));

        Assert.Empty(candidates);
    }

    [Theory]
    [InlineData("12 ")]  // sem trimming
    [InlineData(" 12")]  // sem trimming
    [InlineData("1")]    // sem prefixo/contains
    public async Task FindCandidatesAsync_InexactLotValues_DoNotMatch(string lot)
    {
        await SeedRegistryAsync();

        var candidates = await _query.FindCandidatesAsync(
            new ToolCandidateFilter(Type: ToolType.Bq, Reference: "9389T194", Lot: lot));

        Assert.Empty(candidates);
    }

    [Fact]
    public async Task FindCandidatesAsync_BlankFilterDimensions_AreTreatedAsNotApplied_NotAsInexactMatches()
    {
        await SeedRegistryAsync();

        // Valores nulos/vazios/whitespace significam "dimensão não
        // aplicada" (estilo do IToolLookupQuery com valores
        // desconhecidos): não lançam exceção, não filtram a zero e não são
        // interpretados como um valor a corresponder.
        var emptyDimensions = await _query.FindCandidatesAsync(new ToolCandidateFilter(Reference: "", Lot: "   "));
        Assert.Equal(5, emptyDimensions.Count);

        var nullDimensions = await _query.FindCandidatesAsync(
            new ToolCandidateFilter(Type: ToolType.Bq, Reference: null, Lot: null));
        Assert.Equal(3, nullDimensions.Count);
    }

    [Fact]
    public async Task FindCandidatesAsync_IsReadOnly_DoesNotTrackOrMutate()
    {
        await SeedRegistryAsync();

        // Destaca as entidades semeadas para que qualquer estado de
        // tracking observado adiante provenha apenas da query em teste.
        _db.ChangeTracker.Clear();

        var candidates = await _query.FindCandidatesAsync(new ToolCandidateFilter(Type: ToolType.Bq));

        Assert.Equal(3, candidates.Count);

        // AsNoTracking: nenhuma Tool fica rastreada e não há entradas
        // Added/Modified/Deleted — a query nunca muta estado.
        Assert.Empty(_db.ChangeTracker.Entries<Tool>());
        Assert.DoesNotContain(
            _db.ChangeTracker.Entries(),
            entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        // As linhas persistidas mantêm-se inalteradas.
        var persistedCount = await _db.Tools.AsNoTracking().CountAsync();
        Assert.Equal(5, persistedCount);
        var persistedBq = await _db.Tools.AsNoTracking().SingleAsync(tool => tool.ToolId == "BQ-TOOL-9");
        Assert.Equal(ToolType.Bq, persistedBq.Type);
        Assert.Equal("9389T194", persistedBq.Reference);
        Assert.Equal("12", persistedBq.Lot);
    }

    [Fact]
    public async Task FindCandidatesAsync_IsDeterministic_AlwaysAscendingByCanonicalToolId()
    {
        await SeedRegistryAsync();

        var first = await _query.FindCandidatesAsync(filter: null);
        var second = await _query.FindCandidatesAsync(filter: null);

        // A mesma execução devolve a mesma sequência, ordenada de forma
        // determinística pela identidade canónica (tool_id).
        Assert.Equal(first, second);
        Assert.Equal(first.Select(c => c.ToolId), first.Select(c => c.ToolId).Order(StringComparer.Ordinal));

        // A ordenação determinística mantém-se com filtros aplicados.
        var filtered = await _query.FindCandidatesAsync(new ToolCandidateFilter(Type: ToolType.Bq));
        Assert.Equal(
            filtered.Select(c => c.ToolId),
            filtered.Select(c => c.ToolId).Order(StringComparer.Ordinal));
    }
}
