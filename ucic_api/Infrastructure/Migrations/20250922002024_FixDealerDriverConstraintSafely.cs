using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class FixDealerDriverConstraintSafely : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely drop foreign key if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_DealerDrivers_AspNetUsers_ApplicationUserId')
                BEGIN
                    ALTER TABLE [DealerDrivers] DROP CONSTRAINT [FK_DealerDrivers_AspNetUsers_ApplicationUserId]
                END
            ");

            // Safely drop index if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DealerDrivers_ApplicationUserId' AND object_id = OBJECT_ID('DealerDrivers'))
                BEGIN
                    DROP INDEX [IX_DealerDrivers_ApplicationUserId] ON [DealerDrivers]
                END
            ");

            // Safely drop column if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DealerDrivers') AND name = 'ApplicationUserId')
                BEGIN
                    ALTER TABLE [DealerDrivers] DROP COLUMN [ApplicationUserId]
                END
            ");

            // Create index on UserId if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DealerDrivers_UserId' AND object_id = OBJECT_ID('DealerDrivers'))
                BEGIN
                    CREATE INDEX [IX_DealerDrivers_UserId] ON [DealerDrivers] ([UserId])
                END
            ");

            // Add foreign key on UserId if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_DealerDrivers_AspNetUsers_UserId')
                BEGIN
                    ALTER TABLE [DealerDrivers] WITH CHECK ADD CONSTRAINT [FK_DealerDrivers_AspNetUsers_UserId] 
                    FOREIGN KEY([UserId]) REFERENCES [auth].[AspNetUsers] ([Id])
                    
                    ALTER TABLE [DealerDrivers] CHECK CONSTRAINT [FK_DealerDrivers_AspNetUsers_UserId]
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Safely drop the correct foreign key
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_DealerDrivers_AspNetUsers_UserId')
                BEGIN
                    ALTER TABLE [DealerDrivers] DROP CONSTRAINT [FK_DealerDrivers_AspNetUsers_UserId]
                END
            ");

            // Safely drop the correct index
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DealerDrivers_UserId' AND object_id = OBJECT_ID('DealerDrivers'))
                BEGIN
                    DROP INDEX [IX_DealerDrivers_UserId] ON [DealerDrivers]
                END
            ");

            // Add back ApplicationUserId column if it doesn't exist
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DealerDrivers') AND name = 'ApplicationUserId')
                BEGIN
                    ALTER TABLE [DealerDrivers] ADD [ApplicationUserId] nvarchar(450) NULL
                END
            ");

            // Create index on ApplicationUserId
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DealerDrivers_ApplicationUserId' AND object_id = OBJECT_ID('DealerDrivers'))
                BEGIN
                    CREATE INDEX [IX_DealerDrivers_ApplicationUserId] ON [DealerDrivers] ([ApplicationUserId])
                END
            ");

            // Add foreign key on ApplicationUserId
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_DealerDrivers_AspNetUsers_ApplicationUserId')
                BEGIN
                    ALTER TABLE [DealerDrivers] WITH CHECK ADD CONSTRAINT [FK_DealerDrivers_AspNetUsers_ApplicationUserId] 
                    FOREIGN KEY([ApplicationUserId]) REFERENCES [auth].[AspNetUsers] ([Id])
                END
            ");
        }
    }
}