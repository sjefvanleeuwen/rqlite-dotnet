using RqLite.Client.Diagnostics.Model;
using System.Text.Json;

namespace RqLite.Client.Diagnostics
{
    public static class RqLiteDiagnosticsSerializer
    {
        public static RqLiteDiagnosticsConfiguration FromJson(string json) => JsonSerializer.Deserialize<RqLiteDiagnosticsConfiguration>(json);
        public static string ToJson(this RqLiteDiagnosticsConfiguration self) => JsonSerializer.Serialize(self);
    }
}
