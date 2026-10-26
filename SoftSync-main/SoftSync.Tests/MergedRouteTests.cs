using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using SoftSync.Presentation.Components;
using Xunit;

namespace SoftSync.Tests;

public sealed class MergedRouteTests
{
    [Fact]
    public void Component_routes_are_unambiguous_after_branch_integration()
    {
        var duplicates = typeof(App).Assembly.GetTypes()
            .SelectMany(type => type.GetCustomAttributes<RouteAttribute>()
                .Select(route => new
                {
                    Template = Regex.Replace(route.Template.ToLowerInvariant(),
                        @"\{[^}:?]+(?=[:}?])", "{parameter"),
                    Component = type.FullName
                }))
            .GroupBy(route => route.Template)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key}: {string.Join(", ", group.Select(route => route.Component))}")
            .ToArray();

        Assert.True(duplicates.Length == 0, string.Join(Environment.NewLine, duplicates));
    }
}
