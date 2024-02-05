using Microsoft.CodeAnalysis;

namespace SatelliteRpc.Client.SourceGenerator;

/// <summary>
///  The extension of GeneratorExecutionContext
///  Use this class to log message, warning and error
/// </summary>
public static class GeneratorExecutionContextExtensions
{
    public static void LogMessage(this GeneratorExecutionContext context, string message)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                id: "GEN001", 
                title: "Generator Message", 
                messageFormat: message, 
                category: "SatelliteRpc.Generator", 
                DiagnosticSeverity.Info, 
                isEnabledByDefault: true), 
            location: null));
    }

    public static void LogWarning(this GeneratorExecutionContext context, string message)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                id: "GEN002", 
                title: "Generator Warning", 
                messageFormat: message, 
                category: "SatelliteRpc.Generator", 
                DiagnosticSeverity.Warning, 
                isEnabledByDefault: true), 
            location: null));
    }

    public static void LogError(this GeneratorExecutionContext context, string message)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            new DiagnosticDescriptor(
                id: "GEN003", 
                title: "Generator Error", 
                messageFormat: message, 
                category: "SatelliteRpc.Generator", 
                DiagnosticSeverity.Error, 
                isEnabledByDefault: true), 
            location: null));
    }
}