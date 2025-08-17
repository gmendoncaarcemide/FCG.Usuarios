using FCG.Usuarios.Application.Usuarios.Interfaces;
using FCG.Usuarios.Application.Usuarios.ViewModels;
using FCG.Usuarios.Domain.Usuarios.Entities;
using FCG.Usuarios.Domain.Usuarios.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FCG.Usuarios.Application.Usuarios.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public UsuarioService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    public async Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request)
    {
        var usuarioExistente = await _usuarioRepository.ObterPorEmailAsync(request.Email);
        if (usuarioExistente != null)
            throw new InvalidOperationException($"Usuário com email {request.Email} já existe");

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            TipoUsuario = request.TipoUsuario,
            Telefone = request.Telefone,
            DataNascimento = request.DataNascimento,
            Endereco = request.Endereco
        };

        var usuarioCriado = await _usuarioRepository.AdicionarAsync(usuario);
        return MapearParaResponse(usuarioCriado);
    }

    public async Task<UsuarioResponse?> ObterPorIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id);
        return usuario != null ? MapearParaResponse(usuario) : null;
    }

    public async Task<UsuarioResponse?> ObterPorEmailAsync(string email)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
        return usuario != null ? MapearParaResponse(usuario) : null;
    }

    public async Task<IEnumerable<UsuarioResponse>> ObterTodosAsync()
    {
        var usuarios = await _usuarioRepository.ObterTodosAsync();
        return usuarios.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<UsuarioResponse>> ObterPorTipoAsync(TipoUsuario tipoUsuario)
    {
        var usuarios = await _usuarioRepository.ObterPorTipoAsync(tipoUsuario);
        return usuarios.Select(MapearParaResponse);
    }

    public async Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id);
        if (usuario == null)
            throw new InvalidOperationException($"Usuário com ID {id} não encontrado");

        if (!string.IsNullOrEmpty(request.Nome))
            usuario.Nome = request.Nome;

        if (!string.IsNullOrEmpty(request.Email) && request.Email != usuario.Email)
        {
            var usuarioComEmail = await _usuarioRepository.ObterPorEmailAsync(request.Email);
            if (usuarioComEmail != null && usuarioComEmail.Id != id)
                throw new InvalidOperationException($"Email {request.Email} já está em uso");

            usuario.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.Senha))
            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha);

        if (request.TipoUsuario.HasValue)
            usuario.TipoUsuario = request.TipoUsuario.Value;

        if (request.Telefone != null)
            usuario.Telefone = request.Telefone;

        if (request.DataNascimento.HasValue)
            usuario.DataNascimento = request.DataNascimento.Value;

        if (request.Endereco != null)
            usuario.Endereco = request.Endereco;

        usuario.DataAtualizacao = DateTime.UtcNow;

        var usuarioAtualizado = await _usuarioRepository.AtualizarAsync(usuario);
        return MapearParaResponse(usuarioAtualizado);
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        return await _usuarioRepository.ExcluirAsync(id);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(request.Email);
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
            throw new InvalidOperationException("Email ou senha inválidos");

        var token = GerarJwtToken(usuario);
        var dataExpiracao = DateTime.UtcNow.AddHours(24);

        return new LoginResponse
        {
            Token = token,
            Usuario = MapearParaResponse(usuario),
            DataExpiracao = dataExpiracao
        };
    }

    public async Task<bool> AlterarSenhaAsync(Guid id, string senhaAtual, string novaSenha)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(id);
        if (usuario == null)
            return false;

        if (!BCrypt.Net.BCrypt.Verify(senhaAtual, usuario.Senha))
            return false;

        usuario.Senha = BCrypt.Net.BCrypt.HashPassword(novaSenha);
        usuario.DataAtualizacao = DateTime.UtcNow;

        await _usuarioRepository.AtualizarAsync(usuario);
        return true;
    }

    public async Task<bool> VerificarEmailAsync(string email)
    {
        var usuario = await _usuarioRepository.ObterPorEmailAsync(email);
        return usuario != null;
    }

    private string GerarJwtToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Jwt:Key").Value!);
        var expires = double.Parse(_configuration.GetSection("Jwt:ExpiryInHours").Value!);
        var audience = _configuration.GetSection("Jwt:Audience").Value!;
        var issuer = _configuration.GetSection("Jwt:Issuer").Value!;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(expires),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Audience = audience,
            Issuer = issuer
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static UsuarioResponse MapearParaResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            TipoUsuario = usuario.TipoUsuario,
            Telefone = usuario.Telefone,
            DataNascimento = usuario.DataNascimento,
            Endereco = usuario.Endereco,
            DataCriacao = usuario.DataCriacao,
            DataAtualizacao = usuario.DataAtualizacao
        };
    }
} 