using Hopaut.ArchitectureTests.Catalog;
using Hopaut.SharedKernel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Hopaut.ArchitectureTests.Rules;

/// <summary>Type-level DDD rules (clean-architecture-ddd skill §2, §8, §9).</summary>
public static class DomainModelRules
{
    public const string PublicSetter = "ARCH-DOMAIN-SETTER";
    public const string MutableCollection = "ARCH-DOMAIN-COLLECTION";
    public const string AggregateConstructor = "ARCH-DOMAIN-AGGREGATE-CTOR";
    public const string EventRaiser = "ARCH-SHAREDKERNEL-EVENT-RAISER";
    public const string NamespaceMatchesAssembly = "ARCH-NAMESPACE";

    private const BindingFlags DeclaredPublic =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly Type[] MutableCollectionDefinitions =
    [
        typeof(List<>), typeof(IList<>), typeof(ICollection<>), typeof(Collection<>),
        typeof(HashSet<>), typeof(ISet<>), typeof(Dictionary<,>), typeof(IDictionary<,>),
    ];

    /// <summary>Public, non-init setters. Init-only accessors (records, events) are allowed.</summary>
    public static IEnumerable<Violation> PublicSetters(IEnumerable<Type> types) =>
        from type in types.Where(IsUserType)
        from property in type.GetProperties(DeclaredPublic)
        where property.SetMethod is { IsPublic: true } setter && !IsInitOnly(setter)
        select new Violation($"{PublicSetter}: {type.FullName}.{property.Name}", string.Empty);

    public static IEnumerable<Violation> PublicMutableCollections(IEnumerable<Type> types)
    {
        foreach (var type in types.Where(IsUserType))
        {
            foreach (var property in type.GetProperties(DeclaredPublic).Where(p => p.GetMethod is { IsPublic: true }))
            {
                if (IsMutableCollection(property.PropertyType))
                {
                    yield return new Violation($"{MutableCollection}: {type.FullName}.{property.Name}", property.PropertyType.Name);
                }
            }

            foreach (var field in type.GetFields(DeclaredPublic).Where(f => IsMutableCollection(f.FieldType)))
            {
                yield return new Violation($"{MutableCollection}: {type.FullName}.{field.Name}", field.FieldType.Name);
            }
        }
    }

    public static IEnumerable<Violation> AggregatePublicConstructors(IEnumerable<Type> types) =>
        from type in types.Where(IsUserType)
        where !type.IsAbstract && InheritsFromGeneric(type, typeof(AggregateRoot<>))
        where type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length > 0
        select new Violation($"{AggregateConstructor}: {type.FullName}", string.Empty);

    /// <summary>Only aggregates may raise domain events, through a protected method (skill §2: Raise is protected).</summary>
    public static IEnumerable<Violation> PublicEventRaisers(IEnumerable<Type> types) =>
        from type in types.Where(IsUserType)
        where typeof(IHasDomainEvents).IsAssignableFrom(type)
        from method in type.GetMethods(DeclaredPublic)
        where method.GetParameters().Any(p => typeof(IDomainEvent).IsAssignableFrom(p.ParameterType))
        select new Violation($"{EventRaiser}: {type.FullName}.{method.Name}", string.Empty);

    /// <summary>The namespace-based layer rules are only sound if every type lives under its assembly's root namespace.</summary>
    public static IEnumerable<Violation> NamespacesMatchAssembly(SolutionAssembly subject) =>
        from type in subject.Assembly.GetTypes().Where(IsUserType)
        let ns = type.Namespace ?? string.Empty
        where ns != subject.Name && !ns.StartsWith(subject.Name + ".", StringComparison.Ordinal)
        select new Violation($"{NamespaceMatchesAssembly}: {type.FullName}", subject.Name);

    public static IEnumerable<Type> TypesOf(IEnumerable<SolutionAssembly> assemblies) =>
        assemblies.SelectMany(a => a.Assembly.GetTypes());

    private static bool IsUserType(Type type)
    {
        // Skip compiler-synthesized types and anything nested inside them (e.g. <>z__ReadOnlySingleElementList`1+Enumerator).
        for (var current = type; current is not null; current = current.DeclaringType)
        {
            if (current.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
                || current.Name.Contains('<')
                || current.GetCustomAttributes(inherit: false).Any(a => a.GetType().Name == "EmbeddedAttribute"))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsInitOnly(MethodInfo setter) =>
        setter.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit));

    private static bool IsMutableCollection(Type type) =>
        type.IsArray
        || (type.IsGenericType && MutableCollectionDefinitions.Contains(type.GetGenericTypeDefinition()));

    private static bool InheritsFromGeneric(Type type, Type openGeneric)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == openGeneric)
            {
                return true;
            }
        }

        return false;
    }
}
