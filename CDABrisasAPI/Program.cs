using Application.Abstractions;
using Application.Abstractions.Interfaces.CommandHandler;
using Application.Abstractions.Interfaces.Dispatchers;
using Application.Abstractions.Interfaces.Dispatchers.Interfaz;
using Application.Abstractions.Interfaces.QueryHandler;
using Application.Manychat.Command;
using Application.Manychat.CommandHandler;
using Application.Messages.CommandHandler;
using Application.Messages.Commands;
using Application.Messages.Queries;
using Application.Messages.QueryHandler;
using Application.SystemUsers.CommandHandler;
using Application.SystemUsers.Commands;
using Application.SystemUsers.Queries;
using Application.SystemUsers.QueryHandler;
using Application.Users.CommandHandler;
using Application.Users.Commands;
using Application.Users.Queries;
using Application.Users.QueryHandler;
using Application.Utilities;
using Application.Utilities.Dtos;
using Application.Utilities.Interfaces;
using DataAccess;
using DataAccess.Repository;
using Domain.Dto;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ✅ Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("allowCors", corsBuilder =>
    {
        corsBuilder
            .WithOrigins(
                // Local
                "https://localhost:7124",
                "http://localhost:4200",
                "http://localhost:4300",

                // Frontend Angular en Azure
                "https://tecnoyaweb-dxfqasf3awcvega2.westus3-01.azurewebsites.net",

                // API (opcional)
                "https://tecnoyaapi-djdjguaccbb6g0fw.brazilsouth-01.azurewebsites.net"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        // ❌ NO AllowCredentials
    });
});



Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();


builder.Services.AddDbContext<CDABrisasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<MetaSettings>(builder.Configuration.GetSection("Meta"));

builder.Services.AddScoped<IDispatcher, Dispatcher>();

// WS Users
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQueryHandler<GetAllUsersWithMessagesQuery, IEnumerable<User>>, GetAllUsersWithMessagesHandler>();
builder.Services.AddScoped<ICommandHandler<CreateUserCommand, User>, CreateUserCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetUserQuery, User>, GetUserHandler>();
builder.Services.AddScoped<IQueryHandler<GetAllUsersWithMessagesAgreemenQuery, IEnumerable<User>>, GetAllUsersWithMessagesAgreemenHandler>();

// Messages
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<ICommandHandler<CreateMessageCommand, Message>, CreateMessageCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetAllMessagesQuery, IEnumerable<Message>>, GetAllMessagesHandler>();
builder.Services.AddScoped<ICommandHandler<SendWhatsAppTemplateCommand, string>, SendWhatsAppTemplateHandler>();
builder.Services.AddTransient<ICommandHandler<PaidAgreementsCommand,PaidAgreementsResult>, PaidAgreementsHandler>();

// Webhook
builder.Services.AddScoped<IWhatsAppWebhookParser, WhatsAppWebhookParser>();

// System User
builder.Services.AddScoped<ISystemUserRepository, SystemUserRepository>();
builder.Services.AddScoped<IQueryHandler<GetSystemUserByUserNameQuery, SystemUser>, GetSystemUserByUserNameHandler>();
builder.Services.AddScoped<ICommandHandler<CreateSystemUserCommand, SystemUser>, CreateSystemUserHandler>();
builder.Services.AddScoped<ICommandHandler<SendReferralListCommand, Message>, SendReferralListHandler>();

//ManyChat
builder.Services.AddScoped<ICommandHandler<SetSurveyFieldsToUsersCommand, ManyChatUserResponse>, SetSurveyFieldsToUsersHandler>();
builder.Services.Configure<ManyChatOptions>(builder.Configuration.GetSection("ManyChat"));
builder.Services.AddHttpClient<SendWhatsAppTemplateHandler>();

//Azure
builder.Services.AddTransient<IBlobService,BlobService>();

//Excel
builder.Services.AddTransient<IExcelFileService, ExcelFileService>();
builder.Services.AddScoped<ICommandHandler<SetUserForAgreementCommand, IEnumerable<Message>>, SetUserForAgreementHandler>();

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// ✅ Activa CORS ANTES de Authorization
app.UseCors("allowCors");

app.UseAuthorization();

app.MapControllers();

app.Run();
