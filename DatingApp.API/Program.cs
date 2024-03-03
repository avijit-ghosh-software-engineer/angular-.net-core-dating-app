using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContextPool<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));
builder.Services.AddSingleton(builder.Configuration.GetSection("Images").Get<ImagesPathSettings>());
builder.Services.AddControllers();
builder.Services.AddCors();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


IMapper mapper = AutoMapperProfiles.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddTransient<Seed>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddDependency();
builder.Services.AddScoped<LogUserActivity>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DatingApp API");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler(bulder =>
    {
        bulder.Run(async context =>
        {
            context.Response.StatusCode =(int)HttpStatusCode.InternalServerError;
            var error = context.Features.Get<IExceptionHandlerFeature>();
            if(error != null)
            {
                context.Response.AddApplicationError(error.Error.Message);
                await context.Response.WriteAsync(error.Error.Message);
            }
        });
    });
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.Run();

void ApplyMigration()
{
    using(var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<DataContext>();

        if(_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}