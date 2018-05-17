using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.Schedules
{
    public class IndexModel : PageModel
    {

        public FileUpload FileUpload { get; set; }

        public IList<Schedule> Schedule { get; set; }

        public void OnGet()
        {
            
        }
    }
}