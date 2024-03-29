﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MoneyTracker.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [MaxLength(100)]
    public string ? FirstName { get; set; }

    [PersonalData]
    [MaxLength(100)]
    public string ? LastName { get; set; }

    [PersonalData]
    public DateTime DOB { get; set; }

    [Range(1,31)]
    public int DayOfCycleReset { get; set; } = 1;
}

