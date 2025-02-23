var builder = WebApplication.CreateBuilder(args);
ConfigureBuilder(builder);

var app = builder.Build();
await app.BootUmbracoAsync();
Configure(app, app.Environment);
await app.RunAsync();

static void ConfigureBuilder(WebApplicationBuilder builder)
{
    // Configure Kestrel to not add the Server header
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.AddServerHeader = false;
    });

    // Configure Umbraco
    builder.CreateUmbracoBuilder()
        .AddBackOffice()
        .AddWebsite()
        .AddDeliveryApi()
        .AddComposers()
        .Build();
}

static void Configure(WebApplication app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }

    app.UseUmbraco()
        .WithMiddleware(u =>
        {
            u.UseBackOffice();
            u.UseWebsite();
        })
        .WithEndpoints(u =>
        {
            u.UseInstallerEndpoints();
            u.UseBackOfficeEndpoints();
            u.UseWebsiteEndpoints();
        });

    // Add static files to the request pipeline.
    app.UseStaticFiles();
}