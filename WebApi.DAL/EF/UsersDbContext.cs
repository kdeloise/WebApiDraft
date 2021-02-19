using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApi.DAL.Entities;

namespace WebApi.DAL.EF
{
    public class UsersDbContext : IdentityDbContext<User>
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options)
            : base(options)
        {
        }
    }
}
