using Fiorello_PB101.Helpers;
using Fiorello_PB101.Services.Interfaces;
using Fiorello_PB101.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;

namespace Fiorello_PB101.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ArchiveController : Controller
    {
        private readonly ICategoryService _categoryService;

        public ArchiveController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        //public async Task<IActionResult> CategoryArchive()
        //{
        //    return View(await _categoryService.GetAllArchiveAsync());
        //}

        [HttpGet]
        public async Task<IActionResult> CategoryArchive(int page = 1)
        {
            var archives = await _categoryService.GetAllPaginateAsync(page, 4);

            var mappedDatas = await _categoryService.GetAllArchiveAsync();

            int totalPage = await GetPageCountAsync(4);

            Paginate<CategoryArchiveVM> paginateDatas = new(mappedDatas, totalPage, page);

            return View(paginateDatas);
        }

        private async Task<int> GetPageCountAsync(int take)
        {
            int archiveCount = await _categoryService.GetCountAsync();

            return (int)Math.Ceiling((decimal)archiveCount / take);
        }

        //sehv var duzeltmek*


    }
}
