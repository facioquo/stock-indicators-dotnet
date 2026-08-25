using System.Reflection;

namespace ObsoleteShims;
#pragma warning disable CS0618

/// <summary>
/// Tests for the obsolete v3 <c>GetTrix</c> shim that rejects a signal-line argument.
/// </summary>
/// <remarks>
/// The three-argument overload is <c>Obsolete(.., true)</c>, so C# cannot call it at all
/// and CS0619 is not suppressible -- but its error message shared text written for
/// sibling shims whose removed argument really was named <c>smaPeriods</c>. This
/// overload's argument is <c>signalPeriods</c>, so the message told callers to rename a
/// parameter that does not exist on it. Reflection reaches the message the same way it
/// reaches the overload itself.
/// </remarks>
[TestClass]
public class ObsoleteTrixTests : TestBase
{
    [TestMethod]
    public void ErrorLevelGetTrixOverloadMessageNamesSignalPeriods()
    {
        MethodInfo overload = typeof(Trix).Assembly
            .GetType("FacioQuo.Stock.Indicators.Indicator")
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == "GetTrix"
                      && m.GetParameters().Length == 3
                      && m.GetParameters()[0].ParameterType == typeof(IEnumerable<IBar>));

        ObsoleteAttribute obsolete = overload.GetCustomAttribute<ObsoleteAttribute>();

        obsolete.Should().NotBeNull();

        // the overload's actual parameter is `signalPeriods`; the message must name it,
        // not the unrelated `smaPeriods` name copied from sibling shims
        obsolete.Message.Should().Contain("signalPeriods");
        obsolete.Message.Should().NotContain("smaPeriods");
        obsolete.Message.Should().Contain("ToSma");
    }
}
