using System.ComponentModel.DataAnnotations;
using FCG.Usuarios.Application.Usuarios.Interfaces;
using FCG.Usuarios.Application.Usuarios.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Usuarios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
	private readonly IUsuarioService _service;
	public UsuariosController(IUsuarioService service)
	{
		_service = service;
	}

	[HttpPost]
	[AllowAnonymous]
	[ProducesResponseType(typeof(object), 201)]
	[ProducesResponseType(typeof(IEnumerable<ValidationResult>), 400)]
	[ProducesResponseType(409)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> CriarUsuario([FromBody] CriarUsuarioRequest request)
	{
		var validationContext = new ValidationContext(request);
		var validationResults = new List<ValidationResult>();
		if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
			return BadRequest(validationResults);
		try
		{
			var usuario = await _service.CriarAsync(request);
			return Created($"/api/usuarios/{usuario.Id}", usuario);
		}
		catch (InvalidOperationException ex)
		{
			return Conflict(ex.Message);
		}
		catch (Exception ex)
		{
			return Problem($"Erro interno: {ex.Message}");
		}
	}

	[HttpPost("login")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(object), 200)]
	[ProducesResponseType(typeof(IEnumerable<ValidationResult>), 400)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		var validationContext = new ValidationContext(request);
		var validationResults = new List<ValidationResult>();
		if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
			return BadRequest(validationResults);
		try
		{
			var response = await _service.LoginAsync(request);
			return Ok(response);
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(ex.Message);
		}
		catch (Exception ex)
		{
			return Problem($"Erro interno: {ex.Message}");
		}
	}

	[HttpGet]
	[Authorize(Policy = "Admin")]
	[ProducesResponseType(typeof(IEnumerable<object>), 200)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> ObterTodos()
	{
		try
		{
			var usuarios = await _service.ObterTodosAsync();
			return Ok(usuarios);
		}
		catch (Exception ex)
		{
			return Problem($"Erro interno: {ex.Message}");
		}
	}

	[HttpGet("{id:guid}")]
	[Authorize(Policy = "Admin")]
	[ProducesResponseType(typeof(object), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> ObterPorId(Guid id)
	{
		try
		{
			var usuario = await _service.ObterPorIdAsync(id);
			if (usuario == null)
				return NotFound($"Usuário com ID {id} não encontrado");
			return Ok(usuario);
		}
		catch (Exception ex)
		{
			return Problem($"Erro interno: {ex.Message}");
		}
	}

	[HttpGet("email/{email}")]
	[Authorize(Policy = "Admin")]
	[ProducesResponseType(typeof(object), 200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> ObterPorEmail(string email)
	{
		try
		{
			var usuario = await _service.ObterPorEmailAsync(email);
			if (usuario == null)
				return NotFound($"Usuário com email {email} não encontrado");
			return Ok(usuario);
		}
		catch (Exception ex)
		{
			return Problem($"Erro interno: {ex.Message}");
		}
	}

	[HttpPut("{id:guid}")]
	[Authorize(Policy = "Admin")]
	[ProducesResponseType(typeof(object), 200)]
	[ProducesResponseType(typeof(IEnumerable<ValidationResult>), 400)]
	[ProducesResponseType(404)]
	[ProducesResponseType(409)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarUsuarioRequest request)
	{
		var validationContext = new ValidationContext(request);
		var validationResults = new List<ValidationResult>();
		if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
			return BadRequest(validationResults);
		try
		{
			var usuario = await _service.AtualizarAsync(id, request);
			return Ok(usuario);
		}
		catch (InvalidOperationException ex)
		{
			if (ex.Message.Contains("não encontrado"))
				return NotFound(ex.Message);
			if (ex.Message.Contains("já está em uso"))
				return Conflict(ex.Message);
			return BadRequest(ex.Message);
		}
		catch (Exception ex)
		{
			return Problem($"Erro interno: {ex.Message}");
		}
	}

	[HttpDelete("{id:guid}")]
	[Authorize(Policy = "Admin")]
	[ProducesResponseType(200)]
	[ProducesResponseType(404)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> Excluir(Guid id)
	{
		try
		{
			var sucesso = await _service.ExcluirAsync(id);
			if (!sucesso)
				return NotFound($"Usuário com ID {id} não encontrado ou já foi excluído");
			return Ok($"Usuário com ID {id} foi desativado com sucesso");
		}
		catch (Exception ex)
		{
			return Problem($"Erro interno: {ex.Message}");
		}
	}

	[HttpPost("{id:guid}/alterar-senha")]
	[Authorize(Policy = "Admin")]
	[ProducesResponseType(200)]
	[ProducesResponseType(typeof(IEnumerable<ValidationResult>), 400)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> AlterarSenha(Guid id, [FromBody] AlterarSenhaRequest request)
	{
		var validationContext = new ValidationContext(request);
		var validationResults = new List<ValidationResult>();
		if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
			return BadRequest(validationResults);
		try
		{
			var sucesso = await _service.AlterarSenhaAsync(id, request.SenhaAtual, request.NovaSenha);
			if (!sucesso)
				return BadRequest("Senha atual incorreta ou usuário não encontrado");
			return Ok("Senha alterada com sucesso");
		}
		catch (Exception ex)
		{
			return Problem($"Erro interno: {ex.Message}");
		}
	}

	[HttpGet("verificar-email/{email}")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(object), 200)]
	[ProducesResponseType(500)]
	public async Task<IActionResult> VerificarEmail(string email)
	{
		try
		{
			var existe = await _service.VerificarEmailAsync(email);
			return Ok(new { Email = email, Existe = existe });
		}
		catch (Exception ex)
		{
			return Problem($"Erro interno: {ex.Message}");
		}
	}
}
