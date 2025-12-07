using System.Net.Http.Json;
using System.Text.Json;

namespace ProyectoSeguridadInformatica.Services
{
    public class FirebaseAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public FirebaseAuthService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Firebase:ApiKey"] ?? string.Empty;
        }

        public async Task<AuthResponse?> RegisterAsync(string email, string password)
        {
            var payload = new
            {
                email,
                password,
                returnSecureToken = true
            };

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }

        public async Task<AuthResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new
                {
                    email,
                    password,
                    returnSecureToken = true
                };

                var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";
                var response = await _httpClient.PostAsJsonAsync(url, payload);

                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<AuthResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }
    }

}

public class AuthResponse
{
    public string IdToken { get; set; } = "";
    public string Email { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string LocalId { get; set; } = "";   // UID REAL
}

