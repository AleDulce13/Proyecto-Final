using Microsoft.AspNetCore.Mvc;
using ProyectoSeguridadInformatica.Models;
using ProyectoSeguridadInformatica.Services;
using System.Security.Cryptography;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace ProyectoSeguridadInformatica.Controllers
{
    public class AccountController : Controller
    {
        private readonly FirebaseAuthService _authService;
        private readonly FirebaseUserService _userService;

        public AccountController(
            FirebaseAuthService authService,
            FirebaseUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        // ==========================
        //     VISTA REGISTRO
        // ==========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ==========================
        //     POST REGISTRO
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var auth = await _authService.RegisterAsync(model.Email, model.Password);

            if (auth == null)
            {
                ModelState.AddModelError("", "No se pudo registrar el usuario.");
                return View(model);
            }

            var user = new User
            {
                Id = auth.LocalId,
                Email = auth.Email
            };

            await _userService.CreateUserAsync(user, auth.IdToken);

            SignInUser(auth);
            

            return RedirectToAction("Index", "Home");
        }

        // ==========================
        //     VISTA LOGIN
        // ==========================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // ==========================
        //     POST LOGIN
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var auth = await _authService.LoginAsync(model.Email, model.Password);

            if (auth == null)
            {
                ModelState.AddModelError("", "Credenciales inválidas.");
                return View(model);
            }

            SignInUser(auth);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ==========================
        //     LOGOUT
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private void SignInUser(AuthResponse auth)
        {
            HttpContext.Session.SetString("UserId", auth.LocalId);
            HttpContext.Session.SetString("UserEmail", auth.Email);
            HttpContext.Session.SetString("IdToken", auth.IdToken);
            HttpContext.Session.SetString("RefreshToken", auth.RefreshToken);
        }
    }
}


