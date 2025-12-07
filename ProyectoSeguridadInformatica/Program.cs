using ProyectoSeguridadInformatica.Models;
using ProyectoSeguridadInformatica.Services;

namespace ProyectoSeguridadInformatica
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.Configure<FirebaseOptions>(builder.Configuration.GetSection("Firebase"));
            builder.Services.Configure<RsaOptions>(builder.Configuration.GetSection("Rsa"));

            builder.Services.AddHttpClient<FirebaseAuthService>();
            builder.Services.AddHttpClient<FirebaseUserService>();

            builder.Services.AddHttpClient<FirebaseUserService>();
            builder.Services.AddSingleton<RsaService>();
            builder.Services.AddSession();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseDeveloperExceptionPage();


            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
