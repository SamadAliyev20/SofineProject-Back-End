﻿using System.ComponentModel.DataAnnotations;

namespace SofineProject.Areas.manage.ViewModels.AccountVMs
{
	public class LoginAdminVM
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		public bool RemindMe { get; set; }
	}
}
