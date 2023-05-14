﻿// Copyright (c) 2022 Quetzal Rivera.
// Licensed under the GNU General Public License v3.0, See LICENCE in the project root for license information.

using SauceNAO.Core;
using SauceNAO.Core.Entities;
using SauceNAO.Core.Extensions;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;

namespace SauceNAO.Service;

public sealed class CleanerService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<CleanerService> _logger;

	public CleanerService(IServiceScopeFactory scopeFactory, ILogger<CleanerService> logger)
	{
		this._scopeFactory = scopeFactory;
		this._logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = this._scopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<ISauceDatabase>();
		this._logger.LogInformation("Cleaner Service is running.");
		var oldSauces = db.Sauces.GetAllSauces()
			.Where(s => s.Date < DateTime.UtcNow.AddDays(-20));
		var sauceCount = oldSauces.Count();
#if DEBUG
		this._logger.LogInformation("{sauceCount} sauces will be cleaned", sauceCount);
#endif
		await db.Sauces.DeleteRangeAsync(oldSauces, stoppingToken).ConfigureAwait(false);

		var groups = db.Groups.GetAllGroups().ToList();
#if DEBUG
		this._logger.LogInformation("Missing groups will be cleaned.");
#endif

		var properties = scope.ServiceProvider.GetRequiredService<SnaoBotProperties>();
		var api = properties.Api;
		var me = properties.User;

		foreach (var group in groups)
		{
			try
			{
				await api.GetChatAsync(group.Id, stoppingToken).ConfigureAwait(false);
				var myMemberProfile = await api.GetChatMemberAsync(group.Id, me.Id, stoppingToken).ConfigureAwait(false);

				if (!myMemberProfile.IsMemberOrAdmin())
				{
					try
					{
						await api.LeaveChatAsync(group.Id, stoppingToken).ConfigureAwait(false);
					}
					finally
					{
						await db.Groups.DeleteAsync(group, stoppingToken).ConfigureAwait(false);
					}
					continue;
				}
			}
			catch (BotRequestException e)
			{
#if DEBUG
				this._logger.LogWarning("Unable to get \"{groupTitle}\" group. Error message: {message}\nChat's data will be cleaned from database.", group.Title, e.Message);
#endif
				await db.Groups.DeleteAsync(group, stoppingToken).ConfigureAwait(false);
				continue;
			}
			catch (Exception e)
			{
				this._logger.LogError("Error while checking \"{groupTitle}\" group. Error message: {message}", group.Title, e.Message);
				continue;
			}

			if (group.AntiCheats.Any())
			{
				foreach (AntiCheat item in group.AntiCheats)
				{
					try
					{
						var chatMember = await api.GetChatMemberAsync(group.Id, item.BotId, stoppingToken).ConfigureAwait(false);
						if (chatMember is ChatMemberLeft or ChatMemberBanned)
						{
							group.AntiCheats.Remove(item);
						}
					}
					catch
					{
						group.AntiCheats.Remove(item);
					}
				}
				await db.Groups.UpdateAsync(group, stoppingToken).ConfigureAwait(false);
			}
		}
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		this._logger.LogInformation("Cleaner Service was finished.");
		return base.StopAsync(cancellationToken);
	}
}
