global using FacioQuo.Stock.Indicators;
global using FluentAssertions;
global using Microsoft.VisualStudio.TestTools.UnitTesting;
global using Test.Base;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel, Workers = 0)]
