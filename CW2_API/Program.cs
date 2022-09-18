using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<UsuarioDb>(opt => opt.UseInMemoryDatabase("UsuarioList"));
var app = builder.Build();

app.MapGet("/usuarios", async (UsuarioDb db) =>
    await db.Usuarios.Select(x => new UsuarioDTO(x)).ToListAsync());

app.MapGet("/usuarios/{id}", async (int id, UsuarioDb db) =>
    await db.Usuarios.FindAsync(id)
        is Usuario usuario
            ? Results.Ok(new UsuarioDTO(usuario))
            : Results.NotFound());

app.MapPost("/login", async (Login login, UsuarioDb db) =>
{
    var usuario = db.Usuarios.FirstOrDefault(u => u.login == login.login && u.Senha == login.senha);
    if (usuario is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new UsuarioDTO(usuario));
});

app.MapPost("/usuarios", async (Usuario usuario, UsuarioDb db) =>
{
    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();

    return Results.Created($"/usuarios/{usuario.Id}", new UsuarioDTO(usuario));
});

app.MapPut("/usuarios/{id}", async (int id, UsuarioDTO usuarioDTO, UsuarioDb db) =>
{
    var usuario = await db.Usuarios.FindAsync(id);

    if (usuario is null) return Results.NotFound();

    usuario.Nome = usuarioDTO.Nome;
    usuario.Ativo = usuarioDTO.Ativo;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/usuarios/{id}", async (int id, UsuarioDb db) =>
{
    if (await db.Usuarios.FindAsync(id) is Usuario usuario)
    {
        db.Usuarios.Remove(usuario);
        await db.SaveChangesAsync();
        return Results.Ok(new UsuarioDTO(usuario));
    }

    return Results.NotFound();
});

app.Run();

public class Usuario
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? login { get; set; }
    public bool Ativo { get; set; }
    public string? Senha { get; set; }
    public string? Email { get; set; }
}

public class Login
{
    public string? login { get; set; }
    public string? senha { get; set; }
}

public class UsuarioDTO
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? login { get; set; }
    public bool Ativo { get; set; }
    public string? Email { get; set; }

    public UsuarioDTO() { }
    public UsuarioDTO(Usuario usuario) =>
    (Id, Nome, Ativo, Email, login) = (usuario.Id, usuario.Nome, usuario.Ativo, usuario.Email, usuario.login);
}


class UsuarioDb : DbContext
{
    public UsuarioDb(DbContextOptions<UsuarioDb> options)
        : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
}