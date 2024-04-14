﻿using DressUp.Web.ViewModels.Admin;

namespace DressUp.Services.Data.Interfaces;

public interface IAdminService
{
	Task<IEnumerable<RoleViewModel>> GetAllRoles();

	Task CreateRole(string roleForm);

	Task<bool> IsRoleExist(string roleName);

	Task AddUserToRoleAsync(string userEmail, string roleName);

	Task<bool> IsUserHasRole(string userEmail, string roleName);
}
