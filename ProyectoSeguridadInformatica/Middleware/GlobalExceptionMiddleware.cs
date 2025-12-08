using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ProyectoSeguridadInformatica.Middleware
{
    /// <summary>
    /// Middleware global que captura excepciones no controladas
    /// y redirige al usuario a la página de error.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en {Method} {Path}", context.Request.Method, context.Request.Path);

                if (!context.Response.HasStarted)
                {
                    context.Response.Clear();
                    context.Response.Redirect("/Home/Error");
                    // Ya hemos manejado la respuesta, no volvemos a lanzar la excepción.
                    return;
                }

                // Si la respuesta ya empezó, no podemos redirigir limpiamente,
                // dejamos que la excepción siga su curso normal.
                throw;
            }
        }
    }
}


