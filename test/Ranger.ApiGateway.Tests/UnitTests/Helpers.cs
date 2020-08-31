using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ranger.ApiGateway.Tests
{
    public static class Helpers
    {
        public static string NotEmptyMessage(string name) => $"'{name}' must not be empty.";
    }
}