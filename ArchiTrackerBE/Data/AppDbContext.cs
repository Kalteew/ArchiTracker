using ArchiTrackerBE.Models;
using Microsoft.EntityFrameworkCore;

namespace ArchiTrackerBE.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ArchipelagoRoom> ArchipelagoRooms => Set<ArchipelagoRoom>();
}
