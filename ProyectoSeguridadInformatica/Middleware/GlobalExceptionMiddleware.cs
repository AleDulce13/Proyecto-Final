using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ProyectoSeguridadInformatica.Middleware
{
    /// <summary>
    /// Middleware global que captura excepciones no controladas,
    /// las escribe en consola y redirige al usuario a la página de error.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log sencillo a consola con información del fallo
                Console.WriteLine(
                    $"[{DateTime.UtcNow:O}] ERROR en {context.Request.Method} {context.Request.Path}\n" +
                    $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");

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


