using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoSeguridadInformatica.Models;
using ProyectoSeguridadInformatica.Services;

namespace ProyectoSeguridadInformatica.Controllers
{
    [Authorize]
    public class CryptoController : Controller
    {
        private readonly AesService _aesService;

        public CryptoController(AesService aesService)
        {
            _aesService = aesService;
        }
        
        [Authorize]
        [HttpGet]
        public IActionResult Encrypt()
        {
            return View(new EncryptViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Encrypt(EncryptViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.CipherText = _aesService.Encrypt(model.PlainText, model.Key);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "No se pudo cifrar el texto. Revisa la entrada e inténtalo de nuevo.");
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Decrypt()
        {
            return View(new DecryptViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decrypt(DecryptViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                model.PlainText = _aesService.Decrypt(model.CipherText, model.Key);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "No se pudo desencriptar el texto. Verifica que el Base64 y la clave sean correctos.");
            }

            return View(model);
        }
    }
}


