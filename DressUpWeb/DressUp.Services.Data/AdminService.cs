﻿using DressUp.Data.Migrations;
using DressUp.Data.Models;
using DressUp.Services.Data.Interfaces;
using DressUp.Web.Data;
using DressUp.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DressUp.Services.Data;

public class AdminService : IAdminService
{
	private readonly DressUpDbContext dbContext;
	private readonly UserManager<ApplicationUser> userManager;
	private readonly RoleManager<IdentityRole<Guid>> roleManager;

	public AdminService(
		DressUpDbContext dbContext,
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole<Guid>> roleManager)
	{
		this.dbContext = dbContext;
		this.userManager = userManager;
		this.roleManager = roleManager;
	}

	public async Task AddUserToRoleAsync(string userEmail, string roleName)
	{
		ApplicationUser user = await userManager.FindByEmailAsync(userEmail);

		await userManager.AddToRoleAsync(user, roleName);
	}

	public async Task<bool> IsRoleExist(string roleName)
			=> await roleManager.RoleExistsAsync(roleName);

	public async Task CreateRole(string roleName)
	{
		IdentityRole<Guid> role = new()
		{
			Name = roleName
		};

		await roleManager.CreateAsync(role);
	}

	public async Task<IEnumerable<RoleViewModel>> GetAllRoles()
		=> await dbContext
				.Roles
				.Select(r => new RoleViewModel()
				{
					Id = r.Id.ToString(),
					RoleName = r.Name,
				})
				.ToArrayAsync();

	public async Task<bool> IsUserHasRole(string userEmail, string roleName)
	{
		ApplicationUser user = await userManager.FindByEmailAsync(userEmail);

		return await userManager.IsInRoleAsync(user, roleName);
	}

	public async Task DeleteRoleAsync(string roleName, string roleId)
	{
		IdentityRole<Guid> role = await dbContext
			.Roles
			.FirstAsync(r => r.Id == Guid.Parse(roleId));

		await roleManager.DeleteAsync(role);
	}

	public async Task<IList<string>> GetUserRolesByEmailAsync(string email)
	{
		ApplicationUser user = await userManager.FindByEmailAsync(email);


		return await userManager.GetRolesAsync(user);
	}

	public async Task RemoveUserFromRoleAsync(string userEmail, string roleName)
	{
		ApplicationUser user = await userManager.FindByEmailAsync(userEmail);

		await userManager.RemoveFromRoleAsync(user, roleName);

	}
}

