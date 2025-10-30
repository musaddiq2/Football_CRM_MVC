using FootBallOne.Data;
using FootBallOne.Services; // ✅ ADD THIS LINE
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ Register HttpContextAccessor (for accessing session in services)
builder.Services.AddHttpContextAccessor();

// ✅ Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Register AcademyContext Service (NEW - CRITICAL)
builder.Services.AddScoped<IAcademyContext, AcademyContext>();

// ✅ Configure Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // ✅ Changed to 8 hours for better UX
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".FootballOne.Session"; // ✅ Added explicit cookie name
});

var app = builder.Build();

// ✅ IMPORTANT: UseSession() must come BEFORE UseRouting()
app.UseSession();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // ✅ Changed default to Admin/Login

app.Run();