using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace DataLibrary;

using Microsoft.EntityFrameworkCore;

public class User : IEnumerable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
    
    [NotMapped]
    public string PasswordConfirm { get; set; }
    
    private object[] GetProperties()
    {
        return new object[]
        {
            Id,
            Name,
            Email,
            Role
        };
    }
    public IEnumerator GetEnumerator()
    {
        return GetProperties().GetEnumerator();
    }
    public string[] GetPropertyNames()
    {
        return new[] { "Id", "Name", "Email", "Role" };
    }
}

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }

    public class AddRoleToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: true);
           
           migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'User'");
           
           migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Admin' WHERE \"Id\" = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
    }