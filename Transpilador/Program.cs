using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;
using Transpilador.Errors;
using Transpilador.Parser;
using Transpilador.Generator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestMethod
                          | HttpLoggingFields.RequestPath
                          | HttpLoggingFields.RequestHeaders
                          | HttpLoggingFields.ResponseStatusCode
                          | HttpLoggingFields.Duration;
    logging.RequestHeaders.Add("Origin");
    logging.RequestHeaders.Add("Content-Type");
    logging.CombineLogs = true;
});

var app = builder.Build();

var logger = app.Logger;

// ── Middleware global de errores (solo errores inesperados → 500) ───────────
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature   = context.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;
        var log       = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var traceId   = context.TraceIdentifier;

        log.LogError(exception, "[ExceptionHandler] Error inesperado. TraceId: {TraceId}", traceId);

        context.Response.StatusCode  = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(
            new ApiError("internal", "Error interno del servidor.", null, traceId)
        );
    });
});

app.UseHttpLogging();
app.UseCors("ReactApp");

logger.LogInformation("=== Transpilador API iniciada ===");
logger.LogInformation("Escuchando en: {Urls}", string.Join(", ", builder.WebHost.GetSetting("urls") ?? "http://localhost:5000"));
logger.LogInformation("CORS habilitado para: {Origin}", builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173");
logger.LogInformation("Endpoints disponibles:");
logger.LogInformation("  POST /api/transpile       → body JSON {{ \"code\": \"...\" }}");
logger.LogInformation("  POST /api/transpile/file  → multipart/form-data campo 'file'");

// ── POST /api/transpile ─────────────────────────────────────────────────────
app.MapPost("/api/transpile", (TranspileRequest request, ILogger<Program> log) =>
{
    if (string.IsNullOrWhiteSpace(request.Code))
    {
        log.LogWarning("[/api/transpile] Rechazada: el campo 'code' está vacío");
        return Results.BadRequest(new ApiError("validation", "El campo 'code' es requerido."));
    }

    int lines = request.Code.Split('\n').Length;
    log.LogInformation("[/api/transpile] Transpilando código ({Lines} líneas)...", lines);

    try
    {
        var parser    = new CSharpParser();
        var ir        = parser.ParseToIR(request.Code);
        var generator = new JavaGenerator();
        var javaCode  = generator.GenerateJava(ir);

        log.LogInformation("[/api/transpile] Éxito → {OutputLines} líneas de Java generadas", javaCode.Split('\n').Length);
        return Results.Ok(new { javaCode });
    }
    catch (TranspileException ex)
    {
        log.LogWarning("[/api/transpile] {Type}: {Message}", ex.ErrorType, ex.Message);
        return Results.Json(new ApiError(ex.ErrorType, ex.Message, ex.Details),
            statusCode: ex.ErrorType == "no_classes"
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status422UnprocessableEntity);
    }
    catch (NotSupportedException ex)
    {
        log.LogWarning("[/api/transpile] Construcción no soportada: {Message}", ex.Message);
        return Results.Json(
            new ApiError("unsupported", "Construcción de C# no soportada por el transpilador.",
                [new ErrorDetail("UNSUPPORTED", ex.Message)]),
            statusCode: StatusCodes.Status422UnprocessableEntity);
    }
});

// ── POST /api/transpile/file ────────────────────────────────────────────────
app.MapPost("/api/transpile/file", async (IFormFile file, ILogger<Program> log) =>
{
    if (file is null || file.Length == 0)
    {
        log.LogWarning("[/api/transpile/file] Rechazada: no se envió ningún archivo");
        return Results.BadRequest(new ApiError("validation", "No se proporcionó ningún archivo."));
    }

    if (!file.FileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
    {
        log.LogWarning("[/api/transpile/file] Rechazada: extensión inválida '{Ext}'", Path.GetExtension(file.FileName));
        return Results.Json(
            new ApiError("validation", $"El archivo debe tener extensión .cs. Se recibió: '{file.FileName}'."),
            statusCode: StatusCodes.Status415UnsupportedMediaType
        );
    }

    const long maxBytes = 1 * 1024 * 1024;
    if (file.Length > maxBytes)
    {
        log.LogWarning("[/api/transpile/file] Rechazada: archivo demasiado grande ({Size} bytes)", file.Length);
        return Results.Json(
            new ApiError("validation", "El archivo supera el tamaño máximo permitido de 1 MB."),
            statusCode: StatusCodes.Status413RequestEntityTooLarge
        );
    }

    log.LogInformation("[/api/transpile/file] Archivo recibido: '{Name}' ({Size} bytes)", file.FileName, file.Length);

    using var reader = new StreamReader(file.OpenReadStream());
    var code = await reader.ReadToEndAsync();

    try
    {
        var parser    = new CSharpParser();
        var ir        = parser.ParseToIR(code);
        var generator = new JavaGenerator();
        var javaCode  = generator.GenerateJava(ir);
        var outputFileName = Path.ChangeExtension(file.FileName, ".java");

        log.LogInformation("[/api/transpile/file] Éxito → '{Output}' ({OutputLines} líneas de Java generadas)", outputFileName, javaCode.Split('\n').Length);
        return Results.Ok(new { javaCode, fileName = outputFileName });
    }
    catch (TranspileException ex)
    {
        log.LogWarning("[/api/transpile/file] {Type}: {Message}", ex.ErrorType, ex.Message);
        return Results.Json(new ApiError(ex.ErrorType, ex.Message, ex.Details),
            statusCode: ex.ErrorType == "no_classes"
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status422UnprocessableEntity);
    }
    catch (NotSupportedException ex)
    {
        log.LogWarning("[/api/transpile/file] Construcción no soportada: {Message}", ex.Message);
        return Results.Json(
            new ApiError("unsupported", "Construcción de C# no soportada por el transpilador.",
                [new ErrorDetail("UNSUPPORTED", ex.Message)]),
            statusCode: StatusCodes.Status422UnprocessableEntity);
    }
}).DisableAntiforgery();

app.Run();

record TranspileRequest(string Code);