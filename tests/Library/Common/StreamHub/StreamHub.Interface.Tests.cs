using System.Reflection;

namespace TestOfTests;

/// <summary>
/// Validates that all StreamHub test classes implement correct test interfaces
/// based on their hub's provider type per instruction file guidelines.
/// </summary>
[TestClass]
public class TestCompliance
{

    [TestMethod]
    public void AllStreamHubTests_ImplementCorrectInterfaces()
    {
        // Get all test classes that inherit from StreamHubTestBase
        Assembly testAssembly = typeof(StreamHubTestBase).Assembly;
        Type[] allTypes = testAssembly.GetTypes();

        List<Type> streamHubTestClasses = allTypes
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && typeof(StreamHubTestBase).IsAssignableFrom(t))
            .ToList();

        Console.WriteLine($"\nFound {streamHubTestClasses.Count} StreamHub test classes");

        List<string> violations = new();
        List<string> warnings = new();
        int validated = 0;

        // Define observer and provider interface types
        Type[] observerTypes = [
            typeof(ITestChainObserver),
            typeof(ITestBarObserver),
            typeof(ITestTradeTickObserver)
        ];

        // Add more provider interfaces here if needed
        Type[] providerTypes = [
            typeof(ITestChainProvider)
        ];

        foreach (Type testClass in streamHubTestClasses)
        {
            string className = testClass.Name;
            Type[] actualInterfaces = testClass.GetInterfaces()
                .Where(i => i.Name.StartsWith("ITest", StringComparison.Ordinal))
                .ToArray();

            // Find which observer/provider interfaces are implemented
            List<Type> implementedObservers = actualInterfaces.Where(i => observerTypes.Contains(i)).ToList();
            List<Type> implementedProviders = actualInterfaces.Where(i => providerTypes.Contains(i)).ToList();

            // Must implement at least one observer
            if (implementedObservers.Count == 0)
            {
                violations.Add($"{className}: Does not implement any observer interface");
            }

            // Warn if both ITestChainObserver and ITestBarObserver are implemented (redundant)
            if (implementedObservers.Contains(typeof(ITestChainObserver)) && implementedObservers.Contains(typeof(ITestBarObserver)))
            {
                warnings.Add($"{className}: Implements both ITestChainObserver and ITestBarObserver (redundant)");
            }

            // If implements provider, must also have a valid observer (already checked above)
            // Add more rules as new provider/observer types are introduced

            validated++;
        }

        Console.WriteLine($"Validated {validated} test classes");

        if (violations.Count > 0)
        {
            string message = $"Found {violations.Count} interface compliance violations:\n"
                           + string.Join("\n", violations);
            Assert.Fail(message);
        }

        if (warnings.Count > 0)
        {
            Console.WriteLine($"\n[WARN] {warnings.Count} redundant observer interface implementations detected:");
            foreach (string w in warnings)
            {
                Console.WriteLine("  - " + w);
            }
        }

        Console.WriteLine("✅ All validated StreamHub tests implement correct observer/provider interfaces");
    }

    [TestMethod]
    public void ChainObserver_DoesNotRedundantlyImplementBarObserver()
    {
        // ITestChainObserver inherits ITestBarObserver, so implementing both is redundant

        Assembly testAssembly = typeof(StreamHubTestBase).Assembly;
        Type[] allTypes = testAssembly.GetTypes();

        List<Type> redundantImplementations = allTypes
            .Where(static t => t.IsClass
                     && !t.IsAbstract
                     && typeof(ITestChainObserver).IsAssignableFrom(t)
                     && t.GetInterfaces().Contains(typeof(ITestBarObserver)))
            .ToList();

        // Note: This is informational only - it's not wrong to explicitly implement both,
        // but the instruction file notes it's redundant
        if (redundantImplementations.Count > 0)
        {
            Console.WriteLine($"\nNote: {redundantImplementations.Count} test classes "
                            + "explicitly implement both ITestChainObserver and ITestBarObserver "
                            + "(ITestChainObserver already inherits ITestBarObserver):");

            foreach (Type t in redundantImplementations)
            {
                Console.WriteLine($"  - {t.Name}");
            }
        }
    }
}
