using Mono.Cecil;

static IEnumerable<TypeDefinition> AllTypes(IEnumerable<TypeDefinition> roots)
{
    foreach (var type in roots)
    {
        yield return type;
        foreach (var nested in AllTypes(type.NestedTypes))
            yield return nested;
    }
}

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: GeneratedAssemblyFixer <input> <output>");
    return 2;
}

var input = Path.GetFullPath(args[0]);
var output = Path.GetFullPath(args[1]);
using var assembly = AssemblyDefinition.ReadAssembly(input, new ReaderParameters { InMemory = true });

var candidates = AllTypes(assembly.MainModule.Types)
    .Where(type => type.Name is "<>O" or "__O")
    .ToList();

static string NormalizedName(TypeDefinition type) =>
    type.FullName.Replace("<>", "__", StringComparison.Ordinal);

var duplicateGroups = candidates
    .GroupBy(NormalizedName)
    .Where(group => group.Count() > 1)
    .ToList();

if (duplicateGroups.Count == 0)
    throw new InvalidOperationException(
        $"Expected a duplicate <>O group. Candidates: {string.Join(", ", candidates.Select(type => type.FullName))}");

var removed = 0;
foreach (var group in duplicateGroups)
{
    var keep = group.FirstOrDefault(type => type.Name == "__O") ?? group.First();
    foreach (var duplicate in group.Where(type => type != keep))
    {
        if (duplicate.DeclaringType is not null)
            duplicate.DeclaringType.NestedTypes.Remove(duplicate);
        else
            assembly.MainModule.Types.Remove(duplicate);
        removed++;
    }
}

assembly.Write(output);

using var verification = AssemblyDefinition.ReadAssembly(output, new ReaderParameters { InMemory = true });
var remainingDuplicateGroups = AllTypes(verification.MainModule.Types)
    .Where(type => type.Name is "<>O" or "__O")
    .GroupBy(NormalizedName)
    .Count(group => group.Count() > 1);

if (remainingDuplicateGroups != 0)
    throw new InvalidOperationException($"Verification found {remainingDuplicateGroups} duplicate <>O groups.");

Console.WriteLine($"Removed {removed} duplicate generated <>O types.");
return 0;
