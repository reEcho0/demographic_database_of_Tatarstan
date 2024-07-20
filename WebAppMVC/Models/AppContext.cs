using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore;

namespace WebAppMVC.Models
{
    public class ApplicationContext : DbContext
    {
        public virtual DbSet<Demographics> Demographics { get; set; } = null!;
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("User Id=postgres.hljapwtpzmqjovchyylz;Password=MM0yI98jmB7eQwDQ;Server=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;");

    }
}
