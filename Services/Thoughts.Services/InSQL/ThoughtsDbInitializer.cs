﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Thoughts.DAL;
using Thoughts.Services.Data;

namespace Thoughts.Services.InSQL;

public class ThoughtsDbInitializer
{
    private readonly ThoughtsDB _db;
    private readonly ILogger<ThoughtsDbInitializer> _Log;

    public ThoughtsDbInitializer(ThoughtsDB db, ILogger<ThoughtsDbInitializer> Log)
    {
        _db = db;
        _Log = Log;
    }

    public async Task DeleteAsync(CancellationToken Cancel = default)
    {
        await _db.Database.EnsureDeletedAsync(Cancel).ConfigureAwait(false);
    }

    public async Task InitializeAsync(bool RemoveBefore = false, bool InitializeTestData = false, CancellationToken Cancel = default)
    {
        if (RemoveBefore)
            await DeleteAsync(Cancel).ConfigureAwait(false);

        var pending_migrations = await _db.Database.GetPendingMigrationsAsync(Cancel).ConfigureAwait(false);
        var applied_migrations = await _db.Database.GetPendingMigrationsAsync(Cancel);

        if(applied_migrations.Any())
            _Log.LogInformation("К БД применены миграции: {0}", string.Join(",", applied_migrations));

        if (pending_migrations.Any())
        {
            _Log.LogInformation("Применение миграций: {0}...", string.Join(",", pending_migrations));
            await _db.Database.MigrateAsync(Cancel);
            _Log.LogInformation("Применение миграций выполнено");
        }
        else
        {
            await _db.Database.EnsureCreatedAsync(Cancel);
        }

        if (InitializeTestData)
            await InitializeTestDataAsync(Cancel);
    }

    private async Task InitializeTestDataAsync(CancellationToken Cancel = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(Cancel).ConfigureAwait(false);

        await _db.AddAsync(TestDbData.Categories, Cancel);
        await _db.AddAsync(TestDbData.Tags, Cancel);
        await _db.AddAsync(TestDbData.Users, Cancel);
        await _db.AddAsync(TestDbData.Posts, Cancel);

        await _db.SaveChangesAsync(Cancel);

        await transaction.CommitAsync(Cancel);
    }
}
