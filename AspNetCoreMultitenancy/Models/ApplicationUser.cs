using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNetCoreMultitenancy.Models
{
    public class ApplicationUser : IdentiyUserMultiTenant<Guid, string>
    {
    }


    // Add profile data for application users by adding properties to the ApplicationUser class
}
