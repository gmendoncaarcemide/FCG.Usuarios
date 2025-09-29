using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using FCG.Usuarios.Application.Usuarios.Services;
using FCG.Usuarios.Domain.Usuarios.Interfaces;
using FCG.Usuarios.Domain.Usuarios.Entities;
using FCG.Usuarios.Application.Usuarios.ViewModels;

namespace FCG.Usuarios.Tests
{
    public class UsuariosServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly UsuarioService _usuarioService;

        public UsuariosServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c.GetSection("Jwt:Key").Value).Returns("super_secret_key_1234567890_abcdefg");
            _configurationMock.Setup(c => c.GetSection("Jwt:ExpiryInHours").Value).Returns("24");
            _configurationMock.Setup(c => c.GetSection("Jwt:Audience").Value).Returns("test_audience");
            _configurationMock.Setup(c => c.GetSection("Jwt:Issuer").Value).Returns("test_issuer");
            _usuarioService = new UsuarioService(_usuarioRepositoryMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task CriarAsync_DeveCriarUsuario_QuandoEmailNaoExiste()
        {
            var request = new CriarUsuarioRequest
            {
                Nome = "Teste",
                Email = "teste@email.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Usuario
            };
            _usuarioRepositoryMock.Setup(r => r.ObterPorEmailAsync(request.Email)).ReturnsAsync((Usuario?)null);
            _usuarioRepositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>()))
                .ReturnsAsync((Usuario u) => { u.Id = Guid.NewGuid(); u.DataCriacao = DateTime.UtcNow; return u; });

            var result = await _usuarioService.CriarAsync(request);

            Assert.Equal(request.Nome, result.Nome);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal(request.TipoUsuario, result.TipoUsuario);
            _usuarioRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task CriarAsync_DeveLancarExcecao_QuandoEmailJaExiste()
        {
            var request = new CriarUsuarioRequest
            {
                Nome = "Teste",
                Email = "teste@email.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Usuario
            };
            _usuarioRepositoryMock.Setup(r => r.ObterPorEmailAsync(request.Email)).ReturnsAsync(new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                Senha = request.Senha,
                TipoUsuario = request.TipoUsuario
            });

            await Assert.ThrowsAsync<InvalidOperationException>(() => _usuarioService.CriarAsync(request));
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarUsuarioResponse_QuandoUsuarioExiste()
        {
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = "Teste",
                Email = "teste@email.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Usuario,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            };
            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(usuario.Id)).ReturnsAsync(usuario);

            var result = await _usuarioService.ObterPorIdAsync(usuario.Id);

            Assert.NotNull(result);
            Assert.Equal(usuario.Id, result.Id);
        }

        [Fact]
        public async Task ObterPorIdAsync_DeveRetornarNull_QuandoUsuarioNaoExiste()
        {
            var id = Guid.NewGuid();
            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Usuario?)null);

            var result = await _usuarioService.ObterPorIdAsync(id);

            Assert.Null(result);
        }

        [Fact]
        public async Task ExcluirAsync_DeveRetornarFalse_QuandoUsuarioNaoExiste()
        {
            var id = Guid.NewGuid();
            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Usuario?)null);

            var result = await _usuarioService.ExcluirAsync(id);

            Assert.False(result);
        }

        [Fact]
        public async Task ExcluirAsync_DeveRetornarTrue_QuandoUsuarioExisteEAtivo()
        {
            var id = Guid.NewGuid();
            var usuario = new Usuario
            {
                Id = id,
                Nome = "Teste",
                Email = "teste@email.com",
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Usuario,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.ExcluirAsync(id)).ReturnsAsync(true);

            var result = await _usuarioService.ExcluirAsync(id);

            Assert.True(result);
        }

        [Fact]
        public async Task LoginAsync_DeveRetornarToken_QuandoCredenciaisValidas()
        {
            var senha = "senha123";
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = "Teste",
                Email = "teste@email.com",
                Senha = senhaHash,
                TipoUsuario = TipoUsuario.Usuario,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            var request = new LoginRequest
            {
                Email = usuario.Email,
                Senha = senha
            };
            _usuarioRepositoryMock.Setup(r => r.ObterPorEmailAsync(usuario.Email)).ReturnsAsync(usuario);

            var result = await _usuarioService.LoginAsync(request);

            Assert.NotNull(result.Token);
            Assert.Equal(usuario.Email, result.Usuario.Email);
        }

        [Fact]
        public async Task LoginAsync_DeveLancarExcecao_QuandoCredenciaisInvalidas()
        {
            var request = new LoginRequest
            {
                Email = "naoexiste@email.com",
                Senha = "senha"
            };
            _usuarioRepositoryMock.Setup(r => r.ObterPorEmailAsync(request.Email)).ReturnsAsync((Usuario?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _usuarioService.LoginAsync(request));
        }

        [Fact]
        public async Task AlterarSenhaAsync_DeveRetornarFalse_QuandoUsuarioNaoExiste()
        {
            var id = Guid.NewGuid();
            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Usuario?)null);

            var result = await _usuarioService.AlterarSenhaAsync(id, "senhaAtual", "novaSenha");

            Assert.False(result);
        }

        [Fact]
        public async Task AlterarSenhaAsync_DeveRetornarFalse_QuandoSenhaAtualIncorreta()
        {
            var id = Guid.NewGuid();
            var usuario = new Usuario
            {
                Id = id,
                Nome = "Teste",
                Email = "teste@email.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("senhaCorreta"),
                TipoUsuario = TipoUsuario.Usuario,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);

            var result = await _usuarioService.AlterarSenhaAsync(id, "senhaErrada", "novaSenha");

            Assert.False(result);
        }

        [Fact]
        public async Task AlterarSenhaAsync_DeveRetornarTrue_QuandoSenhaAtualCorreta()
        {
            var id = Guid.NewGuid();
            var senhaAtual = "senhaCorreta";
            var usuario = new Usuario
            {
                Id = id,
                Nome = "Teste",
                Email = "teste@email.com",
                Senha = BCrypt.Net.BCrypt.HashPassword(senhaAtual),
                TipoUsuario = TipoUsuario.Usuario,
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };
            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.AtualizarAsync(usuario)).ReturnsAsync(usuario);

            var result = await _usuarioService.AlterarSenhaAsync(id, senhaAtual, "novaSenha");

            Assert.True(result);
        }

        [Fact]
        public async Task VerificarEmailAsync_DeveRetornarTrue_QuandoUsuarioExiste()
        {
            var email = "teste@email.com";
            _usuarioRepositoryMock.Setup(r => r.ObterPorEmailAsync(email)).ReturnsAsync(new Usuario
            {
                Nome = "Teste",
                Email = email,
                Senha = "senha123",
                TipoUsuario = TipoUsuario.Usuario
            });

            var result = await _usuarioService.VerificarEmailAsync(email);

            Assert.True(result);
        }

        [Fact]
        public async Task VerificarEmailAsync_DeveRetornarFalse_QuandoUsuarioNaoExiste()
        {
            var email = "naoexiste@email.com";
            _usuarioRepositoryMock.Setup(r => r.ObterPorEmailAsync(email)).ReturnsAsync((Usuario?)null);

            var result = await _usuarioService.VerificarEmailAsync(email);

            Assert.False(result);
        }
    }
}