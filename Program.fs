open System
open System.Net.Http
open System.Xml.Xsl
open Sentry

// Some operation that may fail with an exception.
// Sentry SDK will pick up unhandled exceptions automatically.
let performDangerousOperation () = raise <| Exception("Expected exception in Sentry F# Demo."); ()

[<EntryPoint>]
let main _ =
    // ---------------------
    // *-- SDK configuration
    // ---------------------

    // Initialize and configure the SDK.
    // (returns a disposable which is responsible for flushing queued events)
    use __ = SentrySdk.Init (fun o ->
        // Set the project's DSN.
        // Also can be set through environment (SENTRY_DSN), appsettings.json, assembly attributes, etc.
        o.Dsn <- "https://80aed643f81249d4bed3e30687b310ab@o447951.ingest.sentry.io/5428537"

        // Optional: attach stack traces even for events without errors.
        o.AttachStacktrace <- true

        // Optional: hide code from specified libraries in stack traces to reduce noise.
        o.AddInAppExclude ("LibraryX.") // wildcard

        // Optional: always include code from the specified libraries in stack traces.
        o.AddInAppInclude ("LibraryY")

        // Optional: send personal identifiable information like the username logged on to the computer and machine name.
        o.SendDefaultPii <- true

        // Optional: event sampling.
        // o.SampleRate <- Nullable 0.5f

        // Optional: event transformation hook.
        o.BeforeSend <- (fun event -> event)

        // Optional: breadcrumb transformation hook.
        o.BeforeBreadcrumb <- (fun breadcrumb -> breadcrumb)

        // Optional: ignore specific exceptions.
        o.AddExceptionFilterForType<XsltCompileException> ()

        // Optional: configure the max amount of time to wait for all events to flush on shutdown.
        o.ShutdownTimeout <- TimeSpan.FromSeconds 10.0

        // Optional: HTTP proxy.
        o.HttpProxy <- null

        // Optional: customize HttpClientHandler used by Sentry SDK.
        o.CreateHttpClientHandler <- (fun () -> new HttpClientHandler())

        // Optional: enable debug-level SDK logging for troubleshooting.
        o.Debug <- true

        // Optional: change logging verbosity.
        // o.DiagnosticsLevel <- SentryLevel.Info

        // Optional: use a custom logger.
        // o.DiagnosticLogger <- ...
        ())

    // -----------
    // *---- Usage
    // -----------

    // Optional: add breadcrumb to the next event.
    SentrySdk.AddBreadcrumb ("Checkout Page")

    // Capture an exception directly.
    SentrySdk.CaptureException <| Exception ("Uh oh.") |> ignore

    // Optional: configure event scope.
    SentrySdk.WithScope (fun s ->
        s.Level <- Nullable SentryLevel.Fatal
        s.Transaction <- "main"
        s.Environment <- "staging"

        // Capture a scoped message.
        SentrySdk.CaptureMessage "Something went wrong..." |> ignore
        ())

    // Optional: manually flush the event queue with a timeout.
    SentrySdk.FlushAsync (TimeSpan.FromSeconds 10.0) |> Async.AwaitTask |> Async.RunSynchronously

    // Invoke some code that may trigger an exception.
    // Unhandled exceptions are automatically collected by Sentry.
    performDangerousOperation ()

    0
