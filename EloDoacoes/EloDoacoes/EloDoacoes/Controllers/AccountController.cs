using EloDoacoes.Data;
using EloDoacoes.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly EloDoacoesContext _context;
    // Consider injecting ILogger<AccountController> for better error logging in the future.

    public AccountController(EloDoacoesContext context)
    {
        _context = context;
    }

    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == model.Email);

            // Generic error message to avoid revealing whether the email exists
            const string loginError = "E-mail ou senha incorretos.";

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, loginError);
                return View(model);
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                authProperties);

            // If a returnUrl was provided (user was redirected to login), send them back to it.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            // Log exception minimally and present a friendly message
            // (controller doesn't have logger injected; in production add ILogger)
            ModelState.AddModelError(string.Empty, "Ocorreu um erro no servidor. Tente novamente mais tarde.");
            return View(model);
        }
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(model);

            // Normalize email to lower-case to enforce uniqueness correctly
            var normalizedEmail = model.Email?.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(x => x.Email.ToLower() == normalizedEmail))
            {
                // Provide clearer feedback and suggest login if account exists
                ModelState.AddModelError(string.Empty, "O e-mail já está cadastrado. Se for seu, tente fazer login.");
                return View(model);
            }

            User user = new User()
            {
                Name = model.Name?.Trim(),
                Email = normalizedEmail,
                Phone = model.Phone?.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RegistrationDate = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cadastro realizado com sucesso. Você já pode entrar no sistema.";

            return RedirectToAction(nameof(Login));
        }
        catch (DbUpdateException)
        {
            // Handle potential unique constraint race condition
            ModelState.AddModelError(string.Empty, "Não foi possível concluir o cadastro. O e-mail pode já estar em uso.");
            return View(model);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Ocorreu um erro no servidor. Tente novamente mais tarde.");
            return View(model);
        }
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        return RedirectToAction(nameof(Login));
    }
}