using consumirAPI.DTO;
using consumirAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace consumirAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // GET: Exibe a página de login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Faz o login e obtém o token JWT
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }

            var client = _clientFactory.CreateClient("APIClient");
            var response = await client.PostAsJsonAsync("api/auth/login", loginViewModel); // Substitua pela rota correta de login da sua API

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                var token = loginResponse?.Token;


                // Armazenar o token JWT em uma sessão ou cookie para ser usado nas requisições subsequentes
                HttpContext.Session.SetString("JWToken", token);

                // Redirecionar para a página de listagem de produtos
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Login falhou. Verifique suas credenciais.");
                return View(loginViewModel);
            }
        }

        // Exemplo de como usar o token para acessar rotas privadas
        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("APIClient");

            // Adicionar o token JWT no cabeçalho das requisições
            var token = HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            List<Produto> produtos = null;

            HttpResponseMessage response = await client.GetAsync("api/produtos"); // Rota protegida da API

            if (response.IsSuccessStatusCode)
            {
                produtos = await response.Content.ReadFromJsonAsync<List<Produto>>();
            }

            return View(produtos);
        }
    }
}
