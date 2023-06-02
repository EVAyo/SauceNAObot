﻿// Copyright (c) 2023 Quetzal Rivera.
// Licensed under the GNU General Public License v3.0, See LICENCE in the project root for license information.

using SauceNAO.Core.Entities;
using System.Runtime.InteropServices;
using Telegram.BotAPI.AvailableTypes;

namespace SauceNAO.Core.Data;

public interface IUserRepository : IRepository<UserData>
{
	/// <summary>Get all users.</summary>
	/// <returns>An <see cref="IQueryable"/> object of <see cref="UserData"/>.</returns>
	IQueryable<UserData> GetAllUsers();

	/// <summary>Get user data.</summary>
	/// <param name="telegramUser">Telegram user.</param>
	/// <param name="isPrivate">Is private.</param>
	UserData GetUser(ITelegramUser telegramUser);
	/// <summary>Get user data.</summary>
	/// <param name="telegramUser">Telegram user.</param>
	/// <param name="isPrivate">Is private.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	Task<UserData> GetUserAsync(ITelegramUser telegramUser, [Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Insert a new sauce in the user's sauce history.
	/// </summary>
	/// <param name="userId">Unique identifier for the user.</param>
	/// <param name="userSauce">User sauce.</param>
	/// <returns><see cref="UserSauce"/></returns>
	UserSauce InsertSauce(long userId, UserSauce userSauce);

	/// <summary>
	/// Insert a new sauce in the user's sauce history.
	/// </summary>
	/// <param name="userId">Unique identifier for the user.</param>
	/// <param name="userSauce">User sauce.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns><see cref="UserSauce"/></returns>
	Task<UserSauce> InsertSauceAsync(long userId, UserSauce userSauce, [Optional] CancellationToken cancellationToken);

	/// <summary>
	/// Clear the user's sauce history.
	/// </summary>
	/// <param name="userId">Unique identifier for the user.</param>
	void ClearSauces(long userId);
}
